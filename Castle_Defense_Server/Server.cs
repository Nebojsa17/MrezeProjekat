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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Castle_Defense_Server
{
    public class Server
    {
        public const int SERVER_PORT = 51000;
        public static List<Line> trake = new List<Line>();
        public static Random localRandom = new Random();
        private static CancellationTokenSource _cts;
        private static bool GameRunning = true;
        private static int turn = 0;
        private static Task _lTask;
        private static List<Socket> igraciSoketi = new List<Socket>(); 

        static void Main(string[] args)
        {
            Console.WriteLine($"Server pocinje sa radom na adresi: {GetLocalIPAddress()}");

            #region Unos validnog broja igraca

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
            #endregion

            #region POCETNA UDP POVEZIVANJA
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
            #endregion

            #region TCP POVEZIVANJA
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Any, SERVER_PORT));
            listenSocket.Blocking = false;
            listenSocket.Listen(brojIgraca);

            Console.WriteLine("Server slusa...");

            while (igraciSoketi.Count < brojIgraca)
            {
                if (listenSocket == null)
                {
                    Thread.Sleep(50);
                    continue;
                }

                List<Socket> readList = new List<Socket>();
                readList.Add(listenSocket);

                try
                {
                    Socket.Select(readList, null, null, 200000);
                }
                catch
                {
                    continue;
                }
                try
                {
                    Socket klijentSocket = listenSocket.Accept();
                    klijentSocket.Blocking = false;
                    igraciSoketi.Add(klijentSocket);
                    Console.WriteLine("Klijent povezan.");
                }

                catch (SocketException)
                {
                    // ignore (non-blocking accept race)
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Accept error: " + ex.Message);
                }
            }
            #endregion

            #region GAME LOGIKA I KOMUNIKACIJA
            GenerateGame(brojIgraca, igraciSoketi);

            _cts = new CancellationTokenSource();

            GameRunning = true;
            turn = 0;

            _lTask = Task.Run(() => { RecieveLoop(_cts.Token); });

            while (GameRunning) 
            {


            }

            #endregion
            Console.ReadKey();
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

        public static void RecieveLoop(CancellationToken token)
        {
            List<Socket> listener = new List<Socket>();

            while (!token.IsCancellationRequested) 
            {
                foreach (Socket s in igraciSoketi) listener.Add(s);

                Socket.Select(listener, null, null, 10000);

                foreach (Socket s in listener)
                {
                    byte[] recvBuffer = new byte[4096 * 2];
                    int recvBytes = s.Receive(recvBuffer);

                    Packet primljenPaket;

                    using (MemoryStream ms = new MemoryStream(recvBuffer))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        primljenPaket = (Packet)bf.Deserialize(ms);
                        ObradiPaket(primljenPaket);
                    }
                }

            }
        }

        public static void ObradiPaket(Packet paket) 
        {
            switch (paket.Vrsta) 
            {
                case PacketType.DISCARD:
                    Card vracena = (Card)paket.Sadrzaj;
                    Deck.ReturnCard(vracena);
                    Console.WriteLine("Vracena karta: "+vracena.Name+" - "+vracena.CColor);
                    break;
                case PacketType.PLAYCARD:

                    turn++;
                    break;
                case PacketType.PASS:

                    turn++;
                    break;
            }
        }

        public static void GenerateGame(int brojIgraca, List<Socket> klijenti)
        {
            Console.WriteLine("Priprema igru za "+ klijenti.Count + ". igraca");
            #region Inicijalizacija traka
            for (int i = 1; i <= brojIgraca; i++) 
            {
                int br = i * 2;
                trake.Add(new Line(br - 1, (LineColor)i));
                trake.Add(new Line(br, (LineColor)i));
            }
            Console.WriteLine("Gotovo "+trake.Count+" traka");
            #endregion
            #region Dodavanje Neprijatelja
            EnemyDeck.InitializeDeck(brojIgraca);
            if (brojIgraca == 1)
            {
                int brTrake = localRandom.Next(0, 2);

                trake[brTrake].StrelacZona.Add(new Goblin());
                trake[1 - brTrake].StrelacZona.Add(new Ork());
            }
            else if (brojIgraca == 2)
            {
                List<int> brTrake = new List<int> { 0, 1, 2, 3 };
                int ind = localRandom.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = localRandom.Next(0, brTrake.Count);

                trake[ind].StrelacZona.Add(new Goblin());

                brTrake.RemoveAt(ind);
                ind = localRandom.Next(0, brTrake.Count);
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
            Console.WriteLine("Gotovi neprijatelji");
            #endregion
            #region Generisanje Karata
            Deck.InitializeDeck(brojIgraca);
            for(int i=0; i<brojIgraca; i++) 
            {
                List<Card> karte = new List<Card>();
                for(int j=0; j<5; j++) 
                {
                    karte.Add(Deck.GetRadnomCard());
                }
                Posalji(klijenti[i], new Packet(PacketType.HAND, karte));
            }
            Console.WriteLine("Gotove karte");
            #endregion

            foreach (Socket s in klijenti) Posalji(s, new Packet(PacketType.INILINES,trake));
        }

        public static void Posalji(Socket sock, Packet paket)
        {
            try
            {
                byte[] paketBuffer = new byte[4096 * 2];

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, paket);
                    paketBuffer = ms.ToArray();
                }

                sock.Send(paketBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
