using Scribe.Input.Enums;

namespace Scribe.Input.Types;

public interface IInputManager: IDisposable {
	public void SendKey(char key);
	public void SendKey(VirtualKey key);
	public void SendText(string text, bool enter = true);
}