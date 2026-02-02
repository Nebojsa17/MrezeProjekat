using CommonLibrary;
using CommonLibrary.Cards;
using CommonLibrary.Enemies;
using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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

            int brojTraka = brojIgraca * 2;
            LineColor boja = LineColor.PLAVA;

            // Otvaranje UDP uticnice za prijavu igraca

            Socket prijavaSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            prijavaSocket.Bind(serverEP);

            Console.WriteLine("Server ceka prijave. Da biste se prijavili, posaljite poruku PRIJAVA.");

            // Provera prijava i slanje informacija igracima o njihovoj TCP uticnici

            List<EndPoint> igraci = new List<EndPoint>(); // Lista igraca
            List<Socket> igraciSoketi = new List<Socket>(); // Lista soketa igraca
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
                igraciSoketi.Add(klijentSocket);
                Console.WriteLine("Klijent povezan.");
            }

            // Slanje karata klijentima

            try
            {
                for (int i = 0; i < brojIgraca; i++)
                {
                    Hand karte = new Hand();
                    byte[] karteBuffer = new byte[1024];

                    for (int j = 0; j < 5; j++)
                    {
                        karte.Cards.Add(Deck.GetRadnomCard());
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, karte);
                        karteBuffer = ms.ToArray();
                    }

                    igraciSoketi[i].Send(karteBuffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // Kreiranje traka

            List<Line> trake = new List<Line>();

            for (int i = 0; i < brojTraka; i++)
            {
                Line traka = new Line(i + 1, boja);
                trake.Add(traka);

                if (i != 0 && i % 2 == 0)
                {
                    boja++;
                }
            }

            // Inicijalno rasporedjivanje protivnika

            if (brojIgraca == 1)
            {
                int brTrake = new Random().Next(0, 2);

                trake[brTrake].StrelacZona.Add(new Goblin());
                trake[1 - brTrake].StrelacZona.Add(new Ork());
            }
            else if (brojIgraca == 2)
            {
                Random r = new Random();
                List<int> brTrake = new List<int> { 0, 1, 2, 3 };
                int ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Ork());

                brTrake.RemoveAt(ind);

                trake[0].StrelacZona.Add(new Trol());
            }
            else if (brojIgraca == 3)
            {
                Random r = new Random();
                List<int> brTrake = new List<int> { 0, 1, 2, 3, 4, 5 };
                int ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Ork());

                brTrake.RemoveAt(ind);
                ind = r.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Ork());

                brTrake.RemoveAt(ind);

                trake[0].StrelacZona.Add(new Trol());
            }

            byte[] trakeBuffer = new byte[1024];

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, trake);
                    trakeBuffer = ms.ToArray();
                }

                for (int i = 0; i < brojIgraca; i++)
                {
                    igraciSoketi[i].Send(trakeBuffer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
