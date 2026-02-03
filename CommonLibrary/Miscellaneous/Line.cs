using CommonLibrary.Enemies;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace CommonLibrary.Miscellaneous
{
    public enum LineColor { PLAVA, ZELENA, CRVENA, LJUBICASTA }
    
    [Serializable]
    public class Line
    {
        public int Broj { get; set; }
        public LineColor LColor { get; set; }
        public List<Enemy> SumaZona { get; set; }
        public List<Enemy> StrelacZona { get; set; }
        public List<Enemy> VitezZona { get; set; }
        public List<Enemy> MacevalacZona { get; set; }
        public int BrojZidina { get; set; }

        public Line(int br, LineColor boja) 
        {
            SumaZona = new List<Enemy>();
            StrelacZona = new List<Enemy>();
            VitezZona = new List<Enemy>();
            MacevalacZona = new List<Enemy>();
            BrojZidina = 2;
            Broj = br;
            LColor = boja;
        }

        public List<Enemy> GetZone( int zoneIndx ) 
        {
            switch (zoneIndx)
            {
                case 0: return SumaZona;
                case 1: return StrelacZona;
                case 2: return VitezZona;
                case 3: return MacevalacZona;
                default: return null;
            }
        }

        public bool DmgEnemy(int zoneIndx, int enemyIndx, int dmg) 
        {
            if (zoneIndx > 3 || zoneIndx < 0) return false;
            if(enemyIndx == 0)return false;
            if (GetZone(zoneIndx).Count < enemyIndx || GetZone(zoneIndx).Count==0) return false;

            if (GetZone(zoneIndx)[enemyIndx - 1].TakeDmg(dmg)) 
            {
                GetZone(zoneIndx).Remove(GetZone(zoneIndx)[enemyIndx - 1]);
                return true;
            }

            return true;
        }

        public void Advance()
        {
            for (int i = 0; i < MacevalacZona.Count; i++)
            {
                if (MacevalacZona[i].TakeDmg(1))
                {
                    MacevalacZona.RemoveAt(i);
                    i--;
                }
                BrojZidina--;
            }

            foreach (Enemy e in VitezZona)
            {
                MacevalacZona.Add(e);
            }
            VitezZona.Clear();

            foreach (Enemy e in StrelacZona)
            {
                VitezZona.Add(e);
            }
            StrelacZona.Clear();

            foreach (Enemy e in SumaZona)
            {
                StrelacZona.Add(e);
            }
            SumaZona.Clear();


        }



        #region ISCRTAVANJE
        private Color ConvertToColour()
        {
            switch (LColor)
            {
                case LineColor.PLAVA:
                    return Color.FromRgb(0, 0, 255);
                case LineColor.ZELENA:
                    return Color.FromRgb(0, 255, 0);
                case LineColor.CRVENA:
                    return Color.FromRgb(255, 0, 0);
                default:
                    return Color.FromRgb(0, 0, 255);
            }
        }

        public int GetSelected(Point select, int zona, double height, Point origin)
        {
            if (GetZone(zona + 1).Count == 0) return 0;

            int lineW = 42;
            Rect r = new Rect(new Point(origin.X + lineW, origin.Y + zona*(height / 3)), new Point(origin.X - lineW, origin.Y + (zona+1) * (height / 3)));
            double sampleWidth = r.Width / 2; 
            double sampleHeight = r.Height / 2;
            int enemyItt = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (enemyItt >= GetZone(zona + 1).Count) return 0;

                    if (select.X >= r.X + (j * sampleWidth) && select.X <= r.X + ((j + 1) * sampleWidth) && select.Y >= r.Y + (i) * sampleHeight && select.Y <= r.Y + (i + 1) * sampleHeight)
                    {
                        return enemyItt+1;
                    }
                    enemyItt++;
                }
            }

            return 0;
        }

        public void Draw(DrawingContext dc, Point origin, double height, double maxHeight) 
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(189, 159, 62));
            Pen pen = new Pen(new SolidColorBrush(ConvertToColour()), 2);

            int lineW = 42;

            //strelac zona
            Rect r = new Rect(new Point(origin.X+ lineW, origin.Y), new Point(origin.X- lineW, origin.Y + height));
            dc.DrawRectangle(brush, pen, r);
            DrawZone(StrelacZona, dc, new Point(origin.X - 20, origin.Y));

            //vitez zona
            lineW = 41;
            brush = new SolidColorBrush(Color.FromRgb(156, 139, 87));
            pen = new Pen(new SolidColorBrush(ConvertToColour()), 0);
            r = new Rect(new Point(origin.X + lineW, origin.Y + height / 3), new Point(origin.X - lineW, origin.Y + 2*(height / 3)));
            dc.DrawRectangle(brush, pen, r);
            DrawZone(VitezZona, dc, new Point(origin.X-20, origin.Y + (height / 3)));

            //macevalac zona
            brush = new SolidColorBrush(Color.FromRgb(148, 139, 111));
            r = new Rect(new Point(origin.X + lineW, origin.Y + 2*(height / 3)), new Point(origin.X - lineW, origin.Y + height));
            dc.DrawRectangle(brush, pen, r);
            DrawZone(MacevalacZona, dc, new Point(origin.X-20, origin.Y + 2*(height / 3)));


            double wallPadding = 30;
           if(BrojZidina>0) DrawWall(dc, new Point(origin.X - (lineW + wallPadding), maxHeight+5), 2*(lineW + wallPadding));
        }

        public void DrawZone(List<Enemy> zone, DrawingContext dc, Point origin) 
        {
            int enemyItt = 0;
            for(int i=0; i<4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (enemyItt >= zone.Count) return;
                    zone[enemyItt].Draw(dc, new Point(origin.X + 40*j, origin.Y + 20 + 40*i));

                    enemyItt++;
                }
            }
        }
        
        int[,] wall = {
            { 1, 1, 0, 1, 2, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 3, 3, 0, 1, 1},
            { 1, 2, 2, 3, 3, 1, 1, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 1, 1},
            { 0, 1, 2, 2, 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 0},
            { 0, 3, 3, 1, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 3, 2, 2, 1, 0},
            { 0, 1, 2, 2, 3, 3, 1, 0, 3, 3, 0, 3, 1, 0, 1, 1, 0, 2, 1, 0, 2, 2, 1, 1, 3, 3, 0},
            { 0, 1, 1, 3, 3, 1, 1, 2, 2, 1, 1, 2, 2, 3, 3, 1, 1, 3, 3, 1, 1, 1, 1, 2, 2, 1, 0},
            { 0, 1, 1, 1, 1, 1, 3, 3, 1, 1, 3, 3, 1, 1, 1, 1, 3, 3, 2, 2, 1, 1, 3, 3, 1, 1, 0},
            { 0, 3, 3, 1, 1, 1, 1, 2, 2, 1, 1, 3, 3, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 2, 0},
            { 0, 2, 1, 1, 3, 3, 2, 2, 1, 1, 3, 3, 1, 1, 1, 1, 1, 1, 1, 3, 3, 1, 1, 1, 3, 3, 0},
            { 0, 2, 2, 2, 2, 1, 1, 3, 3, 1, 1, 2, 2, 2, 2, 3, 3, 1, 2, 2, 1, 1, 2, 2, 1, 1, 0},
            { 0, 1, 3, 3, 1, 1, 2, 2, 2, 2, 1, 1, 3, 3, 1, 1, 2, 2, 1, 1, 1, 2, 2, 3, 3, 1, 0}
        };

        private void DrawWall(DrawingContext dc, Point origin, double width)
        {
            double segmentSize = width / wall.GetLength(1);
            SolidColorBrush brush;
            Pen pen;

            for (int i = 0; i < wall.GetLength(0); i++)
            {
                for (int j = 0; j < wall.GetLength(1); j++)
                {
                    byte shade = (byte)(190 - 10 * wall[i, j]);
                    brush = new SolidColorBrush(Color.FromRgb(shade, shade, shade));
                    pen = new Pen(brush, 1);

                    if (wall[i, j] != 0)
                    {
                        Rect rect = new Rect(new Point(origin.X + segmentSize * j, origin.Y - segmentSize * (wall.GetLength(0) - i)), new Point(origin.X + segmentSize * j + segmentSize, origin.Y - segmentSize * (wall.GetLength(0) - i) - segmentSize));
                        dc.DrawRectangle(brush, pen, rect);
                    }
                }
            }

            brush = new SolidColorBrush(Color.FromRgb(110, 110, 110));
            pen = new Pen(brush, 1);
            Rect r = new Rect(new Point(origin.X + segmentSize * (wall.GetLength(1) / 2) - 20, origin.Y - 10), new Point(origin.X + segmentSize * (wall.GetLength(1) / 2) + 20, origin.Y - 30));
            dc.DrawRoundedRectangle(brush, pen, r, 5, 5);

            FormattedText text = new FormattedText(BrojZidina.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 15, Brushes.White, 3);
            dc.DrawText(text, new Point(origin.X + segmentSize * (wall.GetLength(1) / 2) - 5, origin.Y - 28));
        }

        #endregion

    }
}
