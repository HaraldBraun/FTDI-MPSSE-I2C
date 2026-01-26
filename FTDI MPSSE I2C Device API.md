
# FTDI_MPSSE_I2C – API Dokumentation

## Klasse: `MpsseI2cDevice`

Wrapper für die FTDI libMPSSE‑I2C Funktionen.  
Ermöglicht I²C‑Lesen/Schreiben, GPIO‑Zugriffe und Abfrage der Bibliotheksversionen.

---

## Eigenschaften

### `ChannelIndex : uint`

Index des geöffneten FTDI‑MPSSE‑Kanals (0‑basiert).

### `ClockRateHz : uint`

I²C‑Bustakt in Hertz (z. B. 100000 für 100 kHz).

### `LatencyTimer : byte`

USB‑Latenz‑Timer des FTDI‑Geräts (2–255 ms).

### `IsOpen : bool`

Gibt an, ob ein nativer Kanal geöffnet ist.

---

## Konstruktor

### `MpsseI2cDevice(uint channelIndex, uint clockRateHz = 100000, byte latencyTimer = 2)`

Öffnet und initialisiert den angegebenen I²C‑MPSSE‑Kanal.

**Parameter:**

- `channelIndex` – Welcher FTDI‑MPSSE‑Kanal genutzt wird  
- `clockRateHz` – I²C‑Takt (Standard: 100 kHz)  
- `latencyTimer` – FTDI‑USB Latenz (üblich: 2 ms)  

**Exceptions:**  
Wirft eine `Exception` bei `FT_STATUS != FT_OK`.

---

## Methoden (alphabetisch sortiert)

### `Dispose() : void`

Gibt den MPSSE‑Kanal frei und schließt den nativen Handle.

**Hinweise:**

- Nach Aufruf sind keine weiteren I²C‑Operationen möglich.
- In LabVIEW/TestStand unbedingt im Cleanup‑Bereich aufrufen.

---

### `GetLibraryVersions() : (uint mpsse, uint ftdi)`

Liefert die Versionen der nativen Bibliotheken libMPSSE und FTD2XX.

**Rückgabe:**

- `mpsse` – libMPSSE DLL Version
- `ftdi` – FTD2XX DLL Version

---

### `Read(byte sevenBitAddress, uint length, bool stop = true) : byte[]`

Liest Bytes von einem I²C‑Slave.

**Parameter:**

- `sevenBitAddress` – 7‑Bit I²C‑Adresse (0x00–0x7F)  
- `length` – Anzahl zu lesender Bytes  
- `stop` – STOP‑Bedingung am Ende senden (Standard = true)

**Rückgabe:** Byte‑Array der gelesenen Daten.

**Besonderheiten:**

- Der Wrapper verschiebt die Adresse intern (7‑Bit → 8‑Bit "addr<<1").

- Das letzte Byte wird per NACK bestätigt, wie bei vielen I²C‑Slaves gefordert.
- Wird `stop = false` gesetzt, erzeugt die Methode einen Repeated‑START.

---

### `ReadGpio() : byte`

Liest den Zustand der 8 GPIO‑Leitungen (High‑Byte des MPSSE‑Kanals).

**Rückgabe:** Bitmaske der Pins (`1 = High`).

---

### `Write(byte sevenBitAddress, byte[] data, bool stop = true) : void`

Schreibt Daten an einen I²C‑Slave.

**Parameter:**

- `sevenBitAddress` – 7‑Bit I²C‑Adresse  
- `data` – Zu sendende Bytes  
- `stop` – STOP‑Bedingung setzen

**Besonderheiten:**

- Für Repeated‑START `stop = false` setzen.
- Adressierung erfolgt intern über `(addr << 1)`.

---

### `WriteGpio(byte directionMask, byte valueMask) : void`

Schreibt Richtung und Zustand der 8 MPSSE‑GPIO‑Pins.

**Parameter:**

- `directionMask` – 1 = Output, 0 = Input  
- `valueMask` – Ausgangszustand für Output‑Bits (1 = High)

---

## Typische Anwendungsbeispiele

### 1) Write‑Then‑Read (Repeated‑START)

```csharp
using (var dev = new MpsseI2cDevice(0, 100_000))
{
    byte addr = 0x50;          // 7-Bit
    byte regHi = 0x00, regLo = 0x10;

    dev.Write(addr, new[]{ regHi, regLo }, stop: false);
    byte[] data = dev.Read(addr, 16, stop: true);
}
```

### 2) Nur Schreiben

```csharp
dev.Write(0x1E, new byte[]{ 0x20, 0x0F }, stop: true);
```

### 3) GPIO lesen

```csharp
dev.WriteGpio(directionMask: 0x00, valueMask: 0x00); // alle auf Input
byte pins = dev.ReadGpio();
```

### 4) Versionen ermitteln

```csharp
var (mpsse, ftdi) = dev.GetLibraryVersions();
```

## Hinweise

- **Prozessarchitektur:** 32-bit **( x86 )** (TestStand 32‑bit, LabVIEW 32‑bit).
- **DLL-Ablage:** libmpsse.dll und ftd2xx.dll müssen im **gleichen Ordner** liegen wie FTDI_MPSSE_I2C.dll.
- **Error Handling:** Bei Fehlern wirft jede Methode eine Exception mit FT_STATUS Hexcode.
