using Scribe.Core;
using Scribe.Input;
using Scribe.Hackmud.Shell;
using Scribe.Hackmud.State;

using Timer = System.Timers.Timer;

namespace Scribe.Hackmud;

public class GameClient : IDisposable {
	private readonly GameProcess _proc;
	private readonly Timer _timer = new();
	
	public readonly StateWatcher State;
	public readonly InputManager Input;
	public readonly ScriptScheduler Scheduler;

	private bool _isDisposed;
	
	public GameClient(
		GameProcess proc,
		StateWatcher state,
		InputManager input,
		ScriptScheduler scheduler
	) {
		this._proc = proc;
		this.State = state;
		this.Input = input;
		this.Scheduler = scheduler;
	}
	
	public bool Enabled {
		get => !this._isDisposed && this._timer.Enabled;
		set => this._timer.Enabled = value;
	}

	public void Initialize() {
		this.State.Initialize();
		this.Scheduler.Initialize();
		
		this._timer.Elapsed += this.Update;
		this._timer.AutoReset = true;
		this._timer.Interval = 1;
		this._timer.Start();
	}

	private void Update(object? sender, object _) {
		if (!this.Enabled) return;
		
		this.State.Update();
		this.Scheduler.Update();
	}

	public void Dispose() {
		if (this._isDisposed) return;
		this._timer.Dispose();
		this._isDisposed = true;
	}
}