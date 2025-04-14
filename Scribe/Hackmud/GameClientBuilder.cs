using Scribe.Core;
using Scribe.Input;
using Scribe.Hackmud.Shell;
using Scribe.Hackmud.State;

namespace Scribe.Hackmud;

public sealed class GameClientBuilder {
	private readonly GameProcess _proc;
	
	public GameClientBuilder(
		GameProcess proc
	) {
		this._proc = proc;
	}

	public GameClient Build() {
		var reader = this._proc.OpenReader();
		var state = new StateWatcher(reader);
		var input = new InputManager(this._proc);
		var scheduler = new ScriptScheduler(state, input);
		return new GameClient(this._proc, state, input, scheduler);
	}
}