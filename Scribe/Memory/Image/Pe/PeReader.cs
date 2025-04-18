using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Image.Pe;

// https://learn.microsoft.com/en-us/windows/win32/debug/pe-format (PE32+)

public class PeReader(IMemoryReader mr) {
	private const int HeaderCoffOffset = 0x3C;
	private const int HeaderCoffSize = 0x18;
	private const int OptHeaderDataDir = 0x70;
	
	public bool TryGetExportDirectory(nint baseAddr, out PeExportDirectory dir) {
		var coffOffset = mr.Read<int>(baseAddr + HeaderCoffOffset);

		var tablePtr = mr.Read<uint>(baseAddr + coffOffset + HeaderCoffSize + OptHeaderDataDir);
		if (tablePtr == 0) {
			dir = default;
			return false;
		}

		dir = mr.Read<PeExportDirectory>(baseAddr + (nint)tablePtr);
		return true;
	}

	public bool TryGetExportByName(nint baseAddr, PeExportDirectory dir, string name, out nint addr) {
		for (var i = 0; i < dir.NameCount; i++) {
			var namePtr = mr.Read<uint>(baseAddr + (nint)dir.NameTable + i * 4);
			var entryName = mr.ReadString(baseAddr + (nint)namePtr);
			if (entryName != name) continue;

			addr = baseAddr + (nint)mr.Read<uint>(baseAddr + (nint)dir.FunctionTable + i * 4);
			return true;
		}
		addr = nint.Zero;
		return false;
	}
}