﻿using System;
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
        // public byte bilkis;
        // public bool hasBilkis;

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

        public bool waitingForDebtTokenResponse = false;

        //Player's left and right neighbours
        public Player leftNeighbour { get; set; }

        public Player rightNeighbour { get; set; }

        public Boolean changeNickName {get; set; }
        public String newNickName {get; set; }

        public AIMoveBehaviour AIBehaviour;

        private GameManager gm;

        public ResourceManager resourceMgr { get; private set; }

        public bool bUIRequiresUpdating { get; set; }

        /// <summary>
        /// Constructor. Create a Player with a given nickname
        /// </summary>
        public Player(String nickname, bool isAI, GameManager gm)
        {
            resourceMgr = new ResourceManager();

            this.nickname = nickname;

            this.isAI = isAI;

            // add the AI algorithm
            if (isAI)
                AIBehaviour = new AIMoveAlgorithm4();

            currentStageOfWonder = 0;
            changeNickName = false;
            newNickName = "";

            //set bilkis to nothing
            // hasBilkis = false;

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

                    case CardId.Bilkis:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.Bilkis);
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
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.SecretWarehouse);
                        break;

                    case CardId.Gambling_House:
                        addTransaction(9);
                        rightNeighbour.addTransaction(2);
                        leftNeighbour.addTransaction(2);
                        break;

                    case CardId.Black_Market:
                        if (!resourceMgr.GetCommerceEffect().HasFlag(ResourceManager.CommerceEffects.BlackMarket1))
                            resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.BlackMarket1);
                        else
                            resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.BlackMarket2);

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
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.Marketplace);
                        break;

                    case CardId.Clandestine_Dock_West:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.ClandestineDockWest);
                        break;

                    case CardId.Clandestine_Dock_East:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.ClandestineDockEast);
                        break;

                    case CardId.West_Trading_Post:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.WestTradingPost);
                        break;

                    case CardId.East_Trading_Post:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.EastTradingPost);
                        break;

                    case CardId.Olympia_B_s1:
                        resourceMgr.AddCommerceEffect(ResourceManager.CommerceEffects.WestTradingPost | ResourceManager.CommerceEffects.EastTradingPost);
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
                    resourceMgr.add(act as ResourceEffect);
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

            waitingForDebtTokenResponse = false;
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
                    // we have to wait for this player to respond to this message.  Not all players will receive this message,
                    // but more than one may.
                    waitingForDebtTokenResponse = true;
                    gm.gmCoordinator.sendMessage(this, string.Format("GetDebtTokens&coin={0}&coinsToLose={1}", coin, coinsToLose));
                }
            }
        }

        public int CountVictoryPoints(CoinsAndPointsEffect cpe)
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
        public CommerceOptions isCardBuildable(Card card)
        {
            CommerceOptions ret = new CommerceOptions();

            //retrieve the cost
            Cost cost = card.cost;

            //if the player already owns a copy of the card, Return F immediatley
            // Note cannot use playedStructure.Contains(card) because Age 1 Loom != Age 2 Loom, so it's possible to build more than one of them.
            if (playedStructure.Exists(x => x.Id == card.Id))
            {
                ret.bAreResourceRequirementsMet = false;
                ret.buildable = CommerceOptions.Buildable.StructureAlreadyBuilt;
                return ret;
            }

            //if the cost is !, that means its free. Return T immediately
            if (cost.coin == 0 && cost.resources == string.Empty)
            {
                ret.bAreResourceRequirementsMet = true;
                ret.buildable = CommerceOptions.Buildable.True;
                return ret;
            }

            //if the player owns the prerequiste, Return T immediately
            if (playedStructure.Exists(x => (x.chain[0] == card.strName) || (x.chain[1] == card.strName)))
            {
                ret.bAreResourceRequirementsMet = true;
                ret.buildable = CommerceOptions.Buildable.True;
                return ret;
            }

            if (card.structureType == StructureType.Guild && playedStructure.Exists(x => x.Id == CardId.Ramses))
            {
                // Ramses: The player can build any Guild card for free.
                ret.bAreResourceRequirementsMet = true;
                ret.buildable = CommerceOptions.Buildable.True;
                return ret;
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
                ret.bAreResourceRequirementsMet = false;
                ret.buildable = CommerceOptions.Buildable.InsufficientCoins;
                return ret;
            }

            ret = resourceMgr.CanAfford(cost,
                leftNeighbour.resourceMgr.getResourceList(false),
                rightNeighbour.resourceMgr.getResourceList(false),
                ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor);

            if (ret.bAreResourceRequirementsMet)
            {
                if ((ret.leftCoins == 0 && ret.rightCoins == 0) && (ret.bankCoins == 0 || (ret.bankCoins == cost.coin)))
                {
                    if (coin < ret.bankCoins)
                        ret.buildable = CommerceOptions.Buildable.InsufficientCoins;
                    else
                        ret.buildable = CommerceOptions.Buildable.True;
                }
                else if (coin < ret.bankCoins)
                {
                    ret.buildable = CommerceOptions.Buildable.InsufficientCoins;
                }
                else
                {
                    ret.buildable = CommerceOptions.Buildable.CommerceRequired;
                }
            }
            else
            {
                ret.buildable = CommerceOptions.Buildable.InsufficientResources;
            }

            return ret;
        }

        /// <summary>
        /// Determines if the Player's current stage is buildable
        /// Returns "T" if it is, returns "F" if it is not
        /// </summary>
        /// <returns></returns>
        public CommerceOptions isStageBuildable()
        {
            CommerceOptions ret = new CommerceOptions();

            //check if the current Stage is already the maximum stage
            if (currentStageOfWonder >= playerBoard.numOfStages)
            {
                ret.bAreResourceRequirementsMet = false;
                ret.buildable = CommerceOptions.Buildable.StructureAlreadyBuilt;
                return ret;
            }

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
                    ret.bAreResourceRequirementsMet = false;
                    ret.buildable = CommerceOptions.Buildable.InsufficientCoins;
                }
                else
                {
                    ret.bAreResourceRequirementsMet = true;
                    ret.buildable = CommerceOptions.Buildable.True;
                }

                return ret;
            }

            ret = resourceMgr.CanAfford(cost,
                leftNeighbour.resourceMgr.getResourceList(false), rightNeighbour.resourceMgr.getResourceList(false));

            if (ret.bAreResourceRequirementsMet)
            {
                if (ret.leftCoins == 0 && ret.rightCoins == 0)
                {
                    if (coin < ret.bankCoins)
                        ret.buildable = CommerceOptions.Buildable.InsufficientCoins;
                    else
                        ret.buildable = CommerceOptions.Buildable.True;
                }
                else
                {
                    ret.buildable = CommerceOptions.Buildable.CommerceRequired;
                }
            }
            else
            {
                ret.buildable = CommerceOptions.Buildable.InsufficientResources;
            }

            return ret;
        }

        /// <summary>
        /// AI Player makes a move
        /// </summary>
        public void makeMove()
        {
            AIBehaviour.makeMove(this, gm);
        }
    }
}
