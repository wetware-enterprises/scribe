using System.Runtime.InteropServices;

using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClassField {
	[FieldOffset(0x00)] public ulong Type;
	[FieldOffset(0x08)] public nint Name;
	[FieldOffset(0x10)] public nint Parent;
	[FieldOffset(0x18)] public uint Offset;
	
	public string? ReadName(IMemoryReader reader)
		=> reader.ReadString(this.Name);
}