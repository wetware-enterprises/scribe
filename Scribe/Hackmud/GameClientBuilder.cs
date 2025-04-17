using Scribe.Core;
using Scribe.Input;
using Scribe.Hackmud.Shell;
using Scribe.Hackmud.State;

namespace Scribe.Hackmud;

public sealed class GameClientBuilder {
	private GameProcess? _proc;

	public GameClientBuilder WithProcess(GameProcess proc) {
		this._proc = proc;
		return this;
	}

	public GameClient Build() {
		this._proc ??= ProcResolver.Resolve();
		var reader = this._proc.OpenReader();
		var state = new StateWatcher(reader);
		var input = InputManager.Create(this._proc);
		var scheduler = new ScriptScheduler(state, input);
		return new GameClient(this._proc, state, input, scheduler);
	}
}