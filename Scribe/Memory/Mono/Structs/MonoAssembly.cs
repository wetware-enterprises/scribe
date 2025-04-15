using System.Runtime.InteropServices;

using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoAssembly {
	[FieldOffset(0x08)] public nint Location;
	[FieldOffset(0x10)] public nint Name;
	[FieldOffset(0x18)] public nint TypeData;
	[FieldOffset(0x60)] public nint Image;

	public string? ReadName(IMemoryReader reader)
		=> reader.ReadString(this.Name);

	public bool TryReadImage(IMemoryReader reader, out MonoImage result)
		=> reader.TryRead(this.Image, out result);
}