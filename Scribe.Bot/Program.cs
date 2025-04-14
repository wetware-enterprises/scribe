using Scribe.Hackmud;
using Scribe.Hackmud.Game.Enums;

var client = new GameClientBuilder().Build();

// todo: everything else
client.State.Kernel.OnChanged += kernel => {
	if (kernel.HardlineStep != HardlineStep.Patching)
		return;
	
	Console.WriteLine($"Digits: {kernel.HardlineDigits}");
	client.Input.SendText(kernel.HardlineDigits);
};

client.Initialize();

Console.ReadLine();