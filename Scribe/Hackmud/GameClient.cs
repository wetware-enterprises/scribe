using Scribe.Core;
using Scribe.Input;
using Scribe.Hackmud.Shell;
using Scribe.Hackmud.State;

namespace Scribe.Hackmud;

public class GameClient : IDisposable {
	private readonly GameProcess _proc;
	
	private readonly Lock _lock = new();
	
	public readonly StateWatcher State;
	public readonly InputManager Input;
	public readonly ScriptScheduler Scheduler;

	private Task? _task;
	private CancellationTokenSource? _cts;
	
	private bool _isEnabled;
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
		get {
			lock (this._lock)
				return this._isEnabled;
		}
		set {
			if (value)
				this.Enable();
			else
				this.Disable();
		}
	}

	public void Initialize() {
		this.State.Initialize();
		this.Scheduler.Initialize();
		this.Enable();
	}

	private void Enable() {
		if (this.Enabled)
			this.Disable();
		lock (this._lock)
			this._isEnabled = true;
		this._cts = new CancellationTokenSource();
		this._task = this.Update(this._cts.Token);
	}

	private void Disable() {
		this._cts?.Cancel();
		this._task?.Dispose();
		this._task = null;
		lock (this._lock)
			this._isEnabled = false;
	}

	private async Task Update(CancellationToken ct) {
		while (this.Enabled) {
			ct.ThrowIfCancellationRequested();
			this.State.Update();
			this.Scheduler.Update();
			await Task.Delay(1, ct);
		}
	}

	public void Dispose() {
		if (this._isDisposed) return;
		this.Disable();
		this._isDisposed = true;
	}
}