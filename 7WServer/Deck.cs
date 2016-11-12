using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.IO;
//using System.Reflection;
using NLog;

namespace SevenWonders
{
    public class Deck
    {
        public int age;

        private static Logger logger = LogManager.GetLogger("SevenWondersServer");

        static Guid shuffleGuid = Guid.NewGuid();

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
                    this.cardList.Add(c);
                }
            }

            if (age == 1)
            {
                // Log the shuffle GUID so this game can be played again if a bug is found.
                logger.Info("Shuffle Guid = {0}", shuffleGuid);
            }
        }

        public void removeCityCards(int nCardsToRemove)
        {
            //shuffle first to randomize the locations of the city cards in the deck
            shuffle();

            for (int i = cardList.Count - 1; i >= 0 && nCardsToRemove > 0; --i)
            {
                if (cardList[i].structureType == StructureType.City)
                {
                    cardList.RemoveAt(i);
                    --nCardsToRemove;
                }
            }
        }

        // find and remove all unused cards Guild cards
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
            int[] shuffledcards = Enumerable.Range(0, cardList.Count).ToArray();
            // var shuffledcards = c.OrderBy(a => shuffleGuid).ToArray();
            // var g = Guid.NewGuid();
            // var shuffledcards = c.OrderBy(a => g).ToArray();

            int randomSeed = 0;

            logger.Info("Shuffle random seed = {0}", randomSeed);

            Random r = new Random(randomSeed);

            for (int i = shuffledcards.Length; i > 0; i--)
            {
                int j = r.Next(i);  // j = a random number between 0 & i.

                // swap the number at jth index of the array with the ith one.
                int k = shuffledcards[j];
                shuffledcards[j] = shuffledcards[i - 1];
                shuffledcards[i - 1] = k;
            }

            // Console.Write("Shuffled card array: [");
            // Console.Write("{0}, ", string.Join(", ", shuffledcards));
            // Console.WriteLine(" ]");

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
