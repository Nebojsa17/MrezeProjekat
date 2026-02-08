using CommonLibrary.Enemies;
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
    public class Cigla : Card
    {
        public Cigla() : base("Cigla", "sa malterom\npovecava zid", LineColor.LJUBICASTA)
        {

        }

        public override Card Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            int indx = 0;
            if (!played)
            {
                foreach (Card c in hand)
                {
                    if (typeof(Malter) == c.GetType())
                    {
                        break;
                    }
                    indx++;
                }
                if (indx>=hand.Count) return null;
            }

            traka.BrojZidina++;
            if (traka.BrojZidina > 2) traka.BrojZidina = 2;

            if (hand != null)
            {
                hand.RemoveAt(indx);
                hand.Remove(this);
            }

            played = true;
            enemyStruck = enemy;
            zoneTargeted = zone;
            linePlayed = traka.Broj;

            return this;
        }

        public override Card Copy()
        {
            return new Cigla();
        }
    }
}
