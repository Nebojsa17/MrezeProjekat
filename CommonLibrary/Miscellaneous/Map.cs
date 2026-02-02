using CommonLibrary.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CommonLibrary.Miscellaneous
{
    public class Map
    {
        const int block = 30;
        const int TreeZone = 4;
        const int LineZone = 4;
        const double TreeFactor = 4.5;

        private  List<IEnvironmentObject> eObject = new List<IEnvironmentObject>();
        public List<Line> trake { get; set; }
        Point blockSize = new Point();
        Point actualSize = new Point();

        public Map(double width, double height, List<Line> tr) 
        {
            blockSize.X = width / (int)(width/block);
            blockSize.Y = height / (int)(height / block);
            actualSize = new Point(width, height);
            trake = tr;

            Initialize();
        }

        public void Initialize() 
        {
            int widthCells = (int)(actualSize.X / block);
            int heightCells = (int)(actualSize.Y / block);

            for (int i = 0; i < heightCells; i++)
            {
                for (int j = 0; j < widthCells; j++)
                {
                    //random na osnovu i,j
                    int seed = i * 57394675 ^ j * 204785690;
                    Random rand = new Random(seed);

                    //rasporedi drvece po gornjoj ivici:
                    if (GenerateSemiRandom(i, j, 0, 10 ,rand) <= TreeFactor && i < TreeZone)
                    {
                        eObject.Add(new Tree(new Point(blockSize.X / 2 + blockSize.X * j, blockSize.Y / 2 + blockSize.Y * i), GenerateSemiRandom(i, j, 0.64, 1.2, rand),Math.Sign(GenerateSemiRandom(i, j, -1, 1, rand))));
                    }
                }
            }

        }
        double GenerateSemiRandom(int i, int j, double floor, double ceil, Random rand)
        {
            return floor + rand.NextDouble() * (ceil - floor);
        }

        public void Draw(DrawingContext dc) 
        {
            //Draw base
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(96, 189, 62));
            Pen pen = new Pen(brush, 1);
            Rect floor = new Rect(new Point(0, 0), actualSize);
            dc.DrawRectangle(brush, pen, floor);

            //Draw Env object
            foreach(IEnvironmentObject ie in eObject) 
            {
                ie.Draw(dc);
            }

            //Draw lines 
            double lineAvailableWidth = actualSize.X / trake.Count;
            double lineAvailableHeight = actualSize.Y - blockSize.Y*LineZone;
            foreach (Line t in trake) 
            {
                t.Draw(dc, new Point(lineAvailableWidth / 2 + lineAvailableWidth * (t.Broj - 1), blockSize.Y * LineZone), lineAvailableHeight - blockSize.Y, actualSize.Y);
            }
        }

    }
}
