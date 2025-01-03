using System.Runtime.InteropServices;

using Scribe.Hackmud.Game.Enums;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct Kernel {
	[FieldOffset(0x0D8)] public nint Hardline;
	[FieldOffset(0x0F8)] public nint HackModeCountdown;
	[FieldOffset(0x1D4)] public KernelMode CurrentMode;
}