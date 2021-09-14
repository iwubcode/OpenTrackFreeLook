using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OpenTrackFreeLook
{
    public class OpenTrackData
    {
        public double x, y, z;
        public double yaw, pitch, roll;
    }

    public class OpenTrackReceiver
    {
        private bool Running
        {
            get;
            set;
        } = false;

        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private FreeLookClient _client = null;

        private Thread _packet_receive_thread;
        private bool _waiting_on_packet = false;

        private Thread _process_queue_thread;

        private ConcurrentQueue<OpenTrackData> _queued_items = new ConcurrentQueue<OpenTrackData>();

        // Last received opentrack data (x, y, z, yaw, pitch, roll)
        double[] _last_received_opentrack_data = new double[6];

        public OpenTrackReceiver(string ip, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));

            Console.WriteLine($"OpenTrack receiver listening on ip '{ip}' and port '{port.ToString()}'");
        }

        public OpenTrackReceiver(string ip, int port, FreeLookClient client)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            _client = client;

            Console.WriteLine($"OpenTrack receiver listening on ip '{ip}' and port '{port.ToString()}'");
        }

        public void Start()
        {
            Running = true;
            _queued_items.Clear();

            _packet_receive_thread = new Thread(() =>
            {
                while (Running)
                {
                    ReceiveUDPPacket();
                }
            });
            _packet_receive_thread.Start();

            _process_queue_thread = new Thread(() =>
            {
                while (Running)
                {
                    OpenTrackData data;
                    if (_queued_items.TryDequeue(out data))
                    {
                        ReceiveOpenTrackPacket(data);
                    }
                }
            });
            _process_queue_thread.Start();
        }

        public void Stop()
        {
            Running = false;
            _packet_receive_thread.Join();
            _process_queue_thread.Join();
            _socket.Close();
        }

        private void ReceiveUDPPacket()
        {
            if (_waiting_on_packet)
            {
                Thread.Sleep(1);
                return;
            }

            _waiting_on_packet = true;
            byte[] received_bytes = new byte[100];
            EndPoint client_endpoint = new IPEndPoint(IPAddress.Any, 0);
            _socket.BeginReceiveFrom(received_bytes, 0, received_bytes.Length, SocketFlags.None, ref client_endpoint, (ar) =>
            {
                try
                {
                    int message_size = _socket.EndReceiveFrom(ar, ref client_endpoint);

                    if (message_size < 48)
                    {
                        return;
                    }

                    double[] received_values = new double[6];
                    for (int i = 0; i < received_values.Length; i++)
                    {
                        received_values[i] = BitConverter.ToDouble(received_bytes, i*8);
                    }

                    _queued_items.Enqueue(new OpenTrackData { x = received_values[0], y = received_values[1], z = received_values[2], yaw = received_values[3], pitch = received_values[4], roll = received_values[5] });

                    _waiting_on_packet = false;
                }
                catch (SocketException)
                {
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    _socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

                    for (int i = 0; i < _last_received_opentrack_data.Length; i++)
                    {
                        _last_received_opentrack_data[i] = 0.0;
                    }
                }

            }, null);
        }

        private void ReceiveOpenTrackPacket(OpenTrackData data)
        {
            if (_client == null)
            {
                Console.WriteLine($"OpenTrack debugging...Position: [{data.x}, {data.y}, {data.z}], Rotation: [{data.yaw}°, {data.pitch}°, {data.roll}°]");
            }
            else
            {
                _client.SendOpenTrackData(data);
            }
        }
    }
}
