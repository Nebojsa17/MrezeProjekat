using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonLibrary.Enemies
{
    [Serializable]
    public class Rock : Enemy
    {
        public Rock() : base("Veliki Kamen", -1)
        {

        }

        public override void Play(List<Line> lin, int indx)
        {
            Line l = lin[indx];

            List<List<Enemy>> zone = new List<List<Enemy>>() { l.SumaZona, l.StrelacZona, l.VitezZona, l.MacevalacZona };
            l.BrojZidina = 0;
            foreach (List<Enemy> zona in zone)
            {
                for (int i = 0; i < zona.Count; i++)
                {
                    l.DmgEnemy(zone.IndexOf(zona), i+1, 1000);
                }
            }
        }

        public override Enemy Copy()
        {
            return new Rock();
        }
    }
}
