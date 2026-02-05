using CommonLibrary.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonLibrary.Cards
{
    public class Deck
    {
        public static Random random = new Random();
        public static List<CardPair> deck = new List<CardPair>();

        public static void InitializeDeck(int plNum) 
        {
            deck.Clear();
            for (int i = 0; i < plNum; i++)
            {
                deck.Add(new CardPair(new Strelac((LineColor)i), 3));
                deck.Add(new CardPair(new Strelac(LineColor.LJUBICASTA), 1));
                deck.Add(new CardPair(new Vitez((LineColor)i), 3));
                deck.Add(new CardPair(new Vitez(LineColor.LJUBICASTA), 1));
                deck.Add(new CardPair(new Macevalac((LineColor)i), 3));
                deck.Add(new CardPair(new Macevalac(LineColor.LJUBICASTA), 1));
                deck.Add(new CardPair(new Heroj((LineColor)i), 1));
                deck.Add(new CardPair(new Varvarin(), 1));
                deck.Add(new CardPair(new VracanjeUnazad(), 1));
            }
        }

        public static void ReturnCard(Card returned) 
        {
            foreach(CardPair cardP in deck) 
            {
                if (cardP.Card == returned)
                {
                    MessageBox.Show("returned");
                    cardP.Num++;
                    return;
                }
            }
        }

        public static Card GetRadnomCard() 
        {

            Card izabrana = null;

            List<int> slobodni = new List<int>();
            for(int i=0; i<deck.Count; i++) 
            {
                if (deck[i].Num > 0) slobodni.Add(i);
            }

            if(slobodni.Count==0) return null;

            int indx = slobodni[random.Next(0, slobodni.Count)];

            izabrana = deck[indx].Card.Copy();
            deck[indx].Num--;


            return izabrana;
        }
    }

    public class CardPair 
    {
        public Card Card { get; set; }
        public int Num { get; set; }

        public CardPair(Card card, int num) 
        {
            this.Card = card;
            this.Num = num;
        }
    }
}
