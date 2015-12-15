using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;
//using System.Reflection;

namespace SevenWonders
{
    public class Deck
    {
        public int age;

        //array of cards, which will represent the cards in the deck
        List<Card> cardList = new List<Card>();

        /// <summary>
        /// Load the cards by reading the File.
        /// Add Card objects to the card array
        /// </summary>
        /// <param name="cardFile"></param>
        public Deck(List<Card> cardList, int age, int numOfPlayers)
        {
            this.age = age;

            // Create the card list for this age & number of players
            foreach (Card c in cardList.Where(x => x.age == age))
            {
                for (int i = 0; i < c.GetNumCardsAvailable(numOfPlayers); ++i)
                {
                    if (c.expansion == ExpansionSet.Original || c.expansion == ExpansionSet.Leaders)
                    {
                        // remove the Cities cards for now.
                        this.cardList.Add(c);
                    }
                }
            }
        }

        //find and remove all unused cards Guild cards
        public void removeAge3Guilds(int nCardsToRemove)
        {
            //shuffle first to randomize the locations of the guild cards in the deck
            shuffle();

            for (int i = cardList.Count - 1; i >= 0 && nCardsToRemove > 0; --i)
            {
                if (cardList[i].structureType == StructureType.Guild)
                {
                    cardList.RemoveAt(i);
                    --nCardsToRemove;
                }
            }
        }

        //shuffle the cards in the deck
        public void shuffle()
        {
            var c = Enumerable.Range(0, cardList.Count);
            var shuffledcards = c.OrderBy(a => Guid.NewGuid()).ToArray();

            Console.Write("Shuffled card array: [");
            Console.Write("{0}, ", string.Join(", ", shuffledcards));
            Console.WriteLine(" ]");

            List<Card> d = new List<Card>(cardList.Count);

            for (int i = 0; i < cardList.Count; ++i)
            {
#if TRUE
                d.Add(cardList[shuffledcards[i]]);
#else
                // Make the game deterministic for now.
                d.Add(cardList[i]);
#endif
            }

            cardList = d;
        }

        public Card GetTopCard()
        {
            Card topCard = cardList.First();

            //remove the random card
            cardList.RemoveAt(0);

            //return the random card
            return topCard;
        }
    }
}
