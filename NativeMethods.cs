using System;
using System.Runtime.InteropServices;

namespace FTDI_MPSSE_I2C {
    internal static class NativeMethods {
        private const string DLL ="libmpsse.dll";

        // Typdefinition aus ftd2xx.h
        internal const int FT_OK = 0;

        // FT_HANDLE = void*
        internal struct FT_HANDLE {
            public IntPtr Handle;
        }

        // ChannelConfig aus libmpsse_i2c.h
        [StructLayout( LayoutKind.Sequential )]
        internal struct ChannelConfig {
            public uint ClockRate;      // I2C_CLOCKRATE enum (als uint)
            public byte LatencyTimer;   // UCHAR
            public uint Options;        // DWORD
            public uint Pin;            // DWORD
            public ushort CurrentPinState;
        }

        // Transfer Options - direkte Übernahme aus Header
        [Flags]
        internal enum TransferOptions: uint {
            START_BIT           = 0x00000001,
            STOP_BIT            = 0x00000002,
            BREAK_ON_NACK       = 0x00000004,
            NACK_LAST_BYTE      = 0x00000008,
            FAST_TRANSFER       = 0x00000030, // intern
            FAST_TRANSFER_BYTES = 0x00000010,
            FAST_TRANSFER_BITS  = 0x00000020,
            NO_ADDRESS          = 0x00000040
        }

        // Grundsätzlich alle I2C-Funktionen aus libmpsse_i2c.h
        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern void Init_libMPSSE( );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern void Cleanup_libMPSSE( );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_GetNumChannels( out uint numChannels );

        // FT_DEVICE_LIST_INFO_NODE ist in ftd2xx.h definiert
        // Für I2C_GetChannelInfo wird es als Pointer verwendet
        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi )]
        internal struct FT_DEVICE_LIST_INFO_NODE {
            public uint Flags;
            public uint Type;
            public uint ID;
            public uint LocId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string SerialNumber;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string Description;

            public IntPtr ftHandle;
        }

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_GetChannelInfo(
            uint index,
            out FT_DEVICE_LIST_INFO_NODE chanInfo );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_OpenChannel(
            uint index,
            out IntPtr handle );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_InitChannel(
            IntPtr handle,
            ref ChannelConfig config );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_CloseChannel(
            IntPtr handle );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_DeviceRead(
            IntPtr handle,
            byte deviceAddress,
            uint sizeToTransfer,
            byte[] buffer,
            out uint sizeTransferred,
            uint options );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_DeviceWrite(
            IntPtr handle,
            byte deviceAddress,
            uint sizeToTransfer,
            byte[] buffer,
            out uint sizeTransferred,
            uint options );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint I2C_GetDeviceID(
            IntPtr handle,
            byte deviceAddress,
            byte[] deviceID );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint FT_WriteGPIO(
            IntPtr handle,
            byte directionMask,
            byte valueMask );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint FT_ReadGPIO(
            IntPtr handle,
            out byte valueMask );

        [DllImport( DLL, CallingConvention = CallingConvention.StdCall )]
        internal static extern uint Ver_libMPSSE(
            out uint libmpsseVer,
            out uint ftd2xxVer );
    }
}
