using System.Runtime.InteropServices;

namespace Scribe.Memory.Common;

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct StdLinkedList<T> where T : unmanaged {
	public nint Head; // Node<T>*

	[StructLayout(LayoutKind.Sequential, Size = 0x10)]
	private struct Node {
		public nint Data; // T*
		public nint Next; // Node<T>*
	}

	public IEnumerable<T> GetEnumerator(
		MemoryReader reader
	) {
		if (!reader.Read<Node>(this.Head, out var node))
			yield break;
		
		while (node.Next != nint.Zero) {
			if (reader.Read<T>(node.Data, out var data))
				yield return data;
			if (node.Next == this.Head || !reader.Read(node.Next, out node))
				break;
		}
	}
}