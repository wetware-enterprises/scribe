using System.Runtime.InteropServices;
using System.Text;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct MonoString {
	public nint Address;

	public string? Read(MemoryReader reader) {
		var offset = this.Address + 0x10;
		if (this.Address == nint.Zero
		    || !reader.Read<uint>(offset, out var length)
		    || !reader.ReadBuffer(offset + 0x4, (int)(length * 2), out var buffer)
		) return null;
		return Encoding.Unicode.GetString(buffer);
	}
}