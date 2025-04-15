using Scribe.Memory;
using Scribe.Hackmud.Game;
using Scribe.Hackmud.Game.Enums;
using Scribe.Memory.Reader.Types;

namespace Scribe.Hackmud.State;

public sealed class KernelState {
	public KernelMode Mode { get; private set; } = 0;

	public HardlineStep HardlineStep { get; private set; } = 0;
	public string HardlineDigits { get; private set; } = string.Empty;

	public event Action<KernelState>? OnChanged; 
	
	public void Update(IMemoryReader reader, nint ptr) {
		var isStateChanged = false;
		
		// Kernel
		
		if (!reader.TryRead<Kernel>(ptr, out var kernel))
			throw new Exception("Could not read Kernel from pointer.");

		isStateChanged |= this.Mode != kernel.CurrentMode;
		this.Mode = kernel.CurrentMode;

		// Hardline
		
		if (!reader.TryRead<Hardline>(kernel.Hardline, out var hardline))
			throw new Exception("Could not read Hardline from pointer.");

		var isHardlineChanged = this.HardlineStep != hardline.CurrentStep;
		isStateChanged |= isHardlineChanged;

		if (isHardlineChanged) {
			this.HardlineStep = hardline.CurrentStep;
			this.HardlineDigits = hardline.Digits.Read(reader) ?? string.Empty;
		}
		
		// Update event

		if (isStateChanged) this.OnChanged?.Invoke(this);
	}
}