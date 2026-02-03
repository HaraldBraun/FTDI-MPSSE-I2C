using System;
using System.Collections.Generic;
using System.Threading;

namespace FTDI_MPSSE_I2C {
    public class I2cEepromDevice {
        private readonly MpsseI2cDevice _bus;
        private readonly byte _addr;
        
        /// <summary>
        /// Creates a generic I²C-EEPROM-Device based on existing I²C-bus-handles.
        /// The class encapsulates EEPROM-typical access (address header, page-splitting, write-cycle
        /// </summary>
        /// <param name="bus">Reference to instance of existing I²C bus (<see cref="MpsseI2cDevice"/>), 
        /// which opens the libMPSSE-channel with configuration</param>
        /// <param name="sevenBitAddress">7-bit I²C-address of EEPROM-chip</param>
        /// <param name="capacityBytes">Overall capacity of the EEPROM-chip in bytes. Needed for address validation</param>
        /// <param name="pageSize">Page-size in bytes (e.g. 32). Writing processes are not allowed to exceed page borders;
        /// the class divides the data automatically to page-conform blocks</param>
        /// <param name="writeCycleDelay">Time needed for one write-cycle</param>
        /// <remarks>This class internally uses the public API of <see cref="MpsseI2cDevice"/> and does not use direkt access of P/Invoke </remarks>
        public I2cEepromDevice(
            MpsseI2cDevice bus,
            byte sevenBitAddress ) {
            _bus = bus ?? throw new ArgumentNullException( nameof( bus ) );
            _addr = sevenBitAddress;
        }

        /// <summary>
        /// Write data to a 16-bit memory address
        /// ATTENTION: Page-spliting is not covered in this implementation
        /// </summary>
        /// <param name="memAddress"></param>
        /// <param name="data"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void WriteEeprom( ushort memAddress, byte[] data, int writeCycleDelayMs ) {
            if (data == null) {
                throw new ArgumentNullException( nameof( data ) );
            }

            // Split 16-bit address in High/Low
            byte high = (byte)(memAddress >> 8);
            byte low = (byte)(memAddress & 0xFF);

            // Finale Frame: [HighAddr][LowAddr][Payload...]
            var frame = new byte[data.Length + 2];
            frame[0] = high;
            frame[1] = low;
            Array.Copy( data, 0, frame, 2, data.Length );

            // Write via MpsseI2cDevice (over libmpsse I2C_DeviceWrite)
            // Set STOP-Bit
            _bus.Write( _addr, frame, stop: true );

            // Wait Write-Cycle
            Thread.Sleep( writeCycleDelayMs );
        }

        /// <summary>
        /// Reads a defined number of bytes, starting at 16-bit memory address
        /// </summary>
        /// <param name="memAddress">Start address (16-bit) inside EEPROM</param>
        /// <param name="length">Number of bytes to read</param>
        /// <returns>Read bytes as array</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] ReadEeprom( ushort memAddress, int length ) {
            if (length < 0) throw new ArgumentOutOfRangeException( nameof( length ) );
            if (length == 0) return Array.Empty<byte>( );

            // Split 16-bit address
            byte high = (byte)(memAddress >> 8);
            byte low = (byte)((memAddress & 0xFF) );

            _bus.Write( _addr, new[] { high, low }, stop: false ); // Prepare Repeated-Start

            // Start reading immediately after write
            var data = _bus.Read(_addr, (uint)length);

            return data;
        }

        // --- Helper Methods ---
        /// <summary>
        /// Checks for capacity overflow (start + length)
        /// </summary>
        /// <param name="start">Start address for read/write</param>
        /// <param name="length">Number of page size in byte. Example 32</param>
        /// <param name="capacityBytes">Memory size from microcontroller. 64kBit = 8KByte = 8192</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ValidateRange( ushort start, int length, int capacityBytes ) {
            if (length < 0) throw new ArgumentOutOfRangeException( nameof( length ) );

            uint end = (uint)start + (uint)length; // cast to uint to prevent overflow

            if (end > (uint) capacityBytes)
                throw new ArgumentOutOfRangeException( nameof( length ),
                    $"Bereich 0x{start:X4}..0x{end - 1:X4} überschreitet Kapazität ({capacityBytes} Bytes)." );
        }
    }
}
