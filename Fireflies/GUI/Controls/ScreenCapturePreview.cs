using Fireflies.Core;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fireflies.Controls {
    public class ScreenCapturePreview : FrameworkElement {
        class SharedBitmapSource : BitmapSource, IDisposable {
            public Bitmap Bitmap { get; private set; }

            public override double DpiX { get { return Bitmap.HorizontalResolution; } }
            public override double DpiY { get { return Bitmap.VerticalResolution; } }
        
            public override int PixelHeight { get { return Bitmap.Height; } }
            public override int PixelWidth { get { return Bitmap.Width; } }

            public override System.Windows.Media.PixelFormat Format { get { return ConvertPixelFormat(Bitmap.PixelFormat); } }
            public override BitmapPalette Palette { get { return null; } }
            
            public SharedBitmapSource(int width, int height, System.Drawing.Imaging.PixelFormat sourceFormat)
                : this(new Bitmap(width, height, sourceFormat)) { }

            public SharedBitmapSource(Bitmap bitmap) {
                Bitmap = bitmap;
            }
            
            ~SharedBitmapSource() {
                Dispose(false);
            }

            public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset) {
                BitmapData sourceData = Bitmap.LockBits(
                    new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height),
                    ImageLockMode.ReadOnly,
                    Bitmap.PixelFormat
                );

                var length = sourceData.Stride * sourceData.Height;

                if (pixels is byte[]) {
                    var bytes = pixels as byte[];
                    Marshal.Copy(sourceData.Scan0, bytes, 0, length);
                }

                Bitmap.UnlockBits(sourceData);
            }

            protected override Freezable CreateInstanceCore() {
                return (Freezable)Activator.CreateInstance(GetType());
            }

            public BitmapSource Resize(int newWidth, int newHeight) {
                System.Drawing.Image newImage = new Bitmap(newWidth, newHeight);

                using (Graphics graphicsHandle = Graphics.FromImage(newImage)) {
                    graphicsHandle.InterpolationMode = InterpolationMode.NearestNeighbor;
                    graphicsHandle.DrawImage(Bitmap, 0, 0, newWidth, newHeight);
                }

                return new SharedBitmapSource(newImage as Bitmap);
            }

            public new BitmapSource Clone() {
                return new SharedBitmapSource(new Bitmap(Bitmap));
            }
            
            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat) {
                switch (sourceFormat) {
                    case System.Drawing.Imaging.PixelFormat.Format24bppRgb:  return PixelFormats.Bgr24;
                    case System.Drawing.Imaging.PixelFormat.Format32bppArgb: return PixelFormats.Pbgra32;
                    case System.Drawing.Imaging.PixelFormat.Format32bppRgb:  return PixelFormats.Bgr32;
                }

                return new System.Windows.Media.PixelFormat();
            }

            private bool _disposed = false;

            protected virtual void Dispose(bool disposing) {
                if (!_disposed) {
                    if (disposing) {
                        // Free other state (managed objects).
                    }

                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    _disposed = true;
                }
            }
        }

        private DrawingVisual visual = new DrawingVisual();
        
        public IFrameSource FrameSource { get; set; }

        public ScreenCapturePreview() {
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        protected override int VisualChildrenCount {
            get { return 1; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index) {
            if (index != 0) {
                throw new ArgumentOutOfRangeException();
            }

            return visual;
        }
        
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource createBitmapSource(Bitmap source) {
            IntPtr bitmapDataPtr = source.GetHbitmap();
            BitmapSource bitmapSource = null;

            try {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bitmapDataPtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()
                );
            } finally {
                DeleteObject(bitmapDataPtr);
            }
        
            return bitmapSource;
        }
        
        public void Update(Bitmap bitmap) {
            var context = visual.RenderOpen();

            context.DrawImage(new SharedBitmapSource(bitmap), new Rect(0, 0, ActualWidth, ActualHeight));

            context.Close();
        }
    }
}
