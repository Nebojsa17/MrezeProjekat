using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Enemies
{
    public class Ork : Enemy
    {
        public Ork() : base("Ork", 2) { }

        public override void Draw(DrawingContext dc, Point origin)
        {
            base.Draw(dc, origin);
        }
    }
}
