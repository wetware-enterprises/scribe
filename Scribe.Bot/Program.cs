using Scribe.Core;
using Scribe.Hackmud;
using Scribe.Hackmud.Game.Enums;

var proc = ProcResolver.Resolve();
var client = new GameClientBuilder(proc).Build();

// todo: everything else
client.State.Kernel.OnChanged += kernel => {
	if (kernel.HardlineStep != HardlineStep.Patching)
		return;
	
	Console.WriteLine($"Digits: {kernel.HardlineDigits}");
	client.Input.SendText(kernel.HardlineDigits);
};

client.Initialize();

Console.ReadLine();