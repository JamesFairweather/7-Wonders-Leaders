using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    /// <summary>
    /// This public class will load up board information from a textfile
    /// After that, it will essentially be read-only
    /// </summary>
    public class Board
    {
        public enum Wonder
        {
            Alexandria_A,
            Alexandria_B,
            Babylon_A,
            Babylon_B,
            Ephesos_A,
            Ephesos_B,
            Giza_A,
            Giza_B,
            Halikarnassos_A,
            Halikarnassos_B,
            Olympia_A,
            Olympia_B,
            Rhodos_A,
            Rhodos_B,
            Roma_A,
            Roma_B,
        };

        public ExpansionSet expansionSet;

        public Wonder otherSide;

        public string name { get; private set; }

        //number of stages
        public int numOfStages { get; private set; }

        // public Effect freeResource { get; private set; }

        // starting resource or effect
        public Card startingResourceCard { get; private set; }

        //stage costs & effects
        public List<Card> stageCard;

        public bool inPlay;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="otherside">Name of the other side of this Wonder.  Kind of useless</param>
        /// <param name="name">Name of the active side of the Wonder (e.g. "Giza (B)")</param>
        /// <param name="effect">This is the starting resource or effect that the Wonder Board provides</param>
        /// <param name="nStages">The number of stages this wonder has (1, 2, 3, 4)</param>
        public Board(ExpansionSet e, Wonder otherside, string name, CardId cardId, Effect boardEffect, int nStages)
        {
            this.expansionSet = e;
            this.otherSide = otherside;
            this.name = name;
            this.startingResourceCard = new Card(cardId, name, boardEffect);
            this.numOfStages = nStages;

            inPlay = false;
        }
    }
}
