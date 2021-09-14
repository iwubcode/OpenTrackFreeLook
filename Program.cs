using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace OpenTrackFreeLook
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build()
            ;

            FreeLookClient client = null;
            if (configuration.GetValue<bool>("DebugOpenTrack") != true)
            {
                string ip = configuration.GetValue<string>("FreeLookServerIp");
                if(ip == null)
                {
                    Console.WriteLine("FreeLookServerIp configuration is not found or invalid");
                    return;
                }

                int? port = configuration.GetValue<int?>("FreeLookServerPort");
                if (port == null)
                {
                    Console.WriteLine("DSUServerPort configuration is not found or invalid");
                    return;
                }

                client = new FreeLookClient(ip, port.Value);
            }

            string open_track_ip = configuration.GetValue<string>("OpenTrackIp");
            if (open_track_ip == null)
            {
                Console.WriteLine("OpenTrackIp configuration is not found or invalid");
                return;
            }

            int? open_track_port = configuration.GetValue<int?>("OpenTrackPort");
            if (open_track_port == null)
            {
                Console.WriteLine("OpenTrackPort configuration is not found or invalid");
                return;
            }

            OpenTrackReceiver receiver = null;
            if (client == null)
            {
                receiver = new OpenTrackReceiver(open_track_ip, open_track_port.Value);
            }
            else
            {
                receiver = new OpenTrackReceiver(open_track_ip, open_track_port.Value, client);
            }

            receiver.Start();
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            receiver.Stop();
        }
    }
}
