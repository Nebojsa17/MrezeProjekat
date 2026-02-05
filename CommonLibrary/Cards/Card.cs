using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonLibrary.Cards
{
    [Serializable]
    public abstract class Card
    {
        protected const double cardWidth = 80;
        protected const double cardHeight = 100;

        public string Name { get; set; }
        public string Effect { get; set; }
        public LineColor CColor { get; set; }

        public Card(string name, string effect, LineColor color) 
        {
            Name = name;
            Effect = effect;
            CColor = color;
        }

        public virtual void Draw(DrawingContext dc, Point origin, bool highlight) 
        {
            SolidColorBrush brush = highlight? Brushes.DarkGray : Brushes.DimGray;
            Pen pen = new Pen(new SolidColorBrush(ConvertToColour()), 6);

            Rect r = new Rect(new Point(origin.X - cardWidth / 2, origin.Y - cardHeight / 2), new Point(origin.X + cardWidth / 2, origin.Y + cardHeight / 2));
            dc.DrawRoundedRectangle(brush, pen, r,4,4);

            FormattedText text = new FormattedText(Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 14, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - Name.Length/2 * 9 , origin.Y - 30)); 
            
            text = new FormattedText(Effect, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 8, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - 35, origin.Y));
        }
        protected Color ConvertToColour()
        {
            switch (CColor)
            {
                case LineColor.PLAVA:
                    return Color.FromRgb(0, 0, 255);
                case LineColor.ZELENA:
                    return Color.FromRgb(0, 255, 0);
                case LineColor.CRVENA:
                    return Color.FromRgb(255, 0, 0);
                case LineColor.LJUBICASTA:
                    return Color.FromRgb(179, 0, 255);
                default:
                    return Color.FromRgb(0, 0, 255);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;

            return ((Card)obj).Name == this.Name;
        }

        public abstract Card Play(List<Card> hand, Line traka, int zone, int enemy);

        public abstract Card Copy();
    }
}
