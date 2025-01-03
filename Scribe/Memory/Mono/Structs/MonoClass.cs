using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClass {
	[FieldOffset(0x00)] public nint ElementClass;
	[FieldOffset(0x08)] public nint CastClass;
	[FieldOffset(0x48)] public nint Name;
	[FieldOffset(0x50)] public nint Namespace;
	[FieldOffset(0x58)] public uint Handle;
	[FieldOffset(0xD0)] public nint RuntimeInfo;
}