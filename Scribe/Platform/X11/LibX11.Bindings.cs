using System.Runtime.InteropServices;

namespace Scribe.Platform.X11;

#if OS_LINUX
public static partial class LibX11 {
	public enum Atom : ulong {
		AnyPropertyType = 0
	}

	public enum Event {
		KeyPress = 2,
		KeyRelease = 3
	}

	public enum EventMask : ulong {
		KeyPressMask = 1 << 0,
		KeyReleaseMask = 1 << 1
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct XKeyEvent {
		public Event Type;
		public ulong Serial;
		public bool SendEvent;
		public nint Display;
		public nint Window;
		public nint RootWindow;
		public nint SubWindow;
		public ulong Time;
		public int X, Y;
		public int XRoot, YRoot;
		public uint State;
		public uint KeyCode;
		public bool SameScreen;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct XModifierKeymap {
		public int MaxKeyPerMod;
		public byte* ModifierMap; // 8 * MaxKeyPerMod
	}
	
	[DllImport("libX11.so.6")]
	public extern static nint XOpenDisplay(string display);

	[DllImport("libX11.so.6")]
	public extern static int XCloseDisplay(nint display);

	[DllImport("libX11.so.6")]
	public extern static nint XRootWindow(nint display, int screen);

	[DllImport("libX11.so.6")]
	public extern static int XQueryTree(nint display, nint window, out nint rootRtn, out nint parentRtn, out nint childrenRtn, out uint nChildrenRtn);

	[DllImport("libX11.so.6")]
	public extern static int XFetchName(nint display, nint window, out string nameRtn);
	
	[DllImport("libX11.so.6")]
	public extern static Atom XInternAtom(nint display, string name, bool onlyIfExists);

	[DllImport("libX11.so.6")]
	public extern static int XGetWindowProperty(
		nint display,
		nint window,
		Atom property,
		ulong offset,
		ulong length,
		bool delete,
		Atom reqType,
		out Atom typeRtn,
		out int formatRtn,
		out ulong nItemsRtn,
		out ulong bytesAfterRtn,
		out nint propRtn
	);

	[DllImport("libX11.so.6")]
	public unsafe extern static int XSendEvent(nint display, nint window, bool propagate, EventMask eventMask, void* eventSend);

	[DllImport("libX11.so.6")]
	public extern static int XDisplayKeycodes(nint display, out int minKeycodes, out int maxKeycodes);

	[DllImport("libX11.so.6")]
	public extern static ref XModifierKeymap XGetModifierMapping(nint display);

	[DllImport("libX11.so.6")]
	public extern static int XFreeModifiermap(ref XModifierKeymap modMap);

	[DllImport("libX11.so.6")]
	public extern static int XFlush(nint display);
}
#endif