using System.Runtime.InteropServices;

using Scribe.Memory.Mono.Structs;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct HackmudApi {
	[FieldOffset(0x20)] public MonoString CurrentUser;

	[FieldOffset(0x30)] public nint Queue;
}