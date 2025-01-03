using Scribe.Core;
using Scribe.WinApi;

namespace Scribe.Input;

public class InputManager {
	private readonly GameProcess _proc;

	public InputManager(GameProcess proc) {
		this._proc = proc;
	}

	public void SendKey(char key) {
		User32.SendMessage(this._proc.MainWindowHandle, 0x0102, key, 0);
	}
	
	public void SendKey(VirtualKey key) {
		this.SendKey((char)key);
	}

	public void SendText(string text, bool enter = true) {
		foreach (var key in text)
			this.SendKey(key);
		if (enter) this.SendKey(VirtualKey.Enter);
	}
}