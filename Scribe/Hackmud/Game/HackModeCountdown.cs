using System.Runtime.InteropServices;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct HackModeCountdown {
	[FieldOffset(0x38)] public nint Timer;
	[FieldOffset(0x50)] public bool IsOvertime;
}