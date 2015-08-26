using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RootKill.Component.Screen
{
    // Source: https://msdn.microsoft.com/en-us/library/windows/desktop/ms724385%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
    public static class Desktop
    {
        #region Capture
        public static Bitmap Capture(bool includeCursor)
        {
            if (includeCursor) return CaptureIncludingCursor();
            return CaptureWithoutCursor();
        }

        #region Private Capture Methods
        private static Bitmap CaptureWithoutCursor()
        {
            // response object
            Bitmap bmp = null;

            try
            {
                // desktop handlers
                Size size = Size();
                IntPtr hBitmap;
                IntPtr hDC = WinApi.User32.GetDC(WinApi.User32.GetDesktopWindow());
                IntPtr hMemDC = WinApi.Gdi32.CreateCompatibleDC(hDC);

                hBitmap = WinApi.Gdi32.CreateCompatibleBitmap(hDC, size.Width, size.Height);

                if (hBitmap != IntPtr.Zero)
                {
                    IntPtr hOld = (IntPtr)WinApi.Gdi32.SelectObject(hMemDC, hBitmap);
                    WinApi.Gdi32.BitBlt(hMemDC, 0, 0, size.Width, size.Height, hDC, 0, 0, WinApi.Gdi32.SRCCOPY);
                    WinApi.Gdi32.SelectObject(hMemDC, hOld);
                    bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                }

                // dispose desktop handlers
                WinApi.Gdi32.DeleteDC(hMemDC);
                WinApi.User32.ReleaseDC(WinApi.User32.GetDesktopWindow(), hDC);
                WinApi.Gdi32.DeleteObject(hBitmap);
                GC.Collect();
            }
            catch
            {
                // Do nothing, return what you have
            }

            // return
            return bmp;
        }

        private static Bitmap CaptureCursor(ref Point location)
        {
            try
            {
                WinApi.User32.CURSORINFO cursorInfo = new WinApi.User32.CURSORINFO();
                cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
                if (!WinApi.User32.GetCursorInfo(out cursorInfo)) return null;
                if (cursorInfo.flags != WinApi.User32.CURSOR_SHOWING) return null;

                IntPtr hicon = WinApi.User32.CopyIcon(cursorInfo.hCursor);
                WinApi.User32.ICONINFO icInfo;
                if (!WinApi.User32.GetIconInfo(hicon, out icInfo)) return null;

                location.X = cursorInfo.ptScreenPos.x - ((int)icInfo.xHotspot);
                location.Y = cursorInfo.ptScreenPos.y - ((int)icInfo.yHotspot);

                return Icon.FromHandle(hicon).ToBitmap();
            }
            catch
            {
                // do nothing, just return null
                location = new Point();
                return null;
            }
        }

        private static Bitmap CaptureIncludingCursor()
        {
            try
            {
                Bitmap desktopCapture = CaptureWithoutCursor();
                Point cursorLocation = new Point();
                Bitmap cursorCapture = CaptureCursor(ref cursorLocation);
                if (desktopCapture == null) return null;
                if (cursorCapture == null) return desktopCapture;

                Rectangle rect = new Rectangle(cursorLocation.X, cursorLocation.Y, cursorCapture.Width, cursorCapture.Height);
                Graphics g = Graphics.FromImage(desktopCapture);
                g.DrawImage(cursorCapture, rect);
                g.Flush();
                return desktopCapture;
            }
            catch
            {
                // return null
                return null;
            }
        }
        #endregion Private Capture Methods

        #endregion Capture

        #region Size
        public static Size Size()
        {
            Size size = new Size();
            size.Width = WinApi.User32.GetSystemMetrics(WinApi.User32.SM_CXSCREEN);
            size.Height = WinApi.User32.GetSystemMetrics(WinApi.User32.SM_CYSCREEN);
            return size;
        }
        #endregion Size
    }
}
