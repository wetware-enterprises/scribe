using Scribe.Hackmud.Game;
using Scribe.Hackmud.Game.Enums;
using Scribe.Memory.Reader.Types;

namespace Scribe.Hackmud.State;

public class StateWatcher {
	private readonly IMemoryReader _reader;

	public readonly WindowState Window = new();
	public readonly KernelState Kernel = new();
	
	private nint _shellInst;
	
	public bool IsProcessing { get; private set; }
	public string ProcessingId { get; private set; } = string.Empty;

	public bool IsInputMode => this.Kernel.Mode is KernelMode.Gui or KernelMode.GuiHackMode;
	public bool CanInput => this.IsInputMode && !this.IsProcessing && this.ProcessingId.Length == 0;

	public bool IsInHardline => this.Kernel.Mode is KernelMode.Hardline;
	public bool IsEnteringHardline { get; set; }

	public event Action<StateWatcher>? OnProcessed; 

	public StateWatcher(IMemoryReader reader) {
		this._reader = reader;
	}

	public void Initialize() {
		var shell = this._reader.GetClass("hackmud", "ShellParsing");
		this._shellInst = this._reader.FindClassInstance(shell, addr =>
			this._reader.TryRead<ShellParsing>(addr, out var obj)
				&& obj.Kernel != nint.Zero
				&& this._reader.TryRead<Kernel>(obj.Kernel, out var kernel)
				&& kernel.ShellParsing == addr
		);
		Console.WriteLine($"Shell: {this._shellInst:X}");
	}

	public void Update() {
		if (!this._reader.TryRead<ShellParsing>(this._shellInst, out var shell))
			throw new Exception("Could not read ShellParsing instance.");

		var prevCanInput = this.CanInput;
		this.IsProcessing = shell.IsProcessing;
		this.ProcessingId = shell.ProcessingId.Read(this._reader) ?? string.Empty;

		this.Window.Update(this._reader, shell.Window);
		this.Kernel.Update(this._reader, shell.Kernel);

		this.IsEnteringHardline = this.Kernel.Mode switch {
			KernelMode.ToHardline => true,
			KernelMode.Gui or KernelMode.GuiHackMode or KernelMode.ToGuiHackMode => false,
			_ => this.IsEnteringHardline
		};

		if (!this.CanInput) return;
		
		if (prevCanInput)
			this.Window.Output.Clear();
		else
			this.OnProcessed?.Invoke(this);
	}
}