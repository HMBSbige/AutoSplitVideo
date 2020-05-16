using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace Kyozy.MiniblinkNet
{
    internal delegate IntPtr WndProcCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct SIZE
    {
        public int cx;
        public int cy;
        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct PAINTSTRUCT
    {
        public int hdc;
        public int fErase;
        public RECT rcPaint;
        public int fRestore;
        public int fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct COMPOSITIONFORM
    {
        public int dwStyle;
        public POINT ptCurrentPos;
        public RECT rcArea;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct BITMAP
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public short bmPlanes;
        public short bmBitsPixel;
        public int bmBits;
    }

    [StructLayout(LayoutKind.Explicit,Size = 4)]
    internal struct BLENDFUNCTION
    {
        [FieldOffset(0)]
        public byte BlendOp;
        [FieldOffset(1)]
        public byte BlendFlags;
        [FieldOffset(2)]
        public byte SourceConstantAlpha;
        [FieldOffset(3)]
        public byte AlphaFormat;
    }

    internal enum WinConst:int
    {
        GWL_EXSTYLE = -20,
        GWL_WNDPROC = -4,
        WS_EX_LAYERED = 524288,
        WM_PAINT = 15,
        WM_ERASEBKGND = 20,
        WM_SIZE = 5,
        WM_KEYDOWN = 256,
        WM_KEYUP = 257,
        WM_CHAR = 258,
        WM_LBUTTONDOWN = 513,
        WM_LBUTTONUP = 514,
        WM_MBUTTONDOWN = 519,
        WM_RBUTTONDOWN = 516,
        WM_LBUTTONDBLCLK = 515,
        WM_MBUTTONDBLCLK = 521,
        WM_RBUTTONDBLCLK = 518,
        WM_MBUTTONUP = 520,
        WM_RBUTTONUP = 517,
        WM_MOUSEMOVE = 512,
        WM_CONTEXTMENU = 123,
        WM_MOUSEWHEEL = 522,
        WM_SETFOCUS = 7,
        WM_KILLFOCUS = 8,
        WM_IME_STARTCOMPOSITION = 269,
        WM_NCHITTEST = 132,
        WM_GETMINMAXINFO = 36,
        WM_DESTROY = 2,
        WM_SETCURSOR = 32,
        MK_CONTROL = 8,
        MK_SHIFT = 4,
        MK_LBUTTON = 1,
        MK_MBUTTON = 16,
        MK_RBUTTON = 2,
        KF_REPEAT = 16384,
        KF_EXTENDED = 256,
        SRCCOPY = 13369376,
        CAPTUREBLT = 1073741824,
        CFS_POINT = 2,
        CFS_FORCE_POSITION = 32,
        OBJ_BITMAP = 7,
        AC_SRC_OVER = 0,
        AC_SRC_ALPHA = 1,
        ULW_ALPHA = 2,
        WM_INPUTLANGCHANGE = 81,
        WM_NCDESTROY = 130,
    }

    
    internal class Help
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        public static extern IntPtr GetWindowLongIntPtr(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        public static extern IntPtr SetWindowLongDelegate(IntPtr hwnd, int nIndex, Delegate dwNewLong);
        [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern int GetClientRect(IntPtr hwnd, ref RECT lpRect);
        [DllImport("user32.dll", EntryPoint = "BeginPaint")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", EntryPoint = "IntersectRect")]
        public static extern int IntersectRect(ref RECT lpDestRect, ref RECT lpSrc1Rect, ref RECT lpSrc2Rect);
        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        public static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("user32.dll", EntryPoint = "EndPaint")]
        public static extern int EndPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", EntryPoint = "GetFocus")]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll", EntryPoint = "SetFocus")]
        public static extern IntPtr SetFocus(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "SetCapture")]
        public static extern int SetCapture(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        public static extern int ReleaseCapture();
        [DllImport("user32.dll", EntryPoint = "ScreenToClient")]
        public static extern int ScreenToClient(IntPtr hwnd,ref POINT lpPoint);
        [DllImport("imm32.dll", EntryPoint = "ImmGetContext")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);
        [DllImport("imm32.dll", EntryPoint = "ImmSetCompositionWindow")]
        public static extern int ImmSetCompositionWindow(IntPtr himc, ref COMPOSITIONFORM lpCompositionForm);
        [DllImport("imm32.dll", EntryPoint = "ImmReleaseContext")]
        public static extern int ImmReleaseContext(IntPtr hwnd, IntPtr himc);
        [DllImport("user32.dll", EntryPoint = "DefWindowProcA")]
        public static extern IntPtr DefWindowProc(IntPtr hwnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);
        [DllImport("user32.dll", EntryPoint = "OffsetRect")]
        public static extern int OffsetRect(ref RECT lpRect, int x, int y);

        [DllImport("gdi32.dll", EntryPoint = "GetCurrentObject")]
        public static extern IntPtr GetCurrentObject(IntPtr hdc, int uObjectType);
        [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
        public static extern int GetObject(IntPtr hObject, int nCount,ref BITMAP lpObject);
        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll", EntryPoint = "UpdateLayeredWindow")]
        public static extern int UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, IntPtr pptDst,ref SIZE psize, IntPtr hdcSrc,ref POINT pptSrc, int crKey,ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern int DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern int DeleteDC(IntPtr hdc);
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("user32.dll", EntryPoint = "InvalidateRect")]
        public static extern int InvalidateRect(IntPtr hwnd, ref RECT lpRect, bool bErase);
        [DllImport("kernel32.dll", EntryPoint = "lstrlenA")]
        private static extern int lstrlen(IntPtr lpString);



        public static int LOWORD(IntPtr dword)
        {
            return (int)dword & 65535;
        }

        public static int HIWORD(IntPtr dword)
        {
            return (int)dword >> 16;
        }

        public static string PtrToStringUTF8(IntPtr utf8)
        {
            if (utf8 == IntPtr.Zero)
                return null;
            int len = lstrlen(utf8) ;
            byte[] bytes = new byte[len];
            Marshal.Copy(utf8, bytes, 0, len);
            return Encoding.UTF8.GetString(bytes);
        }

    }
}
