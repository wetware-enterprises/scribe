namespace Scribe.Hackmud.Game.Enums;

public enum KernelMode : uint {
	PreLoad = 0,
	StartupText = 1,
	StartupLogo = 2,
	Shutdown = 3,
	ToGui = 4,
	Gui = 5,
	ToHardline = 6,
	Hardline = 7,
	ResumeGuiHackMode = 8,
	ToGuiHackMode = 9,
	GuiHackMode = 10
}