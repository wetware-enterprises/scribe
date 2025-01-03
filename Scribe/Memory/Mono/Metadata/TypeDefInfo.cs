namespace Scribe.Memory.Mono.Metadata;

public struct TypeDefInfo {
	public uint Flags;
	public ushort Name;
	public ushort Namespace;
	public ushort Extends;
	public ushort FieldList;
	public ushort MethodList;

	public TypeDefInfo(uint[] data) {
		this.Flags = data[0];
		this.Name = (ushort)data[1];
		this.Namespace = (ushort)data[2];
		this.Extends = (ushort)data[3];
		this.FieldList = (ushort)data[4];
		this.MethodList = (ushort)data[5];
	}
}