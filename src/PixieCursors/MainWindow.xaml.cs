using static PixieCursors.Tools;
using LazZiya.ImageResize;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Image = System.Drawing.Image;
using Path = System.IO.Path;
using HandyControl.Data;


namespace PixieCursors
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Variables
        // bitmap settings
        private int canvasResolutionX = 32, canvasResolutionY = 32;
        private float canvasScaleX = 1, left, top;
        private readonly int dpiX = 96, dpiY = 96;
        private WriteableBitmap canvasBitmap, gridBitmap, outlineBitmap;

        // simple undo
        private readonly Stack<WriteableBitmap> undoStack = new Stack<WriteableBitmap>();
        private readonly Stack<WriteableBitmap> redoStack = new Stack<WriteableBitmap>();
        private WriteableBitmap currentUndoItem;

        // colors
        private PixelColor lightColor, darkColor, currentColor;
        private PixelColor eraseColor = new PixelColor(0, 0, 0, 0);
        private byte gridAlpha = 32;
        private string color = "light";

        // mouse
        private int prevX, prevY, count = 0;
        private bool leftShiftDown = false, leftCtrlDown = false;
        private int posX = 1, posY = 1;

        // drawing lines
        private readonly int ddaMODIFIER_X = 0x7fff, ddaMODIFIER_Y = 0x7fff;

        // smart fill with double click
        private bool wasDoubleClick = false;
        private ToolMode previousToolMode = ToolMode.Draw;
        private PixelColor previousPixelColor, previousColor;

        // clear buffers
        private Int32Rect emptyRect;
        private int bytesPerPixel, emptyStride;
        private byte[] emptyPixels;

        // settings
        private string pngName, tempFolder;
        private bool cropImage;
        private ToolMode _currentTool = ToolMode.Draw;
        public ToolMode CurrentTool
        {
            get { return _currentTool; }
            set { _currentTool = value; OnPropertyChanged(); }
        }

        // files
        private string saveFile = null;
        #endregion

        #region Helpers
        private unsafe PixelColor GetPixel(int x, int y)
        {
            // return canvas pixel color from x,y
            var pix = new PixelColor();
            byte[] ColorData = { 0, 0, 0, 0 }; // B G R !
            IntPtr pBackBuffer = canvasBitmap.BackBuffer;
            byte* pBuff = (byte*)pBackBuffer.ToPointer();
            var b = pBuff[4 * x + (y * canvasBitmap.BackBufferStride)];
            var g = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 1];
            var r = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 2];
            var a = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 3];
            pix.Red = r;
            pix.Green = g;
            pix.Blue = b;
            pix.Alpha = a;
            return pix;
        }

        private Brush PalettePicker(SolidColorBrush color)
        {
            // TODO: take current color as param to return if cancelled?
            Picker dlg = new Picker
            {
                Owner = this
            };

            dlg.PickerControl.SelectedBrush = color;

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.PickerControl.SelectedBrush;
            }
            return null;
        }

        private void CopyBitmapPixels(WriteableBitmap source, WriteableBitmap target)
        {
            byte[] data = new byte[source.BackBufferStride * source.PixelHeight];
            source.CopyPixels(data, source.BackBufferStride, 0);
            target.WritePixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), data, source.BackBufferStride, 0);
        }

        private void ReplacePixels(PixelColor find, PixelColor replace)
        {
            for (int x = 0; x < canvasResolutionX; x++)
            {
                for (int y = 0; y < canvasResolutionY; y++)
                {
                    PixelColor pixel = GetPixelColor(x, y, canvasBitmap);

                    if (pixel == find)
                    {
                        SetPixel(canvasBitmap, x, y, (int)replace.ColorBGRA);
                    }
                }
            }
        }

        private void DrawPixel(int x, int y)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?redirectedfrom=MSDN&view=netframework-4.7.2
            // The DrawPixel method updates the WriteableBitmap by using
            // unsafe code to write a pixel into the back buffer.
            if (x < 0 || x > canvasResolutionX - 1) return;
            if (y < 0 || y > canvasResolutionY - 1) return;

            // draw
            SetPixel(canvasBitmap, x, y, (int)currentColor.ColorBGRA);
        }

        private void ErasePixel(int x, int y)
        {
            // byte[] ColorData = { 0, 0, 0, 0 }; // B G R
            if (x < 0 || x > canvasResolutionX - 1) return;
            if (y < 0 || y > canvasResolutionY - 1) return;

            // Int32Rect rect = new Int32Rect(x, y, 1, 1);
            // canvasBitmap.WritePixels(rect, ColorData, 4, 0);
            SetPixel(canvasBitmap, x, y, (int)eraseColor.ColorBGRA);
        }

        private void DrawLine(int startX, int startY, int endX, int endY)
        {
            // https://github.com/Chris3606/GoRogue/blob/master/GoRogue/Lines.cs
            int dx = endX - startX;
            int dy = endY - startY;

            int nx = Math.Abs(dx);
            int ny = Math.Abs(dy);

            // Calculate octant value
            int octant = ((dy < 0) ? 4 : 0) | ((dx < 0) ? 2 : 0) | ((ny > nx) ? 1 : 0);
            int move;
            int frac = 0;
            int mn = Math.Max(nx, ny);

            if (mn == 0)
            {
                //yield return new Coord(startX, startY);
                //yield break;
                return;
            }

            if (ny == 0)
            {
                if (dx > 0)
                    for (int x = startX; x <= endX; x++)
                        DrawPixel(x, startY);
                //yield return new Coord(x, startY);
                else
                    for (int x = startX; x >= endX; x--)
                        DrawPixel(x, startY);
                //yield return new Coord(x, startY);

                //yield break;
                return;
            }

            if (nx == 0)
            {
                if (dy > 0)
                    for (int y = startY; y <= endY; y++)
                        DrawPixel(startX, y);
                //yield return new Coord(startX, y);
                else
                    for (int y = startY; y >= endY; y--)
                        DrawPixel(startX, y);
                // yield return new Coord(startX, y);

                //                yield break;
                return;
            }

            switch (octant)
            {
                case 0: // +x, +y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary <= endX; primary++, frac += move)
                        //yield return new Coord(primary, startY + ((frac + MODIFIER_Y) >> 16));
                        DrawPixel(primary, startY + ((frac + ddaMODIFIER_Y) >> 16));
                    break;

                case 1:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary <= endY; primary++, frac += move)
                        //yield return new Coord(startX + ((frac + MODIFIER_X) >> 16), primary);
                        DrawPixel(startX + ((frac + ddaMODIFIER_X) >> 16), primary);
                    break;

                case 2: // -x, +y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary >= endX; primary--, frac += move)
                        //                        yield return new Coord(primary, startY + ((frac + MODIFIER_Y) >> 16));
                        DrawPixel(primary, startY + ((frac + ddaMODIFIER_Y) >> 16));
                    break;

                case 3:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary <= endY; primary++, frac += move)
                        //                        yield return new Coord(startX - ((frac + MODIFIER_X) >> 16), primary);
                        DrawPixel(startX - ((frac + ddaMODIFIER_X) >> 16), primary);
                    break;

                case 6: // -x, -y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary >= endX; primary--, frac += move)
                        //                        yield return new Coord(primary, startY - ((frac + MODIFIER_Y) >> 16));
                        DrawPixel(primary, startY - ((frac + ddaMODIFIER_Y) >> 16));
                    break;

                case 7:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary >= endY; primary--, frac += move)
                        //                        yield return new Coord(startX - ((frac + MODIFIER_X) >> 16), primary);
                        DrawPixel(startX - ((frac + ddaMODIFIER_X) >> 16), primary);
                    break;

                case 4: // +x, -y
                    move = (ny << 16) / nx;
                    for (int primary = startX; primary <= endX; primary++, frac += move)
                        //                        yield return new Coord(primary, startY - ((frac + MODIFIER_Y) >> 16));
                        DrawPixel(primary, startY - ((frac + ddaMODIFIER_Y) >> 16));
                    break;

                case 5:
                    move = (nx << 16) / ny;
                    for (int primary = startY; primary >= endY; primary--, frac += move)
                        DrawPixel(startX + ((frac + ddaMODIFIER_X) >> 16), primary);
                    //                    yield return new Coord(startX + ((frac + MODIFIER_X) >> 16), primary);
                    break;
            }
        }

        private void FloodFill(int x, int y, int fillColor)
        {
            // get hit color pixel
            PixelColor hitColor = GetPixel(x, y);

            // if same as current color, exit
            if (hitColor.ColorBGRA == fillColor) return;

            SetPixel(canvasBitmap, x, y, (int)hitColor.ColorBGRA);

            List<int> ptsx = new List<int>
            {
                x
            };
            List<int> ptsy = new List<int>
            {
                y
            };

            int maxLoop = canvasResolutionX * canvasResolutionY + canvasResolutionX;
            while (ptsx.Count > 0 && maxLoop > 0)
            {
                maxLoop--;

                if (ptsx[0] - 1 >= 0 && GetPixel(ptsx[0] - 1, ptsy[0]).ColorBGRA == hitColor.ColorBGRA)
                {
                    ptsx.Add(ptsx[0] - 1); ptsy.Add(ptsy[0]);
                    SetPixel(canvasBitmap, ptsx[0] - 1, ptsy[0], fillColor);
                }

                if (ptsy[0] - 1 >= 0 && GetPixel(ptsx[0], ptsy[0] - 1).ColorBGRA == hitColor.ColorBGRA)
                {
                    ptsx.Add(ptsx[0]); ptsy.Add(ptsy[0] - 1);
                    SetPixel(canvasBitmap, ptsx[0], ptsy[0] - 1, fillColor);
                }

                if (ptsx[0] + 1 < canvasResolutionX && GetPixel(ptsx[0] + 1, ptsy[0]).ColorBGRA == hitColor.ColorBGRA)
                {
                    ptsx.Add(ptsx[0] + 1); ptsy.Add(ptsy[0]);
                    SetPixel(canvasBitmap, ptsx[0] + 1, ptsy[0], fillColor);
                }

                if (ptsy[0] + 1 < canvasResolutionY && GetPixel(ptsx[0], ptsy[0] + 1).ColorBGRA == hitColor.ColorBGRA)
                {
                    ptsx.Add(ptsx[0]); ptsy.Add(ptsy[0] + 1);
                    SetPixel(canvasBitmap, ptsx[0], ptsy[0] + 1, fillColor);
                }
                ptsx.RemoveAt(0);
                ptsy.RemoveAt(0);
            } // while can floodfill
        }

        private void BitmapFlip(bool horizontal)
        {
            // clone canvas, FIXME not really needed..could just copy pixels to array or backbuffer directly
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
            WriteableBitmap tempCanvasBitmap = new WriteableBitmap(canvasResolutionX, canvasResolutionY, dpiX, dpiY, PixelFormats.Bgra32, null);
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
            tempCanvasBitmap = canvasBitmap.Clone();
            for (int x = 0; x < canvasResolutionX; x++)
            {
                for (int y = 0; y < canvasResolutionY; y++)
                {
                    int xx = horizontal ? (canvasResolutionX - x - 1) : x;
                    int yy = !horizontal ? (canvasResolutionY - y - 1) : y;
                    PixelColor c = GetPixelColor(xx, yy, tempCanvasBitmap);
                    SetPixel(canvasBitmap, x, y, (int)c.ColorBGRA);
                }
            }
        }

        private void ScrollCanvas(int sx, int sy)
        {
            // clone canvas, FIXME not really needed..could just copy pixels to array or so..
#pragma warning disable IDE0059 // Asignación innecesaria de un valor
            WriteableBitmap tempCanvasBitmap = new WriteableBitmap(canvasResolutionX, canvasResolutionY, dpiX, dpiY, PixelFormats.Bgra32, null);
#pragma warning restore IDE0059 // Asignación innecesaria de un valor
            tempCanvasBitmap = canvasBitmap.Clone();

            // TODO add wrap or clamp option?

            for (int x = 0; x < canvasResolutionX; x++)
            {
                for (int y = 0; y < canvasResolutionY; y++)
                {
                    PixelColor c = GetPixelColor(x, y, tempCanvasBitmap);
                    int xx = Repeat(x + sx, canvasResolutionX);
                    int yy = Repeat(y + sy, canvasResolutionY);
                    SetPixel(canvasBitmap, xx, yy, (int)c.ColorBGRA);
                }
            }
        }

        private void ImageToCanvas(string path)
        {
            Uri uri = new Uri(path);
            BitmapImage img = new BitmapImage(uri);

            // get colors
            PixelColor[,] pixels = GetPixels(img);

            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            PixelColor[] palette = new PixelColor[width * height];

            int index = 0;
            int x;
            int y;
            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    var c = pixels[x, y];
                    palette[index++] = c;
                }
            }

            // put pixels on palette canvas
            for (int i = 0, len = palette.Length; i < len; i++)
            {
                x = i % canvasResolutionX;
                y = (i % len) / canvasResolutionY;
                SetPixel(canvasBitmap, y, x, (int)palette[i].ColorBGRA);
            }
        }

        private void ShowMousePos(int x, int y)
        {
            lblMousePos.Content = (x + 1) + "," + (y + 1);
        }

        private void ShowMousePixelColor(int x, int y)
        {
            PixelColor col = GetPixel(x, y);
            lblPixelColor.Content = col.Red + "," + col.Green + "," + col.Blue + "," + col.Alpha;
        }

        private void Start(bool loadSettings = true)
        {
            // needed for binding
            DataContext = this;

            // setup background grid
            gridBitmap = new WriteableBitmap(canvasResolutionX, canvasResolutionY, dpiX, dpiY, PixelFormats.Bgra32, null);
            gridImage.Source = gridBitmap;

            // get values from settings
            if (loadSettings == true) LoadSettings();

            // build drawing area
            canvasBitmap = new WriteableBitmap(canvasResolutionX, canvasResolutionY, dpiX, dpiY, PixelFormats.Bgra32, null);
            drawingImage.Source = canvasBitmap;
            canvasScaleX = (float)drawingImage.Width / (float)canvasResolutionX;

            // setup outline bitmap
            outlineBitmap = new WriteableBitmap(canvasResolutionX, canvasResolutionY, dpiX, dpiY, PixelFormats.Bgra32, null);
            outlineImage.Source = outlineBitmap;

            // init clear buffers
            emptyRect = new Int32Rect(0, 0, canvasBitmap.PixelWidth, canvasBitmap.PixelHeight);
            bytesPerPixel = canvasBitmap.Format.BitsPerPixel / 8;
            emptyPixels = new byte[emptyRect.Width * emptyRect.Height * bytesPerPixel];
            emptyStride = emptyRect.Width * bytesPerPixel;

            // setup preview images
            imgPreview.Source = canvasBitmap;

            // set pixel box size based on resolution
            rectPixelPos.Width = 23 * (23 / (float)canvasResolutionX);
            rectPixelPos.Height = 23 * (23 / (float)canvasResolutionY);

            rectHotspot.Width = 23 * (23 / (float)canvasResolutionX);
            rectHotspot.Height = 23 * (23 / (float)canvasResolutionY);

            // clear undos
            undoStack.Clear();
            redoStack.Clear();
            currentUndoItem = null;

            // Obtener directorio temporal
            tempFolder = Path.GetTempPath() + @"cur-output\";
            _ = Directory.CreateDirectory(tempFolder);
        }

        private void LoadSettings()
        {
            lightColor.Red = 255;
            lightColor.Green = 255;
            lightColor.Blue = 255;

            darkColor.Red = 233;
            darkColor.Green = 233;
            darkColor.Blue = 233;

            gridAlpha = 255;
            canvasResolutionX = canvasResolutionY = 32;

            currentColor.Red = 255;
            currentColor.Green = 255;
            currentColor.Blue = 255;
            currentColor.Alpha = 255;

            DrawBackgroundGrid(gridBitmap, canvasResolutionX, canvasResolutionY, lightColor, darkColor, gridAlpha);
        }

        private void RegisterUndo()
        {
            // save undo state
            currentUndoItem = canvasBitmap.Clone();
            undoStack.Push(currentUndoItem);
            redoStack.Clear();
        }

        private void DoUndo()
        {
            // restore to previous bitmap
            if (undoStack.Count > 0)
            {
                // TODO: clear redo?
                // save current image in top of redo stack
                redoStack.Push(canvasBitmap.Clone());
                // take latest image from top of undo stack
                currentUndoItem = undoStack.Pop();
                // show latest image
                CopyBitmapPixels(currentUndoItem, canvasBitmap);
            }
        }

        private void DoRedo()
        {
            // go to next existing undo buffer, if available
            if (redoStack.Count > 0)
            {
                // save current image in top of redo stack
                undoStack.Push(canvasBitmap.Clone());
                // take latest image from top of redo stack
                currentUndoItem = redoStack.Pop();
                // show latest redo image
                CopyBitmapPixels(currentUndoItem, canvasBitmap);
            }
        }

        private void SaveAsCursor(int hotspotX, int hotspotY, string curPath)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder
                {
                    Interlace = PngInterlaceOption.On
                };
                encoder.Frames.Add(BitmapFrame.Create(canvasBitmap));
                encoder.Save(ms);

                using (Bitmap bmp = new Bitmap(ms))
                {
                    using (MemoryStream icoStream = new MemoryStream())
                    {
                        // Write header
                        using (BinaryWriter writer = new BinaryWriter(icoStream))
                        {
                            writer.Write((short)0);  // Reserved
                            writer.Write((short)2);  // Type: 2 for cursor
                            writer.Write((short)1);  // Number of images

                            // Image entry (16 bytes)
                            writer.Write((byte)bmp.Width);
                            writer.Write((byte)bmp.Height);
                            writer.Write((byte)0);  // Color count
                            writer.Write((byte)0);  // Reserved
                            writer.Write((short)hotspotX); // Hotspot X
                            writer.Write((short)hotspotY); // Hotspot Y

                            using (MemoryStream imgStream = new MemoryStream())
                            {
                                bmp.Save(imgStream, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] imgData = imgStream.ToArray();

                                writer.Write(imgData.Length); // Size
                                writer.Write(22); // Offset from beginning

                                // Write image data
                                writer.Write(imgData);
                            }
                        }

                        // Save to file
                        File.WriteAllBytes(curPath, icoStream.ToArray());
                    }
                }
                ms.Close();
            }
        }

        private static Bitmap CreateShadow(Bitmap source)
        {
            Bitmap shadow = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(shadow))
            {
                // Set black semi-transparent color
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0.2f, 0},
                new float[] {0, 0, 0, 0, 0}
                });

                ImageAttributes attr = new ImageAttributes();
                attr.SetColorMatrix(colorMatrix);

                g.DrawImage(
                    source,
                    new System.Drawing.Rectangle(-3, -3, source.Width, source.Height),
                    0, 0, source.Width, source.Height,
                    GraphicsUnit.Pixel,
                    attr
                );
            }
            return shadow;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        // https://github.com/crclayton/WPF-DataBinding-Example
        public event PropertyChangedEventHandler PropertyChanged;

        private void DrawingRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RegisterUndo();

            int x = (int)(e.GetPosition(drawingImage).X / canvasScaleX);
            int y = (int)(e.GetPosition(drawingImage).Y / canvasScaleX);

            ErasePixel(x, y);
        }

        private void DrawingMiddleButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                int x = (int)(e.GetPosition(drawingImage).X / canvasScaleX);
                int y = (int)(e.GetPosition(drawingImage).Y / canvasScaleX);

                currentColor = GetPixel(x, y);
                SetCurrentColorPreviewBox(rectCurrentColor, currentColor);
            }
        }

        private void DrawingLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // clicked, but not moved
            int x = (int)(e.GetPosition(drawingImage).X / canvasScaleX);
            int y = (int)(e.GetPosition(drawingImage).Y / canvasScaleX);

            // take current bitmap as currentimage
            RegisterUndo();

            // check for double click
            if (e.ClickCount == 2)
            {
                if (CurrentTool == ToolMode.Draw)
                {
                    previousToolMode = CurrentTool;
                    CurrentTool = ToolMode.Fill;
                    wasDoubleClick = true;
                }
            }
            else // keep old color
            {
                previousPixelColor = GetPixel(x, y);
            }

            switch (CurrentTool)
            {
                case ToolMode.Draw:
                    // check if shift is down, then do line to previous point
                    if (leftShiftDown)
                    {
                        DrawLine(prevX, prevY, x, y);
                    }
                    else
                    {
                        DrawPixel(x, y);
                    }
                    break;
                case ToolMode.Fill:
                    // non-contiguous fill, fills all pixels that match target pixel color
                    if (leftCtrlDown)
                    {
                        ReplacePixels(previousPixelColor, currentColor);
                    }
                    else
                    {
                        FloodFill(x, y, (int)currentColor.ColorBGRA);
                    }
                    break;
                case ToolMode.Eraser:
                    btnEraser.IsChecked = true;
                    previousColor = currentColor;
                    currentColor = eraseColor;
                    rectCurrentColor.Fill = eraseColor.AsSolidColorBrush();
                    break;
                case ToolMode.Hotspot:
                    // En dónde dará clic en cursor
                    rectHotspot.Visibility = Visibility.Visible;
                    posX = (x + 1);
                    posY = (y + 1);
                    Console.WriteLine(posX + ", " + posY);

                    rectHotspot.Margin = new Thickness(64 + left, 50 + top, 0, 0);
                    PixelColor pc = GetPixelColor(x, y, canvasBitmap).Inverted(128);
                    rectHotspot.Stroke = pc.AsSolidColorBrush();
                    break;
                case ToolMode.ColorPicker:
                    // Herramienta gotero
                    btnColorPicker.IsChecked = true;

                    int _x = (int)(e.GetPosition(drawingImage).X / canvasScaleX);
                    int _y = (int)(e.GetPosition(drawingImage).Y / canvasScaleX);

                    currentColor = GetPixel(_x, _y);
                    SetCurrentColorPreviewBox(rectCurrentColor, currentColor);
                    colorPicker.SelectedBrush = (SolidColorBrush)rectCurrentColor.Fill;

                    btnColorPicker.IsChecked = false;

                    // Cambia a la herramienta anterior, excepto con None y Hotspot
                    // en ese caso, regresa al Draw (Brush).
                    CurrentTool = previousToolMode != ToolMode.None && previousToolMode != ToolMode.Hotspot ? previousToolMode : ToolMode.Draw;
                    break;
                case ToolMode.None:
                    // Esta herramienta evita pintar un pixel accidental al importar
                    // una imágen, se utiliza una sola vez al iniciar.
                    CurrentTool = previousToolMode;
                    break;
            }

            prevX = x;
            prevY = y;

            if (wasDoubleClick == true)
            {
                wasDoubleClick = false;
                CurrentTool = previousToolMode;
            }
        }

        private void DrawingAreaMouseMoved(object sender, MouseEventArgs e)
        {
            // update mousepos info
            int x = (int)(e.GetPosition(drawingImage).X / canvasScaleX);
            int y = (int)(e.GetPosition(drawingImage).Y / canvasScaleX);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (CurrentTool)
                {
                    case ToolMode.Draw:
                        DrawPixel(x, y);
                        break;
                    case ToolMode.Fill:
                        FloodFill(x, y, (int)currentColor.ColorBGRA);
                        break;
                }
                prevX = x;
                prevY = y;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                ErasePixel(x, y);
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                currentColor = GetPixel(x, y);
            }

            ShowMousePos(x, y);
            ShowMousePixelColor(x, y);

            // snap preview rectangle to grid
            int fix = 256 / canvasResolutionX;
            float off = ((float)256 / (float)canvasResolutionX) - fix;
            left = (x * canvasScaleX) + (x * (16 / canvasResolutionX) * (off > 0 ? 1 : 0));
            top = (y * canvasScaleX) + (y * (16 / canvasResolutionY) * (off > 0 ? 1 : 0));

            rectPixelPos.Margin = new Thickness(64 + left, 50 + top, 0, 0);
            PixelColor pc = GetPixelColor(x, y, canvasBitmap).Inverted(128);
            rectPixelPos.Stroke = pc.AsSolidColorBrush();
        }

        private void OnOpenButton(object sender, RoutedEventArgs e)
        {
            // Abrir imagen e importarla al canvas dependiendo de su tamaño.
            previousToolMode = CurrentTool;

            OpenFileDialog filedialog = new OpenFileDialog
            {
                Title = Properties.Resources.OpenTitle,
                Filter = Properties.Resources.PNGFiles + "(*.png)|*.png"
            };

            if (filedialog.ShowDialog() == true)
            {
                string pngPath = Path.GetFullPath(filedialog.FileName);
                pngName = Path.GetFileName(filedialog.FileName);

                using (Bitmap bmp = new Bitmap(pngPath))
                {
                    int bmpH = bmp.Height;
                    int bmpW = bmp.Width;

                    if (bmpH == 32 && bmpW == 32)
                    {
                        // Importar directamente al tener 32px de ancho y alto.
                        ImageToCanvas(pngPath);
                    }
                    else
                    {
                        string imgPath = pngPath;

                        if (cropImage)
                        {
                            // Recortar margenes de pixeles vacios.
                            CropEmptyPixels(bmp).SaveAs(tempFolder + "imageCropped.png");
                            imgPath = tempFolder + "imageCropped.png";
                        }

                        using (Image img = Image.FromFile(imgPath))
                        {
                            int imgH = img.Height;
                            int imgW = img.Width;

                            // Escalar la imagen a partir de las dimensiones.
                            if (imgH > imgW)
                            {
                                // Si el height es mayor a width, escalar el height a 31px
                                // y cortarlo a 32px (tamaño del canvas).
                                img.ScaleByHeight(31)
                                .Crop(32, 32)
                                .SaveAs(tempFolder + "image.png");
                            }
                            else if (imgW > imgH)
                            {
                                // Si el width es mayor a height, escalar el width a 31px
                                // y cortarlo a 32px (tamaño del canvas).
                                img.ScaleByWidth(31)
                                .Crop(32, 32)
                                .SaveAs(tempFolder + "image.png");
                            }
                            else if (imgW == imgH && imgW > 32 && imgH > 32)
                            {
                                // Si la imagen es 1:1 pero height y width son mayores a
                                // 32px, escalar el width a 31px y cortarlo a 32px.
                                img.ScaleByWidth(31)
                                .Crop(32, 32)
                                .SaveAs(tempFolder + "image.png");
                            }
                            else if (imgW < 32 && imgH < 32)
                            {
                                // Si el height y width son menores a 32px, cortarlo a 32px.
                                img.Crop(32, 32)
                                .SaveAs(tempFolder + "image.png");
                            }

                            ImageToCanvas(tempFolder + "image.png");
                            img.Dispose();
                            GC.Collect();
                        }
                    }
                }

            }

            // Herramienta que evita pintar un pixel accidental al
            // importar una imagen.
            CurrentTool = ToolMode.None;
        }

        private void OnSaveButton(object sender, RoutedEventArgs e)
        {
            // Guardar cursor
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = "",
                DefaultExt = ".cur",
                Filter = "Cursors CUR|*.cur",
                Title = "Save as..."
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveAsCursor(posX, posY, saveFileDialog.FileName);
                saveFile = saveFileDialog.FileName;
            }
        }

        private void OnNewButton(object sender, MouseButtonEventArgs e)
        {
            // Crear nuevo proyecto.
            MessageBoxResult mssg = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Caption = Properties.Resources.NewFileTitle,
                Message = Properties.Resources.CreateAnotherFile,
                IconBrushKey = ResourceToken.PrimaryBrush,
                IconKey = ResourceToken.AskGeometry,
                Button = MessageBoxButton.YesNo,
                YesContent = Properties.Resources.Yes,
                NoContent = Properties.Resources.No
            });

            if (mssg == MessageBoxResult.Yes)
            {
                RegisterUndo();
                ClearImage(canvasBitmap, emptyRect, emptyPixels, emptyStride);
                saveFile = null;
                posX = 0;
                posY = 0;
                rectHotspot.Margin = new Thickness(64 + 0, 50 + 0, 0, 0);
                rectHotspot.Visibility = Visibility.Hidden;
                LoadSettings();
                rectCurrentColor.Fill = currentColor.AsSolidColorBrush();
                colorPicker.SelectedBrush = (SolidColorBrush)rectCurrentColor.Fill;
                Preview.Fill = new SolidColorBrush(Color.FromRgb(244, 244, 244));
            }
        }

        private void OnTransparencyButton(object sender, RoutedEventArgs e)
        {
            // Cambiar color del fondo a cuadros aka transparencia.
            switch (color)
            {
                case "light":
                    color = "gray";
                    break;
                case "gray":
                    color = "dark";
                    break;
                case "dark":
                    color = "light";
                    break;
            }

            gridAlpha = 255;

            switch (color)
            {
                case "light":
                    lightColor.Red = 255;
                    lightColor.Green = 255;
                    lightColor.Blue = 255;

                    darkColor.Red = 233;
                    darkColor.Green = 233;
                    darkColor.Blue = 233;

                    DrawBackgroundGrid(gridBitmap, canvasResolutionX, canvasResolutionY, lightColor, darkColor, gridAlpha);
                    Preview.Fill = new SolidColorBrush(Color.FromRgb(244, 244, 244));
                    break;
                case "gray": // cancelled
                    lightColor.Red = 232;
                    lightColor.Green = 232;
                    lightColor.Blue = 232;

                    darkColor.Red = 212;
                    darkColor.Green = 214;
                    darkColor.Blue = 222;

                    DrawBackgroundGrid(gridBitmap, canvasResolutionX, canvasResolutionY, lightColor, darkColor, gridAlpha);
                    Preview.Fill = new SolidColorBrush(Color.FromRgb(232, 232, 232));
                    break;
                case "dark":
                    lightColor.Red = 212;
                    lightColor.Green = 214;
                    lightColor.Blue = 221;

                    darkColor.Red = 183;
                    darkColor.Green = 188;
                    darkColor.Blue = 202;

                    DrawBackgroundGrid(gridBitmap, canvasResolutionX, canvasResolutionY, lightColor, darkColor, gridAlpha);
                    Preview.Fill = new SolidColorBrush(Color.FromRgb(198, 201, 212));
                    break;
            }
        }

        private void OnShadowButton(object sender, RoutedEventArgs e)
        {
            // Crear sombra en el cursor.
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder
                {
                    Interlace = PngInterlaceOption.On
                };
                encoder.Frames.Add(BitmapFrame.Create(canvasBitmap));
                encoder.Save(ms);

                using (Bitmap original = new Bitmap(ms))
                using (Bitmap result = new Bitmap(original.Width, original.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.Clear(System.Drawing.Color.Transparent);

                    // Shadow offset
                    int offsetX = 4;
                    int offsetY = 4;

                    // Draw shadow behind
                    using (Bitmap shadow = CreateShadow(original))
                    {
                        g.DrawImage(
                            shadow,
                            new System.Drawing.Rectangle(offsetX, offsetY, original.Width - offsetX, original.Height - offsetY),
                            new System.Drawing.Rectangle(0, 0, original.Width - offsetX, original.Height - offsetY),
                            GraphicsUnit.Pixel
                        );
                    }

                    // Draw original on top (no scaling)
                    g.DrawImage(original, 0, 0, original.Width, original.Height);

                    try
                    {
                        result.Save(tempFolder + "imageShadow.png", ImageFormat.Png);
                    }
                    catch
                    {
                        // do nothing
                    }
                }

                // Al cliquear boton Undo, sombra y cambios anteriores desaparecen
                // esto hará que solo la sombra desaparezca.
                undoStack.Push(canvasBitmap.Clone());

                // Guardar cambios actuales.
                ImageToCanvas(tempFolder + "imageShadow.png");
            }
        }

        private void OnFolderCursors(object sender, RoutedEventArgs e)
        {
            using (Process folderCursors = new Process())
            {
                folderCursors.StartInfo.FileName = @"C:\Windows\Cursors\- Pixie Cursors -";
                try
                {
                    folderCursors.Start();
                }
                catch
                {
                    MessageBoxResult mssg = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                    {
                        Caption = Properties.Resources.FolderDoesnTExists,
                        Message = Properties.Resources.YouDonTHaveThisFolder,
                        IconBrushKey = ResourceToken.PrimaryBrush,
                        IconKey = ResourceToken.ErrorGeometry,
                        Button = MessageBoxButton.OK,
                    });
                }
            }
        }

        private void OnMouseProperties(object sender, RoutedEventArgs e)
        {
            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                cmd.StartInfo.Arguments = "/C control mouse";
                cmd.Start();
                cmd.Close();
            }
        }

        private void OnInfoButton(object sender, RoutedEventArgs e)
        {
            About dlgextract = new About() { Owner = this };
            dlgextract.Show();
        }

        private void OnUndoButtonDown(object sender, RoutedEventArgs e)
        {
            DoUndo();
        }

        private void OnRedoButtonDown(object sender, RoutedEventArgs e)
        {
            DoRedo();
        }

        private void OnColorPickerButton(object sender, RoutedEventArgs e)
        {
            previousToolMode = CurrentTool;
            CurrentTool = ToolMode.ColorPicker;
        }

        private void OnEraserButton(object sender, RoutedEventArgs e)
        {
            CurrentTool = ToolMode.Eraser;
        }

        private void OnHotspotButton(object sender, RoutedEventArgs e)
        {
            rectPixelPos.RadiusX = 30;
            rectPixelPos.RadiusY = 30;
            lblInfo.Content = Properties.Resources.SelectWhereTheCursorWillClick;
        }

        private void OnFlipXButtonDown(object sender, RoutedEventArgs e)
        {
            BitmapFlip(horizontal: true);
            btnFlipX.IsChecked = false;
        }

        private void OnFlipYButtonDown(object sender, RoutedEventArgs e)
        {
            BitmapFlip(horizontal: false);
            btnFlipY.IsChecked = false;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // if key is pressed down globally
            switch (e.Key)
            {
                case Key.D: // reset to default colors (current, secondary)
                    currentColor = PixelColor.White;
                    SetCurrentColorPreviewBox(rectCurrentColor, currentColor);
                    rectSecondaryColor.Fill = PixelColor.Black.AsSolidColorBrush();
                    eraseColor = PixelColor.Transparent;
                    break;
                case Key.I: // color picker
                    CurrentTool = ToolMode.ColorPicker;
                    break;
                case Key.X: // swap current/secondary colors
                    (rectSecondaryColor.Fill, rectCurrentColor.Fill) = (rectCurrentColor.Fill, rectSecondaryColor.Fill);
                    currentColor = new PixelColor(((SolidColorBrush)rectCurrentColor.Fill).Color);
                    break;
                case Key.B: // brush
                    CurrentTool = ToolMode.Draw;
                    break;
                case Key.F: // fill
                    CurrentTool = ToolMode.Fill;
                    break;
                case Key.E: // eraser
                    CurrentTool = ToolMode.Eraser;
                    break;
                case Key.LeftShift: // left shift
                    if (CurrentTool == ToolMode.Draw)
                    {
                        lblInfo.Content = Properties.Resources.DrawLine;
                    }
                    leftShiftDown = true;
                    break;
                case Key.LeftCtrl: // left control
                    if (CurrentTool == ToolMode.Fill)
                    {
                        lblInfo.Content = Properties.Resources.DoubleClickToReplaceAllPixels;
                    }
                    leftCtrlDown = true;
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    lblInfo.Content = "";
                    leftShiftDown = false;
                    break;
                case Key.LeftCtrl:
                    leftCtrlDown = false;
                    lblInfo.Content = "";
                    break;
            }
        }

        public void Executed_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            DoUndo();
        }

        public void CanExecute_Undo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void Executed_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            DoRedo();
        }

        public void CanExecute_Redo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void Executed_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            saveFile = null;
            OnSaveButton(null, null);
        }

        public void CanExecute_SaveAs(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void Executed_Open(object sender, ExecutedRoutedEventArgs e)
        {
            OnOpenButton(null, null);
        }

        public void CanExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        public void Executed_New(object sender, ExecutedRoutedEventArgs e)
        {
            OnNewButton(null, null);
        }

        public void CanExecute_New(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnMoveButtonUp(object sender, RoutedEventArgs e)
        {
            ScrollCanvas(0, -1);
        }

        private void OnMoveButtonDown(object sender, RoutedEventArgs e)
        {
            ScrollCanvas(0, 1);
        }

        private void OnMoveButtonRight(object sender, RoutedEventArgs e)
        {
            ScrollCanvas(1, 0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            GC.Collect();
            try
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
            catch
            {
                // do nothing
            }
        }

        private void OnMoveButtonLeft(object sender, RoutedEventArgs e)
        {
            ScrollCanvas(-1, 0);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnHotspot_Unchecked(object sender, RoutedEventArgs e)
        {
            if (rectPixelPos.RadiusX != 0)
            {
                rectPixelPos.RadiusX = 0;
                rectPixelPos.RadiusY = 0;
            }

            lblInfo.Content = "";
        }

        private void BtnEraser_Unchecked(object sender, RoutedEventArgs e)
        {
            currentColor = previousColor;
            SetCurrentColorPreviewBox(rectCurrentColor, currentColor);
        }

        private void CheckCropImage_Checked(object sender, RoutedEventArgs e)
        {
            cropImage = true;
        }

        private void CheckCropImage_Unchecked(object sender, RoutedEventArgs e)
        {
            cropImage = false;
        }

        private void RectColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Shapes.Rectangle rec = (System.Windows.Shapes.Rectangle)sender;
            SolidColorBrush solidColor = (SolidColorBrush)rec.Fill;

            Brush newcolor = PalettePicker(solidColor);
            if (newcolor != null)
            {
                rec.Fill = newcolor;
            }
        }

        private void SwatchColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Obtener el color de las swatches.
            Brush swatchColor = ((System.Windows.Shapes.Rectangle)sender).Fill;

            if (swatchColor != null)
            {
                currentColor = new PixelColor((SolidColorBrush)swatchColor);
                rectCurrentColor.Fill = swatchColor;
                colorPicker.SelectedBrush = (SolidColorBrush)swatchColor;
            }
        }

        private void DrawingImage_MouseLeave(object sender, MouseEventArgs e)
        {
            rectPixelPos.Visibility = Visibility.Hidden;
        }

        private void DrawingImage_MouseEnter(object sender, MouseEventArgs e)
        {
            rectPixelPos.Visibility = Visibility.Visible;
        }

        private void ColorPicker_SelectedColorChanged(object sender, FunctionEventArgs<Color> e)
        {
            SolidColorBrush colorBrush = colorPicker.SelectedBrush;
            rectCurrentColor.Fill = colorBrush;
            currentColor = new PixelColor(colorBrush);
        }

        private void OnAddSwatchButton(object sender, RoutedEventArgs e)
        {
            // Agregar y/o recorrer el color de las swatches
            count++;

            switch (count)
            {
                case 1:
                    swatch1.Fill = rectCurrentColor.Fill;
                    break;
                case 2:
                    swatch2.Fill = swatch1.Fill;
                    swatch1.Fill = rectCurrentColor.Fill;
                    break;
                case 3:
                    swatch3.Fill = swatch2.Fill;
                    swatch2.Fill = swatch1.Fill;
                    swatch1.Fill = rectCurrentColor.Fill;
                    break;
                case 4:
                    swatch4.Fill = swatch3.Fill;
                    swatch3.Fill = swatch2.Fill;
                    swatch2.Fill = swatch1.Fill;
                    swatch1.Fill = rectCurrentColor.Fill;
                    break;
                default:
                    if (count == 5 || count > 5)
                    {
                        swatch4.Fill = swatch3.Fill;
                        swatch3.Fill = swatch2.Fill;
                        swatch2.Fill = swatch1.Fill;
                        swatch1.Fill = rectCurrentColor.Fill;
                    }
                    break;
            }
        }

        private void OnPreviewBackgButton(object sender, RoutedEventArgs e)
        {
            SolidColorBrush solidColor = (SolidColorBrush)Preview.Fill;

            Brush newcolor = PalettePicker(solidColor);
            if (newcolor != null)
            {
                Preview.Fill = newcolor;
            }
        }

        private void BgbtnLight_Click(object sender, RoutedEventArgs e)
        {
            Preview.Fill = new SolidColorBrush(Color.FromRgb(244, 244, 244));
        }

        private void BgbtnDark_Click(object sender, RoutedEventArgs e)
        {
            Preview.Fill = new SolidColorBrush(Color.FromRgb(198, 201, 212));
        }

        private void BgbtnGray_Click(object sender, RoutedEventArgs e)
        {
            Preview.Fill = new SolidColorBrush(Color.FromRgb(232, 232, 232));
        }

        private void BtnSwitchColors_Click(object sender, RoutedEventArgs e)
        {
            (rectSecondaryColor.Fill, rectCurrentColor.Fill) = (rectCurrentColor.Fill, rectSecondaryColor.Fill);
            currentColor = new PixelColor(((SolidColorBrush)rectCurrentColor.Fill).Color);
        }
    }
}