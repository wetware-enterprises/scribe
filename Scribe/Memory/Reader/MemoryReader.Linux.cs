#if OS_LINUX

using System.Diagnostics;

using Scribe.Memory.Elf;
using Scribe.Memory.Map;
using Scribe.Memory.Mono.Structs;
using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Reader;

public class MemoryReader : MemoryReaderBase, IMemoryReaderImpl {
	private readonly Process _process;
	private readonly ProcessModule _mono;
	private readonly FileReader _fr;
	
	private MemoryReader(
		Process process,
		ProcessModule mono,
		FileReader fr
	) {
		this._process = process;
		this._mono = mono;
		this._fr = fr;
	}
	
	// IMemoryReaderImpl
	
	public static IMemoryReaderImpl Open(Process process, ProcessModule mono) {
		var fr = FileReader.Open($"/proc/{process.Id}/mem");
		return new MemoryReader(process, mono, fr);
	}
	
	// Memory reading

	public override bool TryReadBuffer(nint address, int size, out byte[] buffer) {
		if (!this._fr.CanSeek || size < 0) {
			buffer = [];
			return false;
		}
		this._fr.Position = address;
		buffer = this._fr.ReadBuffer(size);
		return true;
	}
	
	// Mono

	public unsafe override nint FindClassInstance(MonoClass monoClass, Func<nint, bool> validate) {
		const MappedFilePerms perms = MappedFilePerms.Read | MappedFilePerms.Write | MappedFilePerms.Private;
		
		if (!this.TryRead<MonoRuntimeInfo>(monoClass.RuntimeInfo, out var runtimeInfo))
			return nint.Zero;
		
		var regions = MappedFile.GetAll(this._process)
			.Where(file => file is { PathName.Length: 0, Perms: perms });
		
		foreach (var region in regions) {
			var size = region.Ceiling - region.Base;
			var length = size / 8;
			
			if (!this.TryReadBuffer(region.Base, (int)size, out var buffer)) {
				Console.WriteLine("Read failed");
				continue;
			}

			fixed (void* pVoid = buffer) {
				var pPtr = (nint*)pVoid;
				for (var i = 0; i < length; i++) {
					if (pPtr[i] != runtimeInfo.VfTable) continue;

					try {
						var addr = region.Base + 8 * i;
						if (validate(addr)) return addr;
					} catch {
						// continue
					}
				}
			}
		}
		
		return nint.Zero;
	}

	protected override bool TryResolveDomainPtr(out nint gDomainPtr) {
		var reader = new ElfReader(this._fr);
		var header = reader.ReadHeader(this._mono.BaseAddress);
		if (!reader.TryFindSymbol(this._mono.BaseAddress, header, "mono_get_root_domain", out var symbol)) {
			gDomainPtr = nint.Zero;
			return false;
		}
		var address = this._mono.BaseAddress + symbol.Address;
		this._fr.Position = address + 3;
		var asmPtr = address + this._fr.ReadInt32() + 7;
		return this.TryReadPtr(asmPtr, out gDomainPtr);
	}
}

#endif