using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Malter : Card
    {
        public Malter() : base("Malter", "sa ciglom\npovecava zid", LineColor.LJUBICASTA)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            Cigla m = null;
            foreach (Card c in hand)
            {
                if (typeof(Cigla) == c.GetType())
                {
                    m = (Cigla)c;
                }
            }
            if (m == null) return null;

            traka.BrojZidina++;
            if (traka.BrojZidina > 2) traka.BrojZidina = 2;

            hand.Remove(this);
            hand.Remove(m);

            return this;
        }

        public override Card Copy()
        {
            return new Malter();
        }
    }
}
