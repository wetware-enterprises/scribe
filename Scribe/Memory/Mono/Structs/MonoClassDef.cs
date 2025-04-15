using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClassDef {
#if OS_WINDOWS
	private const int _002 = 0x108;
#elif OS_LINUX
	private const int _002 = 0x100;
#endif
	
	[FieldOffset(0x00)] public nint Class;
	[FieldOffset(0xF8)] public nint FieldCount;
	[FieldOffset(_002)] public nint Next;
}