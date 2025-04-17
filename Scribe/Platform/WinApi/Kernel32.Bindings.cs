using System.Runtime.InteropServices;

namespace Scribe.Platform.WinApi;

#if OS_WINDOWS
public static class Kernel32 {
	[DllImport("kernel32.dll", SetLastError = true)]
	public extern static nint OpenProcess(ProcessVmAccess dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	public extern static bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, ref nint lpNumberOfBytesRead);

	[DllImport("kernel32.dll")]
	public extern static void GetSystemInfo(ref SystemInfo lpSystemInfo);

	[DllImport("kernel32.dll", SetLastError = true)]
	public extern static int VirtualQueryEx(nint hProcess, nint lpAddress, ref MemoryBasicInformation lpBuffer, int dwLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	public extern static bool CloseHandle(nint handle);

	[StructLayout(LayoutKind.Sequential)]
	public struct SystemInfo {
		public ushort ProcessorArchitecture;
		public ushort _reserved;
		public uint PageSize;
		public nint MinimumApplicationAddress;
		public nint MaximumApplicationAddress;
		public nint ActiveProcessorMask;
		public uint NumberOfProcessors;
		public uint ProcessorType;
		public uint AllocationGranularity;
		public ushort ProcessorLevel;
		public ushort ProcessorRevision;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MemoryBasicInformation {
		public nint BaseAddress;
		public nint AllocationBase;
		public int AllocationProtect;
		public nint RegionSize;
		public uint State;
		public PageProtect Protect;
		public PageType Type;
	}
	
	[Flags]
	public enum ProcessVmAccess : uint {
		Terminate = 0x0001,
		Read = 0x0010,
		Write = 0x0020,
		Query = 0x0400,
		Commit = 0x1000
	}
	
	[Flags]
	public enum PageProtect : uint {
		NoAccess = 0x01,
		ReadOnly = 0x02,
		ReadWrite = 0x04,
		WriteCopy = 0x08,
		Execute = 0x10,
		ExecuteRead = 0x20,
		ExecuteReadWrite = 0x40
	}

	[Flags]
	public enum PageType : uint {
		Private = 0x0020000,
		Mapped = 0x0040000,
		Image = 0x1000000
	}
}
#endif