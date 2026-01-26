# FTDI‑MPSSE‑I2C

.NET Class Library zur I²C‑Kommunikation über FTDI‑MPSSE‑basierte Geräte
(FT232H, FT2232H, FT4232H, FT2232D, UMFT201XA usw.)
Diese Bibliothek kapselt den Zugriff auf das FTDI‑MPSSE‑Interface über die libMPSSE‑I2C und D2XX‑Treiber und stellt eine moderne, verwaltete C#‑API bereit.
Sie eignet sich besonders für NI TestStand, LabVIEW oder jede .NET‑basierte Testumgebung.

## 🚀 Features

Einfache I²C‑Read/Write‑Funktionen
Unterstützung für Repeated‑START
Vollständiges GPIO‑Interface (8 GPIO‑Pins pro MPSSE‑Kanal)
Zugriff auf libMPSSE‑ und D2XX‑Versionen
Kompatibel mit TestStand 32‑bit (x86‑Build vorgesehen)
Basierend auf FTDI‑D2XX‑API und libMPSSE‑I2C (DLL‑Interop)

## 📂 Projektstruktur

```text
FTDI-MPSSE-I2C/  
 ├─ MpsseI2cDevice.cs        ← Hauptklasse, I2C & GPIO High‑Level‑API  
 ├─ NativeMethods.cs         ← P/Invoke nach libmpsse.dll & ftd2xx.dll  
 ├─ Properties/  
 │    └─ AssemblyInfo.cs  
 ├─ bin/  
 │   ├─ Debug/  
 │   │   ├─ FTDI MPSSE I2C.dll    ← deine .NET‑Library  
 │   │   ├─ libmpsse.dll  
 │   │   └─ ftd2xx.dll  
 │   └─ Release/  
 └─ Documentation/  
     ├─ AN_135_MPSSE_Basics.pdf  
     ├─ AN_177_User_Guide_For_LibMPSSE‑I2C.pdf  
     └─ weitere FTDI‑Dokumente  
```

## 🧩 Voraussetzungen

### Software

- .NET Framework 4.7.2
- Visual Studio 2019/2022
- NI TestStand / NI LabVIEW (optional)

### Native Abhängigkeiten

Folgende DLLs müssen **im selben Ordner wie deine FTDI‑MPSSE‑I2C.dll** liegen:

| **Datei**        | **Zweck**                          |
| ---------------- | ---------------------------------- |
| **ftd2xx.dll**   | FTDI D2XX USB_Treiber              |
| **libmpsse.dll** | High-Level LibMPSSE-I2C-Bibliothek |

## 📦 Installation / Deployment

Für TestStand:

1. Ordner bin/Release in Engine‑Suchpfad kopieren
2. DLL FTDI MPSSE I2C.dll als .NET‑Assembly laden
3. Auf MpsseI2cDevice‑Methoden zugreifen

Für LabVIEW (.NET Nodes):

- .NET Constructor Node → FTDI_MPSSE_I2C.MpsseI2cDevice

## 🧱 Klassen-Überblick

### class MpsseI2cDevice : IDisposable

High‑level Interface, das einen FTDI‑MPSSE‑Kanal als I²C‑Master abstrahiert.

### Eigenschaften

| **Property** | **Typ** | **Beschreibung**                    |
| ------------ | ------- | ----------------------------------- |
| ChannelIndex | uint    | FTDI‑Kanalnummer                    |
| ClockRateHz  | uint    | I²C‑Clockrate (100k, 400k, 1M usw.) |
| LatencyTimer | byte    | USB‑Latency (1–255 ms)              |
| IsOpen       | bool    | True nach erfolgreichem OpenChannel |

## 🔌 I²C‑Funktionen

### Write

```C#
void Write(byte address7Bit, byte[] data, bool stop = true)
```

### Read

```C#
byte[] Read(byte address7Bit, uint length, bool stop = true)
```

### Repeated‑START Beispiel

```C#
using (var dev = new MpsseI2cDevice(0, 400_000))
{    
    // Register 0x10 lesen    
    dev.Write(0x50, new byte[]{ 0x10 }, stop:false);    
    var data = dev.Read(0x50, 1, stop:true);
}
```

## 🖧 GPIO-Funktionen

### WriteGpio

```C#
void WriteGpio(byte directionMask, byte valueMask)
```

### ReadGpio

```C#
byte value = dev.ReadGpio();
```

## 🧪 Beispiel – komplettes I2C‑Register lesen

```C#
using(var i2c = new MpsseI2cDevice(0, 100_000))
{    
    // Register 0x00 lesen    
    i2c.Write(0x50, new byte[]{ 0x00 }, stop:false);    
    var result = i2c.Read(0x50, 16);    
    Console.WriteLine(BitConverter.ToString(result));
}
```

## 🛠️ Fehlerbehandlung

Alle internen D2XX‑Aufrufe prüfen FT_STATUS und werfen Exceptions bei Fehlern:

- Gerät nicht gefunden
- Kanal bereits geöffnet
- I²C‑Slave antwortet nicht (NACK)
- Transferfehler

## 🧹 Dispose / Cleanup
```C#
using (var dev = new MpsseI2cDevice(0))
{    
    // Nutzung…
} // → Kanal wird automatisch geschlossenWeitere Zeilen anzeigen
```

## 📘 Dokumentation
Das Projekt bringt alle relevanten FTDI‑Dokumente mit:

- AN 135 – MPSSE Basics
- AN 177 – LibMPSSE‑I2C User Guide
- D2XX Programmer’s Guide

Alle Dokumente befinden sich unter:  

```text
Documentation/FTDI/
```

## 📄 Lizenz

Dieses Projekt steht unter der **MIT‑Lizenz**  
→ maximale Freiheit für private & kommerzielle Nutzung.

## 👤 Autor

Harald Braun