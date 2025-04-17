using Scribe.Hackmud;
using Scribe.Hackmud.Game.Enums;
using Scribe.Hackmud.State;

namespace Scribe.Bot;

public class ScribeBot : IDisposable {
	private readonly GameClient _client;

	public ScribeBot(
		GameClient client
	) {
		this._client = client;
	}
	
	// Initialization

	public void Initialize() {
		this._client.State.Kernel.OnChanged += this.OnKernelStateChanged;
		this._client.OnError += this.OnError;
		this._client.Initialize();
	}
	
	// Event handlers

	private void OnKernelStateChanged(KernelState kernel) {
		if (kernel.HardlineStep != HardlineStep.Patching)
			return;
	
		Console.WriteLine($"Digits: {kernel.HardlineDigits}");
		this._client.Input.SendText(kernel.HardlineDigits);
	}

	private void OnError(Exception error) {
		Console.WriteLine($"Error: {error}");
	}
	
	// IDisposable

	public void Dispose() {
		this._client.Dispose();
		GC.SuppressFinalize(this);
	}
}