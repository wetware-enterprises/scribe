using Scribe.Hackmud.State;
using Scribe.Input.Types;

namespace Scribe.Hackmud.Shell;

public class ScriptScheduler {
	private readonly StateWatcher _state;
	private readonly IInputManager _input;

	private readonly Queue<QueuedCommand> _queue = new();

	private QueuedCommand? _currentCmd;

	public ScriptScheduler(
		StateWatcher state,
		IInputManager input
	) {
		this._state = state;
		this._input = input;
	}

	public void Initialize() {
		this._state.OnProcessed += this.OnScriptProcessed;
	}

	private void OnScriptProcessed(StateWatcher state) {
		if (this._currentCmd is not QueuedCommand cmd)
			return;

		var result = new ScriptResult(cmd.Command, cmd.Parameters);
		result.Populate(this._state.Window);
		cmd.IsProcessed = true;
		cmd.Callback?.Invoke(result);
	}

	public void Update() {
		if (!this._state.CanInput || this._state.IsEnteringHardline) return;

		if (this._currentCmd != null) {
			if (this._currentCmd.IsProcessed)
				this._currentCmd = null;
			else return;
		}

		lock (this._queue) {
			if (this._queue.Count == 0)
				return;
			this._currentCmd = this._queue.Dequeue();
			this.ProcessCommand(this._currentCmd);
		}
	}
	
	// Queue handler
	
	private void ProcessCommand(QueuedCommand cmd) {
		Console.WriteLine($"Dispatching queued command: {cmd.Command}");
		
		if (cmd.Command == "kernel.hardline")
			this._state.IsEnteringHardline = true;

		var input = cmd.Command;
		if (cmd.Parameters.Length > 0)
			input += $" {cmd.Parameters}";
		this._input.SendText(input, enter: true);
	}
	
	public void QueueCommand(string cmd, string param = "", Action<ScriptResult>? callback = null) {
		lock (this._queue) {
			this._queue.Enqueue(new QueuedCommand {
				Command = cmd,
				Parameters = param,
				Callback = callback
			});
		}
	}
	
	public async Task<ScriptResult> RunCommandAsync(string cmd, string param = "") {
		var handle = new AsyncHandle();
		this.QueueCommand(cmd, param, handle.Complete);
		while (!handle.HasResult)
			await Task.Delay(10);
		return handle.Result!;
	}
	
	// Queue objects

	private class QueuedCommand {
		public bool IsProcessed;
		public string Command = string.Empty;
		public string Parameters = string.Empty;
		public Action<ScriptResult>? Callback;
	}

	private class AsyncHandle {
		public bool HasResult;
		public ScriptResult? Result;

		public void Complete(ScriptResult result) {
			this.HasResult = true;
			this.Result = result;
		}
	}
}