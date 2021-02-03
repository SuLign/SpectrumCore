using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace SpectrumCore
{
    public class Spectrum : FrameworkElement
    {
        WriteableBitmap bitmap;
        private System.Drawing.Color backGround = System.Drawing.Color.Transparent;

        public System.Drawing.Color BackGround { get => backGround; set => backGround = value; }

        /// <summary>
        /// Create a instance of core.
        /// </summary>
        public Spectrum()
        {
            this.bitmap = new WriteableBitmap(1260, 410, 96, 96, PixelFormats.Pbgra32, null);

            this.bitmap.Lock();

            using (Bitmap backBufferBitmap = new Bitmap(bitmap.PixelWidth, bitmap.PixelHeight, this.bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, this.bitmap.BackBuffer))
            {
                using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                {
                    backBufferGraphics.Clear(BackGround);
                    backBufferGraphics.Flush();
                }
            }
            this.bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            this.bitmap.Unlock();
        }

        /// <summary>
        /// Redraw the background of board.
        /// </summary>
        /// <param name="NewActualSize"></param>
        /// <param name="Background"></param>
        public void RefreshBoard(System.Drawing.Size NewActualSize, System.Drawing.Color Background)
        {
            bitmap = new WriteableBitmap(NewActualSize.Width, NewActualSize.Height, 96, 96, PixelFormats.Pbgra32, null);
            bitmap.Lock();
            using (Bitmap backBufferBitmap = new Bitmap(NewActualSize.Width, NewActualSize.Height, bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, this.bitmap.BackBuffer))
            {
                using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                {
                    backBufferGraphics.Clear(Background);
                    backBufferGraphics.Flush();
                }
            }
            bitmap.AddDirtyRect(new Int32Rect(0, 0, NewActualSize.Width, NewActualSize.Height));
            bitmap.Unlock();
        }

        /// <summary>
        /// Draw pixels on the board.
        /// </summary>
        /// <param name="drawingAction"></param>
        public void DrawOnMap(Action<Graphics> drawingAction)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                bitmap.Lock();
                using (Bitmap backBufferBitmap = new Bitmap(bitmap.PixelWidth, bitmap.PixelHeight, this.bitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, this.bitmap.BackBuffer))
                {
                    using (Graphics backBufferGraphics = Graphics.FromImage(backBufferBitmap))
                    {
                        backBufferGraphics.Clear(BackGround);
                        drawingAction(backBufferGraphics);
                        backBufferGraphics.Flush();
                    }
                }
                this.bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            this.bitmap = new WriteableBitmap((int)RenderSize.Width, (int)RenderSize.Height, 96, 96, PixelFormats.Pbgra32, null);
            drawingContext.PushTransform(new ScaleTransform(1, -1, 0, RenderSize.Height / 2));
            drawingContext.DrawImage(bitmap, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
        }
    }
}
