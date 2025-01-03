using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoRuntimeInfo {
	[FieldOffset(0x0)] public nint MaxDomain;
	[FieldOffset(0x8)] public nint VfTable;
}