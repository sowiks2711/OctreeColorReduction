using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace octree
{

    public static class BitmapExtensions
    {
        #region External
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);
        #endregion
        public static void SetPixel(this WriteableBitmap wbm, int x, int y, Color c)
        {
            wbm.Lock();
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * Stride + x * 4;
                pbuff[loc] = c.B;
                pbuff[loc + 1] = c.G;
                pbuff[loc + 2] = c.R;
                pbuff[loc + 3] = c.A;
            }

            wbm.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            wbm.Unlock();
        }
        public static void SetPoint(this WriteableBitmap wbm, int x, int y, int r, Color c)
        {
            wbm.Lock();
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                for (int i = x - r; i < x + r; i++)
                {
                    for (int j = y - r; j < y + r; j++)
                    {
                        int loc = j * Stride + i * 4;
                        pbuff[loc] = c.B;
                        pbuff[loc + 1] = c.G;
                        pbuff[loc + 2] = c.R;
                        pbuff[loc + 3] = c.A;
                    }
                }
            }

            wbm.AddDirtyRect(new Int32Rect(x - r, y - r, 2 * r, 2 * r));
            wbm.Unlock();
        }
        public static unsafe void FastClear(this WriteableBitmap _myBitmap, byte[] _blankImage)
        {
            fixed (byte* b = _blankImage)
            {
                CopyMemory(_myBitmap.BackBuffer, (IntPtr)b, (uint)_blankImage.Length);
            }
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _myBitmap.Lock();
                _myBitmap.AddDirtyRect(new Int32Rect(0, 0, _myBitmap.PixelWidth, _myBitmap.PixelHeight));
                _myBitmap.Unlock();
            });
        }
        public static Color GetPixel(this WriteableBitmap wbm, int x, int y)
        {
            IntPtr buff = wbm.BackBuffer;
            int Stride = wbm.BackBufferStride;
            Color c;
            unsafe
            {
                byte* pbuff = (byte*)buff.ToPointer();
                int loc = y * Stride + x * 4;
                c = Color.FromArgb(pbuff[loc + 3],
                  pbuff[loc + 2], pbuff[loc + 1],
                    pbuff[loc]);
            }
            return c;
        }
    }
    
}
