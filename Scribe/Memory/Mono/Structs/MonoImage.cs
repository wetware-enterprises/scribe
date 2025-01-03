using System.Runtime.InteropServices;
using Scribe.Memory.Mono.Enums;
using Scribe.Memory.Mono.Metadata;

namespace Scribe.Memory.Mono.Structs;

[StructLayout(LayoutKind.Explicit)]
public struct MonoImage {
	private const int TableInfoLength = 0x37;
	
	[FieldOffset(0x020)] public nint FilePath;
	
	[FieldOffset(0x030)] public nint FileName;
	[FieldOffset(0x038)] public nint FileNameAndExtension;

	[FieldOffset(0x048)] public nint Version;
	
	[FieldOffset(0x058)] public nint GUID;

	[FieldOffset(0x078)] public nint StringHeap;
	[FieldOffset(0x080)] public uint StringCount;

	[FieldOffset(0x0F0)] public unsafe fixed byte TableInfo[0x10 * TableInfoLength];

	[FieldOffset(0x4C0)] public nint Assembly;

	[FieldOffset(0x4D0)] public MonoHashTable Classes;

	public unsafe Span<MonoTableInfo> GetTableInfoSpan() {
		fixed (void* ptr = this.TableInfo)
			return new Span<MonoTableInfo>(ptr, TableInfoLength);
	}

	public MonoTableInfo GetTableInfo(MonoTable index) {
		if ((int)index > TableInfoLength)
			throw new IndexOutOfRangeException($"{index}");
		return this.GetTableInfoSpan()[(int)index];
	}

	public IEnumerable<TypeDefInfo> ReadTypeDefInfo(MemoryReader reader) {
		var table = this.GetTableInfo(MonoTable.TypeDef);
		foreach (var data in table.ReadRows(reader))
			yield return new TypeDefInfo(data);
	}

	public MonoClass? FindClassDef(MemoryReader reader, uint handle) {
		var ptr = this.Classes.GetIndex(handle);
		if (!reader.ReadIntPtr(ptr, out var cursor))
			return null;
		
		while (cursor != nint.Zero) {
			if (!reader.Read<MonoClassDef>(cursor, out var def) || !reader.Read<MonoClass>(def.Class, out var defClass))
				break;
			
			if (defClass.Handle == handle)
				return defClass;
			
			cursor = def.Next;
		}

		return null;
	}
}