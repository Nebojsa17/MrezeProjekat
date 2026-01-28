using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Enemies
{
    public class EnemyAdvance : Enemy
    {
        private LineColor strikeColor;
        public EnemyAdvance(LineColor colorCode) : base("Neprijatelji se pomeraju napred", -1) 
        {
            strikeColor = colorCode;
        }

        public override void Play(List<Line> lin, int indx)
        {
            foreach (Line l in lin)
            {
                if(l.LColor==strikeColor)l.Advance();
            }
        }
    }
}
