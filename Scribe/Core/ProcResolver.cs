using System.Diagnostics;

namespace Scribe.Core;

public static class ProcResolver {
	private const string GameProcName = "hackmud_win";
	private const string MonoModuleName = "mono-2.0-bdwgc.dll";
	
	public static GameProcess Resolve() {
		var proc = Process.GetProcessesByName(GameProcName)
			.FirstOrDefault(proc => proc.Responding);
		if (proc == null)
			throw new Exception($"Process not found: {GameProcName}");

		var modules = proc.Modules.Cast<ProcessModule>();
		var mono = modules.FirstOrDefault(mod => mod.ModuleName == MonoModuleName);
		if (mono == null)
			throw new Exception($"Module not found: {MonoModuleName}");

		return new GameProcess(proc, mono);
	}
}