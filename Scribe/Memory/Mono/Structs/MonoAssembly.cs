using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoAssembly {
	[FieldOffset(0x08)] public nint Location;
	[FieldOffset(0x10)] public nint Name;
	[FieldOffset(0x18)] public nint TypeData;

	[FieldOffset(0x60)] public nint Image;

	public string? ReadName(MemoryReader reader)
		=> reader.ReadString(this.Name);

	public MonoImage? ReadImage(MemoryReader reader)
		=> reader.Read<MonoImage>(this.Image, out var image) ? image : null;
}