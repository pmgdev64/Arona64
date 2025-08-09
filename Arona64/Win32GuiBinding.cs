using System;
using System.Runtime.InteropServices;

namespace Arona64
{
    public static class Win32GuiBinding
    {
        // ==== [ GDI Constants ] ====
        public const int SRCCOPY = 0x00CC0020;
        public const int SRCINVERT = 0x00660046;
        public const int SRCAND = 0x008800C6;
        public const int SRCPAINT = 0x00EE0086;
        public const int MERGECOPY = 0x00C000CA;
        public const int MERGEPAINT = 0x00BB0226;
        public const int PATINVERT = 0x005A0049;
        public const int NOTSRCCOPY = 0x00330008;
        public const int NOTSRCERASE = 0x001100A6;
        public const int DSTINVERT = 0x00550009;
        public const int BLACKNESS = 0x00000042;
        public const int WHITENESS = 0x00FF0062;
        public const int PATCOPY = 0x00F00021;

        // ==== [ Structs ] ====
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        // ==== [ BitBlt / Stretch / Plg / Pat / Invert ] ====
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height,
            IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int widthDest, int heightDest,
            IntPtr hdcSrc, int xSrc, int ySrc, int widthSrc, int heightSrc, int rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool PatBlt(IntPtr hdc, int x, int y, int width, int height, int rop);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool PlgBlt(IntPtr hdcDest, [In] POINT[] lpPoint, IntPtr hdcSrc,
            int xSrc, int ySrc, int width, int height, IntPtr hbmMask, int xMask, int yMask);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern bool InvertRect(IntPtr hdc, [In] ref RECT lprc);

        // ==== [ Region / Clipping ] ====
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateEllipticRgn(int left, int top, int right, int bottom);

        [DllImport("gdi32.dll")]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        // ==== [ Pen / Brush / Draw ] ====
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int style, int width, uint color);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int color);

        [DllImport("gdi32.dll")]
        public static extern bool PolyBezier(IntPtr hdc, [In] POINT[] lppt, uint cPoints);

        // ==== [ DC / Bitmap Management ] ====
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern int SetROP2(IntPtr hdc, int drawMode);
    }
}
