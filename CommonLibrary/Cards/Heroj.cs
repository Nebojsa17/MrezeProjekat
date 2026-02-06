using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Heroj : Card
    {
        int dmg = 2;

        public Heroj(LineColor c) : base("Heroj", "udara 1 protivnika", c)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            if (traka.LColor != this.CColor) return null;

            if (!traka.DmgEnemy(zone, enemy, dmg)) return null;
            hand.Remove(this);

            return this;
        }

        public override Card Copy()
        {
            return new Heroj(this.CColor);
        }
    }
}
