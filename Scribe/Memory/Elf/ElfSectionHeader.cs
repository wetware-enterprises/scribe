namespace Scribe.Memory.Elf;

public record ElfSectionHeader {
	public uint NameIndex;
	public ElfSectionType Type;
	public nint Address;
	public nint Offset;
	public uint Size;
	public uint EntrySize;
}