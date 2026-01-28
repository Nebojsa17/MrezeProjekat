using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CommonLibrary.Sprites
{
    public class Tree : IEnvironmentObject
    {

        Point offsetOrigin, origin; 
        double blockWidth;  
        double blockHeight;
        double drawScale;
        int sign = 1;

        public Tree(Point origin, double scale, int sign) 
        {
            this.origin = new Point(origin.X, origin.Y);
            this.drawScale = scale;
            this.sign = sign;
        }

        public void Draw(DrawingContext dc)
        {
            TreeSprite1(dc,drawScale);
        }

        private void SetStats(double logW, double logH, double offsetW, double offsetH, double scale) 
        {
            blockWidth = logW * scale * sign;
            blockHeight = logH * scale;
            offsetOrigin = new Point(origin.X, origin.Y);
            offsetOrigin.X += offsetW * scale * sign;
            offsetOrigin.Y -= offsetH * scale;

        }

        private Rect Shape() 
        {
            return new Rect(new Point(offsetOrigin.X - blockWidth, offsetOrigin.Y - blockHeight), new Point(offsetOrigin.X + blockWidth, offsetOrigin.Y));
        }

        public void TreeSprite1(DrawingContext dc, double scale) 
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(122, 65, 12));
            Pen pen = new Pen(brush, 1);

            SetStats(9, 12, 0, 0, scale);
            Rect rect = Shape();
            dc.DrawRectangle(brush,pen,rect);

            SetStats(6, 20, 0, 12, scale);
            rect = Shape();
            dc.DrawRectangle(brush, pen, rect);

            SetStats(4, 20, 0, 30, scale);
            rect = Shape(); 
            dc.DrawRectangle(brush, pen, rect);

            SetStats(6, 5, 9, 0, scale);
            rect = Shape(); 
            dc.DrawRectangle(brush, pen, rect); 

            SetStats(7, 5, -10, -3, scale);
            rect = Shape(); 
            dc.DrawRectangle(brush, pen, rect);

            brush = new SolidColorBrush(Color.FromRgb(17, 153, 37));
            pen = new Pen(brush, 2);
            SetStats(20, 30, -5, 50, scale);
            rect = Shape();
            dc.DrawRectangle(brush, pen, rect); 

            brush = new SolidColorBrush(Color.FromRgb(23, 173, 45));
            SetStats(25, 27, -20, 36, scale);
            pen = new Pen(brush, 2);
            rect = Shape();
            dc.DrawRectangle(brush, pen, rect); 

            brush = new SolidColorBrush(Color.FromRgb(21, 191, 47));
            pen = new Pen(brush, 2);
            SetStats(20, 21, 7, 39, scale);
            rect = Shape();
            dc.DrawRectangle(brush, pen, rect);
        }
    }
}
