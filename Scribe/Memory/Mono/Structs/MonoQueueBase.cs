using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoQueueBase {
	[FieldOffset(0x10)] public nint Data;
	[FieldOffset(0x14)] public uint Length;
}