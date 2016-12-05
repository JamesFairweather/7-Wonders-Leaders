using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace SevenWonders
{
    class AIMoveAlgorithm4 : AIMoveBehaviour
    {
        //int maxOBW = 2;
        //int maxStone = 3;
        //int maxLPG = 1;
        private static Logger logger = LogManager.GetLogger("SevenWondersServer");

        public void makeMove(Player player, GameManager gm)
        {
            //go for blue cards only on the third age
            //if not, Discard Red Cards
            //otherwise, discard first card

            /*
            string strOutput = string.Format("{0} hand: [ ", player.nickname);

            if (gm.phase == GamePhase.LeaderRecruitment)
            {
                foreach (Card card in player.draftedLeaders)
                {
                    strOutput += card.Id;
                    strOutput += " ";
                }
            }
            else
            {
                foreach (Card card in player.hand)
                {
                    strOutput += card.Id;
                    strOutput += " ";
                }
            }

            strOutput += "]";

            logger.Info(strOutput);
            */

            if (gm.phase == GamePhase.LeaderDraft || gm.phase == GamePhase.LeaderRecruitment)
            {
                // int[] favouredLeaders = { 216, 220, 222, 232, 200, 208, 205, 221, 214, 236, 213 };
                CardId [] favouredLeaders = { CardId.Leonidas, CardId.Nero, CardId.Pericles, CardId.Tomyris, CardId.Alexander, CardId.Hannibal, CardId.Caesar, CardId.Nefertiti, CardId.Cleopatra, CardId.Zenobia, CardId.Justinian };

                Card bestLeader = null;

                //try to find the highest rated card in hand
                //start looking for the highest rated card, then go down to the next highest, etc.
                foreach (CardId leaderName in favouredLeaders)
                {
                    if (gm.phase == GamePhase.LeaderDraft)
                    {
                        bestLeader = player.hand.Find(x => x.Id == leaderName);
                    }
                    else if (gm.phase == GamePhase.LeaderRecruitment)
                    {
                        bestLeader = player.draftedLeaders.Find(x => x.Id == leaderName);
                    }

                    if (bestLeader != null && player.isCardBuildable(bestLeader).buildable == CommerceOptions.Buildable.True)
                    {
                        break;
                    }
                }

                if (bestLeader == null && gm.phase == GamePhase.LeaderDraft)
                {
                    // this hand didn't contain a favoured leader, so draft the first one in the list.  We cannot
                    // discard during the draft.  Leaders may only be discarded for 3 coins during recruitment.
                    bestLeader = player.hand[0];
                }

                if (bestLeader != null)
                {
                    logger.Info(player.nickname + "Drafted leader: {0}", bestLeader.Id);
                    gm.playCard(player, bestLeader, BuildAction.BuildStructure, true, false, 0, 0, false);
                }
                else
                {
                    logger.Info(player.nickname + " Action: Discard {0}", player.draftedLeaders[0].Id);
                    gm.playCard(player, player.draftedLeaders[0], BuildAction.Discard, true, false, 0, 0, false);
                }

                return;
            }

            // Dictionary<Card, CardCost> cardValues = new Dictionary<Card, CardCost>(player.hand.Count);

            // Card cost:

            // NotBuildable
            // Free (the city has sufficient resources)
            // Coin cost to the bank only (flex brown, double, some City cards)
            // Commerce Required (can be constructed by paying neighbors for their resources)
            // If Commerce is required, how many coins to each neighbor and/or the bank

            CommerceOptions[] co = new CommerceOptions[player.hand.Count];
            int [] cardValues = new int[player.hand.Count];

            for (int i = 0; i < player.hand.Count; ++i)
            {
                co[i] = player.isCardBuildable(player.hand[i]);
            }

            CommerceOptions nextStageCost = player.isStageBuildable();

            for (int i = 0; i < player.hand.Count; ++i)
            {
                Card card = player.hand[i];

                if (co[i].buildable == CommerceOptions.Buildable.True || co[i].buildable == CommerceOptions.Buildable.CommerceRequired)
                {
                    switch (card.structureType)
                    {
                        case StructureType.RawMaterial:
                            {
                                ResourceEffect re = card.effect as ResourceEffect;
                                if (re.resourceTypes.Length == 2)
                                {
                                    if (re.resourceTypes[0] == re.resourceTypes[1])
                                    {
                                        // doubles can be useful, but we need to examine whether we
                                        // already have enough of them.
                                        cardValues[i] = 50;
                                    }
                                    else
                                    {
                                        // Flex resources should almost always be taken
                                        cardValues[i] = 80;
                                    }
                                }
                                else
                                {
                                    // single-resource browns are fairly useless.
                                    cardValues[i] = 25;
                                }
                            }
                            break;

                        case StructureType.Goods:
                            {
                                ResourceEffect res = card.effect as ResourceEffect;

                                cardValues[i] = 45;

                                if (player.leftNeighbour.resourceMgr.getResourceList(false).Contains(res) ||
                                    player.rightNeighbour.resourceMgr.getResourceList(false).Contains(res))
                                {
                                    // Yes: drop its value significantly and even more if we have a Marketplace too.
                                    cardValues[i] = player.resourceMgr.GetCommerceEffect().HasFlag(ResourceManager.CommerceEffects.Marketplace) ? 6 : 24;
                                }
                                else
                                {
                                    if (gm.currentAge == 2)
                                    {
                                        if (gm.currentTurn > 5)
                                            cardValues[i] = 90;
                                        else if (gm.currentTurn > 2)
                                            cardValues[i] = 65;
                                    }
                                    else
                                    {
                                        if (gm.currentTurn > 4)
                                        {
                                            cardValues[i] = 55;
                                        }
                                    }

                                    if (player.resourceMgr.GetCommerceEffect().HasFlag(ResourceManager.CommerceEffects.Marketplace) && gm.currentTurn < 5)
                                    {
                                        cardValues[i] /= 2;
                                    }
                                }
                            }
                            break;

                        case StructureType.Civilian:
                            cardValues[i] = ((card.effect as CoinsAndPointsEffect).victoryPointsAtEndOfGameMultiplier - (2 - gm.currentAge)) * 10;
                            break;

                        case StructureType.Commerce:
                            switch (card.Id)
                            {
                                case CardId.Tavern:
                                    cardValues[i] = 30 - (player.coin * 10);
                                    break;

                                case CardId.West_Trading_Post:
                                    {
                                        List<ResourceEffect> leftResources = player.leftNeighbour.resourceMgr.getResourceList(false);
                                        int nLeftResources = 0;
                                        foreach (ResourceEffect re in leftResources)
                                        {
                                            if (!re.IsManufacturedGood())
                                                nLeftResources += re.resourceTypes.Length;
                                        }

                                        cardValues[i] = (nLeftResources - gm.currentTurn + 6) * 10;
                                    }
                                    break;

                                case CardId.East_Trading_Post:
                                    {
                                        List<ResourceEffect> rightResources = player.rightNeighbour.resourceMgr.getResourceList(false);
                                        int nRightResources = 0;
                                        foreach (ResourceEffect re in rightResources)
                                        {
                                            if (!(re.IsManufacturedGood()))
                                                nRightResources += re.resourceTypes.Length;
                                        }

                                        cardValues[i] = (nRightResources - gm.currentTurn + 6) * 10;
                                    }
                                    break;

                                case CardId.Marketplace:
                                    {
                                        string strGoodsNeighbors = "PCG";

                                        List<ResourceEffect> leftResources = player.leftNeighbour.resourceMgr.getResourceList(false);
                                        foreach (ResourceEffect re in leftResources)
                                        {
                                            int resIndex = strGoodsNeighbors.IndexOf(re.resourceTypes[0]);

                                            if (resIndex != -1) strGoodsNeighbors = strGoodsNeighbors.Substring(resIndex, 1);
                                        }

                                        List<ResourceEffect> rightResources = player.rightNeighbour.resourceMgr.getResourceList(false);
                                        foreach (ResourceEffect re in rightResources)
                                        {
                                            int resIndex = strGoodsNeighbors.IndexOf(re.resourceTypes[0]);

                                            if (resIndex != -1) strGoodsNeighbors = strGoodsNeighbors.Substring(resIndex, 1);
                                        }

                                        int nMyCityGoods = player.resourceMgr.getResourceList(true).FindAll(x => x.IsManufacturedGood()).Count;

                                        // for each available grey card in neighboring cities, add 20 points,
                                        // and subtract 30 points for each grey card in our city.
                                        cardValues[i] = 40 + ((3 - strGoodsNeighbors.Length) * 20) - nMyCityGoods * 30;
                                    }

                                    break;

                                case CardId.Caravansery:
                                    // This one is pretty much automatic: if it's there, take it.
                                    cardValues[i] = 90;
                                    break;

                                case CardId.Forum:
                                    cardValues[i] = 35;
                                    break;
                            }

                            break;

                        case StructureType.Military:
                            {
                                int nShieldsLeft = player.leftNeighbour.shield;
                                int nShieldsRight = player.leftNeighbour.shield;
                                int myShields = player.shield;
                                int nShields = (card.effect as MilitaryEffect).nShields;

                                if (myShields > (nShieldsRight + nShields) && myShields > (nShieldsLeft + nShields))
                                {
                                    // our city has more military strength than our neighbors and adding more won't gain us any more points
                                    cardValues[i] = 10;
                                }
                                else if (myShields > nShieldsRight && myShields > nShieldsLeft)
                                {
                                    // Our city's military strength is stronger than both of our neighbors but if one of them plays military and
                                    // we don't, we'll be tied or losing against them.
                                    cardValues[i] = 20;
                                }
                                else if (((myShields + nShields) <= nShieldsRight) && ((myShields + nShields) <= nShieldsLeft))
                                {
                                    // Even if we played this card, we would still have fewer shields than our neighbors.  We are so far behind
                                    // in military it's probably not worth playing any military.
                                    cardValues[i] = 5;
                                }
                                else if (((myShields + nShields) > nShieldsRight) && ((myShields + nShields) > nShieldsLeft))
                                {
                                    // If we play this card, we'll go from losing against both neighbors to winning.
                                    cardValues[i] = 75;
                                }
                                else if (((myShields + nShields) > nShieldsRight) || ((myShields + nShields) > nShieldsLeft))
                                {
                                    // If we play this card, we'll go from losing against one neighbor to winning
                                    cardValues[i] = 60;
                                }
                                else
                                {
                                    // can we logically ever get here?
                                    throw new NotImplementedException();
                                }
                            }
                            break;

                        case StructureType.Science:
                            cardValues[i] = 65;
                            // calculate the value of this card, and consider whether we are going for sets (1, 2, 3) or symbols
                            break;

                        case StructureType.Guild:
                            cardValues[i] = 60;
                            // calculate the value of this card.
                            break;

                        case StructureType.City:
                            break;
                    }
                }
            }

            int bestCardValue = 0;
            int bestCardIndex = -1;

            for (int i = 0; i < player.hand.Count; ++i)
            {
                if (cardValues[i] > bestCardValue)
                {
                    bestCardValue = cardValues[i];
                    bestCardIndex = i;
                }
                else if (cardValues[i] == bestCardValue)
                {
                    // two cards are of equal value.  We need to consider secondary factors
                }
            }

            Card c = null;

            if (bestCardIndex != -1)
                c = player.hand[bestCardIndex];

            // go through the total value of this hand and select the best card

#if FALSE
            /*
            foreach (Card crd in player.hand)
            {
                player.GetCost(crd);
            }
            */
            // Build Guild cards in the 3rd age (except for the Courtesan's Guild, which requires entering a special Game Phase
            // that the AI has not been programmed to think about.
            Card c = player.hand.Find(x => x.structureType == StructureType.Guild && x.Id != CardId.Courtesans_Guild && player.isCardBuildable(x).buildable == CommerceOptions.Buildable.True);

            if (c == null)
            {
                //look for buildable blue cards at the third age ..
                c = player.hand.Find(x => x.structureType == StructureType.Civilian && player.isCardBuildable(x).buildable == CommerceOptions.Buildable.True && x.age == 3);
            }

            if (c == null)
            {
                //look for buildable green cards
                c = player.hand.Find(x => x.structureType == StructureType.Science && player.isCardBuildable(x).buildable == CommerceOptions.Buildable.True);
            }

            if (c == null)
            {
                //look for buildable resource cards that give more than one manufactory resources ...
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.Commerce && player.isCardBuildable(card).buildable == CommerceOptions.Buildable.True) && card.effect is ResourceEffect)
                    {
                        // char resource = player.hand[i].effect[2];        // hunh?
                        string resource = ((ResourceEffect)card.effect).resourceTypes;

                        if (resource.Length < 3)
                            continue;

                        if (resource.Contains("C") && player.loom < maxLPG * 2) { c = card; }
                        else if (resource.Contains("P") && player.papyrus < maxLPG * 2) { c = card; }
                        else if (resource.Contains("G") && player.glass < maxLPG * 2) { c = card; }

                        // not sure what's going on here.  I think there may have been a bug in the original implementation.
                    }
                }
            }

            if (c == null)
            {
                //look for buildable resource cards that give more than one resource ...
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.RawMaterial && player.isCardBuildable(card).buildable == CommerceOptions.Buildable.True) && card.effect is ResourceEffect)
                    {
                        string resource = ((ResourceEffect)card.effect).resourceTypes;

                        if (player.brick < maxOBW && resource.Contains('B') ) { c = card; }
                        else if (player.ore < maxOBW && resource.Contains('O') ) { c = card; }
                        else if (player.stone < maxStone && resource.Contains('S') ) { c = card; }
                        else if (player.wood < maxOBW && resource.Contains('W') ) { c = card; }
                    }
                }
            }


            if (c == null)
            {
                //look for buildable resource cards that only give one and the manufactory resources ..
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.RawMaterial || card.structureType == StructureType.Goods) && player.isCardBuildable(card).buildable == CommerceOptions.Buildable.True && card.effect is ResourceEffect)
                    {
                        ResourceEffect e = card.effect as ResourceEffect;

                        char resource = e.resourceTypes[0];
                        int numOfResource = e.IsDoubleResource() ? 2 : 1;

                        if (resource == 'C' && player.loom < maxLPG) { c = card; }
                        else if (resource == 'G' && player.glass < maxLPG) { c = card; }
                        else if (resource == 'P' && player.papyrus < maxLPG) { c = card; }
                        else if (resource == 'B' && numOfResource + player.brick < maxOBW) { c = card; }
                        else if (resource == 'O' && numOfResource + player.ore < maxOBW) { c = card; }
                        else if (resource == 'S' && numOfResource + player.stone < maxStone) { c = card; }
                        else if (resource == 'W' && numOfResource + player.wood < maxOBW) { c = card; }
                    }
                }
            }

            if (c == null)
            {
                //look for buildable Red cards
                c = player.hand.Find(x => x.structureType == StructureType.Military && player.isCardBuildable(x).buildable == CommerceOptions.Buildable.True);
            }

            if (c == null)
            {
                // play a city card, if there is one
                List<Card> cityCardList = player.hand.FindAll(x => x.structureType == StructureType.City && player.isCardBuildable(x).buildable == CommerceOptions.Buildable.True);

                if (cityCardList.Count > 0)
                {
                    // Try to find a card that causes other players to lose cards
                    c = cityCardList.Find(x => x.effect is LossOfCoinsEffect);

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is CopyScienceSymbolFromNeighborEffect);
                    }

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is MilitaryEffect);
                    }

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is DiplomacyEffect);
                    }

                    if (c == null)
                    {
                        // play a point-scoring card.
                        c = cityCardList.Find(x => x.effect is CoinsAndPointsEffect);
                    }

                    // if none of the above criteria match, it means this card is has a commerce special effect,
                    // such as the Secret Warehouse, Black Market, Clandestine Dock, or Architect Cabinet, which
                    // this AI has not been programmed to think about.  So in that case, just play the first
                    // card in the list of playable city cards.
                    c = cityCardList[0];
                }
            }

#endif

            if (c == null)
            {
                //Discard the non-buildable Red cards
                foreach (Card card in player.hand)
                {
                    if (card.structureType == StructureType.Military && player.isCardBuildable(card).buildable != CommerceOptions.Buildable.True)
                    {
                        logger.Info(player.nickname + " Action: Discard {0}", card.Id);
                        gm.playCard(player, card, BuildAction.Discard, true, false, 0, 0, false);
                        return;
                    }
                }
            }

            if (c != null)
            {
                logger.Info(player.nickname + " Action: Construct {0}", c.Id);
                gm.playCard(player, c, BuildAction.BuildStructure, true, false, co[bestCardIndex].leftCoins, co[bestCardIndex].rightCoins, false);
            }
            else
            {
                // If a card is not found that matches any of the above criteria, discard the first card listed.
                c = player.hand[0];
                logger.Info(player.nickname + " Action: Discard {0}", c.Id);
                gm.playCard(player, c, BuildAction.Discard, true, false, 0, 0, false);
            }
        }
        
        public void loseCoins(Player player, int nCoins)
        {
            int nDebtTokens = 0;

            int c = player.coin - nCoins;

            if (c < 3)
            {
                nDebtTokens = player.coin - c;
            }

            player.takeDebtTokens(nDebtTokens);
        }
    }
}
