using Castle_Defense_Client.Elements;
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
        public const int SERVER_PORT = 51000;
        private Socket _sockUDP, _sockTCP;
        private bool discarded = false;
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
            BoardAdvance();
            Dispatcher.Invoke(() => { Render(); });
            */
            //SwitchScreens();
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
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, SERVER_PORT);

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
        }

        private void RecieveLoop(CancellationToken token) 
        {
            while (!token.IsCancellationRequested) 
            {
                byte[] buf = new byte[4096 * 2];

                while (!token.IsCancellationRequested)
                {
                    Socket s = _sockTCP;
                    if (s == null) break;

                    try
                    {
                        int n = s.Receive(buf);
                        if (n == 0)
                        {
                            Log("Server je zatvorio konekciju.");
                            Dispatcher.Invoke(() => Disconnect());
                            break;
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
                        Log("SocketException: " + ex.SocketErrorCode);
                        Dispatcher.Invoke(() => Disconnect());
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log("Receive error: " + ex.Message);
                        Dispatcher.Invoke(() => Disconnect());
                        break;
                    }
                }
            }
        }

        private void ObradiPaket(Packet packet) 
        {
            switch (packet.Vrsta) 
            {
                case PacketType.HAND:
                    karte = new Hand(CardSpace.Width, CardSpace.Height);
                    this.karte.Cards = (List<Card>)packet.Sadrzaj;
                    break;
                case PacketType.INILINES:
                    this.trake = (List<Line>)packet.Sadrzaj;
                    mapa = new Map(Screen.Width, Screen.Height, trake);
                    break;
            }

            Render();
        }

        private void Log(string line)
        {
            Dispatcher.Invoke(() =>
            {
                gameLog.Text = line;
            });
        }
        private void Posalji(Socket s, Packet paket) 
        {
            byte[] buffer = new byte[1024];

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, paket);
                    buffer = ms.ToArray();
                }

                _sockTCP.Send(buffer);
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
            // logika za discard, serveru samo treba da se posalje karta umesto da je ovde dodajemo
            int cardIndx = int.Parse(choosenCard.Text);
            if (cardIndx < 1 || cardIndx > karte.Cards.Count) return;
            cardIndx--;

            if (discarded) return;

            discarded = true;

            byte[] karteBuffer = new byte[1024];

            Posalji(_sockTCP ,new Packet(PacketType.DISCARD, karte.Cards[cardIndx]));
            karte.Cards.RemoveAt(cardIndx);

            Dispatcher.Invoke(() => { Render(); });
        }

        private void pass_btn_Click(object sender, RoutedEventArgs e)
        {   //logika za teoretski kraj poteza, mozes izignorisati treba da se zameni sa serverom
            //server treba da da enemy i karte, ostalo ostaje

            discarded = false; // ovo treba da resetuje kada mu server da naznaku da nam je ponovo krenuo potez
            BoardAdvance();
            EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
            for (int i = 5 - (5 - karte.Cards.Count); i < 5; i++) karte.Cards.Add(Deck.GetRadnomCard());
            Dispatcher.Invoke(() => { Render(); });
        }

        private void play_btn_Click(object sender, RoutedEventArgs e)
        {
            //logika za igranje karata, treba dodati deo koji ce serveru poslati koja je karta odigrana
            int cardIndx = int.Parse(choosenCard.Text);
            if (cardIndx < 1 || cardIndx > karte.Cards.Count) return;
            cardIndx--;
            int unetaTrakaIndx = int.Parse(unetaTraka.Text);
            if (unetaTrakaIndx < 1 || unetaTrakaIndx > (trake.Count-  1) || karte.Cards.Count==0) return;
            unetaTrakaIndx--;
            int unetaZoneIndx = int.Parse(unetaStaza.Text);
            int unetEnemyIndx = int.Parse(unetNeprijatelj.Text);

            Card odigrana = karte.Cards[cardIndx].Play(karte.Cards, trake[unetaTrakaIndx], unetaZoneIndx, unetEnemyIndx);

            if (odigrana!=null) 
            {
                //karta uspesno odigrana, smestena u odigrana, pa se serveru moze proslediti ili ona ili index odigrane ili sta se vec odluci
                //treba deo za server ovde
                //znamo da je play uvek poslednji u potezu pa posle ovoga treba da cekamo da server da naznaku da mozemo da igramo opet


                //logika za teoretski kraj poteza, mozes izignorisati treba da se zameni sa serverom
                //server treba da da enemy i karte, ostalo ostaje
                discarded = false; // ovo treba da resetuje kada mu server da naznaku da nam je ponovo krenuo potez
                BoardAdvance(); 
                EnemyDeck.GetRadnomEnemy().Play(trake, EnemyDeck.random.Next(0, 5));
                for (int i = 5 - (5 - karte.Cards.Count); i < 5; i++) karte.Cards.Add(Deck.GetRadnomCard());
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
            //logika za disconnect potrebna

            //da se vrati na pocetni screen
            SwitchScreens();
        }
    }
}
