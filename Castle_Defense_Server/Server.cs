using CommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Castle_Defense_Server
{
    public class Server
    {
        public const int SERVER_PORT = 51000;

        static void Main(string[] args)
        {
            Console.WriteLine($"Server pocinje sa radom na adresi: {GetLocalIPAddress()}");

            // Unos validnog broja igraca

            int brojIgraca = 1;

            do
            {
                try
                {
                    Console.Write("Unesite broj igraca: ");
                    brojIgraca = int.Parse(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Greska prilikom unosa: {e.Message}.\n");
                    return;
                }
                
            } while (brojIgraca < 1 || brojIgraca > 3);

            // Otvaranje UDP uticnice za prijavu igraca

            Socket prijavaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            prijavaSocket.Bind(serverEP);

            Console.WriteLine("Server ceka prijave. Da biste se prijavili, posaljite poruku PRIJAVA.");

            // Provera prijava i slanje informacija igracima o njihovoj TCP uticnici

            List<EndPoint> igraci = new List<EndPoint>(); // Lista igraca
            byte[] buffer = new byte[1024];

            while (igraci.Count < brojIgraca)
            {
                try
                {
                    EndPoint igracEP = new IPEndPoint(IPAddress.Any, 0);
                    int primljeno = prijavaSocket.ReceiveFrom(buffer, ref igracEP);

                    string poruka = Encoding.UTF8.GetString(buffer, 0, primljeno);

                    if (poruka == "PRIJAVA")
                    {
                        igraci.Add(igracEP);
                        Console.WriteLine($"Prijavljen igrac: {igracEP}");
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Greska prilikom prijema poruke: {e.Message}.\n");
                }
            }
            
            try
            {
                string tcpInfo = $"{GetLocalIPAddress()}:{SERVER_PORT}";
                byte[] tcpInfoBytes = Encoding.UTF8.GetBytes(tcpInfo);

                foreach (EndPoint ep in igraci)
                {
                    prijavaSocket.SendTo(tcpInfoBytes, ep);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Greska prilikom slanja poruke: {e.Message}.\n");
            }
            
            Console.WriteLine("Server zavrsava sa prijavom. Ocekuje se uspostava veze od strane igraca.");
            prijavaSocket.Close();
            
            
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            listenSocket.Listen(brojIgraca);

            Console.WriteLine("Server slusa...");

            for (int i = 0; i < brojIgraca; i++)
            {
                Socket klijentSocket = listenSocket.Accept();
                Console.WriteLine("Klijent povezan.");
            }
        }

        public static string GetLocalIPAddress() // Pomocna metoda za dobavljanje IP adrese servera
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            
            try
            {
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
                }

                return string.Empty;
            }
            catch
            {
                Console.WriteLine($"Greska prilikom pribavljanja adrese.\n");
                return string.Empty;
            }
            
        }

    }
}
