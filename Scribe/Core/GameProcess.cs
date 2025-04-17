using System.Diagnostics;

using Scribe.Memory.Reader;
using Scribe.Memory.Reader.Types;

namespace Scribe.Core;

public class GameProcess {
	private readonly Process _process;
	private readonly ProcessModule _mono;
	
	public bool IsAlive => !this._process.HasExited;

	public int ProcessId => this._process.Id;

	public nint MainWindowHandle => this._process.MainWindowHandle;
	
	public GameProcess(
		Process process,
		ProcessModule mono
	) {
		this._process = process;
		this._mono = mono;
	}
	
	public IMemoryReader OpenReader() => MemoryReader.Open(this._process, this._mono);
}