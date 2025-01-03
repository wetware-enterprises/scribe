using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoHashTable {
	[FieldOffset(0x10)] public nint Next;
	[FieldOffset(0x18)] public uint Size;
	[FieldOffset(0x1C)] public uint Count;
	[FieldOffset(0x20)] public nint Data;

	public nint GetIndex(uint i) => (nint)(this.Data + ((i % this.Size) * nint.Size));
}