using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Strelac : Card
    {
        int dmg = 1;

        public Strelac(LineColor c) : base("STRELAC","udara 1 protivnika\n u strelac zoni",c)  
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if(zone != 1 && this.CColor != LineColor.LJUBICASTA) return null;
            if(this.CColor != LineColor.LJUBICASTA && traka.LColor != this.CColor) return null;

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
            return new Strelac(this.CColor);
        }
    }
}
