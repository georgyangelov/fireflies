using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Fireflies.Transport {
    class KeyboardController : IDisposable {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ConnectedDevice {
            public ushort vendorId;
            public ushort productId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
            public string serialNumber;
            public uint serialNumberSize;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
            public string instance;
            public uint instanceSize;

            public ushort ledInfoFlag;
            public ushort totalNumLeds;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1000)]
            public string friendlyName;
            public uint friendlyNameSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeColor {
            public byte red;
            public byte green;
            public byte blue;
            public byte alpha;
        }

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create_controller();

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint controller_get_connected_devices(
            IntPtr self, 
            [In, Out] ConnectedDevice[] connectedDevicesArray,
            int connectedDevicesArrayCapacity
        );

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_open_device(IntPtr self, ref ConnectedDevice device);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_close_device(IntPtr self);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int controller_get_pixel_count(IntPtr self);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_fill_pixel_buffer(IntPtr self, [In] NativeColor[] pixels, int pixelCount);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_destroy(IntPtr self);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_send_pixels(IntPtr self);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_enable_timer_sending(IntPtr self);

        [DllImport("KeyboardController.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void controller_disable_timer_sending(IntPtr self);

        private IntPtr handle = IntPtr.Zero;
        private bool deviceHasBeenOpen = false, timerHasBeenEnabled = false;
        private NativeColor[] colorBuffer = null;

        public KeyboardController() {
            handle = create_controller();

            if (handle == IntPtr.Zero) {
                throw new Exception("Cannot initialize native keyboard controller");
            }
        }

        public List<ConnectedDevice> connectedDevices() {
            int capacity = 100;
            ConnectedDevice[] devices = new ConnectedDevice[capacity];

            uint length = controller_get_connected_devices(handle, devices, capacity);

            var result = new List<ConnectedDevice>();

            for (ulong i = 0; i < length; i++) {
                result.Add(devices[i]);
            }

            return result;
        }

        public void openDevice(ConnectedDevice device) {
            deviceHasBeenOpen = true;
            controller_open_device(handle, ref device);

            colorBuffer = new NativeColor[getPixelCount()];
        }

        public void closeDevice() {
            controller_close_device(handle);
            deviceHasBeenOpen = false;
        }

        public int getPixelCount() {
            return controller_get_pixel_count(handle);
        }

        public void setPixels(Color[] colors, int offset, int length) {
            for (int i = 0; i < length; i++) {
                Color color = colors[i + offset];
                colorBuffer[i] = new NativeColor { red = color.R, green = color.G, blue = color.B, alpha = color.A };
            }

            controller_fill_pixel_buffer(handle, colorBuffer, length - offset);
        }

        public void updatePixels() {
            controller_send_pixels(handle);
        }

        public void enableTimerSending() {
            timerHasBeenEnabled = true;
            controller_enable_timer_sending(handle);
        }

        public void disableTimerSending() {
            controller_disable_timer_sending(handle);
            timerHasBeenEnabled = false;
        }

        void IDisposable.Dispose() {
            if (deviceHasBeenOpen) {
                closeDevice();
            }

            if (timerHasBeenEnabled) {
                disableTimerSending();
            }

            controller_destroy(handle);
            handle = IntPtr.Zero;
        }
    }
}
