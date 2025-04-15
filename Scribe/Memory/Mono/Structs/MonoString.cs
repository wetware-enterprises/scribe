using System.Runtime.InteropServices;
using System.Text;

using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct MonoString {
	public nint Address;

	public string? Read(IMemoryReader reader) {
		var offset = this.Address + 0x10;
		if (this.Address == nint.Zero
		    || !reader.TryRead<uint>(offset, out var length)
		    || !reader.TryReadBuffer(offset + 0x4, (int)(length * 2), out var buffer)
		) return null;
		return Encoding.Unicode.GetString(buffer);
	}
}