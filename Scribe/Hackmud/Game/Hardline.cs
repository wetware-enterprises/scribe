using System.Runtime.InteropServices;

using Scribe.Memory.Mono.Structs;
using Scribe.Hackmud.Game.Enums;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct Hardline {
	[FieldOffset(0x80)] public MonoString Digits;
	[FieldOffset(0xB0)] public uint CurrentDigit;
	[FieldOffset(0xB8)] public HardlineStep CurrentStep;
}