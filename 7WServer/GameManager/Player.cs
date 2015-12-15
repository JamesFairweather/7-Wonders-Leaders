using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SevenWonders
{
    public class Player
    {
        public bool isAI {get; set;}

        public String nickname { get; set; }

        // public String GetNickName() { return nickname; }

        public Board playerBoard { get; set; }

        // Last Wonder Stage that has been built (add 1 to get the next wonder stage to be built)
        public int currentStageOfWonder { get; set; }

        //resources
        public int brick {
            get {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('B')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'B');
                }

                return n;
            }
        }

        public int ore {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('O')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'O');
                }

                return n;
            }
        }

        public int stone {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('S')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'S');
                }

                return n;
            }
        }

        public int wood {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('W')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'W');
                }

                return n;
            }
        }

        public int glass {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('G')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'G');
                }

                return n;
            }
        }

        public int loom {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('C')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'C');
                }

                return n;
            }
        }

        public int papyrus {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.RawMaterial && ((ResourceEffect)x.effect).resourceTypes.Contains('P')))
                {
                    n += ((ResourceEffect)c.effect).resourceTypes.Count(x => x == 'P');
                }

                return n;
            }
        }

        public int coin { get; private set; }

        //Points and stuff
        // public int victoryPoint { get; private set; }

        public int shield
        {
            get
            {
                int n = 0;

                foreach (Card c in playedStructure.Where(x => x.effect is MilitaryEffect))
                {
                    n += ((MilitaryEffect)c.effect).nShields;
                }

                // Add Rhodos B Wonder Board military effects
                if (playedStructure.Exists(x => x.Id == CardId.Rhodos_B_s1))
                    n += 1;

                if (playedStructure.Exists(x => x.Id == CardId.Rhodos_B_s2))
                    n += 1;

                return n;
            }
        }

        public int lossToken { get; set; }

        public int conflictTokenOne { get; set; }

        public int conflictTokenTwo { get; set; }

        public int conflictTokenThree { get; set; }

        public List<Card> hand = new List<Card>(7);

        public List<Card> playedStructure = new List<Card>();
        public List<Card> draftedLeaders = new List<Card>(4);

        // After the player builds the 2nd stage of Babylon B's wonder, this will be true.
        public bool babylonPowerEnabled { get; set; }

        // After the player builds the 2nd stage of Olympia A's wonder, this will be true.
        public bool olympiaPowerEnabled { get; set; }

        // if Olympia's Power (play a card for free) has not been used, this is true
        public bool olympiaPowerAvailable { get; set; }

        public bool playCardFromDiscardPile = false;

        public bool draftingExtraLeader = false;

        //bilkis (0 is nothing, 1 is ore, 2 is stone, 3 is glass, 4 is papyrus, 5 is loom, 6 is wood, 7 is brick
        public byte bilkis;
        public bool hasBilkis;

        //stored actions for the turn
        private List<Effect> actions = new List<Effect>();

        private List<int> coinTransactions = new List<int>();

        // Many leaders and cities cards have enduring effects.  The list above is just for effects
        // on this turn.  Actually, I think it's mostly for money exchanges.  If I don't have an
        // effect list here, then any time a card is played I may need to a search for not just the 
        // played cards but also check what board is being used.  I think Rome is the only board that
        // has a very unusual effect though.

        //Player's left and right neighbours
        public Player leftNeighbour { get; set; }

        public Player rightNeighbour { get; set; }

        public Boolean changeNickName {get; set; }
        public String newNickName {get; set; }

        public CommercialDiscountEffect.RawMaterials rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.None;
        public CommercialDiscountEffect.Goods goodsDiscount = CommercialDiscountEffect.Goods.None;

        public AIMoveBehaviour AIBehaviour;

        private GameManager gm;

        public ResourceManager dag { get; private set; }

        public bool bUIRequiresUpdating { get; set; }

        /// <summary>
        /// Constructor. Create a Player with a given nickname
        /// </summary>
        public Player(String nickname, bool isAI, GameManager gm)
        {
            dag = new ResourceManager();

            this.nickname = nickname;

            this.isAI = isAI;

            currentStageOfWonder = 0;
            changeNickName = false;
            newNickName = "";

            //set bilkis to nothing
            bilkis = 0;
            hasBilkis = false;

            //set the Game Manager
            this.gm = gm;
        }

        /// <summary>
        /// Set the neighbouring Players
        /// Used by GameManager.beginningOfSessionActions()
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void setNeighbours(Player left, Player right)
        {
            leftNeighbour = left;
            rightNeighbour = right;
        }

        /// <summary>
        /// Stored actions to be executed at the end of each turn
        /// </summary>
        /// <param name="s"></param>
        public void storeCardEffect(Card c)
        {
            if (c.effect is CoinEffect || c.effect is ResourceEffect || c.effect is CoinsAndPointsEffect || c.effect is PlayACardForFreeOncePerAgeEffect)
            {
                // the effects of these cards do not come into play until the next turn.
                // put them on the actions queue to be run after all players have turned
                // in their card.  Any actions that require UI updates must go on here
                // (e.g. enabling the Olympia button)
                actions.Add(c.effect);
            }
            else
            {
                // other actions 
                executeActionNow(c);
            }
        }

        public void addTransaction(int coins)
        {
            coinTransactions.Add(coins);
        }

        /// <summary>
        /// Check if Salomon (aka Halicarnassus) is stored as an action.
        /// Return false if it is not
        /// Return true if it is, then remove it
        /// </summary>
        /// <returns></returns>
        public bool hasSalomon()
        {
            /*
            for (int i = 0; i < numOfActions; i++)
            {
                //found Salomon
                if (actions[i] == "SALOMON")
                {
                    //remove the item and return true
                    for (int j = i; j < (numOfActions - 1); j++)
                    {
                        actions[j] = actions[j + 1];
                    }

                    numOfActions--;

                    return true;
                }
            }
            */

            return false;
        }

        /// <summary>
        /// Check if Stevie is stored as an action
        /// Return true if it is then remove it
        /// Return false if it is not
        /// </summary>
        /// <returns></returns>
        public bool hasStevie()
        {
            /*
            for (int i = 0; i < numOfActions; i++)
            {
                //found Stevie
                if (actions[i] == "@Pay X coins for board, where X is board cost")
                {
                    //remove the item and return true
                    for (int j = i; j < (numOfActions - 1); j++)
                    {
                        actions[j] = actions[j + 1];
                    }

                    numOfActions--;

                    return true;
                }
            }
            */

            return false;
        }

        /// <summary>
        /// Check if Courtesan's guild is played and remove it if it is
        /// </summary>
        /// <returns></returns>
        public bool hasCourtesan()
        {
            /*
            for (int i = 0; i < numOfActions; i++)
            {
                //found Courtesan's guild
                if (actions[i] == "@(Guild of Courtesans effect)")
                {
                    //remove the item and return true
                    for (int j = i; j < (numOfActions - 1); j++)
                    {
                        actions[j] = actions[j + 1];
                    }

                    numOfActions--;

                    return true;
                }
            }
            */

            return false;
        }

        /*
        /// <summary>
        /// Stored actions to be executed at the end of the game
        /// </summary>
        /// <param name="s"></param>
        public void storeEndOfGameAction(Effect s)
        {
            endOfGameActions.Add(s);
        }
        */
        public void executeActionNow(Card card)
        {
            Effect effect = card.effect;

            // I think the only effects that really need to be dealt with NOW are those
            // that affect game state (e.g. Babylon B, Play a discarded card.)
            if (effect == null)
            {
                switch(card.Id)
                {
                    case CardId.Halikarnassos_B_s1:
                    case CardId.Halikarnassos_B_s2:
                        playCardFromDiscardPile = true;
                        break;

                    case CardId.Rhodos_B_s1:
                        // Add the 3 coins immediately.  The 3 victory points will be included in total for wonders
                        // The Military will also need to be included in the shield calculation.
                        addTransaction(3);
                        break;

                    case CardId.Rhodos_B_s2:
                        addTransaction(4);
                        break;

                    case CardId.Roma_B_s1:
                        // Roma (B) stage 1: draw 4 more leaders from the pile of unused leaders
                        // to add to the players list of recruitable leaders
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Card c = gm.deckList[0].GetTopCard();
                                draftedLeaders.Add(c);
                            }

                            string strMsg = "LeadrIcn";

                            foreach (Card c in draftedLeaders)
                            {
                                strMsg += string.Format("&{0}=", c.Id);
                            }

                            gm.gmCoordinator.sendMessage(this, strMsg);

                            addTransaction(5);
                        }
                        break;

                    case CardId.Roma_B_s2:
                    case CardId.Roma_B_s3:
                        draftingExtraLeader = true;

                        // TODO: can I do this instead of considering the points effect separately at the end of the game?
                        // effect = new CoinsAndPointsEffect(CoinsAndPointsEffect.CardsConsidered.None, StructureType.Constant, 0, 3);
                        break;
                }
            }
            else if (effect is CommercialDiscountEffect)
            {
                CommercialDiscountEffect cde = effect as CommercialDiscountEffect;

                // Set discount effects for future transactions.
                switch (cde.effectString[1])
                {
                    case 'R':
                        switch (cde.effectString[0])
                        {
                            case 'L':
                                if (rawMaterialsDiscount == CommercialDiscountEffect.RawMaterials.None)
                                    rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.LeftNeighbor;
                                else if (rawMaterialsDiscount == CommercialDiscountEffect.RawMaterials.RightNeighbor)
                                    rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.BothNeighbors;
                                break;

                            case 'R':
                                if (rawMaterialsDiscount == CommercialDiscountEffect.RawMaterials.None)
                                    rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.RightNeighbor;
                                else if (rawMaterialsDiscount == CommercialDiscountEffect.RawMaterials.LeftNeighbor)
                                    rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.BothNeighbors;
                                break;

                            case 'B':
                                rawMaterialsDiscount = CommercialDiscountEffect.RawMaterials.BothNeighbors;
                                break;
                        }
                        break;

                    case 'G':
                        if (cde.effectString[0] == 'B')
                            goodsDiscount = CommercialDiscountEffect.Goods.BothNeighbors;
                        break;
                }
            }
            else if (effect is PlayDiscardedCardForFreeEffect)
            {
                playCardFromDiscardPile = true;
            }
            else if (effect is PlayACardForFreeOncePerAgeEffect)
            {
                throw new Exception("This ability needs to be dealt with on the end-of-turn action queue.");
            }
            else
            {
                throw new Exception("Unimplemented effect type");
            }
            // any other effects do not require immediate action, they will be dealt with at the end of the turn,
            // end of the age, or the end of the game.
            /*
            else if (
                effect is ShipOwnersGuildEffect ||
                effect is ScienceWildEffect ||
                effect is PlayLastCardInAgeEffect ||
                effect is CopyGuildFromNeighborEffect ||
                effect is MilitaryEffect ||
                effect is ScienceEffect ||
                effect is FreeLeadersEffect
                effect is PlatoEffect)
            {
                // nothing to do; this card will be included in the end of game point total, or
                // - Military cards are used at the end of each age to resolve conflicts
                // - Science cards are used at the end of the game.
                // - Free Leaders effects are captured when the cards are put into play
            }
            else
            {
                throw new Exception("Unexpected effect type in executeActionNow()");
            }
            */
        }

        //Execute actions
        //change the Player score information based on the actions
        public void executeAction(GameManager gm)
        {
            /*
            //Esteban and Bilkis can be implemented much easier if it has access to GameManager (LeadersGameManager to be exact)
            //Regular GameManager is not useful. Must have LeadersGameManager because nothing in the regular game requires reg GM
            if (gm is LeadersGameManager)
            {
                gm = (LeadersGameManager)gm;
            }
            */
            //go through each action and execute the actions stored
            foreach (Effect act in actions)
            {
                //category $: deduct a given amount of coins
                // if (actactions[i][0] == '$')
                if (act is CoinEffect)
                {
                    // add or subtract coins
                    coin += ((CoinEffect)act).coins;
                }
                /*
                else if (act is MilitaryEffect)
                {
                    // shield += ((MilitaryEffect)act).nShields;
                }
                */
                //category 1: give one kind of non-science thing
                // else if (actions[i][0] == '1')
                else if (act is ResourceEffect)
                {
                    ResourceEffect e = act as ResourceEffect;
                    //increase the appropriate field by num
                    // int num = int.Parse(act[0] + "");
                    //  int num = e.multiplier;
                    dag.add(e);
                }
                else if (act is CoinsAndPointsEffect)
                {
                    CoinsAndPointsEffect e = act as CoinsAndPointsEffect;

                    if (e.coinsGrantedAtTimeOfPlayMultiplier != 0)
                    {
                        if (e.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.None)
                        {
                            coin += e.coinsGrantedAtTimeOfPlayMultiplier;
                        }

                        if (e.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.PlayerAndNeighbors || e.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.Neighbors)
                        {
                            coin += e.coinsGrantedAtTimeOfPlayMultiplier * leftNeighbour.playedStructure.Where(x => x.structureType == e.classConsidered).Count();
                            coin += e.coinsGrantedAtTimeOfPlayMultiplier * rightNeighbour.playedStructure.Where(x => x.structureType == e.classConsidered).Count();
                        }

                        if (e.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.PlayerAndNeighbors || e.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.Player)
                        {
                            coin += e.coinsGrantedAtTimeOfPlayMultiplier * playedStructure.Where(x => x.structureType == e.classConsidered).Count();
                        }
                    }
                }
                else if (act is PlayACardForFreeOncePerAgeEffect)
                {
                    olympiaPowerEnabled = true;
                    olympiaPowerAvailable = true;
                    gm.gmCoordinator.sendMessage(this, "EnableFB&Olympia=true");
                }

                /*
                //Esteban and Bilkis
                // else if(actions[i][0] == '8')
                else if(act is SpecialLeaderEffect)
                {
                    if (act.Substring(0) == "Esteban")
                    {
                        //enable the Esteban button by sending the Esteban message to the client
                        gm.gmCoordinator.sendMessage(this, "EE");
                    }
                    else if (act.Substring(0) == "Bilkis")
                    {
                        hasBilkis = true;
                    }
                }
                */
                else
                {
                    //do nothing for now
                    throw new NotImplementedException();
                }
            }

            actions.Clear();
        }

        int CountVictoryPoints(CoinsAndPointsEffect cpe)
        {
            int sum = 0;

            if (cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.PlayerAndNeighbors ||
                cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.Neighbors)
            {
                sum += cpe.victoryPointsAtEndOfGameMultiplier * leftNeighbour.playedStructure.Where(x => x.structureType == cpe.classConsidered).Count();
                sum += cpe.victoryPointsAtEndOfGameMultiplier * rightNeighbour.playedStructure.Where(x => x.structureType == cpe.classConsidered).Count();

                if (cpe.classConsidered == StructureType.MilitaryLosses)
                {
                    sum += leftNeighbour.lossToken * cpe.victoryPointsAtEndOfGameMultiplier;
                    sum += rightNeighbour.lossToken * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                // Added for Leaders (Alexander)
                if (cpe.classConsidered == StructureType.ConflictToken)
                {
                    sum += (leftNeighbour.conflictTokenOne + leftNeighbour.conflictTokenTwo + leftNeighbour.conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                    sum += (rightNeighbour.conflictTokenOne + rightNeighbour.conflictTokenTwo + rightNeighbour.conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                /*
                if (cpe.classConsidered == StructureType.WonderStage)
                {
                    sum += leftNeighbour.currentStageOfWonder* cpe.victoryPointsAtEndOfGameMultiplier;
                    sum += rightNeighbour.currentStageOfWonder * cpe.victoryPointsAtEndOfGameMultiplier;
                }
                */
            }

            if (cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.PlayerAndNeighbors || cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.Player)
            {
                sum += cpe.victoryPointsAtEndOfGameMultiplier * playedStructure.Where(x => x.structureType == cpe.classConsidered).Count();

                if (cpe.classConsidered == StructureType.MilitaryLosses)
                {
                    sum += lossToken * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                // Added for Leaders (Alexander)
                if (cpe.classConsidered == StructureType.ConflictToken)
                {
                    sum += (conflictTokenOne + conflictTokenTwo + conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                /*
                if (cpe.classConsidered == StructureType.WonderStage)
                {
                    sum += currentStageOfWonder * cpe.victoryPointsAtEndOfGameMultiplier;
                }
                */
            }

            if (cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.None)
            {
                // Civilian structures and wonder stages constructed fall into this category.
                sum += cpe.victoryPointsAtEndOfGameMultiplier;
            }

            return sum;
        }

        struct ScienceScore
        {
            // copy the input parameters
            public int nCompass;
            public int nGear;
            public int nTablet;
            public int groupMultiplier;

            // calculated values
            public int baseScore;       // nCompass^2 + nGear^2 + nTablet^2
            public int nGroups;         // number of complete sets
            public int TotalPoints;     // total number of points this combination is worth
        };

        int CalculateScienceGroupScore(int nCompass, int nGear, int nTablet, int groupMultiplier, out ScienceScore ss)
        {
            ss.nCompass = nCompass;
            ss.nGear = nGear;
            ss.nTablet = nTablet;
            ss.groupMultiplier = groupMultiplier;

            // Compute output values
            ss.baseScore = ss.nCompass * ss.nCompass + ss.nGear * ss.nGear + ss.nTablet * ss.nTablet;
            ss.nGroups = Math.Min(Math.Min(ss.nCompass, ss.nGear), ss.nTablet);
            ss.TotalPoints = ss.baseScore + ss.nGroups * ss.groupMultiplier;

            return ss.TotalPoints;
        }

        ScienceScore FindBestScienceWildcard(int nScienceWildCards, int groupMultiplier)
        {
            ScienceScore tmpResult = new ScienceScore();
            ScienceScore bestResult = tmpResult;

            int maxScienceScore = 0;

            // The player can have up to two science wild cards: Babylon wonder and the Scientists' guild.
            // Try every possible combination of these cards and select the one that yields the highest score
            // for the science cards.
            int nCompass = playedStructure.Where(x => x.structureType == StructureType.Science && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Compass).Count();
            int nGear = playedStructure.Where(x => x.structureType == StructureType.Science && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Gear).Count();
            int nTablet = playedStructure.Where(x => x.structureType == StructureType.Science && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Tablet).Count();

            for (int i = 0; i <= nScienceWildCards; ++i)
            {
                for (int j = 0; j <= nScienceWildCards - i; ++j)
                {
                    for (int k = 0; k <= nScienceWildCards - i - j; ++k)
                    {
                        int score = CalculateScienceGroupScore(nCompass + i, nGear + j, nTablet + k, groupMultiplier, out tmpResult);
                        if (score > maxScienceScore)
                        {
                            maxScienceScore = score;

                            bestResult = tmpResult;

                            /*
                            PointsForScience = CalculateScienceGroupScore(result.nCompass, result.nGear, result.nTablet, 7, out result);

                            // if wild cards are in play, we choose the best combination of wilds to get the maximum overall
                            // score, with Aristotle's bonus factored in.  In some cases, Aristotle's effect will mean it's
                            // more beneficial to use the wild card(s) to make more groups rather than like symbols.
                            // For example: 1/2/5+1.  Without Aristotle, the maximum score is 1/2/6 = 48.
                            // But with Aristotle's bonus, it's better to use the wild card to make a 3rd set instead
                            // 1/2/6 = 51 with Aristotle but if you do 2/2/5 you get 53 points because the exta set
                            // is worth 3 bonus points.
                            if (usingAristotle)
                                PointsForAristotle = CalculateScienceGroupScore(nCompass + i, nGear + j, nTablet + k, 10) - PointsForScience;
                            */
                        }
                    }
                }
            }

            return bestResult;
        }

        /// <summary>
        /// Execute the end of game actions
        /// Most are hardcoded
        /// </summary>
        public Score executeEndOfGameActions()
        {
            Score score = new Score();

            // Console.WriteLine("End of game summary for {0}", playerBoard.name);

            score.coins = coin / 3;
            // Console.WriteLine("  Coins at the end of the game: {0}", coin);

            // Console.WriteLine("  Military victories for 1st age: {0}", conflictTokenOne);
            // Console.WriteLine("  Military victories for 2nd age: {0}", conflictTokenTwo);
            // Console.WriteLine("  Military victories for 3rd age: {0}", conflictTokenThree);
            // Console.WriteLine("  Military losses: {0}", lossToken);

            score.military = conflictTokenOne + conflictTokenTwo * 3 + conflictTokenThree * 5 - lossToken;

            // Console.WriteLine("  Civilian structures constructed:");
            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Civilian))
            {
                score.civilian += ((CoinsAndPointsEffect)c.effect).victoryPointsAtEndOfGameMultiplier;
                // Console.WriteLine("    {0} ({1} VP)", c.name, thisStructurePoints);
                // score.civilian += thisStructurePoints;
            }

            // Console.WriteLine("  Commercial structures constructed:");
            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Commerce && x.effect is CoinsAndPointsEffect))
            {
                // Console.WriteLine("    {0}", c.name);
                score.commerce += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
            }

            /*
            Console.WriteLine("  Scientific structures constructed:");
            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Science))
            {
                Console.WriteLine("    {0} ({1})", c.name, ((ScienceEffect)c.effect).symbol);
            }
            */

            int nScienceWildCards = playedStructure.Where(x => x.effect is ScienceWildEffect).Count();

            // if (nScienceWildCards != 0)
            //    Console.WriteLine("  {0} science wild card effect(s)", nScienceWildCards);

            bool hasAristotle = playedStructure.Exists(x => x.Id == CardId.Aristotle);
            ScienceScore scienceScore = FindBestScienceWildcard(nScienceWildCards, hasAristotle ? 10 : 7);

            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.WonderStage))
            {
                if (c.effect == null)
                {
                    switch (c.Id)
                    {
                        case CardId.Halikarnassos_B_s1:
                            score.wonders += 2;
                            break;

                        case CardId.Halikarnassos_B_s2:
                            score.wonders += 1;
                            break;

                        case CardId.Olympia_B_s3:
                            {
                                // Olympia B 3rd stage.  Check each guild card built by neighboring cities
                                // and pick the one that yields the most number of points to copy.
                                int maxPoints = 0;
                                CardId copiedGuild = CardId.Lumber_Yard;    // needs to be initialized to avoid compiler error.
                                ScienceScore tmpScienceScore = scienceScore;

                                IEnumerable<Card> neighborsGuilds = leftNeighbour.playedStructure.Where(x => x.structureType == StructureType.Guild).Concat(
                                    rightNeighbour.playedStructure.Where(x => x.structureType == StructureType.Guild));

                                foreach (Card card in neighborsGuilds)
                                {
                                    int pointsForThisGuild = 0;

                                    if (card.effect is CoinsAndPointsEffect)
                                    {
                                        pointsForThisGuild = CountVictoryPoints(card.effect as CoinsAndPointsEffect);
                                    }
                                    else if (card.Id == CardId.Shipowners_Guild)
                                    {
                                        pointsForThisGuild = playedStructure.Where(x => x.structureType == StructureType.RawMaterial || x.structureType == StructureType.Goods || x.structureType == StructureType.Guild).Count();
                                    }
                                    else if (card.effect is ScienceWildEffect)
                                    {
                                        tmpScienceScore = FindBestScienceWildcard(nScienceWildCards + 1, hasAristotle ? 10 : 7);
                                        pointsForThisGuild = tmpScienceScore.TotalPoints - scienceScore.TotalPoints;
                                    }

                                    if (pointsForThisGuild > maxPoints)
                                    {
                                        maxPoints = pointsForThisGuild;
                                        copiedGuild = card.Id;
                                    }
                                }

                                if (copiedGuild == CardId.Scientists_Guild)
                                {
                                    // If the copied guild is the Scientist's Guild, update the overall 
                                    // This gets really messy because we're trying for the overall maximum score while breaking out
                                    // which parts of that score came from different components.  What needs to happen is for the returned
                                    // science score 
                                    scienceScore = tmpScienceScore;
                                }

                                /*
                                if (maxPoints != 0)
                                {
                                    Console.WriteLine("Olympia B's 3rd wonder has a maximum value of {0} points.  Guild copied: {1}", maxPoints, copiedGuild);
                                }
                                else
                                {
                                    Console.WriteLine("Neither of Olympia's neighbors built any guilds that were worth any value to Olympia.  0 points scored for this wonder stage.");
                                }
                                */

                                score.wonders += maxPoints;
                            }
                            break;

                        case CardId.Rhodos_B_s1:
                            score.wonders += 3;
                            break;

                        case CardId.Rhodos_B_s2:
                            score.wonders += 4;
                            break;

                        case CardId.Roma_B_s2:
                        case CardId.Roma_B_s3:
                            score.wonders += 3;
                            break;
                    }
                }
                else if (c.effect is CoinsAndPointsEffect)
                {
                    score.wonders += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
                }
            }

            // After the Wonders are done, we can input the science score.
            score.science = scienceScore.baseScore + scienceScore.nGroups * 7;

            // Console.WriteLine("  Guilds constructed:");
            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Guild))
            {
                // Console.WriteLine("    {0}", c.name);

                if (c.effect is CoinsAndPointsEffect)
                {
                    // most guilds fall into this category: they count points based on something the neighboring cities.
                    score.guilds += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
                }
                else if (c.Id == CardId.Shipowners_Guild)
                {
                    // Shipowners guild counts 1 point for each Brown, Grey, and Purple card in the players' city.
                    score.guilds += playedStructure.Where(x => x.structureType == StructureType.RawMaterial || x.structureType == StructureType.Goods || x.structureType == StructureType.Guild).Count();
                }
            }

            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Leader))
            {
                if (c.effect is CoinsAndPointsEffect)
                {
                    score.leaders += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
                }
            }

            if (hasAristotle)
                score.leaders += scienceScore.nGroups * 3;

#if FALSE
            foreach (Effect act in endOfGameActions)
            {
                if (act is CoinsAndPointsEffect)
                {
                    CoinsAndPointsEffect e = act as CoinsAndPointsEffect;
                }
                //category 6: special guild cards and leader cards
                //6_132 or 6_135
                else if (act is SpecialAbilityEffect)
                {
                    /*
                    //card number 132: Scientist guild
                    //award a science that gives the most points
                    if (act.Substring(1) == "132")
                    {
                        //try adding each one and see which artifact will give the highest score

                        //try adding bear trap
                        int sum1 = ((bearTrap + 1) * (bearTrap + 1)) + (sextant * sextant) + (tablet * tablet) + (Math.Min(Math.Min(bearTrap+1, sextant), tablet) * 7);
                        //try adding sextant
                        int sum2 = (bearTrap * bearTrap) + ((sextant+1) * (sextant+1)) + (tablet * tablet) + (Math.Min(Math.Min(bearTrap, sextant+1), tablet) * 7);
                        //try adding tablet
                        int sum3 = (bearTrap * bearTrap) + (sextant * sextant) + ((tablet+1) * (tablet+1)) + (Math.Min(Math.Min(bearTrap, sextant), (tablet+1)) * 7);
                        //choose the one that has the highest sum
                        //if the max is sum1
                        if (Math.Max(Math.Max(sum1, sum2), sum3) == sum1) bearTrap++;
                        //if the max is sum2
                        else if (Math.Max(Math.Max(sum1, sum2), sum3) == sum2) sextant++;
                        //if the max is sum3
                        else if (Math.Max(Math.Max(sum1, sum2), sum3) == sum3) tablet++;
                    }

                    //card number 135
                    //add 1 victory for each brown card, grey card, purple card played
                    if (act.Substring(1) == "135")
                    {
                        for (int j = 0; j < numOfPlayedCards; j++)
                        {
                            if (playedStructure[j].colour == "Brown" || playedStructure[j].colour == "Grey" || playedStructure[j].colour == "Purple")
                            {
                                victoryPoint++;
                            }
                        }

                    }

                    //card number 302 (Gamer's Guild) or 218 (Midas)
                    //add 1 victory point for each 3 coins at the end of the game
                    if (act.Substring(1) == "302" || act.Substring(2) == "218")
                    {
                        victoryPoint += (int)(coin / 3);
                    }

                    //card number 203
                    //add 3 victory point for each 3 science at the end of the game
                    //so instead of adding 7 for each set of science, add 10 instead.

                    if (act.Substring(1) == "203")
                    {
                        int least = Math.Min(Math.Min(bearTrap, sextant), tablet);
                        victoryPoint += (least * 3);
                    }

                    //card number 213 (Justinian)
                    //add 3 VP for every set of blue, red, and green card
                    if (act.Substring(1) == "213")
                    {
                        int blue = 0, red = 0, green = 0;

                        for (int j = 0; j < numOfPlayedCards; j++)
                        {
                            if (playedStructure[j].colour == "Blue") blue++;
                            else if (playedStructure[j].colour == "Green") green++;
                            else if (playedStructure[j].colour == "Red") red++;
                        }

                        int least = Math.Min(Math.Min(blue, red), green);
                        victoryPoint += (least * 3);
                    }

                    //card number 224 (Platon)
                    //add 7 victory point for each set of brown, grey, blue, yellow, green, red, and purple card played
                    if (act.Substring(1) == "224")
                    {
                        //let the 7 numbers be put into an array of integers. Sort these integers. The lowest number will be the least amount

                        int []colours = new int[7];
                        for(int j = 0; j < 7; j++) colours[j] = 0;

                        for (int j = 0; j < numOfPlayedCards; j++)
                        {
                            if (playedStructure[j].colour == "Blue") colours[0]++;
                            else if (playedStructure[j].colour == "Green") colours[1]++;
                            else if (playedStructure[j].colour == "Red") colours[2]++;
                            else if (playedStructure[j].colour == "Brown") colours[3]++;
                            else if (playedStructure[j].colour == "Grey") colours[4]++;
                            else if (playedStructure[j].colour == "Yellow") colours[5]++;
                            else if (playedStructure[j].colour == "Purple") colours[6]++;
                        }

                        //sort the numbers
                        Array.Sort(colours);

                        //the lowest number is the first element. That tells how many sets there are.
                        int least = colours[0];

                        //add multiple of 7 of the least amount.
                        victoryPoint += (least * 7);
                    }

                    //card number 200
                    //Alexander: one extra point per conflict token
                    if (act.Substring(1) == "200")
                    {
                        victoryPoint += (conflictTokenOne + conflictTokenTwo + conflictTokenThree);
                    }

                    //card number 238
                    //Louis Armstrong
                    if (act.Substring(1) == "238")
                    {
                        victoryPoint += (7 - (conflictTokenOne + conflictTokenTwo + conflictTokenThree));
                    }
                    */
                }
                //category 7: end of game board powers
                else if (act is SpecialAbilityEffect)
                {
                    /*
                    //copy best neighbouring purple card
                    if (endOfGameActions[i] == "7OB")
                    {
                        throw new NotImplementedException();
                    }
                    */

                    throw new NotImplementedException();
                }
            }
#endif
            return score;
        }

        /// <summary>
        /// Add a card to the Player's played structure pile
        /// </summary>
        /// <param name="card"></param>
        public void addPlayedCardStructure(Card card)
        {
            playedStructure.Add(card);
            bUIRequiresUpdating = true;
        }

        /// <summary>
        /// Determines if a given card is buildable.
        /// Returns "T" if it is, returns "F" if it is not
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Buildable isCardBuildable(Card card)
        {
            //retrieve the cost
            Cost cost = card.cost;

            //if the player already owns a copy of the card, Return F immediatley
            if (playedStructure.Contains(card))
                return Buildable.StructureAlreadyBuilt;

            //if the cost is !, that means its free. Return T immediately
            if (cost.coin == 0 && cost.wood == 0 && cost.stone == 0 && cost.clay == 0 &&
                cost.ore == 0 &&  cost.cloth == 0 && cost.glass == 0 && cost.papyrus == 0)
            {
                return Buildable.True;
            }

            //if the player owns the prerequiste, Return T immediately
            if (playedStructure.Exists(x => (x.chain[0] == card.strName) || (x.chain[1] == card.strName)))
                return Buildable.True;

            //if the owner has built card 217: free leader cards
            //if the owner has Rome A board, then same
            //return T if the card is white
#if FALSE
            if ((playerBoard.freeResource == 'D' || hasIDPlayed(/*217*/"Maecenas")) && card.structureType == StructureType.Leader)
            {
                return 'T';
            }

            //if the owner has Rome B board, then get 2 coin discount
            //return F otherwise (since you cannot get more coins from initiating commerce; you can only get resources)
            if (playerBoard.freeResource == 'd' && card.structureType == StructureType.Leader)
            {
                if ((card.cost.coin - 2) <= coin) return 'T';
                else return 'F';
            }

            //if a neighbour own Rome B board, then get a 1 coin discount
            else if ((leftNeighbour.playerBoard.freeResource == 'd' || rightNeighbour.playerBoard.freeResource == 'd') && card.structureType == StructureType.Leader)
            {
                if ((card.cost.coin - 1) <= coin) return 'T';
                else return 'F';
            }
#endif

            //if the owner has built card 228: free guild cards
            //return T if the card is purple
            if (card.structureType == StructureType.Guild && playedStructure.Exists(x => x.Id == CardId.Ramses))
            {
                return Buildable.True;
            }

            int nWildResources = 0;
            if (playedStructure.Exists(x => x.effect is StructureDiscountEffect && ((StructureDiscountEffect)x.effect).discountedStructureType == card.structureType))
            {
                // A leader card has been played that matches the structure type being built, so we can add a wild resource
                // e.g. We're building a science structure while Archimedes is in play for this player, or a military structure
                // when Leonidas is in play.
                ++nWildResources;
            }
            /*
            //202, 207, 216: Discount on green, blue and red respectively
            //If a discount applies, determine if it is possible to play the card
            if ((hasIDPlayed(202) && card.colour == "Green") || 
                (hasIDPlayed(207) && card.colour == "Blue") ||
                (hasIDPlayed(216) && card.colour == "Red"))
            {
                bool newCostResult = DAG.canAffordOffByOne(dag, cost);

                if (newCostResult == true) return 'T';
            }
            */

            int coinCost = cost.coin;

            if (card.structureType == StructureType.Leader)
            {
                if (playerBoard.name == "Roma (A)" || playedStructure.Exists(x => x.Id == CardId.Maecenas))
                {
                    coinCost = 0;
                }
                else if (playerBoard.name == "Roma (B)")
                {
                    coinCost = Math.Max(0, coinCost - 2);
                }
                else if (leftNeighbour.playerBoard.name == "Roma (B)" || rightNeighbour.playerBoard.name == "Roma (B)")
                {
                    coinCost -= 1;
                }
            }

            if (coin < coinCost)
            {
                // if the card has a coin cost and we don't have enough money, the card is not buildable.
                return Buildable.InsufficientCoins;
            }

            //can player afford cost with DAG resources?
            if (isCostAffordableWithDAG(cost, nWildResources) == Buildable.True)
                return Buildable.True;

            //can player afford cost by conducting commerce?
            if (isCostAffordableWithCommerce(cost, nWildResources) == Buildable.CommerceRequired)
                return Buildable.CommerceRequired;

            return Buildable.InsufficientResources;
        }

        /// <summary>
        /// Assuming no pre-reqs, free cards, etc.
        /// Determine if a given cost is affordable
        /// </summary>
        /// <param name="card"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        private Buildable isCostAffordableWithDAG(Cost cost, int nWildResources)
        {
            // the passed-in cost structure must not be modified.  C# doesn't support const correctness?!?
            // WTF!
            cost = cost.Copy();

            //get rid of the coins from the cost, and see if DAG can afford the cost (already checked for coins at previous step)
            //this is relevant for the Black cards in the Cities expansion
            cost.coin = 0;

            if (cost.IsZero())
            {
                // The card only costs money (no resources), so it's affordable.
                // If it was not affordable, the 
                return Buildable.True;
            }

            //can I afford the cost with resources in my DAG?
            if (dag.canAfford(cost, nWildResources)) return Buildable.True;

            return Buildable.InsufficientResources;
        }

        /// <summary>
        /// Determine, given a cost, if Player can afford a cost with his and his 2 neighbours' DAGs combined.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        private Buildable isCostAffordableWithCommerce(Cost cost, int nWildResources)
        {
            cost = cost.Copy();

            cost.coin = 0;

            //combine the left, centre, and right DAG
            ResourceManager combinedDAG = ResourceManager.addThreeDAGs(leftNeighbour.dag, dag, rightNeighbour.dag);

            if (playedStructure.Exists(x => x.Id == CardId.Bilkis))
            {
                combinedDAG.add(new ResourceEffect(false, "WSBOCGP"));
            }

            //determine if the combined DAG can afford the cost
            if (combinedDAG.canAfford(cost, nWildResources)) return Buildable.CommerceRequired;

            return Buildable.InsufficientResources;
        }

        /// <summary>
        /// Determines if the Player's current stage is buildable
        /// Returns "T" if it is, returns "F" if it is not
        /// </summary>
        /// <returns></returns>
        public Buildable isStageBuildable()
        {
            //check if the current Stage is already the maximum stage
            if (currentStageOfWonder >= playerBoard.numOfStages)
                return Buildable.StructureAlreadyBuilt;

            //retrieve the cost
            Cost cost = playerBoard.stageCard[currentStageOfWonder].cost;

            //check for the stage discount card (Imhotep)
            int nWildResources = 0;
            if (playedStructure.Exists(x => x.effect is StructureDiscountEffect && ((StructureDiscountEffect)x.effect).discountedStructureType == StructureType.WonderStage))
            {
                // A leader card has been played that matches the structure type being built, so we can add a wild resource
                // e.g. We're building a science structure while Archimedes is in play for this player, or a military structure
                // when Leonidas is in play.
                ++nWildResources;
            }

            //can player afford cost with DAG resources
            if (isCostAffordableWithDAG(cost, nWildResources) == Buildable.True) return Buildable.True;

            //can player afford cost by conducting commerce?
            if (isCostAffordableWithCommerce(cost, nWildResources) == Buildable.CommerceRequired)
                return Buildable.CommerceRequired;

            //absolutely all options exhausted. return F
            return Buildable.InsufficientResources;
        }

        /*
        /// <summary>
        /// return the final score
        /// </summary>
        /// <returns></returns>
        public int finalScore()
        {
            int score = 0;

            //1. Red: Add the military conflict points
            score += conflictTokenOne + (conflictTokenTwo * 3) + (conflictTokenThree * 5);
            score -= lossToken;

            //2. Count the coins. Every 3 coins counts for 1 score at the end
            score += ((int)(coin / 3));

            //3. Add victory points from blue cards
            score += victoryPoint;

            //4. Green: Add scientific structures  
            //add up each artifact
            //bearTraps
            score += bearTrap * bearTrap;
            score += sextant * sextant;
            score += tablet * tablet;

            //add 7 points for each three of a kind
            //find the least of the artifact among the three
            int least = Math.Min(Math.Min(bearTrap, sextant), tablet);
            score += (least * 7);

            return score;
        }
        */

#if FALSE
        public int doHaveEnoughCoinsToCommerce(String c)
        {
            //retrieve the cost
            String cost = c;

            //parse the tcost of each possible resource
            int brickf = 0, oref = 0, stonef = 0, woodf = 0, glassf = 0, loomf = 0, papyrusf = 0;

            Boolean b = false, o = false, t = false, w = false, g = false, l = false, p = false;


            for (int i = 0; i < cost.Length; i++)
            {
                if (cost[i] == 'B') { brickf++; b = true; }
                else if (cost[i] == 'O') { oref++; o = true; }
                else if (cost[i] == 'T') { stonef++; t = true; }
                else if (cost[i] == 'W') { woodf++; w = true; }
                else if (cost[i] == 'G') { glassf++; g = true; }
                else if (cost[i] == 'L') { loomf++; l = true; }
                else if (cost[i] == 'P') { papyrusf++; p = true; }
            }

            int costB = 0, costO = 0, costT = 0, costW = 0, costG = 0, costP = 0, costL = 0;
            int leftMultiRaw = 2, leftMultiManu = 2, rightMultiRaw = 2, rightMultiManu = 2;

            // left & right manu aren't necessary.  The only card that affects the cost of manufactured
            // goods is the Marketplace, and its effect applies to both neighbors.
            if (leftRaw) leftMultiRaw--;
            if (leftManu) leftMultiManu--;
            if (rightRaw) rightMultiRaw--;
            if (rightManu) rightMultiManu--;


            //calculate how much of each resource i need, for exampe i need to buy 1 ore, 1 wood things
            if (brickf > 0 && brickf > brick) costB = brickf - brick;
            if (oref > 0 && oref > ore) costO = oref - ore;
            if (stonef > 0 && stonef > stone) costT = stonef - stone;
            if (woodf > 0 && woodf > wood) costW = woodf - wood;
            if (glassf > 0 && glassf > glass) costG = glassf - glass;
            if (papyrusf > 0 && papyrusf > papyrus) costP = papyrusf - papyrus;
            if (loomf > 0 && loomf > loom) costL = loomf - loom;


            int amountOfRawFromLeft = 0, amountOfManuFromLeft = 0;
            int amountOfRawFromRight = 0, amountOfManuFromRight = 0;

            Boolean leftFirst = false;

            if (leftRaw) leftFirst = true;
            if (leftManu) leftFirst = true;


            for (int j = 0; j < 2; j++)
            {
                if (leftFirst)
                {
                    //if i can buy from the left player calculate the cost 
                    if (leftNeighbour.brick >= costB && b) { for (int i = 0; i < leftNeighbour.brick; i++) { if (costB > 0) { amountOfRawFromLeft++; costB--; } } }
                    if (leftNeighbour.ore >= costO && o) { for (int i = 0; i < leftNeighbour.ore; i++) { if (costO > 0) { amountOfRawFromLeft++; costO--; } } }
                    if (leftNeighbour.stone >= costT && t) { for (int i = 0; i < leftNeighbour.stone; i++) { if (costT > 0) { amountOfRawFromLeft++; costT--; } } }
                    if (leftNeighbour.wood >= costW && w) { for (int i = 0; i < leftNeighbour.wood; i++) { if (costW > 0) { amountOfRawFromLeft++; costW--; } } }
                    if (leftNeighbour.glass >= costG && g) { for (int i = 0; i < leftNeighbour.glass; i++) { if (costG > 0) { amountOfManuFromLeft++; costG--; } } }
                    if (leftNeighbour.papyrus >= costP && p) { for (int i = 0; i < leftNeighbour.papyrus; i++) { if (costP > 0) { amountOfManuFromLeft++; costP--; } } }
                    if (leftNeighbour.loom >= costL && l) { for (int i = 0; i < leftNeighbour.loom; i++) { if (costL > 0) { amountOfManuFromLeft++; costL--; } } }
                    leftFirst = false;
                }

                else
                {
                    //if i can buy from the right player calculate the cost 
                    if (rightNeighbour.brick >= costB && b) { for (int i = 0; i < rightNeighbour.brick; i++) { if (costB > 0) { amountOfRawFromRight++; costB--; } } }
                    if (rightNeighbour.ore >= costO && o) { for (int i = 0; i < rightNeighbour.ore; i++) { if (costO > 0) { amountOfRawFromRight++; costO--; } } }
                    if (rightNeighbour.stone >= costT && t) { for (int i = 0; i < rightNeighbour.stone; i++) { if (costT > 0) { amountOfRawFromRight++; costT--; } } }
                    if (rightNeighbour.wood >= costW && w) { for (int i = 0; i < rightNeighbour.wood; i++) { if (costW > 0) { amountOfRawFromRight++; costW--; } } }
                    if (rightNeighbour.glass >= costG && g) { for (int i = 0; i < rightNeighbour.glass; i++) { if (costG > 0) { amountOfManuFromRight++; costG--; } } }
                    if (rightNeighbour.papyrus >= costP && p) { for (int i = 0; i < rightNeighbour.papyrus; i++) { if (costP > 0) { amountOfManuFromRight++; costP--; } } }
                    if (rightNeighbour.loom >= costL && l) { for (int i = 0; i < rightNeighbour.loom; i++) { if (costL > 0) { amountOfManuFromRight++; costL--; } } }
                    if (!leftFirst && j == 0) leftFirst = true;
                }
            }

            int totalCost = 0;
            totalCost = (amountOfRawFromLeft * leftMultiRaw) + (amountOfManuFromLeft * leftMultiManu);
            totalCost += (amountOfRawFromRight * rightMultiRaw) + (amountOfManuFromRight * rightMultiManu);

            return totalCost;
        }

#endif

        /// <summary>
        /// AI Player makes a move
        /// </summary>
        /// <param name="gm"></param>
        public void makeMove(GameManager gm)
        {
            if (AIBehaviour != null)
            {
                AIBehaviour.makeMove(this, gm);
            }
            /*
            else if (LeadersAIBehaviour != null)
            {
                LeadersAIBehaviour.makeMove(this, (LeadersGameManager)gm);
            }
            */
        }
    }
}
