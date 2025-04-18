#if OS_WINDOWS

using System.Diagnostics;
using System.Runtime.InteropServices;

using Scribe.Memory.Image.Pe;
using Scribe.Memory.Mono.Structs;
using Scribe.Memory.Reader.Types;
using Scribe.Platform.WinApi;

namespace Scribe.Memory.Reader;

public class MemoryReader : MemoryReaderBase, IMemoryReaderImpl {
	private readonly nint _hProcess;
	private readonly ProcessModule _mono;
	
	private byte[] _buffer = new byte[4096];
	
	private MemoryReader(
		nint hProcess,
		ProcessModule mono
	) {
		this._hProcess = hProcess;
		this._mono = mono;
	}
	
	// IMemoryReaderImpl
	
	public static IMemoryReaderImpl Open(Process process, ProcessModule mono) {
		var hProc = Kernel32.OpenProcess(
			Kernel32.ProcessVmAccess.Read | Kernel32.ProcessVmAccess.Query,
			false,
			process.Id
		);

		if (hProc == nint.Zero)
			throw new Exception($"Failed to open handle to PID {process.Id} (Error: {Marshal.GetLastWin32Error()})");

		return new MemoryReader(hProc, mono);
	}
	
	// Memory reading
	
	private bool TryReadBufferCopy(nint address, int size, out byte[] buffer) {
		var bytesRead = nint.Zero;
		buffer = new byte[size];
		return address != nint.Zero && Kernel32.ReadProcessMemory(
			this._hProcess,
			address,
			buffer,
			size,
			ref bytesRead
		);
	}

	public override bool TryReadBuffer(nint address, int size, out byte[] buffer) {
		var bytesRead = nint.Zero;
		if (size > this._buffer.Length)
			this._buffer = new byte[size];
		var result = address != nint.Zero && Kernel32.ReadProcessMemory(
			this._hProcess,
			address,
			this._buffer,
			size,
			ref bytesRead
		);
		buffer = result ? this._buffer[..size] : [];
		return result;
	}

	// Mono

	public unsafe override nint FindClassInstance(MonoClass monoClass, Func<nint, bool> validate) {
		if (!this.TryRead<MonoRuntimeInfo>(monoClass.RuntimeInfo, out var runtimeInfo))
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
				if (this.TryReadBufferCopy(info.BaseAddress, size, out var buffer)) {
					fixed (void* pVoid = buffer) {
						var pPtr = (nint*)pVoid;
						for (var i = 1; i < length; i++) {
							if (pPtr[i] != monoClass.ElementClass) continue;

							var instPtr = pPtr[i - 1];
							if (instPtr == nint.Zero) continue;

							if (!this.TryReadPtr(instPtr, out var vfPtr) || vfPtr != runtimeInfo.VfTable)
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

	protected override bool TryResolveDomainPtr(out nint gDomainPtr) {
		var baseAddr = this._mono.BaseAddress;
		gDomainPtr = nint.Zero;
		
		var pe = new PeReader(this);
		if (!pe.TryGetExportDirectory(baseAddr, out var dir))
			return false;
		if (!pe.TryGetExportByName(baseAddr, dir, "mono_get_root_domain", out var addr))
			return false;
		
		var asmPtr = addr + this.Read<int>(addr + 3) + 7;
		return this.TryReadPtr(asmPtr, out gDomainPtr);
	}
	
	// IDisposable

	public override void Dispose() {
		if (!Kernel32.CloseHandle(this._hProcess))
			Console.WriteLine("Warning: Failed to close process handle!");
		GC.SuppressFinalize(this);
	}
}

#endif