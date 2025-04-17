using System.Runtime.InteropServices;

namespace Scribe.Platform.X11;

// ReSharper disable InconsistentNaming

#if OS_LINUX
public static partial class Xkb {
	public const uint XkbUseCoreKbd = 0x100;
	public const uint XkbUseCorePrt = 0x200;
	
	public enum XkbClientInfoMask : uint {
		KeyTypesMask = 1 << 0,
		KeySymsMask = 1 << 1,
		ModifierMapMask = 1 << 2,
		ExplicitComponentsMask = 1 << 3,
		KeyActionsMask = 1 << 4,
		KeyBehaviorsMask = 1 << 5,
		VirtualModsMask = 1 << 6,
		VirtualModMapMask = 1 << 7,
		AllClientInfoMask = KeyTypesMask | KeySymsMask | ModifierMapMask,
		AllServerInfoMask = ExplicitComponentsMask | KeyActionsMask | KeyBehaviorsMask | VirtualModsMask | VirtualModMapMask,
		AllMapComponentsMask = AllClientInfoMask | AllServerInfoMask
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct XkbDescRec {
		public nint Display;
		public ushort Flags;
		public byte MinKeyCode;
		public byte MaxKeyCode;
		public nint Controls;
		public nint Server;
		public XkbClientMapRec* Map;
		public nint Indicators;
		public nint Names;
		public nint Compat;
		public nint Geom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct XkbClientMapRec {
		public byte SizeTypes;
		public byte NumTypes;
		public XkbKeyTypeRec* Types;
		public ushort SizeSyms;
		public ushort NumSyms;
		public nint Syms;
		public XkbSymMapRec* KeySymMap;
		public nint ModMap;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct XkbKeyTypeRec {
		public XkbModsRec Mods;
		public byte NumLevels;
		public byte MapCount;
		public XkbKTMapEntryRec* Map;
		public XkbModsRec* Preserve;
		public LibX11.Atom Name;
		public LibX11.Atom* LevelNames;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct XkbSymMapRec {
		public fixed byte KtIndex[4];
		public byte GroupInfo;
		public byte Width;
		public ushort Offset;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct XkbModsRec {
		public byte Mask;
		public byte RealMods;
		public ushort VMods;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct XkbKTMapEntryRec {
		public int Active;
		public byte Level;
		public XkbModsRec Mods;
	}
	
	[DllImport("libX11.so.6")]
	public extern static ref XkbDescRec XkbGetMap(nint display, XkbClientInfoMask which, uint deviceSpec);

	[DllImport("libX11.so.6")]
	public extern static uint XkbKeycodeToKeysym(nint display, byte keycode, int group, int level);
	
	[DllImport("libX11.so.6")]
	public extern static void XkbFreeClientMap(ref XkbDescRec xkb, XkbClientInfoMask which, int freeAll);
	
	[DllImport("libX11.so.6")]
	public extern static void XkbFreeKeyboard(ref XkbDescRec xkb, XkbClientInfoMask which, int freeAll);

	[DllImport("libxkbcommon-x11.so.0")]
	public extern static uint xkb_keysym_to_utf32(uint keysym);
}
#endif