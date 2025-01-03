using System.Runtime.InteropServices;

namespace Scribe.Memory.Common;

[StructLayout(LayoutKind.Explicit)]
public struct StringBuilder {
	[FieldOffset(0x10)] public nint m_ChunkChars;
	[FieldOffset(0x18)] public nint m_ChunkPrevious;
	[FieldOffset(0x20)] public uint m_ChunkLength;
	[FieldOffset(0x24)] public uint m_ChunkOffset;
	[FieldOffset(0x28)] public uint m_MaxCapacity;
}