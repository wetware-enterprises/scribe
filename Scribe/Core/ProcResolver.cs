using System.Diagnostics;
using Scribe.Memory.Reader;

namespace Scribe.Core;

public static class ProcResolver {
	#if OS_WINDOWS
	private const string GameProcName = "hackmud_win";
	private const string MonoModuleName = "mono-2.0-bdwgc.dll";
	#elif OS_LINUX
	private const string GameProcName = "hackmud_lin.x86_64";
	private const string MonoModuleName = "libmonobdwgc-2.0.so";
	#endif
	
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