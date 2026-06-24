using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkService.Models;

namespace NetworkService.Services
{

    public class SimulatorServer
    {
        private const string Host = "127.0.0.1";
        private const int Port = 55555;

        private readonly Func<List<Entity>> _entitiesSnapshotProvider;
        private TcpListener _listener;
        private Thread _listenThread;
        private Process _simulatorProcess;
        private volatile bool _running;

        public event Action<Entity, double, DateTime> MeasurementReceived;

        public SimulatorServer(Func<List<Entity>> entitiesSnapshotProvider)
        {
            _entitiesSnapshotProvider = entitiesSnapshotProvider;
        }

        public void Start()
        {
            if (_running)
            {
                return;
            }

            _running = true;
            _listener = new TcpListener(IPAddress.Parse(Host), Port);
            _listener.Start();

            _listenThread = new Thread(ListenLoop) { IsBackground = true, Name = "SimulatorServerListener" };
            _listenThread.Start();

            LaunchSimulator();
        }

        public void Stop()
        {
            _running = false;

            try { _listener?.Stop(); } catch { /* ignored */ }
            KillSimulator();
        }


        public void RestartSimulator()
        {
            KillSimulator();
            LaunchSimulator();
        }

        private void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();
                    var thread = new Thread(() => HandleClient(client)) { IsBackground = true };
                    thread.Start();
                }
                catch (SocketException)
                {
                    // Listener stopped - exit loop.
                    break;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                {
                    string line;
                    while (_running && (line = reader.ReadLine()) != null)
                    {
                        ProcessMessage(line.Trim(), writer);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void ProcessMessage(string message, StreamWriter writer)
        {
            if (message.Equals("Object count?", StringComparison.OrdinalIgnoreCase))
            {
                int count = _entitiesSnapshotProvider()?.Count ?? 0;
                writer.WriteLine(count.ToString(CultureInfo.InvariantCulture));
                return;
            }


            if (message.StartsWith("Object_", StringComparison.OrdinalIgnoreCase))
            {
                int colon = message.IndexOf(':');
                if (colon < 0)
                {
                    return;
                }

                string indexPart = message.Substring("Object_".Length, colon - "Object_".Length);
                string valuePart = message.Substring(colon + 1);

                if (int.TryParse(indexPart, out int index) &&
                    double.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    List<Entity> snapshot = _entitiesSnapshotProvider();
                    if (snapshot != null && index >= 0 && index < snapshot.Count)
                    {
                        MeasurementReceived?.Invoke(snapshot[index], value, DateTime.Now);
                    }
                }
            }
        }

        private void LaunchSimulator()
        {
            string exePath = ResolveSimulatorPath();
            if (exePath == null || !File.Exists(exePath))
            {
                Debug.WriteLine("MeteringSimulator.exe not found. Start it manually if needed.");
                return;
            }

            try
            {
                _simulatorProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(exePath)
                    }
                };
                _simulatorProcess.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to start MeteringSimulator: " + ex.Message);
            }
        }

        private void KillSimulator()
        {
            try
            {
                if (_simulatorProcess != null && !_simulatorProcess.HasExited)
                {
                    _simulatorProcess.Kill();
                    _simulatorProcess.WaitForExit(2000);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                _simulatorProcess?.Dispose();
                _simulatorProcess = null;
            }
        }


        private static string ResolveSimulatorPath()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 7 && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, "MeteringSimulator");
                if (Directory.Exists(candidate))
                {
                    string exe = Directory
                        .GetFiles(candidate, "MeteringSimulator.exe", SearchOption.AllDirectories)
                        .OrderByDescending(File.GetLastWriteTimeUtc)
                        .FirstOrDefault();
                    if (exe != null)
                    {
                        return exe;
                    }
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}
