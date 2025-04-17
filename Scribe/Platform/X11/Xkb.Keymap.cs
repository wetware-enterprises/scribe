namespace Scribe.Platform.X11;

#if OS_LINUX
public static partial class Xkb {
	private unsafe static byte XkbKeyNumGroups(ref XkbDescRec d, byte k) => (byte)(d.Map->KeySymMap[k].GroupInfo & 0x0F);
	private unsafe static int XkbCmKeyTypeIndex(XkbClientMapRec* m, byte k, byte g) => m->KeySymMap[k].KtIndex[g & 0x3];
	private unsafe static ref XkbKeyTypeRec XkbKeyKeyType(ref XkbDescRec d, byte k, byte g) => ref d.Map->Types[XkbCmKeyTypeIndex(d.Map, k, g)];
	
	public class Keymap {
		public readonly Dictionary<uint, (byte Code, byte Group, uint Mod)> Symbols = [];
	}

	public static Keymap GetKeymap(nint display) {
		var desc = XkbGetMap(display, XkbClientInfoMask.AllClientInfoMask, XkbUseCoreKbd);
		try {
			var modMap = LibX11.XGetModifierMapping(display);
			return BuildKeymap(display, ref desc, ref modMap);
		} finally {
			XkbFreeClientMap(ref desc, XkbClientInfoMask.AllClientInfoMask, 1);
		}
	}

	private unsafe static Keymap BuildKeymap(nint display, ref XkbDescRec desc, ref LibX11.XModifierKeymap modMap) {
		_ = LibX11.XDisplayKeycodes(display, out var minKeycodes, out var maxKeycodes);
		
		var result = new Keymap();
		
		for (var key = minKeycodes; key <= maxKeycodes; key++) {
			var groups = XkbKeyNumGroups(ref desc, (byte)key);
			
			for (byte group = 0; group < groups; group++) {
				var keyType = XkbKeyKeyType(ref desc, (byte)key, group);
				
				for (var level = 0; level < keyType.NumLevels; level++) {
					var keySym = XkbKeycodeToKeysym(display, (byte)key, group, level);
					var utfKey = xkb_keysym_to_utf32(keySym);
					if (result.Symbols.ContainsKey(utfKey))
						continue;
					
					var modMask = GetKeyModifier(ref modMap, (byte)key);
					
					for (var mi = 0; mi < keyType.MapCount; mi++) {
						var map = keyType.Map[mi];
						if (map.Active == 0 || map.Level != level)
							continue;
						modMask |= map.Mods.Mask;
						break;
					}
					
					result.Symbols.Add(utfKey, (Code: (byte)key, Group: group, Mod: modMask));
				}
			}
		}

		return result;
	}
	
	private unsafe static uint GetKeyModifier(ref LibX11.XModifierKeymap modMap, byte key) {
		for (var i = 0; i < 8; i++) {
			byte mod;
			for (var j = 0; j < modMap.MaxKeyPerMod && (mod = modMap.ModifierMap[j + i * modMap.MaxKeyPerMod]) != 0; j++) {
				if (key == mod)
					return (byte)(1 << i);
			}
		}
		return 0;
	}
}
#endif