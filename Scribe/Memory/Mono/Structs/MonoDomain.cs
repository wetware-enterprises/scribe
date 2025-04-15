using System.Runtime.InteropServices;

using Scribe.Memory.Common;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoDomain {
#if OS_WINDOWS
	private const int _000 = 0xA0;
#elif OS_LINUX
	private const int _000 = 0x98;
#endif
	
	[FieldOffset(_000)] public StdLinkedList<MonoAssembly> Assemblies;
}