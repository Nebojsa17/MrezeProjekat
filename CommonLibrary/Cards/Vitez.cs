using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Vitez : Card
    {
        int dmg = 1;

        public Vitez(LineColor c) : base("Vitez", "udara 1 protivnika\n u vitez zoni", c)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if (zone != 2 && this.CColor != LineColor.LJUBICASTA) return null;
            if (this.CColor != LineColor.LJUBICASTA && traka.LColor != this.CColor) return null;

            if (!traka.DmgEnemy(zone, enemy, dmg)) return null;
            hand.Remove(this);

            return this;
        }

        public override Card Copy()
        {
            return new Vitez(this.CColor);
        }
    }
}
