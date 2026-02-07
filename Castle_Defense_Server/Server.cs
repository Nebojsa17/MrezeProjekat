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
using System.Xml.Linq;

namespace Castle_Defense_Server
{
    public class Server
    {
        public const int SERVER_PORT = 51000;
        public static List<Line> trake = new List<Line>();
        public static Random localRandom = new Random();
        private static CancellationTokenSource _cts;
        private static bool GameRunning = true;
        private static bool MyTurn = false;
        private static int turn = 0;
        private static Task _lTask;
        private static readonly object _lock = new object();
        private static List<Socket> igraciSoketi = new List<Socket>();
        private static List<Hand> karteIgraca = new List<Hand>();

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
            turn = 1;

            _lTask = Task.Run(() => { RecieveLoop(_cts.Token); });

            Console.WriteLine("Pocetak igre!!!");
            NacrtajTablu();
            int lastTurn = -1;
            while (GameRunning) 
            {
                //pocetak nove runde
                if (MyTurn)
                {
                    foreach (Socket s in igraciSoketi)
                    {
                        int missingCards = (Hand.HandSize - karteIgraca[igraciSoketi.IndexOf(s)].Cards.Count);
                        Console.WriteLine("Saljem " + missingCards + " karata");
                        for (int i = 0; i < missingCards; i++)
                        {
                            Card nabavljena = Deck.GetRadnomCard();
                            if (nabavljena == null)
                            {
                                Posalji(s, new Packet(PacketType.NOCARD, -1));
                            }
                            else
                            {
                                karteIgraca[igraciSoketi.IndexOf(s)].Cards.Add(nabavljena);
                                Posalji(s, new Packet(PacketType.CARD, nabavljena));
                            }
                        }
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        Enemy e = EnemyDeck.GetRadnomEnemy();
                        if (e == null)
                        {
                            break;
                        }
                        e.playIndx = EnemyDeck.random.Next(0, trake.Count);
                        Console.WriteLine("Postavljam neprijatelja: " + e.Name+" na "+ (e.playIndx+1)+". traku");
                        foreach (Socket s in igraciSoketi) Posalji(s, new Packet(PacketType.PLAYENEMY, e));
                        e.Play(trake, e.playIndx);
                    }

                    foreach (Line l in trake)
                    {
                        if (l.BrojZidina < 0)
                        {
                            GameRunning = false;
                            foreach (Socket s in igraciSoketi) Posalji(s, new Packet(PacketType.DEFEAT, -1));
                        }
                    }
                    if (ProveriPobedu())
                    {
                        foreach (Socket s in igraciSoketi)
                        {
                            Posalji(s, new Packet(PacketType.VICTORY, -1));
                            GameRunning = false;
                            _cts.Cancel();
                        }
                    }
                    if (GameRunning)foreach (Socket s in igraciSoketi)Posalji(s, new Packet(PacketType.NEWTURN, -1));
                        
                    MyTurn = false;
                    Console.WriteLine("Kraj runde: " + turn / igraciSoketi.Count);
                    Posalji(igraciSoketi[0], new Packet(PacketType.TURN, -1));
                    BoardAdvance();
                    NacrtajTablu();
                }
                if(turn!= lastTurn)
                {
                    Posalji(igraciSoketi[turn % brojIgraca], new Packet(PacketType.TURN, -1));
                    lastTurn = turn;
                }
            }

            for (int i=0; i<igraciSoketi.Count; i++) 
            {
                RemoveClient(igraciSoketi[i], "kraj igre");
                i--;
            }
            #endregion
            Console.ReadKey();
        }

        private static bool ProveriPobedu() 
        {
            foreach (Line l in trake) 
            {
                if(l.SumaZona.Count!=0 || l.StrelacZona.Count != 0 || l.VitezZona.Count != 0 || l.MacevalacZona.Count != 0) return false;
            }

            foreach(EnemyPair ep in EnemyDeck.deck) 
            {
                if(ep.Num>0) return false;
            }

            return true;
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
      
        static void SendFull(Socket socket, byte[] data)
        {
            int totalSent = 0;
            while (totalSent < data.Length)
            {
                try
                {
                    int sent = socket.Send(data, totalSent, data.Length - totalSent, SocketFlags.None);
                    if (sent == 0)
                        throw new SocketException(); 
                    totalSent += sent;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    Thread.Sleep(1); 
                }
            }
        }
      
        public static void RecieveLoop(CancellationToken token)
        {
            #region old
            /*
            while (!token.IsCancellationRequested) 
            {
                List<Socket> listener = new List<Socket>(igraciSoketi);

                Socket.Select(listener, null, null, 1000);

                for (int i = 0; i < listener.Count; i++) 
                {
                    try
                    {
                        if (listener[i].Available < 4)
                            continue;
                        byte[] recvBuffer = new byte[4096]; 

                        byte[] lenBuf = ReceiveExact(listener[i], 4);
                        int length = BitConverter.ToInt32(lenBuf, 0);
                        recvBuffer = ReceiveExact(listener[i], length);

                        Packet primljenPaket;

                        using (MemoryStream ms = new MemoryStream(recvBuffer))
                        {
                            BinaryFormatter bf = new BinaryFormatter();
                            primljenPaket = (Packet)bf.Deserialize(ms);
                            ObradiPaket(primljenPaket, listener[i]);
                        }
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                    {
                        continue;
                    }
                    catch (SocketException ex) 
                    {
                        try
                        {
                            if (_cts != null) _cts.Cancel();

                            if (listener[i] != null)
                            {
                                try { listener[i].Shutdown(SocketShutdown.Both); } catch { }
                                try { listener[i].Close(); } catch { }
                            }
                            listener[i] = null;
                            listener.RemoveAt(i);
                            i--;
                            Console.WriteLine("Klijent se diskonektovao.");
                            Console.WriteLine(ex.Message);
                        }
                        catch (Exception exs)
                        {
                            Console.WriteLine("Disconnect error: " + exs.Message);
                        }
                    }
                }

            }*/
            #endregion

            while (!token.IsCancellationRequested)
            {
                List<Socket> readList = new List<Socket>();
                lock (_lock)
                {
                    readList.AddRange(igraciSoketi);
                }

                try
                {
                    Socket.Select(readList, null, null, 200000);
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < readList.Count; i++)
                {
                    if (token.IsCancellationRequested) break;

                    ReceiveFromClient(readList[i]);
                }
            }

        }

        private static void ReceiveFromClient(Socket client)
        {
            Console.WriteLine("obradjujem");
            byte[] buf = new byte[4096];

            try
            {
                int n = client.Receive(buf);
                if (n == 0)
                {
                    RemoveClient(client, "Disconnected");
                    return;
                }

                Packet primljenPaket;

                using (MemoryStream ms = new MemoryStream(buf))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    primljenPaket = (Packet)bf.Deserialize(ms);
                    ObradiPaket(primljenPaket, client);
                }


            }
            catch (SocketException ex)
            {
                RemoveClient(client, "SocketException: " + ex.SocketErrorCode);
            }
            catch (Exception ex)
            {
                RemoveClient(client, "Error: " + ex.Message);
            }
        }
        
        private static void RemoveClient(Socket client, string reason)
        {
            bool removed = false;
            lock (_lock)
            {
                removed = igraciSoketi.Remove(client);
            }

            if (removed)
            {
                string ep = (client.RemoteEndPoint != null) ? client.RemoteEndPoint.ToString() : "unknown";
                Console.WriteLine("[-] Klijent " + ep + " uklonjen (" + reason + ")");
            }

            SafeClose(client);
            GameRunning = false;
            _cts.Cancel();
        }

        private static void SafeClose(Socket s)
        {
            if (s == null) return;
            try { s.Shutdown(SocketShutdown.Both); } catch { }
            try { s.Close(); } catch { }
        }
        
        public static void ObradiPaket(Packet paket, Socket s) 
        {
            switch (paket.Vrsta) 
            {
                case PacketType.DISCARD:
                    Card vracena = (Card)paket.Sadrzaj;
                    Deck.ReturnCard(vracena);
                    karteIgraca[igraciSoketi.IndexOf(s)].Cards.Remove((Card)paket.Sadrzaj);
                    Console.WriteLine("Vracena karta: "+vracena.Name+" - "+vracena.CColor);
                    break;
                case PacketType.PLAYCARD:
                    foreach(Socket igrac in igraciSoketi) 
                    {
                        if(igrac!=s)Posalji(igrac, new Packet(PacketType.PLAYCARD,(Card)paket.Sadrzaj));
                    }
                    karteIgraca[igraciSoketi.IndexOf(s)].Cards.Remove((Card)paket.Sadrzaj);
                    Console.WriteLine("igrac "+s.RemoteEndPoint+" je odigrao: "+((Card)paket.Sadrzaj).Name +" - "+((Card)paket.Sadrzaj).CColor);
                    turn++;
                    if (turn % igraciSoketi.Count == 0) MyTurn = true;
                    break;
                case PacketType.PASS:

                    turn++;
                    if (turn % igraciSoketi.Count == 0) MyTurn = true;
                    break;
                case PacketType.CARDREQUEST:
                    for (int i = 0; i<(int)paket.Sadrzaj; i++) 
                    {
                        Card nabavljena = Deck.GetRadnomCard();
                        if (nabavljena == null)
                        {
                            Posalji(s, new Packet(PacketType.NOCARD, -1));
                        }
                        else Posalji(s, new Packet(PacketType.CARD, nabavljena));
                        Console.WriteLine("Saljem "+s.RemoteEndPoint+" klijentu kartu.");
                    }
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
                trake.Add(new Line(br - 1, (LineColor)(i-1)));
                trake.Add(new Line(br, (LineColor)(i-1)));
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
                Hand h = new Hand();
                h.Cards = karte;
                karteIgraca.Add(h);
                Posalji(klijenti[i], new Packet(PacketType.HAND, karte));
            }
            Console.WriteLine("Gotove karte");
            #endregion

            foreach (Socket s in klijenti)
            {
                foreach (Line l in trake)
                {
                    Posalji(s, new Packet(PacketType.INILINES, l));
                }
            }

            List<string> igraciIP = new List<string>();
            foreach (Socket s in igraciSoketi)
            {
                igraciIP.Add(s.RemoteEndPoint.ToString());
            }
            foreach (Socket s in igraciSoketi) Posalji(s, new Packet(PacketType.PLAYERS, igraciIP));
            Posalji(klijenti[0], new Packet(PacketType.TURN, -1));
        }

        private static void NacrtajTablu()
        {
            string output = "";
            int enemyPerLine = 4;
            for (int i = 0; i < enemyPerLine; i++)
            {
                foreach (Line l in trake)
                {
                    output += "| ";
                    if (i < l.SumaZona.Count)
                    {
                        output += string.Format(" {0,-2}{1,-6} ", l.SumaZona[i].HP, l.SumaZona[i].Name);
                    }
                    else output += string.Format("{0,-10}", " ");
                    output += "|   ";
                }
                output += "\n";
            }
            output += "\n";
            for (int i = 0; i < enemyPerLine; i++)
            {
                foreach (Line l in trake)
                {
                    output += "| ";
                    if (i < l.StrelacZona.Count)
                    {
                        output += string.Format(" {0,-2}{1,-6} ", l.StrelacZona[i].HP, l.StrelacZona[i].Name);
                    }
                    else output += string.Format("{0,-10}", " ");
                    output += "|   ";
                }
                output += "\n";
            }
            output += "\n";
            for (int i = 0; i < enemyPerLine; i++)
            {
                foreach (Line l in trake)
                {
                    output += "| ";
                    if (i < l.VitezZona.Count)
                    {
                        output += string.Format(" {0,-2}{1,-6} ", l.VitezZona[i].HP, l.VitezZona[i].Name);
                    }
                    else output += string.Format("{0,-10}", " ");
                    output += "|   ";
                }
                output += "\n";
            }
            output += "\n";
            for (int i = 0; i < enemyPerLine; i++)
            {
                foreach (Line l in trake)
                {
                    output += "| ";
                    if (i < l.MacevalacZona.Count)
                    {
                        output += string.Format(" {0,-2}{1,-6} ", l.MacevalacZona[i].HP, l.MacevalacZona[i].Name);
                    }
                    else output += string.Format("{0,-10}", " ");
                    output += "|   ";
                }
                output += "\n";
            }
            output += "\n";
            foreach (Line l in trake)
            {
                output += string.Format("{1,-6}{0,-2}{1,-6}",l.BrojZidina ," ");
            }
            Console.WriteLine(output);
        }
        private static void BoardAdvance()
        {
            foreach (Line l in trake) l.Advance();
        }

        public static void Posalji(Socket sock, Packet paket)
        {
            try
            {
                if (!sock.Connected) return;

                byte[] paketBuffer = new byte[4096];

                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, paket);
                    paketBuffer = ms.ToArray();
                }

                byte[] lengthPrefix = BitConverter.GetBytes(paketBuffer.Length);

                Thread.Sleep(100);

                SendFull(sock, lengthPrefix);
                sock.Send(paketBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
