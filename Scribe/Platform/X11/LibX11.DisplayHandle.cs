namespace Scribe.Platform.X11;

#if OS_LINUX
public static partial class LibX11 {
	public static DisplayHandle OpenHandle(string display) => new(XOpenDisplay(display));
	
	public class DisplayHandle(nint handle) : IDisposable {
		public nint Value => handle;
		
		public void Dispose() {
			_ = XCloseDisplay(handle);
			GC.SuppressFinalize(this);
		}
		
		public static implicit operator nint(DisplayHandle inst) => inst.Value;
	}
}
#endif