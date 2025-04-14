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

	public event Action<Exception>? OnError; 

	public void Initialize() {
		this.State.Initialize();
		this.Scheduler.Initialize();
		this.Enable();
	}

	private void Enable() {
		if (this.Enabled)
			this.Disable();

		var cts = new CancellationTokenSource();
		lock (this._lock) {
			this._isEnabled = true;
			this._cts = cts;
		}
		this.Update(cts.Token).ContinueWith(task => {
			if (task.Exception == null) return;
			this.OnError?.Invoke(task.Exception);
			this.Disable();
		}, TaskContinuationOptions.OnlyOnFaulted);
	}

	private void Disable() {
		lock (this._lock) {
			if (this._cts is { IsCancellationRequested: false })
				this._cts.Cancel();
			this._cts?.Dispose();
			this._isEnabled = false;
		}
	}

	private async Task Update(CancellationToken ct) {
		while (this.Enabled) {
			lock (this._lock)
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
		GC.SuppressFinalize(this);
	}
}