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
    public class Varvarin : Card
    {
        int dmg = 100;

        public Varvarin() : base("Varvarin", "eleminise 1\n protivnika", LineColor.LJUBICASTA)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if (!traka.DmgEnemy(zone, enemy, dmg)) return null;
            hand.Remove(this);

            return this;
        }

        public override Card Copy()
        {
            return new Varvarin();
        }

        public override void Draw(DrawingContext dc, Point origin, bool highlight)
        {
            SolidColorBrush brush = highlight ? Brushes.DarkGray : Brushes.DimGray;
            Pen pen = new Pen(new SolidColorBrush(ConvertToColour()), 6);

            Rect r = new Rect(new Point(origin.X - cardWidth / 2, origin.Y - cardHeight / 2), new Point(origin.X + cardWidth / 2, origin.Y + cardHeight / 2));
            dc.DrawRoundedRectangle(brush, pen, r, 4, 4);

            FormattedText text = new FormattedText(Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 14, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - Name.Length / 2 * 6, origin.Y - 30));

            text = new FormattedText(Effect, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 8, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - 20, origin.Y));
        }
    }
}
