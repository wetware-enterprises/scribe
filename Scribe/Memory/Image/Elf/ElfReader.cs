using System.Diagnostics.CodeAnalysis;
using Scribe.Memory.Reader;

namespace Scribe.Memory.Image.Elf;

public class ElfReader(FileReader fr) {
	public ElfHeader ReadHeader(nint baseAddr) {
		var header = new ElfHeader();
		
		fr.Position = baseAddr;
		header.Magic = fr.ReadUInt32();
		fr.Seek(0x0C);
		header.Type = fr.ReadUInt16();
		fr.Seek(0x16);
		header.ShOffset = (nint)fr.ReadUInt64();
		fr.Seek(0x0A);
		header.ShEntrySize = fr.ReadUInt16();
		header.ShNum = fr.ReadUInt16();
		header.ShStrIndex = fr.ReadUInt16();

		var sections = this.ReadSections(
			baseAddr,
			header.ShOffset,
			header.ShEntrySize,
			header.ShNum
		).ToArray();

		var strTable = sections[header.ShStrIndex];
		
		foreach (var section in sections) {
			if (section.NameIndex == 0) continue;
			fr.Position = baseAddr + strTable.Offset + section.NameIndex;
			var name = fr.ReadCString();
			header.Sections.Add(name, section);
		}

		return header;
	}

	public IEnumerable<ElfSectionHeader> ReadSections(
		nint baseAddress,
		nint shOff,
		ushort shEntSize,
		ushort shNum
	) {
		for (var i = 0; i < shNum; i++) {
			var section = new ElfSectionHeader();
			
			fr.Position = baseAddress + shOff + shEntSize * i;
			section.NameIndex = fr.ReadUInt32();
			section.Type = (ElfSectionType)fr.ReadUInt32();
			fr.Seek(0x08);
			section.Address = (nint)fr.ReadUInt64();
			section.Offset = (nint)fr.ReadUInt64();
			section.Size = fr.ReadUInt32();
			fr.Seek(0x14);
			section.EntrySize = fr.ReadUInt32();
			
			yield return section;
		}
	}

	public bool TryFindSymbol(
		nint baseAddress,
		ElfHeader header,
		string symbolName,
		[NotNullWhen(true)] out ElfSymbol? symbol
	) {
		var dynStr = header.Sections[".dynstr"];
		var dynSym = header.Sections[".dynsym"];
		
		var symCt = uint.DivRem(dynSym.Size, dynSym.EntrySize).Quotient;
		for (var i = 0; i < symCt; i++) {
			var offset = baseAddress + dynSym.Offset + dynSym.EntrySize * i;
			fr.Position = offset;
			
			var nameIdx = fr.ReadUInt32();
			fr.Position = baseAddress + dynStr.Offset + nameIdx;
			var name = fr.ReadCString();
			if (name != symbolName) continue;
			
			fr.Position = offset + 8;
			var addr = (nint)fr.ReadUInt64();

			symbol = new ElfSymbol {
				NameIndex = nameIdx,
				Address = addr
			};

			return true;
		}

		symbol = null;
		return false;
	}
}