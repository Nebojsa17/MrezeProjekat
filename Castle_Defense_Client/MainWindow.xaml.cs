using CommonLibrary;
using CommonLibrary.Cards;
using CommonLibrary.Enemies;
using CommonLibrary.Miscellaneous;
using CommonLibrary.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Castle_Defense_Client
{
    public partial class MainWindow : Window
    {
        //For web
        public const int SERVER_PORT = 51000;
        private Socket _sockUDP, _sockTCP;
        private CancellationTokenSource _cts;
        private Task _rxTask;


        //For game
        DrawingVisual dvGame = new DrawingVisual();
        DrawingVisual dvCards = new DrawingVisual();
        Map mapa;
        Hand karte;
        List<CommonLibrary.Miscellaneous.Line> trake = new List<CommonLibrary.Miscellaneous.Line>();

        public MainWindow()
        {
            InitializeComponent();
            Screen.AddVisual(dvGame);
            CardSpace.AddVisual(dvCards);


            Deck.InitializeDeck(3);

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

            SwitchScreens();

            Dispatcher.Invoke(() => { Render(); });
        }

        public void Render()
        {
            using (DrawingContext dc = dvGame.RenderOpen())
            {
                mapa.Draw(dc);
            }

            using (DrawingContext dc = dvCards.RenderOpen())
            {
                karte.Draw(dc);
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

            }
            catch (SocketException er)
            {
                Logger.Content = $"LOG: Greska prilikom prijema poruke: {er.Message}.";
                return;
            }
        }

        private void discard_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pass_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void play_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void swap_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            //logika za disconnect potrebna

            //da se vrati na pocetni screen
            SwitchScreens();
        }
    }
}
