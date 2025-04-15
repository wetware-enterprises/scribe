namespace Scribe.Memory.Elf;

public record ElfHeader {
	public uint Magic;
	public ushort Type;
	public nint ShOffset;
	public ushort ShEntrySize;
	public ushort ShNum;
	public ushort ShStrIndex;
	public Dictionary<string, ElfSectionHeader> Sections = [];
}