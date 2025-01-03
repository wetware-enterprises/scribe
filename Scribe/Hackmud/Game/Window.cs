using System.Runtime.InteropServices;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct Window {
	[FieldOffset(0x48)] public nint CurrentCommand;
	[FieldOffset(0x78)] public nint Output;
}