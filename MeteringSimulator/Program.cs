using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MeteringSimulator
{
    /// <summary>
    /// MeteringSimulator is a small console application that simulates field
    /// metering equipment. It connects to the NetworkService (which acts as a TCP
    /// server), asks how many entities are currently registered, and then sends
    /// randomly generated measurements for those entities at random time intervals.
    ///
    /// Protocol (text based, one message per line, terminated by '\n'):
    ///   Simulator -> Service : "Object count?"      (request for number of entities)
    ///   Service   -> Simulator : "&lt;count&gt;"          (integer, e.g. "3")
    ///   Simulator -> Service : "Object_&lt;index&gt;:&lt;value&gt;"  (a new measurement)
    ///
    /// The index corresponds to the position of the entity in the list held by the
    /// NetworkService. The NetworkService restarts this process whenever an entity
    /// is added or removed, so that the object count is re-read.
    /// </summary>
    public static class Program
    {
        private const string Host = "127.0.0.1";
        private const int Port = 55555;

        private static readonly Random Random = new Random();

        public static void Main(string[] args)
        {
            Console.Title = "MeteringSimulator";
            Console.WriteLine("MeteringSimulator started. Connecting to NetworkService...");

            try
            {
                RunSimulation();
            }
            catch (Exception ex)
            {
                // If the service is closed or the connection is lost, simply exit.
                Console.WriteLine("Connection lost / error: " + ex.Message);
            }

            Console.WriteLine("MeteringSimulator stopped.");
        }

        private static void RunSimulation()
        {
            using (TcpClient client = ConnectWithRetry())
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
            {
                int count = RequestObjectCount(reader, writer);
                Console.WriteLine($"NetworkService reports {count} object(s). Starting measurements...");

                while (count > 0)
                {
                    int index = Random.Next(0, count);
                    double value = GenerateValue();
                    string message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Object_{0}:{1:0.00}",
                        index,
                        value);

                    writer.WriteLine(message);
                    Console.WriteLine("Sent -> " + message);

                    // Random interval between measurements (0.8s - 2.5s).
                    Thread.Sleep(Random.Next(800, 2500));
                }
            }
        }

        private static TcpClient ConnectWithRetry()
        {
            while (true)
            {
                try
                {
                    return new TcpClient(Host, Port);
                }
                catch (SocketException)
                {
                    Console.WriteLine("NetworkService not available yet, retrying in 1s...");
                    Thread.Sleep(1000);
                }
            }
        }

        private static int RequestObjectCount(StreamReader reader, StreamWriter writer)
        {
            int count = 0;
            while (count <= 0)
            {
                writer.WriteLine("Object count?");
                string response = reader.ReadLine();
                if (response == null)
                {
                    throw new IOException("Service closed the connection.");
                }

                if (!int.TryParse(response.Trim(), out count) || count <= 0)
                {
                    // No entities yet - wait and ask again.
                    count = 0;
                    Thread.Sleep(2000);
                }
            }

            return count;
        }

        /// <summary>
        /// Generates a measurement value. The range is intentionally wider than the
        /// valid reactor-temperature interval (250-350 C) so that both valid and
        /// invalid (out-of-range) values are produced for demonstration purposes.
        /// </summary>
        private static double GenerateValue()
        {
            // Range 150 - 420; produces a healthy mix of valid and invalid readings.
            return 150.0 + Random.NextDouble() * 270.0;
        }
    }
}
