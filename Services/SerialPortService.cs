using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Linq;

namespace SimuladorASTMv2.Services
{
    public class SerialPortService
    {
        private SerialPort _serialPort;

        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public List<string> GetAvailablePorts()
{
    var ports = new List<string>();

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        ports.AddRange(SerialPort.GetPortNames());
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        // Buscar en /dev/
        try
        {
            if (System.IO.Directory.Exists("/dev/"))
            {
                var devPorts = System.IO.Directory.GetFiles("/dev/", "ttyS*")
                    .Concat(System.IO.Directory.GetFiles("/dev/", "ttyUSB*"));
                ports.AddRange(devPorts);
            }
        }
        catch { }

        // Buscar en /dev/pts/
        try
        {
            if (System.IO.Directory.Exists("/dev/pts/"))
            {
                var ptsPorts = System.IO.Directory.GetFiles("/dev/pts/", "*");
                ports.AddRange(ptsPorts);
            }
        }
        catch { }
    }

    return ports.Any() ? ports : new List<string> { "No hay puertos" };
}

        public void Connect(string portName)
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                    Disconnect();

                _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                _serialPort.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al conectar: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al desconectar: {ex.Message}");
            }
        }

        public void SendData(string data)
        {
            if (!IsConnected)
                throw new Exception("No estás conectado a ningún puerto");

            try
            {
                _serialPort.Write(data + "\r\n");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar: {ex.Message}");
            }
        }
    }
}