using CommonLibrary.Enemies;
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
    public class VracanjeUnazad : Card
    {
        public VracanjeUnazad() : base("Vracanje\nUnazad", "vraca 1 protivnika\nnazad u sumu", LineColor.LJUBICASTA)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if (traka.GetZone(zone).Count == 0 || traka.GetZone(zone).Count < enemy) return null;

            Enemy e = traka.GetZone(zone)[enemy - 1];

            traka.GetZone(zone).RemoveAt(enemy-1);
            traka.GetZone(0).Add(e);

            hand.Remove(this);

            return this;
        }

        public override Card Copy()
        {
            return new VracanjeUnazad();
        }
        public override void Draw(DrawingContext dc, Point origin, bool highlight)
        {
            SolidColorBrush brush = highlight ? Brushes.DarkGray : Brushes.DimGray;
            Pen pen = new Pen(new SolidColorBrush(ConvertToColour()), 6);

            Rect r = new Rect(new Point(origin.X - cardWidth / 2, origin.Y - cardHeight / 2), new Point(origin.X + cardWidth / 2, origin.Y + cardHeight / 2));
            dc.DrawRoundedRectangle(brush, pen, r, 4, 4);

            FormattedText text = new FormattedText(Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 14, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - Name.Length / 2 * 3.4, origin.Y - 30));

            text = new FormattedText(Effect, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Sagoe UI"), 8, Brushes.White, 2);
            dc.DrawText(text, new Point(origin.X - 30, origin.Y+10));
        }
    }
}
