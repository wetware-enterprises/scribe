using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClass {
#if OS_WINDOWS
	private const int _002 = 0x48;
	private const int _003 = 0x50;
	private const int _004 = 0x58;
	private const int _005 = 0xD0;
#elif OS_LINUX
	private const int _002 = 0x40;
	private const int _003 = 0x48;
	private const int _004 = 0x50;
	private const int _005 = 0xC8;
#endif
	
	[FieldOffset(0x00)] public nint ElementClass;
	[FieldOffset(0x08)] public nint CastClass;
	[FieldOffset(_002)] public nint Name;
	[FieldOffset(_003)] public nint Namespace;
	[FieldOffset(_004)] public uint Handle;
	[FieldOffset(_005)] public nint RuntimeInfo;
	[FieldOffset(0xF8)] public nint FieldCount;
}