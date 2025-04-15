using System.Diagnostics;

namespace Scribe.Memory.Reader.Types;

public interface IMemoryReaderImpl : IMemoryReader {
	public abstract static IMemoryReaderImpl Open(Process process, ProcessModule mono);
}