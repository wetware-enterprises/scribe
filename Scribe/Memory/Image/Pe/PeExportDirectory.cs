using System.Runtime.InteropServices;

namespace Scribe.Memory.Image.Pe;

[StructLayout(LayoutKind.Sequential)]
public struct PeExportDirectory {
	public uint ExportFlags;
	public uint TimeDateStamp;
	public ushort MajorVersion;
	public ushort MinorVersion;
	public uint Name;
	public uint Base;
	public uint FunctionCount;
	public uint NameCount;
	public uint FunctionTable;
	public uint NameTable;
	public uint OrdinalTable;
}