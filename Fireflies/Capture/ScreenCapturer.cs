using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using System.Drawing.Imaging;
using SharpDX;
using System.IO;
using System.Runtime.InteropServices;

namespace Fireflies.Capture {
    public class ScreenCapturer : IDisposable {
        private Object swapLock = new Object();
        private byte[][] buffer = new byte[3][];

        private int adapterIndex, outputIndex;

        private Adapter1 gpu;
        private Device device;
        private Output5 output;
        private Texture2D stagingTexture;

        private bool frameReady = false;

        public byte[] CurrentFrame {
            get => buffer[2];
        }

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public ScreenCapturer(int adapterIndex, int outputIndex) {
            this.adapterIndex = adapterIndex;
            this.outputIndex = outputIndex;

            initializeCapture();
        }

        private void SwapBuffers(int i, int j) {
            byte[] tmp = buffer[i];

            buffer[i] = buffer[j];
            buffer[j] = tmp;
        }

        private void frameRendered() {
            lock (swapLock) {
                SwapBuffers(0, 1);
                frameReady = true;
            }
        }

        public bool NextFrame() {
            if (frameReady) {
                lock (swapLock) {
                    SwapBuffers(1, 2);
                    frameReady = false;
                }

                return true;
            }

            return false;
        }

        private void initializeCapture() {
            using (var factory = new Factory1()) {
                gpu = factory.GetAdapter1(adapterIndex);
            }

            device = new Device(gpu);
            output = gpu.GetOutput(outputIndex).QueryInterface<Output5>();

            var desktopBounds = output.Description.DesktopBounds;

            ScreenWidth = desktopBounds.Right - desktopBounds.Left;
            ScreenHeight = desktopBounds.Bottom - desktopBounds.Top;

            buffer[0] = new byte[ScreenWidth * ScreenHeight * 4];
            buffer[1] = new byte[ScreenWidth * ScreenHeight * 4];
            buffer[2] = new byte[ScreenWidth * ScreenHeight * 4];

            // A texture that can be accessed by the CPU (by copying from the GPU memory)
            // https://msdn.microsoft.com/en-us/library/windows/desktop/bb205132(v=vs.85).aspx#Accessing
            // - The GPU cannot read or write to staging resources directly.
            // - The CPU can only read from resources created with the D3D10_USAGE_STAGING flag.
            // - Applications that wish to copy data from a resource with default usage to a
            //   resource with staging usage (to allow the CPU to read the data --
            //   i.e. the GPU readback problem) must do so with care.
            stagingTexture = new Texture2D(device, new Texture2DDescription {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = ScreenWidth,
                Height = ScreenHeight,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            });
        }

        public void Dispose() {
            stagingTexture.Dispose();
            output.Dispose();
            device.Dispose();
            gpu.Dispose();
        }

        private void CaptureFrame(OutputDuplication outputDuplication) {
            SharpDX.DXGI.Resource screenResource;
            OutputDuplicateFrameInformation frameInformation;

            // Try to get duplicated frame within given time
            // https://msdn.microsoft.com/en-us/library/windows/desktop/hh404615(v=vs.85).aspx
            outputDuplication.AcquireNextFrame(1000, out frameInformation, out screenResource);

            try {
                if (frameInformation.TotalMetadataBufferSize == 0) {
                    return;
                }

                // Ignore this frame if only the mouse position changed
                if (frameInformation.LastPresentTime == 0) {
                    return;
                }

                // Copy the screen texture to the staging texture
                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    device.ImmediateContext.CopyResource(screenTexture2D, stagingTexture);

                // Each texture has multiple subresources (detail levels?).
                //
                // The worst-case scenario for GPU/CPU parallelism is the need to force one processor to wait for the results of work done by another.
                // Direct3D 10 tries to remove this cost by making the ID3D10Device::CopyResource and ID3D10Device::CopySubresourceRegion methods 
                // asynchronous; the copy has not necessarily executed by the time the method returns. The benefit of this is that the application 
                // does not pay the performance cost of actually copying the data until the CPU accesses the data, which is when Map is called. 
                // If the Map method is called after the data has actually been copied, no performance loss occurs. On the other hand, 
                // if the Map method is called before the data has been copied, then a pipeline stall will occur.
                //
                // So if an application wants to map a resource that originates in video memory and calls ID3D10Device::CopyResource 
                // or ID3D10Device::CopySubresourceRegion at frame N, this call will actually begin to execute at frame N+1, 
                // when the application is submitting calls for the next frame. The copy should be finished when the application is processing frame N+2.
                var mapSource = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

                try {
                    // Copy pixels from screen capture Texture to GDI bitmap
                    var bitmap = buffer[0];
                    var sourcePtr = mapSource.DataPointer;

                    // for (int y = 0; y < ScreenHeight; y++) {
                    //     Marshal.Copy(sourcePtr, bitmap, 0, ScreenWidth * 4);
                    //         
                    //     sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    // }
                    Marshal.Copy(sourcePtr, bitmap, 0, ScreenWidth * ScreenHeight * 4);

                    frameRendered();
                } finally {
                    device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
                }
            } finally {
                screenResource.Dispose();
                outputDuplication.ReleaseFrame();
            }
        }

        public void Capture() {
            using (var duplication = output.DuplicateOutput(device)) {
                while (true) {
                    try {
                        CaptureFrame(duplication);
                    } catch (SharpDXException e) {
                        if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code) {
                            throw e;
                        }
                    }
                }
            }

            // currentFrame.Save("test.bmp");

            // Display the texture using system associated viewer
            // System.Diagnostics.Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "test.bmp")));
        }
    }
}
