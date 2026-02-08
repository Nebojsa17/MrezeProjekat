using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace CommonLibrary.Enemies
{
    [Serializable]
    public class Trol : Enemy
    {
        public Trol() : base("Trol", 3) { }

        public override void Draw(DrawingContext dc, Point origin)
        {
            int w = 25;
            Rect r = new Rect(origin.X - 7, origin.Y - 35, 14, 15);
            dc.DrawRoundedRectangle(Brushes.DarkRed, new Pen(Brushes.DarkRed, 1), r, 1, 1);
            FormattedText text = new FormattedText(HP.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 12, Brushes.White, 3);
            dc.DrawText(text, (Point)Point.Subtract(origin, new Point(5, 35)));

            BitmapImage image = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Enemies/Sprites/TrolSprite.png"), UriKind.Relative));
            dc.DrawImage(image, new Rect(origin.X - w, origin.Y - w, 2 * w, 2 * w));

        }
        public override Enemy Copy()
        {
            return new Trol();
        }
    }
}
