using System.Diagnostics;
using System.Runtime.InteropServices;

using Scribe.WinApi;
using Scribe.Memory.Mono.Structs;
using Scribe.Scanner;

namespace Scribe.Memory;

public class MemoryReader : IDisposable {
	private const string MainAssembly = "Core";
	
	private readonly nint _hProcess;
	private readonly ProcessModule _mono;

	private readonly Dictionary<string, Dictionary<string, MonoClass>> _classMap = new();

	private byte[] _buffer = new byte[4096];

	private bool _isDisposed;
	
	public MemoryReader(
		nint hProcess,
		ProcessModule mono
	) {
		this._hProcess = hProcess;
		this._mono = mono;
	}
	
	// Memory reading
	
	private bool ReadBufferCopy(nint lpBaseAddress, int dwSize, out byte[] buffer) {
		var bytesRead = nint.Zero;
		buffer = new byte[dwSize];
		return lpBaseAddress != nint.Zero && Kernel32.ReadProcessMemory(
			this._hProcess,
			lpBaseAddress,
			buffer,
			dwSize,
			ref bytesRead
		);
	}

	public bool ReadBuffer(nint lpBaseAddress, int dwSize, out byte[] buffer) {
		var bytesRead = nint.Zero;
		if (dwSize > this._buffer.Length)
			this._buffer = new byte[dwSize];
		var result = lpBaseAddress != nint.Zero && Kernel32.ReadProcessMemory(
			this._hProcess,
			lpBaseAddress,
			this._buffer,
			dwSize,
			ref bytesRead
		);
		buffer = result ? this._buffer[..dwSize] : Array.Empty<byte>();
		return result;
	}

	public unsafe bool Read<T>(nint lpBaseAddress, out T result) where T : unmanaged {
		var bytesRead = nint.Zero;
		var dwSize = sizeof(T);
		if (dwSize > this._buffer.Length)
			this._buffer = new byte[dwSize];
		var success = lpBaseAddress != nint.Zero && Kernel32.ReadProcessMemory(
			this._hProcess,
			lpBaseAddress,
			this._buffer,
			dwSize,
			ref bytesRead
		);
		if (!success) {
			result = default;
			return false;
		}
		fixed (void* data = this._buffer)
			result = *(T*)data;
		return true;
	}

	public bool ReadIntPtr(nint lpBaseAddress, out nint result)
		=> this.Read(lpBaseAddress, out result);

	public unsafe string? ReadString(nint lpBaseAddress) {
		if (lpBaseAddress == nint.Zero) return null;
		
		var bytes = new List<byte>();
		var cursor = lpBaseAddress;
		while (this.Read<byte>(cursor++, out var b) && b != 0)
			bytes.Add(b);

		var buffer = bytes.ToArray();
		fixed (void* data = buffer)
			return Marshal.PtrToStringUTF8((nint)data);
	}

	public T ReadUnchecked<T>(nint lpBaseAddress) where T : unmanaged {
		this.Read<T>(lpBaseAddress, out var result);
		return result;
	}
	
	// Mono

	private bool IsClassMapPopulated => this._classMap.Count != 0;
	
	public MonoClass GetClass(string nameSpace, string name) {
		if (!this.IsClassMapPopulated)
			this.BuildClassMap();

		if (!this._classMap.TryGetValue(nameSpace, out var nameSpaceMap))
			throw new Exception($"No mapping for namespace: '{nameSpace}'");

		if (!nameSpaceMap.TryGetValue(name, out var monoClass))
			throw new Exception($"Unknown class '{name}' in '{nameSpace}'");

		return monoClass;
	}
	
	public unsafe nint FindClassInstance(MonoClass monoClass) {
		if (!this.Read<MonoRuntimeInfo>(monoClass.RuntimeInfo, out var runtimeInfo))
			return nint.Zero;
		
		var systemInfo = new Kernel32.SystemInfo();
		Kernel32.GetSystemInfo(ref systemInfo);
		
		var address = systemInfo.MinimumApplicationAddress;
		var addressMax = systemInfo.MaximumApplicationAddress;
		
		while (address < addressMax) {
			var info = new Kernel32.MemoryBasicInformation();

			var result = Kernel32.VirtualQueryEx(
				this._hProcess,
				address,
				ref info,
				Marshal.SizeOf<Kernel32.MemoryBasicInformation>()
			);
			
			if (result == 0) throw new Exception($"VirtualQueryEx failed: {Marshal.GetLastWin32Error():X}");

			if (info is { Protect: Kernel32.PageProtect.ReadWrite, Type: Kernel32.PageType.Private }) {
				var size = (int)info.RegionSize;
				var length = size / 8;
				if (this.ReadBufferCopy(info.BaseAddress, size, out var buffer)) {
					fixed (void* pVoid = buffer) {
						var pPtr = (nint*)pVoid;
						for (var i = 1; i < length; i++) {
							if (pPtr[i] != monoClass.ElementClass) continue;

							var instPtr = pPtr[i - 1];
							if (instPtr == nint.Zero) continue;

							if (!this.ReadIntPtr(instPtr, out var vfPtr) || vfPtr != runtimeInfo.VfTable)
								continue;
							
							return instPtr;
						}
					}
				} else {
					Console.WriteLine("Failed to copy buffer.");
					break;
				}
			}

			address += info.RegionSize;
		}

		return nint.Zero;
	}

	private unsafe bool FindDomainPtr(out nint gDomainPtr) {
		const int headerCoffOffset = 0x3C;
		const int headerCoffSize = 0x18;
		const int headerSectionSize = 0x28;

		var baseAddr = this._mono.BaseAddress;
		
		var coffOffset = this.ReadUnchecked<int>(baseAddr + headerCoffOffset);
		var sectionCt = this.ReadUnchecked<ushort>(baseAddr + coffOffset + 0x06);
		var optHeaderSize = this.ReadUnchecked<ushort>(baseAddr + coffOffset + 0x14);

		var sectionOffset = coffOffset + headerCoffSize + optHeaderSize;
		var sectionSize = sectionCt * headerSectionSize;

		if (!this.ReadBuffer(baseAddr, sectionOffset + sectionSize, out var headerBuf))
			throw new Exception("Failed to read PE header.");

		nint textBase;
		byte[] textBuf;
		
		fixed (void* pHeaderBuf = headerBuf) {
			var textPtr = SigScanner.LookupSectionName((nint)pHeaderBuf, ".text", out var size);
			textBase = baseAddr + (textPtr - (nint)pHeaderBuf);
			this.ReadBufferCopy(textBase, (int)size, out textBuf);
		}

		nint sigAddr;
		
		fixed (void* pTextBuf = textBuf) {
			var sigPtr = SigScanner.ScanMemory((nint)pTextBuf, textBuf.Length, "48 3B 0D ?? ?? ?? ?? 4C 8B E9");
			sigAddr = textBase + (sigPtr - (nint)pTextBuf);
		}

		var asmPtr = sigAddr + this.ReadUnchecked<int>(sigAddr + 3) + 7;
		return this.ReadIntPtr(asmPtr, out gDomainPtr);
	}

	private void BuildClassMap() {
		this._classMap.Clear();

		if (!this.FindDomainPtr(out var gDomainPtr))
			throw new Exception("Failed to scan for global domain pointer.");

		if (!this.Read<MonoDomain>(gDomainPtr, out var domain))
			throw new Exception("Failed to read mono domain.");

		MonoAssembly assembly;
		try {
			assembly = domain.Assemblies.GetEnumerator(this)
				.First(item => item.ReadName(this) == MainAssembly);
		} catch {
			throw new Exception($"Failed to find assembly: {MainAssembly}");
		}
		
		if (assembly.ReadImage(this) is not MonoImage image)
			throw new Exception("Failed to read MonoImage from assembly.");
		
		var i = 0;
		foreach (var type in image.ReadTypeDefInfo(this)) {
			i++;
			
			var nameSpace = this.ReadString(image.StringHeap + type.Namespace) ?? string.Empty;
			if (!this._classMap.TryGetValue(nameSpace, out var nameSpaceMap))
				this._classMap.Add(nameSpace, nameSpaceMap = []);
			
			var name = this.ReadString(image.StringHeap + type.Name);
			if (name == null || nameSpaceMap.ContainsKey(name)) continue;

			var handle = (uint)(i | 0x02000000);
			var monoClass = image.FindClassDef(this, handle);
			if (monoClass != null) nameSpaceMap.Add(name, monoClass.Value);
		}
	}
	
	// IDisposable

	public void Dispose() {
		if (this._isDisposed)
			throw new Exception("Object is already disposed.");
		Kernel32.CloseHandle(this._hProcess);
		this._isDisposed = true;
	}
}