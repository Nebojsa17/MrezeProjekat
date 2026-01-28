using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Enemies
{
    public class Enemy
    {
        string Name { get; set; } = string.Empty;

        int HP { get; set; } = 0;

        public Enemy(string name, int hp)
        {
            this.Name = name;
            this.HP = hp;
        }

        public virtual void Play(List<Line> lin, int indx) 
        {
            lin[indx].SumaZona.Add(this);
        }

        public virtual bool TakeDmg(int dmg) 
        {
            HP -= dmg;

            if (HP <= 0) return true;
            return false;
        }

        public virtual void Draw(DrawingContext dc, Point origin) 
        {
            FormattedText text = new FormattedText(" "+Name+"\n "+HP, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 12, Brushes.Black,3);

            dc.DrawText(text, (Point)Point.Subtract(origin,new Point(Name.Length*4,0)));
        }
    }
}
