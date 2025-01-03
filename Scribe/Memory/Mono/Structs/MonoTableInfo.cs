using System.Runtime.InteropServices;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public struct MonoTableInfo {
	[FieldOffset(0x00)] public nint Data;
	[FieldOffset(0x08)] public uint RowCountAndSize;
	[FieldOffset(0x0C)] public uint BitField;

	public uint GetRowCount() => this.RowCountAndSize & 0x00FFFFFF;
	public uint GetRowSize() => this.RowCountAndSize >> 24;
	public uint GetColumns() => this.BitField >> 24;

	public uint[] ReadRow(MemoryReader reader, int row) {
		var count = this.GetColumns();
		var columns = new uint[count];
		
		var cursor = (nint)(this.Data + row * this.GetRowSize());
		for (var i = 0; i < count; i++) {
			var size = ((this.BitField >> (i * 2)) & 0x3) + 1;
			columns[i] = size switch {
				1 => reader.ReadUnchecked<byte>(cursor),
				2 => reader.ReadUnchecked<ushort>(cursor),
				4 => reader.ReadUnchecked<uint>(cursor),
				_ => throw new Exception($"Invalid column size: ${size}")
			};
			cursor += (nint)size;
		}

		return columns;
	}

	public IEnumerable<uint[]> ReadRows(MemoryReader reader) {
		var count = this.GetRowCount();
		for (var i = 0; i < count; i++)
			yield return this.ReadRow(reader, i);
	}
}