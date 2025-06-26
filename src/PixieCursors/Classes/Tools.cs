using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixieCursors
{
    public static class Tools
    {
        public unsafe static void CopyPixels2(BitmapSource source, PixelColor[,] resultPixels, int stride, int offset, bool dummy)
        {
            // get pixel colors from bitmapsource https://stackoverflow.com/a/1740553/5452781
            fixed (PixelColor* buffer = &resultPixels[0, 0])
                source.CopyPixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
                    (IntPtr)(buffer + offset), resultPixels.GetLength(0) * resultPixels.GetLength(1) * sizeof(PixelColor), stride);
        }

        public static unsafe PixelColor GetPixelColor(int x, int y, WriteableBitmap source)
        {
            var pix = new PixelColor();
            byte[] ColorData = { 0, 0, 0, 0 }; // B G R !
            IntPtr pBackBuffer = source.BackBuffer;
            byte* pBuff = (byte*)pBackBuffer.ToPointer();
            var b = pBuff[4 * x + (y * source.BackBufferStride)];
            var g = pBuff[4 * x + (y * source.BackBufferStride) + 1];
            var r = pBuff[4 * x + (y * source.BackBufferStride) + 2];
            var a = pBuff[4 * x + (y * source.BackBufferStride) + 3];
            pix.Red = r;
            pix.Green = g;
            pix.Blue = b;
            pix.Alpha = a;
            return pix;
        }

        public static PixelColor[,] GetPixels(BitmapSource source)
        {
            // get pixels from source bitmap
            if (source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[height, width];

            CopyPixels2(source, result, width * 4, 0, false);
            return result;
        }

        public static Bitmap CropEmptyPixels(Bitmap bmp)
        {
            // Recortar márgenes de pixeles vacíos.
            int w = bmp.Width;
            int h = bmp.Height;

            bool transparentRow(int row)
            {
                for (int i = 0; i < w; ++i)
                    if (bmp.GetPixel(i, row).A != 0)
                        return false;
                return true;
            }

            bool transparentColumn(int col)
            {
                for (int i = 0; i < h; ++i)
                    if (bmp.GetPixel(col, i).A != 0)
                        return false;
                return true;
            }

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (transparentRow(row))
                    topmost = row;
                else break;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (transparentRow(row))
                    bottommost = row;
                else break;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (transparentColumn(col))
                    leftmost = col;
                else
                    break;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (transparentColumn(col))
                    rightmost = col;
                else
                    break;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }

        public static void SetPixel(WriteableBitmap bitmap, int x, int y, int color)
        {
            // set single pixel to target bitmap
            try
            {
                // Reserve the back buffer for updates.
                bitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    int pBackBuffer = (int)bitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += y * bitmap.BackBufferStride;
                    pBackBuffer += x * 4;

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color;
                }

                // Specify the area of the bitmap that changed.
                bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                bitmap.Unlock();
            }
        }

        public static void ClearImage(WriteableBitmap targetBitmap, Int32Rect emptyRect, byte[] emptyPixels, int emptyStride)
        {
            // clears bitmap by writing empty pixels to it
            targetBitmap.WritePixels(emptyRect, emptyPixels, emptyStride, 0);
        }

        public static void SetCurrentColorPreviewBox(System.Windows.Shapes.Rectangle targetRectangle, PixelColor color)
        {
            var col = System.Windows.Media.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
            targetRectangle.Fill = new SolidColorBrush(col);
        }

        public static int Repeat(int val, int max)
        {
            // wrap-repeat value
            int result = val % max;
            if (result < 0) result += max;
            return result;
        }

        public static void DrawBackgroundGrid(WriteableBitmap targetBitmap, int canvasResolutionX, int canvasResolutionY, PixelColor c1, PixelColor c2, byte alpha)
        {
            for (int x = 0; x < canvasResolutionX; x++)
            {
                for (int y = 0; y < canvasResolutionY; y++)
                {
                    PixelColor v = ((x % 2) == (y % 2)) ? c1 : c2;
                    v.Alpha = alpha;
                    SetPixel(targetBitmap, x, y, (int)v.ColorBGRA);
                }
            }
        }
      
        public static double Lerp(double firstFloat, double secondFloat, double by)
        {
            return firstFloat + (secondFloat - firstFloat) * by;
        }

        public static double Frac(double v)
        {
            return v - Math.Floor(v);
        }

        public static double Clamp(double x, double a, double b)
        {
            return Math.Max(a, Math.Min(b, x));
        }

        public static SolidColorBrush SystemDrawingColorToSolidColorBrush(System.Drawing.Color c)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static PixelColor SystemDrawingColorToPixelColor(System.Drawing.Color c)
        {
            var pc = new PixelColor
            {
                Alpha = c.A,
                Red = c.R,
                Green = c.G,
                Blue = c.B
            };
            return pc;
        }
    }
}
