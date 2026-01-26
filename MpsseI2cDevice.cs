using System;

namespace FTDI_MPSSE_I2C {
    public class MpsseI2cDevice: IDisposable {
        private IntPtr _handle;
        private bool _disposed;

        public uint ChannelIndex { get; }
        public uint ClockRateHz { get; }
        public byte LatencyTimer { get; }

        public bool IsOpen => _handle != IntPtr.Zero;

        public MpsseI2cDevice( uint channelIndex, uint clockRateHz = 100_000, byte latencyTimer = 2 ) {
            ChannelIndex = channelIndex;
            ClockRateHz = clockRateHz;
            LatencyTimer = latencyTimer;

            // Öffnen
            CheckStatus(
                NativeMethods.I2C_OpenChannel( channelIndex, out _handle ),
                nameof( NativeMethods.I2C_OpenChannel ) );

            // Initialisierung
            var cfg = new NativeMethods.ChannelConfig
            {
                ClockRate = ClockRateHz,
                LatencyTimer = LatencyTimer,
                Options = 0,
                Pin = 0,
                CurrentPinState = 0
            };

            CheckStatus( NativeMethods.I2C_InitChannel( _handle, ref cfg ),
                nameof( NativeMethods.I2C_InitChannel ) );
        }

        // Schreibvorgang (WRITE)
        public void Write( byte sevenBitAddress, byte[] data, bool stop = true ) {
            if (data == null) throw new ArgumentNullException( nameof( data ) );

            EnsureNotDisposed( );
            uint options = (uint)NativeMethods.TransferOptions.START_BIT;
            if (stop)
                options |= (uint) NativeMethods.TransferOptions.STOP_BIT;

            CheckStatus(
                NativeMethods.I2C_DeviceWrite(
                    _handle,
                    (byte) (sevenBitAddress << 1),
                    (uint) data.Length,
                    data,
                    out var written,
                    options ),
                nameof( NativeMethods.I2C_DeviceWrite ) );

            if (written != data.Length)
                throw new Exception( $"Kurzschreiben: {written}/{data.Length} Bytes." );
        }

        // Lesevorgang (READ)
        public byte[] Read( byte sevenBitAddress, uint length, bool stop = true ) {
            EnsureNotDisposed( );

            byte[] buffer = new byte[length];

            uint options = (uint)(NativeMethods.TransferOptions.START_BIT
                | NativeMethods.TransferOptions.NACK_LAST_BYTE);

            if (stop)
                options |= (uint) NativeMethods.TransferOptions.STOP_BIT;

            CheckStatus(
                NativeMethods.I2C_DeviceRead(
                    _handle,
                    (byte) (sevenBitAddress << 1),
                    length,
                    buffer,
                    out var read,
                    options ),
                nameof( NativeMethods.I2C_DeviceRead ) );

            if (read != length) {
                Array.Resize( ref buffer, (int) read );
            }

            return buffer;
        }

        // GPIO setzen
        public void WriteGpio( byte directionMask, byte valueMask ) {
            EnsureNotDisposed( );
            CheckStatus( NativeMethods.FT_WriteGPIO( _handle, directionMask, valueMask ),
                nameof( NativeMethods.FT_WriteGPIO ) );
        }

        // GPIO lesen
        public byte ReadGpio( ) {
            EnsureNotDisposed( );
            CheckStatus( NativeMethods.FT_ReadGPIO( _handle, out byte value ),
                nameof( NativeMethods.FT_ReadGPIO ) );

            return value;
        }

        // Versionen anzeigen
        public (uint mpsse, uint ftdi) GetLibraryVersions( ) {
            CheckStatus( NativeMethods.Ver_libMPSSE( out uint mpsse, out uint ftdi ),
                nameof( NativeMethods.Ver_libMPSSE ) );

            return (mpsse, ftdi);
        }

        // Statusergebnis prüfen
        private static void CheckStatus( uint status, string api ) {
            if (status != NativeMethods.FT_OK)
                throw new Exception( $"{api} failed: FT_STATUS = 0x{status:X8}" );
        }

        private void EnsureNotDisposed( ) {
            if (_disposed || _handle != IntPtr.Zero)
                throw new ObjectDisposedException( nameof( MpsseI2cDevice ) );
        }

        // Cleanup
        public void Dispose( ) {
            if (_disposed)
                return;

            if (_handle != IntPtr.Zero) {
                NativeMethods.I2C_CloseChannel( _handle );
                _handle = IntPtr.Zero;
            }

            _disposed = true;
            GC.SuppressFinalize( this );
        }

        ~MpsseI2cDevice( ) {
            Dispose( );
        }
    }
}
