using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Enemies
{
    [Serializable]
    public class Trol : Enemy
    {
        public Trol() : base("Trol", 3) { }

        public override void Draw(DrawingContext dc, Point origin)
        {
            base.Draw(dc, origin);
        }
        public override Enemy Copy()
        {
            return new Trol();
        }
    }
}
