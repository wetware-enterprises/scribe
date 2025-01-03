using System.Runtime.InteropServices;

using Scribe.Memory.Common;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoDomain {
	[FieldOffset(0xA0)] public StdLinkedList<MonoAssembly> Assemblies;
}