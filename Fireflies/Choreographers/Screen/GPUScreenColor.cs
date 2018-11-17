using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Fireflies.Core;
using Fireflies.Capture;

using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using SharpDX;

namespace Fireflies.Orchestrators {
    public class GPUScreenColor : IChoreographer {
        private ScreenCapturer screen;

        private int adapterIndex = 0, outputIndex = 0;

        private Adapter1 gpu;
        private Device device;
        private Output5 output;
        private ComputeShader computeShader;

        private Texture2D texture;
        private UnorderedAccessView view;

        public GPUScreenColor(ScreenCapturer screenCapturer) {
            screen = screenCapturer;

            initializeDirectX();
        }

        private void initializeDirectX() {
            using (var factory = new Factory1()) {
                gpu = factory.GetAdapter1(adapterIndex);
            }

            device = new Device(gpu, DeviceCreationFlags.Debug, FeatureLevel.Level_11_1);

            var compilationResult = ShaderBytecode.CompileFromFile("test.hlsl", "main", "cs_5_0", ShaderFlags.Debug);

            if (compilationResult.Bytecode == null) {
                Console.WriteLine(compilationResult.Message);
            }

            computeShader = new ComputeShader(device, compilationResult.Bytecode);

            texture = new Texture2D(device, new Texture2DDescription() {
                // CpuAccessFlags = CpuAccessFlags.Read,
                // BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                BindFlags = BindFlags.UnorderedAccess,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                CpuAccessFlags = CpuAccessFlags.None
            });
            
            UnorderedAccessView view = new UnorderedAccessView(device, texture, new UnorderedAccessViewDescription() {
                Format = Format.R8G8B8A8_UNorm,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = { MipSlice = 0 }
            });


            stagingTexture = new Texture2D(device, new Texture2DDescription {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            });

            // TODO: Export the texture as bitmap and show it in the UI
        }

        void IChoreographer.Update(System.Windows.Media.Color[] leds, int offset, int length, FrameInfo frame) {
            // byte[] frame = screen.CurrentFrame;
            // int width = screen.ScreenWidth;
            // int height = screen.ScreenHeight;

        }

        Texture2D stagingTexture;

        public Bitmap getBitmap() {
            device.ImmediateContext.ComputeShader.Set(computeShader);
            device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, view);
            
            device.ImmediateContext.Dispatch(32, 32, 1);
            device.ImmediateContext.CopyResource(texture, stagingTexture);
            var mapSource = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

            Console.WriteLine(Marshal.ReadInt32(IntPtr.Add(mapSource.DataPointer, 0)));

            try {
                // Copy pixels from screen capture Texture to GDI bitmap
                Bitmap bitmap = new Bitmap(1024, 1024, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                BitmapData mapDest = bitmap.LockBits(new Rectangle(0, 0, 1024, 1024), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                try {
                    var sourcePtr = mapSource.DataPointer;
                    var destPtr = mapDest.Scan0;
                    for (int y = 0; y < 1024; y++) {
                        // Copy a single line
                        Utilities.CopyMemory(destPtr, sourcePtr, 1024 * 4);

                        // Advance pointers
                        sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                        destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                    }

                    return bitmap;
                } finally {
                    bitmap.UnlockBits(mapDest);
                }
            } finally {
                device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
            }
        }
    }
}
