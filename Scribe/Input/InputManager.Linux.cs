#if OS_LINUX

using Scribe.Core;
using Scribe.Input.Enums;
using Scribe.Input.Types;
using Scribe.Platform.X11;

namespace Scribe.Input;

public class InputManager : InputManagerBase, IInputManagerImpl {
	private readonly GameProcess _proc;
	private readonly LibX11.DisplayHandle _display;
	private readonly nint _window;
	private readonly Xkb.Keymap _keymap;

	private InputManager(
		GameProcess proc,
		LibX11.DisplayHandle display,
		nint window,
		Xkb.Keymap keymap
	) {
		this._proc = proc;
		this._display = display;
		this._window = window;
		this._keymap = keymap;
	}
	
	// IInputManagerImpl

	public static IInputManagerImpl Create(GameProcess process) {
		var display = LibX11.OpenHandle(":0");

		if (!TryGetWindowByPid(display, process.ProcessId, out var window))
			throw new Exception($"Failed to find window for process: {process.ProcessId}");

		_ = LibX11.XFetchName(display, window, out var name);
		Console.WriteLine($"Found window: {name}");
		
		return new InputManager(
			process,
			display,
			window,
			keymap: Xkb.GetKeymap(display)
		);
	}
	
	// IInputManager

	public override void SendKey(char key) {
		if (!this._keymap.Symbols.TryGetValue(key, out var mapped)) {
			Console.WriteLine($"Warning: No mapping for character '{key}' ({(int)key})");
			return;
		}
		var state = mapped.Mod | (uint)(mapped.Group << 13);
		this.SendKeyEvent(mapped.Code, state, isPressed: true);
		this.SendKeyEvent(mapped.Code, state, isPressed: false);
	}

	public override void SendText(string text, bool enter = true) {
		foreach (var key in text)
			this.SendKey(key);
		if (enter) this.SendKey(VirtualKey.Enter);
		_ = LibX11.XFlush(this._display);
	}

	private unsafe void SendKeyEvent(byte keyCode, uint state, bool isPressed) {
		var keyEvent = new LibX11.XKeyEvent {
			Type = isPressed ? LibX11.Event.KeyPress : LibX11.Event.KeyRelease,
			Display = this._display,
			Window = this._window,
			Time = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
			KeyCode = keyCode,
			State = state,
			SameScreen = true
		};
		
		_ = LibX11.XSendEvent(this._display, this._window, true, LibX11.EventMask.KeyPressMask, &keyEvent);
	}
	
	// IDisposable

	public override void Dispose() {
		this._display.Dispose();
		this._keymap.Symbols.Clear();
		GC.SuppressFinalize(this);
	}
	
	// LibX11 Helpers

	private static bool TryGetWindowByPid(nint display, int tarPid, out nint result) {
		var atom = LibX11.XInternAtom(display, "_NET_WM_PID", false);
		
		var root = LibX11.XRootWindow(display, 0);
		foreach (var window in RecurseWindows(display, root)) {
			if (!TryGetPidForWindow(display, window, atom, out var pid) || pid != tarPid)
				continue;

			result = window;
			return true;
		}

		result = nint.Zero;
		return false;
	}
	
	private static IEnumerable<nint> RecurseWindows(nint display, nint window) {
		_ = LibX11.XQueryTree(display, window, out _, out _, out var children, out var nChildren);
		
		if (children == nint.Zero) yield break;

		for (var x = 0; x < nChildren; x++) {
			var child = Access(children, x);
			yield return child;
			foreach (var subChild in RecurseWindows(display, child))
				yield return subChild;
		}
		
		unsafe static nint Access(nint ptr, int index) {
			return ((nint*)ptr)[index];
		}
	}
	
	private unsafe static bool TryGetPidForWindow(nint display, nint window, LibX11.Atom atom, out uint pid) {
		_ = LibX11.XGetWindowProperty(
			display,
			window,
			atom,
			0,
			~0u,
			false,
			LibX11.Atom.AnyPropertyType,
			out _,
			out _,
			out _,
			out _,
			out var propRtn
		);

		if (propRtn == nint.Zero) {
			pid = 0;
			return false;
		}

		pid = *(uint*)propRtn;
		return true;
	}
}

#endif