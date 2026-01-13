using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using SimuladorASTMv2.Services;

namespace SimuladorASTMv2.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string _selectedPort = "";
        private string _astmFrame = "";
        private string _statusMessage = "Desconectado";
        private ObservableCollection<string> _availablePorts = new();
        private ObservableCollection<string> _sendLog = new();

        private SerialPortService _serialPortService = new();

        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set => this.RaiseAndSetIfChanged(ref _availablePorts, value);
        }

        public string SelectedPort
        {
            get => _selectedPort;
            set => this.RaiseAndSetIfChanged(ref _selectedPort, value);
        }

        public string AstmFrame
        {
            get => _astmFrame;
            set => this.RaiseAndSetIfChanged(ref _astmFrame, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ObservableCollection<string> SendLog
        {
            get => _sendLog;
            set => this.RaiseAndSetIfChanged(ref _sendLog, value);
        }

        public ReactiveCommand<Unit, Unit> ConnectCommand { get; }
        public ReactiveCommand<Unit, Unit> DisconnectCommand { get; }
        public ReactiveCommand<Unit, Unit> SendFrameCommand { get; }

        public MainWindowViewModel()
        {
            ConnectCommand = ReactiveCommand.Create(Connect);
            DisconnectCommand = ReactiveCommand.Create(Disconnect);
            SendFrameCommand = ReactiveCommand.Create(SendFrame);

            RefreshPorts();
            AddLog("Aplicación iniciada");
        }

        private void RefreshPorts()
        {
            AvailablePorts.Clear();
            var ports = _serialPortService.GetAvailablePorts();
            foreach (var port in ports)
            {
                AvailablePorts.Add(port);
            }

            if (AvailablePorts.Count > 0)
                SelectedPort = AvailablePorts[0];

            AddLog($"Puertos disponibles: {string.Join(", ", ports)}");
        }

        private void Connect()
        {
            if (string.IsNullOrEmpty(SelectedPort))
            {
                StatusMessage = "⚠️ Selecciona un puerto";
                AddLog("Error: No se seleccionó puerto");
                return;
            }

            try
            {
                _serialPortService.Connect(SelectedPort);
                StatusMessage = $"✅ Conectado a {SelectedPort}";
                AddLog($"✅ Conectado a {SelectedPort}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ {ex.Message}";
                AddLog($"❌ Error de conexión: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            try
            {
                _serialPortService.Disconnect();
                StatusMessage = "Desconectado";
                AddLog("Desconectado");
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ {ex.Message}";
                AddLog($"❌ Error: {ex.Message}");
            }
        }

        private void SendFrame()
        {
            if (string.IsNullOrEmpty(AstmFrame))
            {
                StatusMessage = "⚠️ Ingresa una trama";
                AddLog("Error: Trama vacía");
                return;
            }

            try
            {
                _serialPortService.SendData(AstmFrame);
                StatusMessage = "📤 Trama enviada";
                AddLog($"📤 Enviado: {AstmFrame}");
                AstmFrame = "";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ {ex.Message}";
                AddLog($"❌ Error al enviar: {ex.Message}");
            }
        }

        private void AddLog(string message)
        {
            SendLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}