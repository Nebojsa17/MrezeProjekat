using CommonLibrary.Cards;
using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonLibrary.Enemies
{
    public class EnemyDeck
    {
        public static Random random = new Random();
        public static List<EnemyPair> deck = new List<EnemyPair>();

        public static void InitializeDeck(int plNum)
        {
            deck.Clear();
            deck.Add(new EnemyPair(new Goblin(), SlashInstance(plNum, 12)));
            deck.Add(new EnemyPair(new Ork(), SlashInstance(plNum, 11)));
            deck.Add(new EnemyPair(new Trol(), SlashInstance(plNum, 8)));
            deck.Add(new EnemyPair(new EnemyAdvance(LineColor.LJUBICASTA), SlashInstance(plNum, 1)));
        }

        private static int SlashInstance(int plNum, int inst)
        {
            if (inst <= 4 && plNum == 1) return 1;
            if (inst == 1 && plNum == 2) return 2;
            return inst;
        }

        public static Enemy GetRadnomEnemy()
        {

            Enemy izabrana = null;

            List<int> slobodni = new List<int>();
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i].Num > 0) slobodni.Add(i);
            }

            if (slobodni.Count == 0) return null;

            int indx = slobodni[random.Next(0, slobodni.Count)];

            izabrana = deck[indx].Enemy.Copy();
            deck[indx].Num--;

            return izabrana;
        }
    }

    public class EnemyPair
    {
        public Enemy Enemy { get; set; }
        public int Num { get; set; }

        public EnemyPair(Enemy enemy, int num)
        {
            this.Enemy = enemy;
            this.Num = num;
        }
    }
}
