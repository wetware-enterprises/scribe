using System.Diagnostics;

using Scribe.Memory;
using Scribe.WinApi;

namespace Scribe.Core;

public class GameProcess {
	private readonly Process _process;
	private readonly ProcessModule _mono;
	
	public bool IsAlive => !this._process.HasExited;

	public nint MainWindowHandle => this._process.MainWindowHandle;
	
	public GameProcess(
		Process process,
		ProcessModule mono
	) {
		this._process = process;
		this._mono = mono;
	}

	public MemoryReader OpenReader() {
		var hProc = Kernel32.OpenProcess(
			Kernel32.ProcessVmAccess.Read | Kernel32.ProcessVmAccess.Query,
			false,
			this._process.Id
		);

		if (hProc == nint.Zero)
			throw new Exception($"Failed to open handle to PID {this._process.Id}");

		return new MemoryReader(hProc, this._mono);
	}
}