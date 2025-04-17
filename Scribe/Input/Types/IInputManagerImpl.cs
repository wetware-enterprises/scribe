using Scribe.Core;

namespace Scribe.Input.Types;

public interface IInputManagerImpl : IInputManager {
	public abstract static IInputManagerImpl Create(GameProcess process);
}