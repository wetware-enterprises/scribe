using System.Runtime.InteropServices;

namespace Scribe.Scanner;

public static partial class SigScanner {
	[LibraryImport("sigscanner.dll", StringMarshalling = StringMarshalling.Utf8)]
	public static partial nint ScanMemory(nint baseAddr, nint size, string sig);

	[LibraryImport("sigscanner.dll", StringMarshalling = StringMarshalling.Utf8)]
	public static partial nint ScanText(nint baseAddr, string sig);

	[LibraryImport("sigscanner.dll", StringMarshalling = StringMarshalling.Utf8)]
	public static partial nint ScanData(nint baseAddr, string sig);

	[LibraryImport("sigscanner.dll", StringMarshalling = StringMarshalling.Utf8)]
	private unsafe static partial nint LookupSectionName(nint baseAddr, string name, nint* size);
	
	[LibraryImport("sigscanner.dll", StringMarshalling = StringMarshalling.Utf8)]
	private unsafe static partial ushort GetSectionTable(nint baseAddr, nint* tablePtr);

	public unsafe static nint LookupSectionName(nint baseAddr, string name, out nint size) {
		size = nint.Zero;
		fixed (nint* pSize = &size)
			return LookupSectionName(baseAddr, name, pSize);
	}

	public unsafe static ushort GetSectionTable(nint baseAddr, out nint tablePtr) {
		tablePtr = nint.Zero;
		fixed (nint* pTablePtr = &tablePtr)
			return GetSectionTable(baseAddr, pTablePtr);
	}
}