using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Cards
{
    public class Hand
    {
        public const int HandSize = 5;
        public List<Card> Cards { get; set; }
        double width; 
        double height;

        public Hand()
        {
            Cards = new List<Card>();
            width = 0;
            height = 0;
        }

        public Hand(double width, double height) 
        {
            Cards = new List<Card>();
            this.width = width;
            this.height = height;
        }

        public void Draw(DrawingContext dc) 
        {
            double heightOrigin = height / 2;
            double widthBlok = width / HandSize;
            int itt = 0;

            foreach (Card card in Cards) 
            {
                if (card == null) continue;
                card.Draw(dc, new Point(widthBlok/2 + widthBlok*itt,heightOrigin));
                itt++;
            }
        }

    }
}
