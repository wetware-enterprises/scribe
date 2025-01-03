using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClassDef {
	[FieldOffset(0x000)] public nint Class;
	[FieldOffset(0x0F8)] public nint FieldCount;
	[FieldOffset(0x108)] public nint Next;
}