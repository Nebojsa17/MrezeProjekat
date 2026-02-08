using Castle_Defense_Client.Elements;
using Castle_Defense_Client.Klijent;
using CommonLibrary;
using CommonLibrary.Cards;
using CommonLibrary.Enemies;
using CommonLibrary.Miscellaneous;
using CommonLibrary.Sprites;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Line = CommonLibrary.Miscellaneous.Line;

namespace Castle_Defense_Client
{
    public partial class MainWindow : Window
    {
        //For web
        private static IPAddress adresaServera = IPAddress.Loopback;
        //private static IPAddress adresaServera = IPAddress.Parse("192.168.0.106");
        public const int SERVER_PORT = 51000;
        private Socket _sockUDP, _sockTCP;
        private static readonly object _lock = new object();
        private bool discarded = false;
        private bool myTurn = false;
        private CancellationTokenSource _cts;
        private Task _rxTask;


        //For game
        DrawingVisual dvGame = new DrawingVisual();
        DrawingVisual dvCards = new DrawingVisual();
        Map mapa;
        Hand karte;
        List<Line> trake = new List<Line>();

        public MainWindow()
        {
            InitializeComponent();
            Screen.AddVisual(dvGame);
            CardSpace.AddVisual(dvCards);

            //Ovaj sav posao ide serveru, treba da prosledi trake sa neprijateljima i karte
            /*
            Deck.InitializeDeck(3);
            EnemyDeck.InitializeDeck(3);

            trake.Add(new CommonLibrary.Miscellaneous.Line(1, LineColor.PLAVA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(2, LineColor.PLAVA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(3, LineColor.ZELENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(4, LineColor.ZELENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(5, LineColor.CRVENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(6, LineColor.CRVENA));

            mapa = new Map(Screen.Width, Screen.Height,trake);
            karte = new Hand(CardSpace.Width, CardSpace.Height);

            karte.Cards.Add(Deck.GetRadnomCard());
            karte.Cards.Add(Deck.GetRadnomCard());
            karte.Cards.Add(Deck.GetRadnomCard());
            karte.Cards.Add(Deck.GetRadnomCard());
            karte.Cards.Add(Deck.GetRadnomCard());

            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            BoardAdvance();
            Dispatcher.Invoke(() => { Render(); });
            
            SwitchScreens();*/
            //do ovde je posao servera

        }

        public void Render()
        {
            using (DrawingContext dc = dvGame.RenderOpen())
            {
               if(mapa != null) mapa.Draw(dc);
            }

            using (DrawingContext dc = dvCards.RenderOpen())
            {
               if(karte != null) karte.Draw(dc);
            }
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!int.TryParse(e.Text, out int res)) 
            {
                ((TextBox)sender).Text = "0";
                e.Handled = true;
                return;
            }

            if (((TextBox)sender).Text.Length == 0)
            {
                e.Handled = false;
                return; 
            }
            ((TextBox)sender).Text = e.Text.Substring(e.Text.Length - 1, 1);
            e.Handled = true;
        }

        private void BoardAdvance() 
        {
            foreach (Line l in trake) l.Advance();
        }

        private void SwitchScreens() 
        {
            if (Meni.IsSelected)
            {
                Meni.IsSelected = !Meni.IsSelected;
                Game.IsSelected = !Game.IsSelected;
            }
            else
            {
                Game.IsSelected = !Game.IsSelected;
                Meni.IsSelected = !Meni.IsSelected;
            }
        }
        //dodatno za izbor karata
        private void choosenCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            choosenCard.Text = karte.GetSelect(e.GetPosition((DrawingPanel)sender)).ToString();
            Render();
        }

        //dedatno za izbor traka/neprijatelja
        private void Screen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Vector3D selectedTrio = mapa.GetSelected(e.GetPosition((DrawingPanel)sender));

            unetaTraka.Text = selectedTrio.X.ToString();
            unetaStaza.Text = selectedTrio.Y.ToString();
            unetNeprijatelj.Text = selectedTrio.Z.ToString();

            Render();
        }
        
        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint serverEP = new IPEndPoint(adresaServera, SERVER_PORT);

                string poruka = "PRIJAVA";
                byte[] buffer = new byte[1024];

                buffer = Encoding.UTF8.GetBytes(poruka);

                int poslato = _sockUDP.SendTo(buffer, 0, buffer.Length, SocketFlags.None, serverEP);

                Logger.Content = "LOG: Poslata poruka za prijavu.";
            }
            catch (SocketException er)
            {
                Logger.Content = $"LOG: Greska prilikom slanja poruke: {er.Message}.";
                return;
            }
            catch (Exception err) 
            {
                Logger.Content = $"LOG: Greska: {err.Message}.";
                return;
            }


            string tcpIP = string.Empty;
            int tcpPort = 0;

            try
            {
                byte[] buffer = new byte[1024];
                EndPoint odgovorEP = new IPEndPoint(IPAddress.Any, 0);

                int primljeno = _sockUDP.ReceiveFrom(buffer, ref odgovorEP);
                string tcpInfo = Encoding.UTF8.GetString(buffer, 0, primljeno);

                _sockUDP.Close();

                string[] delovi = tcpInfo.Split(':');
                tcpIP = delovi[0];
                tcpPort = int.Parse(delovi[1]); 
                
                _sockTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sockTCP.Connect(new IPEndPoint(IPAddress.Parse(tcpIP), tcpPort));

                _cts = new CancellationTokenSource();
                _rxTask = Task.Run(() => { RecieveLoop(_cts.Token); },_cts.Token );
                discard_btn.IsEnabled = true;
                play_btn.IsEnabled = true;
                swap_btn.IsEnabled = true;
                pass_btn.IsEnabled = true;
                SwitchScreens();
            }
            catch (SocketException er)
            {
                Logger.Content = $"LOG: Greska prilikom prijema poruke: {er.Message}.";
                return;
            }
            catch (Exception err)
            {
                Logger.Content = $"LOG: Greska: {err.Message}.";
                return;
            }
        }

        private void RecieveLoop(CancellationToken token)
        {
            byte[] buf = new byte[4096];

            while (!token.IsCancellationRequested)
            {
                List<Socket> readList = new List<Socket>();
                lock (_lock)
                {
                    readList.Add(_sockTCP);
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

                    ReceiveFromServer(readList[i]);
                }
            }
        }

        private void ReceiveFromServer(Socket s)
        {
            byte[] buf = new byte[4096];

            try
            {
                int n = s.Receive(buf);
                if (n == 0)
                {
                    Disconnect();
                    return;
                }

                BinaryFormatter formatter = new BinaryFormatter();

                using (MemoryStream ms = new MemoryStream(buf))
                {
                    Packet paket = (Packet)formatter.Deserialize(ms);
                    Dispatcher.Invoke(() => ObradiPaket(paket));
                }

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
                Log("SocketException: " + ex.SocketErrorCode);
                Dispatcher.Invoke(() => Disconnect());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Log("Receive error: " + ex.Message);
                Dispatcher.Invoke(() => Disconnect());
            }
        }

        private byte[] ReceiveExact(Socket sock, int size)
        {
            byte[] buffer = new byte[size];
            int read = 0;
            while (read < size)
            {
                int n = sock.Receive(buffer, read, size - read, SocketFlags.None);
                if(n == 0) 
                {
                    Log("Server je zatvorio konekciju.");
                    Dispatcher.Invoke(() => Disconnect());
                    break;
                }
                read += n;
            }
            return buffer;
        }
        
        private void ObradiPaket(Packet packet) 
        {
            switch (packet.Vrsta) 
            {
                case PacketType.PLAYENEMY:
                    Enemy newEnemy = (Enemy)packet.Sadrzaj;
                    newEnemy.Play(trake,newEnemy.playIndx);
                    Log("Odigran: "+newEnemy.Name);
                    break;
                case PacketType.PLAYCARD:
                    Card c = (Card)packet.Sadrzaj;
                    if (c.Played())
                    {
                        c.Play(null, trake);
                    }
                    Log("Odigrana karta: "+c.Name+" "+c.CColor);
                    break;
                case PacketType.NEWTURN:
                    BoardAdvance();
                    Log("Pocinje novi potez");
                    break;
                case PacketType.TURN:
                    myTurn = true; 
                    discarded = false;
                    Log("Tvoj potez!!!");
                    break;
                case PacketType.HAND:
                    karte = new Hand(CardSpace.Width, CardSpace.Height);
                    this.karte.Cards = (List<Card>)packet.Sadrzaj;
                    Log("Primljene karte");
                    break;
                case PacketType.INILINES:
                    this.trake.Add((Line)packet.Sadrzaj);
                    mapa = new Map(Screen.Width, Screen.Height, trake);
                    Log("Primljena traka");
                    break;
                case PacketType.CARDREQUEST:
                    Posalji(_sockTCP, new Packet(PacketType.CARDREQUEST, Hand.HandSize - karte.Cards.Count));
                    break;
                case PacketType.CARD:
                    karte.Cards.Add((Card)packet.Sadrzaj);
                    Log("Dodata karta: " + ((Card)packet.Sadrzaj).Name);
                    break;
                case PacketType.NOCARD:
                    Log("Nema vise karata");
                    break;
                case PacketType.DEFEAT:
                    BoardAdvance();
                    MessageBox.Show("Poraz :(");
                    break;
                case PacketType.VICTORY:
                    BoardAdvance();
                    MessageBox.Show("Pobeda :)");
                    break;
                case PacketType.PLAYERS:
                    List<string> ostali = (List<string>)packet.Sadrzaj;
                    string mi = _sockTCP.LocalEndPoint.ToString();
                    playerList.Content = "PL-"+mi+"\n\n";
                    ostali.Remove(mi);
                    foreach ( string str in ostali) 
                    {
                        playerList.Content += str + "\n\n";
                    }
                    Log("Primljeni igraci");
                    break;
                case PacketType.HANDUPDATE:
                    Hand rightHand = (Hand)packet.Sadrzaj;

                    if (rightHand.Cards.Count != karte.Cards.Count) 
                    {
                        Log("Korektovanje ruke!!!");
                        karte.Cards = rightHand.Cards;
                    }

                    break;
            }

            Render();
        }

        private void Log(string line)
        {
            Dispatcher.Invoke(() =>
            {
                gameLog.Text += "\n"+line;
                gameLog.ScrollToEnd();
            });
        }
        
        private void Posalji(Socket s, Packet paket) 
        {
            if (!myTurn) return;

            byte[] buffer = new byte[1024];

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, paket);
                    buffer = ms.ToArray();
                }

                byte[] lengthPrefix = BitConverter.GetBytes(buffer.Length);
                
                _sockTCP.Send(buffer);

                if (paket.Vrsta == PacketType.PASS || paket.Vrsta == PacketType.PLAYCARD) myTurn = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Disconnect()
        {
            try
            {
                if (_cts != null) _cts.Cancel();

                if (_sockTCP != null)
                {
                    try { _sockTCP.Shutdown(SocketShutdown.Both); } catch { }
                    try { _sockTCP.Close(); } catch { }
                }
                _sockTCP = null;

                discard_btn.IsEnabled = false;
                play_btn.IsEnabled = false;
                swap_btn.IsEnabled = false;
                pass_btn.IsEnabled = false;

                Log("Diskonektovan.");
            }
            catch (Exception ex)
            {
                Log("Disconnect error: " + ex.Message);
            }
        }
        
        private void discard_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!myTurn) return;
            // logika za discard, serveru samo treba da se posalje karta umesto da je ovde dodajemo
            int cardIndx = int.Parse(choosenCard.Text);
            if (cardIndx < 1 || cardIndx > karte.Cards.Count) return;
            cardIndx--;

            if (discarded) return;

            discarded = true;

            byte[] karteBuffer = new byte[1024];

            Posalji(_sockTCP ,new Packet(PacketType.DISCARD, karte.Cards[cardIndx]));
            karte.Cards.RemoveAt(cardIndx);
            Log("Odbacena :(");
            Dispatcher.Invoke(() => { Render(); });
        }

        private void pass_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!myTurn) return;
            Posalji(_sockTCP, new Packet(PacketType.PASS,new Strelac(LineColor.LJUBICASTA)));
            Log("Preskocio");
            Dispatcher.Invoke(() => { Render(); });
        }

        private void play_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!myTurn) return;
            //logika za igranje karata, treba dodati deo koji ce serveru poslati koja je karta odigrana
            int cardIndx = int.Parse(choosenCard.Text);
            if (cardIndx < 1 || cardIndx > karte.Cards.Count) return;
            cardIndx--;
            int unetaTrakaIndx = int.Parse(unetaTraka.Text);
            if (unetaTrakaIndx < 1 || unetaTrakaIndx > trake.Count || karte.Cards.Count==0) return;
            unetaTrakaIndx--;
            int unetaZoneIndx = int.Parse(unetaStaza.Text);
            int unetEnemyIndx = int.Parse(unetNeprijatelj.Text);

            Card odigrana = karte.Cards[cardIndx].Play(karte.Cards, trake[unetaTrakaIndx], unetaZoneIndx, unetEnemyIndx);

            if (odigrana!=null) 
            {
                Posalji(_sockTCP, new Packet(PacketType.PLAYCARD,odigrana));
                Dispatcher.Invoke(() => { Render(); });
            }

        }

        private void swap_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            SwitchScreens();
        }
    }
}
