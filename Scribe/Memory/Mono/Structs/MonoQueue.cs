using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoQueue {
	[FieldOffset(0x10)] public nint _array;
	[FieldOffset(0x18)] public nint _syncRoot;
	[FieldOffset(0x20)] public uint _head;
	[FieldOffset(0x24)] public uint _tail;
	[FieldOffset(0x28)] public uint _size;
	[FieldOffset(0x2C)] public uint _version;
}