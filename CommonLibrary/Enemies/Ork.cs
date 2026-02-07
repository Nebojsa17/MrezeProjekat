using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonLibrary.Enemies
{
    [Serializable]
    public class Ork : Enemy
    {
        public Ork() : base("Ork", 2) { }

        public override void Draw(DrawingContext dc, Point origin)
        {
            BitmapImage image = new BitmapImage(new Uri("Enemies/Sprites/OrcSprite.png", UriKind.Relative));
            dc.DrawImage(image, new Rect(origin.X - 15, origin.Y - 15, 30, 30));

        }

        public override Enemy Copy()
        {
            return new Ork();
        }
    }
}
