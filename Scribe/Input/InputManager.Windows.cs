#if OS_WINDOWS

using Scribe.Core;
using Scribe.Input.Enums;
using Scribe.Input.Types;
using Scribe.Platform.WinApi;

namespace Scribe.Input;

public class InputManager : InputManagerBase, IInputManagerImpl {
	private readonly GameProcess _proc;

	private InputManager(GameProcess proc) {
		this._proc = proc;
	}

	// IInputManagerImpl

	public static IInputManagerImpl Create(GameProcess process) {
		return new InputManager(process);
	}
	
	// IInputManager

	public override void SendKey(char key) {
		const uint wmChar = 0x102;
		User32.PostMessage(this._proc.MainWindowHandle, wmChar, key, 0);
	}

	public override void SendText(string text, bool enter = true) {
		foreach (var key in text)
			this.SendKey(key);
		if (enter) this.SendKey(VirtualKey.Enter);
	}
	
	// IDisposable

	public override void Dispose() {
		// nothing to dispose here
		GC.SuppressFinalize(this);
	}
}

#endif