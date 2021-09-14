using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OpenTrackFreeLook
{
    public class FreeLookClient
    {
        private Socket _socket;
        private string _ip;
        private int _port;

        private const ushort max_protocol_version_supported = 1;

        class Header
        {
            public ushort version;
            public byte message_count;
            public uint packet_size;
            public byte message_type;

            public static uint size = 8;
        };

        class TransformByEulerPosition
        {
            public double pitch;
            public double yaw;
            public double roll;

            public double posx;
            public double posy;
            public double posz;

            public static ushort size = 48;
            public static byte type = 1;
        };

        public FreeLookClient(string ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Console.WriteLine($"FreeLookClient sending message to server at ip '{ip}' and port '{port.ToString()}'");

            _ip = ip;
            _port = port;
        }

        private void WriteHeader(ref byte[] message_bytes, Header header)
        {
            int index = 0;
            Array.Copy(BitConverter.GetBytes(header.version), 0, message_bytes, index, 2);
            index += 2;

            Array.Copy(BitConverter.GetBytes(header.message_count), 0, message_bytes, index, 1);
            index += 1;

            Array.Copy(BitConverter.GetBytes(header.packet_size), 0, message_bytes, index, 4);
            index += 4;

            Array.Copy(BitConverter.GetBytes(header.message_type), 0, message_bytes, index, 1);
        }

        private void WriteTransformByEulerPosition(ref byte[] message_bytes, TransformByEulerPosition transform, uint index)
        {
            Array.Copy(BitConverter.GetBytes(transform.pitch), 0, message_bytes, index, 8);
            index += 8;

            Array.Copy(BitConverter.GetBytes(transform.yaw), 0, message_bytes, index, 8);
            index += 8;

            Array.Copy(BitConverter.GetBytes(transform.roll), 0, message_bytes, index, 8);
            index += 8;

            Array.Copy(BitConverter.GetBytes(transform.posx), 0, message_bytes, index, 8);
            index += 8;

            Array.Copy(BitConverter.GetBytes(transform.posy), 0, message_bytes, index, 8);
            index += 8;

            Array.Copy(BitConverter.GetBytes(transform.posz), 0, message_bytes, index, 8);
        }

        public void SendOpenTrackData(OpenTrackData data)
        {
            byte[] output = new byte[Header.size + TransformByEulerPosition.size];
            Header header = new Header();
            header.version = max_protocol_version_supported;
            header.message_count = 1;
            header.packet_size = TransformByEulerPosition.size;
            header.message_type = TransformByEulerPosition.type;
            WriteHeader(ref output, header);

            uint index = Header.size;
            TransformByEulerPosition transform = new TransformByEulerPosition();
            transform.pitch = data.pitch;
            transform.yaw = data.yaw;
            transform.roll = data.roll;

            transform.posx = data.x;
            transform.posy = data.y;
            transform.posz = data.z;
            WriteTransformByEulerPosition(ref output, transform, index);

            try
            {
                IPAddress ip = IPAddress.Parse(_ip);

                IPEndPoint endPoint = new IPEndPoint(ip, _port);
                _socket.SendTo(output, endPoint);
            }
            catch (SocketException)
            {
            }
        }
    }
}
