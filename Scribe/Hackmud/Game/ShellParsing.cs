using System.Runtime.InteropServices;

using Scribe.Memory.Mono.Structs;

namespace Scribe.Hackmud.Game;

[StructLayout(LayoutKind.Explicit)]
public struct ShellParsing {
	[FieldOffset(0x000)] public nint __vfTable;
	[FieldOffset(0x010)] public nint m_CachedPtr;

	[FieldOffset(0x020)] public nint Kernel;
	[FieldOffset(0x030)] public nint Window;

	[FieldOffset(0x060)] public nint Api;

	[FieldOffset(0x088)] public nint SyncQueue;
	[FieldOffset(0x090)] public MonoString ProcessingId;

	[FieldOffset(0x11D)] public bool IsProcessing;
}