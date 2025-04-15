namespace Scribe.Memory.Elf;

public record ElfSymbol {
	public uint NameIndex;
	public nint Address;
}