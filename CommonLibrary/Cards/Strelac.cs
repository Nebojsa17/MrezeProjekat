using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Cards
{
    public class Strelac : Card
    {
        public Strelac(LineColor c) : base("STRELAC","udara 1 protivnika\n u strelac zoni",c)  
        {

        }

        public override void Play(List<Card> hand, Line traka, int zone, int enemy)
        {
            
        }

        public override Card Copy()
        {
            return new Strelac(this.CColor);
        }
    }
}
