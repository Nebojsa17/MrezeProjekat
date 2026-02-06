using CommonLibrary.Enemies;
using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    [Serializable]
    public class Cigla : Card
    {
        public Cigla() : base("Cigla", "sa malterom\npovecava zid", LineColor.LJUBICASTA)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            Malter m = null;
            foreach(Card c in hand) 
            {
                if(typeof(Malter) == c.GetType()) 
                {
                    m = (Malter)c;
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
            return new Cigla();
        }
    }
}
