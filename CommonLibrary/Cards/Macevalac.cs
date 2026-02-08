using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Macevalac : Card
    {
        int dmg = 2;

        public Macevalac(LineColor c) : base("Macevalac", "udara 1 protivnika\n u macevalac zoni", c)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if (zone != 3 && this.CColor != LineColor.LJUBICASTA) return null;
            if (this.CColor!=LineColor.LJUBICASTA && traka.LColor != this.CColor) return null;

            if (!traka.DmgEnemy(zone, enemy, dmg)) return null;

            if (hand != null) hand.Remove(this);
            played = true;
            enemyStruck = enemy;
            zoneTargeted = zone;
            linePlayed = traka.Broj;

            return this;
        }

        public override Card Copy()
        {
            return new Macevalac(this.CColor);
        }
    }
}
