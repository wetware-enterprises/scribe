using System.Runtime.InteropServices;

namespace Scribe.Platform.WinApi;

#if OS_WINDOWS
public static class User32 {
	[DllImport("User32.dll")]
	public extern static nint PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

	[DllImport("User32.dll", SetLastError = true)]
	public extern static nint CreateWindowExW(
		int dwExStyle,
		ushort regResult,
		[MarshalAs(UnmanagedType.LPWStr)]
		string lpWindowName,
		uint dwStyle,
		int x,
		int y,
		int nWidth,
		int nHeight,
		nint hWndParent,
		nint hMenu,
		nint hInstance,
		nint lpParam
	);
	
	[DllImport("User32.dll", SetLastError = true)]
	public extern static ushort RegisterClassEx([In] ref WndClassEx lpWndClass);
	
	[StructLayout(LayoutKind.Sequential)]
	public struct WndClassEx {
		[MarshalAs(UnmanagedType.U4)]
		public int cbSize;
		[MarshalAs(UnmanagedType.U4)]
		public int style;
		public nint lpfnWndProc;
		public int cbClsExtra;
		public int cbWndExtra;
		public nint hInstance;
		public nint hIcon;
		public nint hCursor;
		public nint hbrBackground;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpszMenuName;
		[MarshalAs(UnmanagedType.LPStr)]
		public string lpszClassName;
		public nint hIconSm;
	}
	
	[DllImport("User32.dll")]
	public extern static bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("User32.dll")]
	public extern static bool UpdateWindow(nint hWnd);
	
	[DllImport("user32.dll", SetLastError = true)]
	public extern static bool DestroyWindow(IntPtr hWnd);
	
	[DllImport("User32.dll")]
	public extern static sbyte GetMessage(out uint lpMsg, IntPtr hWnd, uint wMsgFilterMin,uint wMsgFilterMax);
	
	[DllImport("user32.dll")]
	public extern static bool TranslateMessage([In] ref uint lpMsg);

	[DllImport("user32.dll")]
	public extern static IntPtr DispatchMessage([In] ref uint lpMsg);
	
	[DllImport("User32.dll")]
	public extern static nint DefWindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam);
}
#endif