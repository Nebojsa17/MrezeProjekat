using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Hand
    {
        public const int HandSize = 5;
        public List<Card> Cards { get; set; }
        double width; 
        double height;
        int selected = -1;

        public Hand(double width, double height) 
        {
            Cards = new List<Card>();
            this.width = width;
            this.height = height;
        }

        public int GetSelect(Point selected) 
        {
            double x = selected.X;

            int indx =(int)( x / (width / HandSize)) + 1;
            this.selected = indx;
            return indx;
        }

        public void Draw(DrawingContext dc) 
        {
            double heightOrigin = height / 2;
            double widthBlok = width / HandSize;
            int itt = 0;

            for(int i=0;i<Cards.Count; i++) 
            {
                if (Cards[i] == null) continue;
                if(i!=(selected-1))Cards[i].Draw(dc, new Point(widthBlok/2 + widthBlok*itt,heightOrigin),false);
                else Cards[i].Draw(dc, new Point(widthBlok / 2 + widthBlok * itt, heightOrigin), true);
                itt++;
            }
        }

    }
}
