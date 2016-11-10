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

        public int debtToken { get; set; }

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

        // If the player has played the Courtesan's Guild, they copy a leader card from a neighbor.
        // This leader could affect military (Hannibal, Caesar), or give coins (Xenophon, Nero), or
        // have some other effect that needs to be considered whenever leader cards are considered.
        public CardId copiedLeaderId = CardId.Lumber_Yard;

        // This player is in a special game phase (e.g. playing Babylon's extra card or playing from the discard pile)
        // The player needs to provide input as to what to do next.
        public GamePhase phase = GamePhase.None;

        //bilkis (0 is nothing, 1 is ore, 2 is stone, 3 is glass, 4 is papyrus, 5 is loom, 6 is wood, 7 is brick
        public byte bilkis;
        public bool hasBilkis;

        /// <summary>
        /// True if the player has played a card or wonder stage with a diplomacy effect for this
        /// age.  Default is false.  Only applicable with the Cities expansion pack.
        /// </summary>
        public bool diplomacyEnabled = false;

        public bool hasArchitectCabinet = false;

        //stored actions for the turn
        private List<Effect> actions = new List<Effect>();

        private List<int> coinTransactions = new List<int>();

        private int coinsToLose = 0;

        //Player's left and right neighbours
        public Player leftNeighbour { get; set; }

        public Player rightNeighbour { get; set; }

        public Boolean changeNickName {get; set; }
        public String newNickName {get; set; }

        public bool hasWestTradingPost = false;
        public bool hasEastTradingPost = false;
        public bool hasMarketplace = false;
        public bool hasClandestineDockWest = false;
        public bool hasClandestineDockEast = false;
        public bool hasSecretWarehouse = false;
        public int nBlackMarket = 0;    // this is an integer instead of a boolean because it's possible to have more than one Black Market Effect

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

            // add the AI algorithm
            if (isAI)
                AIBehaviour = new AIMoveAlgorithm4();

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
            if (c.effect is ResourceEffect || c.effect is CoinsAndPointsEffect || c.effect is PlayACardForFreeOncePerAgeEffect)
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

        public void addCoinLossTransaction(int ncoins)
        {
            coinsToLose += ncoins;
        }

        public void executeActionNow(Card card)
        {
            Effect effect = card.effect;

            // I think the only effects that really need to be dealt with NOW are those
            // that affect game state (e.g. Babylon B, Play a discarded card.)
            if (effect == null)
            {
                switch(card.Id)
                {
                    case CardId.Babylon_B_s2:
                        // This is a signal that the Babylon player gets to take a 7th turn in this and future ages.
                        // It only does something after the 6th turn, unlike other Powers such as Halikarnassos, Solomon,
                        // and Roma (B)
                        babylonPowerEnabled = true;
                        break;

                    case CardId.Halikarnassos_A_s2:
                    case CardId.Halikarnassos_B_s1:
                    case CardId.Halikarnassos_B_s2:
                    case CardId.Halikarnassos_B_s3:
                        phase = GamePhase.Halikarnassos;
                        break;

                    case CardId.Solomon:
                        phase = GamePhase.Solomon;
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

                        // the player needs to choose another leader from his leader d
                        phase = GamePhase.RomaB;
                        break;

                    case CardId.Courtesans_Guild:
                        phase = GamePhase.Courtesan;
                        break;

                    case CardId.Gambling_Den:
                        addTransaction(6);
                        rightNeighbour.addTransaction(1);
                        leftNeighbour.addTransaction(1);
                        break;

                    case CardId.Secret_Warehouse:
                        hasSecretWarehouse = true;
                        break;

                    case CardId.Gambling_House:
                        addTransaction(9);
                        rightNeighbour.addTransaction(2);
                        leftNeighbour.addTransaction(2);
                        break;

                    case CardId.Black_Market:
                        nBlackMarket += 1;
                        break;

                    case CardId.Architect_Cabinet:
                        hasArchitectCabinet = true;
                        break;
                }
            }
            else if (effect is CommercialDiscountEffect)
            {
                switch (card.Id)
                {
                    case CardId.Marketplace:
                        hasMarketplace = true;
                        break;

                    case CardId.Clandestine_Dock_West:
                        hasClandestineDockWest = true;
                        break;

                    case CardId.Clandestine_Dock_East:
                        hasClandestineDockEast = true;
                        break;

                    case CardId.West_Trading_Post:
                        hasWestTradingPost = true;
                        break;

                    case CardId.East_Trading_Post:
                        hasEastTradingPost = true;
                        break;

                    case CardId.Olympia_B_s1:
                        hasWestTradingPost = hasEastTradingPost = true;
                        break;
                }
            }
            else if (effect is PlayACardForFreeOncePerAgeEffect)
            {
                throw new Exception("This ability needs to be dealt with on the end-of-turn action queue.");
            }
            else if (effect is LossOfCoinsEffect)
            {
                LossOfCoinsEffect loce = effect as LossOfCoinsEffect;

                Player p = rightNeighbour;

                while (p != this)
                {
                    switch(loce.lc)
                    {
                        case LossOfCoinsEffect.LossCounter.Constant:
                            p.addCoinLossTransaction(loce.coinsLost);
                            break;

                        case LossOfCoinsEffect.LossCounter.ConflictToken:
                            p.addCoinLossTransaction(p.conflictTokenOne + p.conflictTokenTwo + p.conflictTokenThree);
                            break;

                        case LossOfCoinsEffect.LossCounter.WonderStage:
                            p.addCoinLossTransaction(p.currentStageOfWonder - 1);
                            break;
                    }

                    p = p.rightNeighbour;
                }
            }
            else if (effect is DiplomacyEffect)
            {
                // Aspasia, Residence, Consulate, Embassy, Wonder stage on Byzantium & China B
                diplomacyEnabled = true;
            }
            else if (
                effect is ScienceWildEffect ||
                effect is ScienceEffect ||
                effect is MilitaryEffect ||
                effect is FreeLeadersEffect ||
                effect is StructureDiscountEffect ||
                effect is CopyScienceSymbolFromNeighborEffect)
            {
                // nothing to do; this card will be included in the end of game point total, or
                // - Military cards are used at the end of each age to resolve conflicts
                // - Science cards are used at the end of the game.
                // - Free Leaders effects are captured when the cards are put into play
            }
            else
            {
                throw new Exception("Unimplemented effect type");
            }
            // any other effects do not require immediate action, they will be dealt with at the end of the turn,
            // end of the age, or the end of the game.
        }

        //Execute actions
        //change the Player score information based on the actions
        public void executeAction()
        {
            foreach(int m in coinTransactions)
            {
                coin += m;
            }

            coinTransactions.Clear();

            //go through each action and execute the actions stored
            foreach (Effect act in actions)
            {
                if (act is ResourceEffect)
                {
                    dag.add(act as ResourceEffect);
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

                            if (e.classConsidered == StructureType.ConflictToken)
                            {
                                // Slave Market
                                coin += conflictTokenOne + conflictTokenTwo + conflictTokenThree;
                            }
                        }
                    }
                }
                else if (act is PlayACardForFreeOncePerAgeEffect)
                {
                    olympiaPowerEnabled = true;
                    olympiaPowerAvailable = true;
                    gm.gmCoordinator.sendMessage(this, "EnableFB&Olympia=true");
                }
                else
                {
                    //do nothing for now
                    throw new NotImplementedException();
                }
            }

            actions.Clear();
        }

        public void takeDebtTokens(int nDebtTokens)
        {
            debtToken += nDebtTokens;
            coin -= (coinsToLose - nDebtTokens);

            coinsToLose = 0;
        }

        public void loseCoins()
        {
            if (coinsToLose != 0)
            {
                if (isAI)
                {
                    AIBehaviour.loseCoins(this, coinsToLose);
                }
                else
                {
                    // TODO: implement a client handler for paying debt tokens
                    //
                    phase = GamePhase.Debt;
                    gm.gmCoordinator.sendMessage(this, "GetDebtTokens");
                }
            }
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

                if (cpe.classConsidered == StructureType.ConflictToken)
                {
                    // Alexander and Mourner's Guild
                    sum += (leftNeighbour.conflictTokenOne + leftNeighbour.conflictTokenTwo + leftNeighbour.conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                    sum += (rightNeighbour.conflictTokenOne + rightNeighbour.conflictTokenTwo + rightNeighbour.conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                if (cpe.classConsidered == StructureType.Leader)
                {
                    // if either neighbor's leader was actually copied using the Courtesan's Guild, remove it from the total.
                    // I'm just assuming that the Courtesan's Guild _acutally_ caused a leader to be copied here.  If it did not,
                    // it would actually be incorrect to subtract this, as no leader was added to that players' list of built
                    // leader structures.
                    if (leftNeighbour.playedStructure.Exists(x => x.Id == CardId.Courtesans_Guild) ||
                        rightNeighbour.playedStructure.Exists(x => x.Id == CardId.Courtesans_Guild))
                    {
                        sum -= 1;
                    }
                }
            }

            if (cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.PlayerAndNeighbors || cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.Player)
            {
                sum += cpe.victoryPointsAtEndOfGameMultiplier * playedStructure.Where(x => x.structureType == cpe.classConsidered).Count();

                if (cpe.classConsidered == StructureType.MilitaryLosses)
                {
                    sum += lossToken * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                if (cpe.classConsidered == StructureType.ConflictToken)
                {
                    // Alexander and Slave Market
                    sum += (conflictTokenOne + conflictTokenTwo + conflictTokenThree) * cpe.victoryPointsAtEndOfGameMultiplier;
                }

                if (cpe.classConsidered == StructureType.ThreeCoins)
                {
                    // Midas and Gamer's Guild give points for each set of 3 coins in the player's possession at the end of the game.
                    sum += coin / 3;
                }
            }

            if (cpe.cardsConsidered == CoinsAndPointsEffect.CardsConsidered.None)
            {
                // Civilian structures and wonder stages constructed fall into this category.
                sum += cpe.victoryPointsAtEndOfGameMultiplier;
            }

            return sum;
        }

        struct ScienceSymbols
        {
            public int nTablet;
            public int nCompass;
            public int nGear;
        };

        struct ScienceScore
        {
            // copy the input parameters
            public ScienceSymbols sym;
            public int groupMultiplier;

            // calculated values
            public int baseScore;       // nCompass^2 + nGear^2 + nTablet^2
            public int nGroups;         // number of complete sets
            public int TotalPoints;     // total number of points this combination is worth
        };

        int CalculateScienceGroupScore(int nCompass, int nGear, int nTablet, int groupMultiplier, out ScienceScore ss)
        {
            ss.sym.nCompass = nCompass;
            ss.sym.nGear = nGear;
            ss.sym.nTablet = nTablet;
            ss.groupMultiplier = groupMultiplier;

            // Compute output values
            ss.baseScore = ss.sym.nCompass * ss.sym.nCompass + ss.sym.nGear * ss.sym.nGear + ss.sym.nTablet * ss.sym.nTablet;
            ss.nGroups = Math.Min(Math.Min(ss.sym.nCompass, ss.sym.nGear), ss.sym.nTablet);
            ss.TotalPoints = ss.baseScore + ss.nGroups * ss.groupMultiplier;

            return ss.TotalPoints;
        }

        ScienceScore FindBestScienceWildcard(int nWildScienceSymbols, ScienceSymbols copiedSymbols, int groupMultiplier)
        {
            ScienceScore tmpResult = new ScienceScore();
            ScienceScore bestResult = tmpResult;

            int maxScienceScore = 0;

            // this player's science cards:
            int nCompass = playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Compass).Count();
            int nGear = playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Gear).Count();
            int nTablet = playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Tablet).Count();

            // now add wild cards and symbols copied from neighbors by mask effect cards.

            // if wild cards are in play, we choose the best combination of wilds to get the maximum overall
            // score, with Aristotle's bonus factored in.  In some cases, Aristotle's effect will mean it's
            // more beneficial to use the wild card(s) to make more groups rather than like symbols.
            // For example: 1/2/5+1 wild.  Without Aristotle, the maximum score is 1/2/6 = 48.
            // But with Aristotle's bonus, it's better to use the wild card to make a 2nd set instead
            // 1/2/6 = 51 with Aristotle but if you use the wild to make 2/2/5, that group is worth 53 points.

            for (int nWildTablets = 0; nWildTablets <= Math.Min(nWildScienceSymbols, copiedSymbols.nTablet); ++nWildTablets)
            {
                for (int nWildCompasses = 0; nWildCompasses <= Math.Min(nWildScienceSymbols - nWildTablets, copiedSymbols.nCompass); ++nWildCompasses)
                {
                    int mWildGears = Math.Min(nWildScienceSymbols - (nWildTablets + nWildCompasses), copiedSymbols.nGear);
                    int score = CalculateScienceGroupScore(nTablet + nWildTablets, nCompass + nWildCompasses, nGear + mWildGears, groupMultiplier, out tmpResult);
                    if (score > maxScienceScore)
                    {
                        maxScienceScore = score;
                        bestResult = tmpResult;
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

            score.coins -= debtToken;
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

            ScienceSymbols wildScienceSymbols;

            int nScienceWildCards = playedStructure.Where(x => x.effect is ScienceWildEffect).Count();

            // figure out what symbols can be copied from neighbors by masks
            int nMasks = playedStructure.Where(x => x.effect is CopyScienceSymbolFromNeighborEffect).Count();

            wildScienceSymbols.nTablet = nScienceWildCards + 
                leftNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Tablet).Count() +
                rightNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Tablet).Count();
            wildScienceSymbols.nCompass = nScienceWildCards +
                leftNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Compass).Count() +
                rightNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Compass).Count();
            wildScienceSymbols.nGear = nScienceWildCards + 
                leftNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Gear).Count() +
                rightNeighbour.playedStructure.Where(x => x.effect is ScienceEffect && ((ScienceEffect)x.effect).symbol == ScienceEffect.Symbol.Gear).Count();

            // if (nScienceWildCards != 0)
            //    Console.WriteLine("  {0} science wild card effect(s)", nScienceWildCards);

            bool hasAristotle = playedStructure.Exists(x => x.Id == CardId.Aristotle);
            ScienceScore scienceScore = FindBestScienceWildcard(nScienceWildCards + nMasks, wildScienceSymbols, hasAristotle ? 10 : 7);

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
                                        // TODO: refactor this for Cities.
                                        ScienceSymbols tmp;

                                        tmp.nTablet = nScienceWildCards + 1;
                                        tmp.nCompass = nScienceWildCards + 1;
                                        tmp.nGear = nScienceWildCards + 1;

                                        tmpScienceScore = FindBestScienceWildcard(nScienceWildCards + 1, tmp, hasAristotle ? 10 : 7);
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
                                    // If the copied guild is the Scientists' Guild, the copied wonder will be added to this player's
                                    // science score.
                                    scienceScore = tmpScienceScore;
                                }
                                else
                                {
                                    // If the copied guild is any other card, the score for that guild is added to the wonder score for
                                    // Olympia.
                                    score.wonders += maxPoints;
                                }
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
                    // Shipowners guild counts 1 point for each RawMaterial, Goods, and Guild structure in the players' city.
                    score.guilds += playedStructure.Where(x => x.structureType == StructureType.RawMaterial || x.structureType == StructureType.Goods || x.structureType == StructureType.Guild).Count();
                }
                else if (c.Id == CardId.Counterfeiters_Guild)
                {
                    LossOfCoinsEffect lce = c.effect as LossOfCoinsEffect;

                    score.guilds += lce.victoryPoints;
                }
            }

            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.Leader))
            {
                if (c.effect == null)
                {
                    switch (c.Id)
                    {
                        case CardId.Justinian:
                            {
                                // Justinian is worth 3 victory points for each set of  3 Age cards(Military + Scientific + Civilian) in the player's city.

                                int nMilitaryCards = playedStructure.Where(x => x.structureType == StructureType.Military).Count();
                                int nScienceCards = playedStructure.Where(x => x.structureType == StructureType.Science).Count();
                                int nCivilianCards = playedStructure.Where(x => x.structureType == StructureType.Civilian).Count();

                                // find the structure type with the fewest number of cards played
                                int least = Math.Min(Math.Min(nMilitaryCards, nCivilianCards), nScienceCards);

                                // Score 3 times that number for Justinian.
                                score.leaders += least * 3;
                            }
                            break;

                        case CardId.Plato:
                            {
                                // Plato is worth 7 victory points for each set of 7 Age cards (Raw Material + Manufactured Goods + Civilian + Commercial + Science + Military + Guild) in the player's city. 

                                int nCards = playedStructure.Where(x => x.structureType == StructureType.RawMaterial).Count();
                                int least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Goods).Count();
                                if (nCards < least) least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Civilian).Count();
                                if (nCards < least) least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Commerce).Count();
                                if (nCards < least) least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Science).Count();
                                if (nCards < least) least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Military).Count();
                                if (nCards < least) least = nCards;
                                nCards = playedStructure.Where(x => x.structureType == StructureType.Guild).Count();
                                if (nCards < least) least = nCards;

                                // Score 7 times the lowest color group for Plato
                                score.leaders += least * 7;
                            }
                            break;
                    }
                }
                else if (c.effect is CoinsAndPointsEffect)
                {
                    score.leaders += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
                }
            }

            if (hasAristotle)
                score.leaders += scienceScore.nGroups * 3;

            foreach (Card c in playedStructure.Where(x => x.structureType == StructureType.City))
            {
                if (c.effect == null)
                {
                    switch (c.Id)
                    {
                        // Special-case this structure as its effect is unique
                        case CardId.Architect_Cabinet:
                            score.cities += 2;
                            break;
                    }
                }
                else if (c.effect is CoinsAndPointsEffect)
                {
                    // Gates of the City, Tabularium, Capitol, Secret Society, Slave Market
                    score.cities += CountVictoryPoints(c.effect as CoinsAndPointsEffect);
                }
                else if (c.effect is LossOfCoinsEffect)
                {
                    // Hideout, Lair, Sepulcher, Builder's Union, Botherhood, Cenotaph,
                    LossOfCoinsEffect lce = c.effect as LossOfCoinsEffect;

                    score.cities += lce.victoryPoints;
                }
                else if (c.effect is DiplomacyEffect)
                {
                    DiplomacyEffect de = c.effect as DiplomacyEffect;

                    score.cities += de.victoryPoints;
                }
            }

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
            // Note cannot use playedStructure.Contains(card) because Age 1 Loom != Age 2 Loom, so it's possible to build more than one of them.
            if (playedStructure.Exists(x => x.Id == card.Id))
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

            if (card.structureType == StructureType.Guild && playedStructure.Exists(x => x.Id == CardId.Ramses))
            {
                // Ramses: The player can build any Guild card for free.
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

            if (hasArchitectCabinet)
            {
                // Wonder stages have no resource cost, but the coin cost (Petra's second stage) must still be paid.
                if (coin < cost.coin)
                {
                    // not enough coins in the treasury
                    return Buildable.InsufficientCoins;
                }
                else
                {
                    // Otherwise it's free
                    return Buildable.True;
                }
            }

            //can player afford cost with DAG resources
            if (isCostAffordableWithDAG(cost, nWildResources) == Buildable.True) return Buildable.True;

            //can player afford cost by conducting commerce?
            if (isCostAffordableWithCommerce(cost, nWildResources) == Buildable.CommerceRequired)
                return Buildable.CommerceRequired;

            //absolutely all options exhausted. return F
            return Buildable.InsufficientResources;
        }

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
        public void makeMove()
        {
            AIBehaviour.makeMove(this, gm);
        }
    }
}
