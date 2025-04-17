using Scribe.Input.Enums;
using Scribe.Input.Types;

namespace Scribe.Input;

public abstract class InputManagerBase : IInputManager {
	// IInputManager
	
	public abstract void SendKey(char key);
	
	public void SendKey(VirtualKey key) {
		this.SendKey((char)key);
	}
	
	public abstract void SendText(string text, bool enter = true);
	
	// IDisposable

	public abstract void Dispose();
}