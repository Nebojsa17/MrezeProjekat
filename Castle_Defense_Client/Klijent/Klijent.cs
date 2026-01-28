using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Castle_Defense_Client.Klijent
{
    public class Klijent
    {
        public const int SERVER_PORT = 51000;

        static void Main(string[] args)
        {
            Socket prijavaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, SERVER_PORT); // Ovde cemo samo umesto IPAddress.Loopback da dodamo adresu koju igrac unese

            try
            {
                string poruka = "PRIJAVA"; // i ovo ce korisnik da unese preku UI-ja
                byte[] buffer = new byte[1024];

                buffer = Encoding.UTF8.GetBytes(poruka);

                int poslato = prijavaSocket.SendTo(buffer, 0, buffer.Length, SocketFlags.None, serverEP);

                Console.WriteLine("Poslata poruka za prijavu.");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Greska prilikom slanja poruke: {e.Message}.\n");
            }

            string tcpIP = string.Empty;
            int tcpPort = 0;

            try
            {
                byte[] buffer = new byte[1024];
                EndPoint odgovorEP = new IPEndPoint(IPAddress.Any, 0);

                int primljeno = prijavaSocket.ReceiveFrom(buffer, ref odgovorEP);
                string tcpInfo = Encoding.UTF8.GetString(buffer, 0, primljeno);

                prijavaSocket.Close();

                string[] delovi = tcpInfo.Split(':');
                tcpIP = delovi[0];
                tcpPort = int.Parse(delovi[1]); // adresa i port TCP uticnice klijenta
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Greska prilikom prijema poruke: {e.Message}.\n");
            }

            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(tcpIP), tcpPort));
        }
    }
}
