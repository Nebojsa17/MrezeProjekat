using CommonLibrary;
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
        DrawingVisual dv = new DrawingVisual();
        Map mapa;
        List<CommonLibrary.Miscellaneous.Line> trake = new List<CommonLibrary.Miscellaneous.Line>();

        public MainWindow()
        {
            InitializeComponent();

            Screen.AddVisual(dv);

            trake.Add(new CommonLibrary.Miscellaneous.Line(1, LineColor.PLAVA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(2, LineColor.PLAVA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(3, LineColor.ZELENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(4, LineColor.ZELENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(5, LineColor.CRVENA));
            trake.Add(new CommonLibrary.Miscellaneous.Line(6, LineColor.CRVENA));
            mapa = new Map(Screen.Width,Screen.Height,trake);

            Enemy e;
            e = new Goblin();
            e.Play(trake, 0);
            e = new Trol();
            e.Play(trake, 0);
            trake[0].Advance();

            e = new Ork();
            e.Play(trake, 0);
            e = new Ork();
            e.Play(trake, 2);
            e = new Ork();
            e.Play(trake, 1);
            e = new EnemyAdvance(LineColor.PLAVA);
            e.Play(trake, -1);

            trake[1].Advance();
            trake[1].Advance();
            trake[1].Advance();
            trake[1].Advance();
            trake[2].Advance();
            trake[2].Advance();
            trake[2].Advance();
            trake[2].Advance();

            Render();
        }

        public void Render()
        {
            using (DrawingContext dc = dv.RenderOpen())
            {
                mapa.Draw(dc);
            }
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }


    }
}
