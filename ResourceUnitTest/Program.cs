using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SevenWonders;

namespace ResourceUnitTest
{
    class Program
    {
        static void Verify(bool expression)
        {
            if (!expression)
                throw new Exception();
        }

        static void Verify2(Cost cost, List<ResourceEffect> cityResources, List<ResourceEffect> leftResources, List<ResourceEffect> rightResources,
            ResourceManager.CommercePreferences pref, ResourceManager.CommerceEffects commerceEffects, CommerceOptions expectedResult)
        {
            ResourceManager resMan = new ResourceManager();

            cityResources.ForEach(x =>
            {
                resMan.add(x);
            });

            resMan.SetCommerceEffect(commerceEffects);

            CommerceOptions co = resMan.GetCommerceOptions(cost, pref, leftResources, rightResources);

            Verify(co.bAreResourceRequirementsMet == expectedResult.bAreResourceRequirementsMet);
            Verify(co.bankCoins == expectedResult.bankCoins);
            Verify(co.leftCoins == expectedResult.leftCoins);
            Verify(co.rightCoins == expectedResult.rightCoins);
        }

        static void Main(string[] args)
        {
            /* I've made the API take a cost, not a card, for now.  I may change this in the future to take a card.
             * 
            using (System.IO.StreamReader file = new System.IO.StreamReader(System.Reflection.Assembly.Load("GameManager").
                GetManifestResourceStream("GameManager.7 Wonders Card list.csv")))
            {
                // skip the header line
                file.ReadLine();

                String line = file.ReadLine();

                while (line != new List<ResourceEffect>() && line != String.Empty)
                {
                    fullCardList.Add(new Card(line.Split(',')));
                    line = file.ReadLine();
                }
            }

            Card cardPawnShop = fullCardList.Find(x => x.Id == CardId.Pawnshop);

            Verify(cardPawnShop.cost.coin == 0);
            Verify(cardPawnShop.cost.wood == 0 && cardPawnShop.cost.stone == 0 && cardPawnShop.cost.clay == 0 && cardPawnShop.cost.ore == 0);
            Verify(cardPawnShop.cost.glass == 0 && cardPawnShop.cost.cloth == 0 && cardPawnShop.cost.papyrus == 0);
            Verify(cardPawnShop.cost.CostAsString() == string.Empty);

            Card cardTimberYard = fullCardList.Find(x => x.Id == CardId.Timber_Yard);

            Verify(cardTimberYard.cost.coin == 1);
            Verify(cardTimberYard.cost.wood == 0 && cardTimberYard.cost.stone == 0 && cardTimberYard.cost.clay == 0 && cardTimberYard.cost.ore == 0);
            Verify(cardTimberYard.cost.glass == 0 && cardTimberYard.cost.cloth == 0 && cardTimberYard.cost.papyrus == 0);
            Verify(cardTimberYard.cost.CostAsString() == string.Empty);

            Card cardScriptorium = fullCardList.Find(x => x.Id == CardId.Scriptorium);

            Verify(cardScriptorium.cost.coin == 0);
            Verify(cardScriptorium.cost.wood == 0 && cardScriptorium.cost.stone == 0 && cardScriptorium.cost.clay == 0 && cardScriptorium.cost.ore == 0);
            Verify(cardScriptorium.cost.glass == 0 && cardScriptorium.cost.cloth == 0 && cardScriptorium.cost.papyrus == 1);
            Verify(cardScriptorium.cost.CostAsString() == "P");

            */

            // Possible resource structures (leaving out leaders, Secret Warehouse, and Black Market for now)
            ResourceEffect wood_1 = new ResourceEffect(true, "W");
            ResourceEffect stone_1 = new ResourceEffect(true, "S");
            ResourceEffect clay_1 = new ResourceEffect(true, "B");
            ResourceEffect ore_1 = new ResourceEffect(true, "O");

            // Resources (either/or)
            ResourceEffect wood_clay = new ResourceEffect(true, "WB");
            ResourceEffect stone_clay = new ResourceEffect(true, "SB");
            ResourceEffect clay_ore = new ResourceEffect(true, "BO");
            ResourceEffect stone_wood = new ResourceEffect(true, "SW");
            ResourceEffect wood_ore = new ResourceEffect(true, "WO");
            ResourceEffect stone_ore = new ResourceEffect(true, "OS");

            // Resources (double)
            ResourceEffect wood_2 = new ResourceEffect(true, "WW");
            ResourceEffect stone_2 = new ResourceEffect(true, "SS");
            ResourceEffect clay_2 = new ResourceEffect(true, "BB");
            ResourceEffect ore_2 = new ResourceEffect(true, "OO");

            // Goods
            ResourceEffect cloth = new ResourceEffect(true, "C");
            ResourceEffect glass = new ResourceEffect(true, "G");
            ResourceEffect papyrus = new ResourceEffect(true, "P");

            // Choose any, but can only be used by player, not neighbors
            ResourceEffect forum = new ResourceEffect(false, "CGP");
            ResourceEffect caravansery = new ResourceEffect(false, "WSBO");

            // create some test cost structures
            /*
            Cost costZero = new Cost();                 // i.e. Pawnshop

            Cost costCoinsOnly = new Cost("3");       // Leader
            Cost costSingleResource = new Cost("S");    // Baths
            Cost costTripleResource = new Cost("OOO");  // Halikarnassos 2nd stage
            Cost costQuadResource = new Cost("BBBB");   // Bablon A 3rd stage
            Cost costSingleGood = new Cost("C");
            Cost costDoubleGood = new Cost("PP");
            Cost costTripleGood = new Cost("CGP");

            Cost costMix1 = new Cost("BBBCP");
            Cost costMix2 = new Cost("OOCG");
            Cost costMix3 = new Cost("SSSO");
            Cost costMix4 = new Cost("SSSSP");          // Giza B 4th stage
            Cost costMix5 = new Cost("WSBOCGP");        // Palace
            Cost costMix6 = new Cost("3OOG");         // Torture Chamber
            Cost costMix7 = new Cost("5C");         // Contingent
            */

            // CommerceOptions costResult;

            CommerceOptions expectedResult = new CommerceOptions();

            expectedResult.bAreResourceRequirementsMet = true;

            Verify2(new Cost(), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bankCoins = 3;
            Verify2(new Cost("3"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // Verify(costResult.commerceOptions[0].purchasedResourceFromLeftNeighbor == false);
            // Verify(costResult.commerceOptions[0].purchasedResourceFromRightNeighbor == false);

            expectedResult.bankCoins = 0;
            expectedResult.bAreResourceRequirementsMet = false;

            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // Given a list of available resources (i.e. this city's resources and those of its neighbors),
            // we want to know a list of commere options for building it.

            // Can we build a single-resource card with that resource missing?
            Verify2(new Cost("S"), new List<ResourceEffect> { wood_1, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // Can we build a single-resource card with that resource present?
            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("S"), new List<ResourceEffect> { stone_1, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bankCoins = 5;
            Verify2(new Cost("5C"), new List<ResourceEffect> { cloth, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // Check that last one again, to confirm it's not buildable if the resource required is missing.
            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.bankCoins = 0;
            Verify2(new Cost("5C"), new List<ResourceEffect> { papyrus, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("WW"), new List<ResourceEffect> { wood_2, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bankCoins = 4;
            Verify2(new Cost("4WW"), new List<ResourceEffect> { wood_2, }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bankCoins = 0;
            Verify2(new Cost("OWW"), new List<ResourceEffect> { wood_2, ore_1 }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("OWW"), new List<ResourceEffect> { wood_2, ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WW"), new List<ResourceEffect> { wood_1, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("WWB"), new List<ResourceEffect> { wood_1, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("WWB"), new List<ResourceEffect> { wood_ore, stone_wood, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // here's a more intersesting case: the structure costs wood and 2 ore, with two flexes and a caravansery.
            // it should be buildable but the algorithm must realize that it must take the ore from the wood/ore option.
            Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, stone_wood, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("OOO"), new List<ResourceEffect> { ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("OOO"), new List<ResourceEffect> { ore_1, ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // Make sure it's not a problem to have an unused resource in the middle of the string
            Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WBOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("BSOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWBS"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay, papyrus }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWOOPPG"), new List<ResourceEffect> { papyrus, papyrus, stone_wood, clay_ore, wood_ore, wood_clay, forum, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWOOPPG"), new List<ResourceEffect> { papyrus, papyrus, stone_wood, clay_ore, stone_clay, wood_ore, wood_clay, forum }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWSOO"), new List<ResourceEffect> { stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { clay_1, stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("WWSS"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("PWWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay, papyrus }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // This one has more than one success path as it contains more resources than requirements.
            Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { clay_2, stone_wood, clay_ore, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            ///////////////////////////
            // Start of commerce tests
            ///////////////////////////

            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { wood_1, }, new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 2;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_1, }, new List<ResourceEffect>(), ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.rightCoins = 4;
            Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 2;
            Verify2(new Cost("SSB"), new List<ResourceEffect>(), new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("SSB"), new List<ResourceEffect> { wood_1, }, new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("SSBW"), new List<ResourceEffect> { wood_1, }, new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("SSSBB"), new List<ResourceEffect> { stone_wood, }, new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            List<ResourceEffect> myCity = new List<ResourceEffect> { wood_1, stone_2, stone_clay, forum, };
            List<ResourceEffect> leftCity = new List<ResourceEffect> { papyrus, cloth, ore_2, stone_wood, };
            List<ResourceEffect> rightCity = new List<ResourceEffect> { papyrus, wood_2, wood_ore, wood_clay, };

            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("SSSG"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 2;
            Verify2(new Cost("SSSSP"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 4;
            Verify2(new Cost("PCG"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 4;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("WWWO"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("BBB"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("WWBBSS"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            myCity = new List<ResourceEffect> { ore_1, caravansery };

            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("WWBBSS"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 2;   // stone
            expectedResult.rightCoins = 6;  // 2 wood, 2 one brick
            Verify2(new Cost("WWBBSO"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 4;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("WWOP"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("WW"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 4;
            Verify2(new Cost("WWP"), myCity, leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 4;
            expectedResult.rightCoins = 10;

            Verify2(new Cost("WWWWBPP"), new List<ResourceEffect>(), leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.None, expectedResult);
            Verify2(new Cost("WWWWBPP"), new List<ResourceEffect>(), leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.rightCoins = 0;
            Verify2(new Cost("WO"), new List<ResourceEffect>(), leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 4;
            Verify2(new Cost("WO"), new List<ResourceEffect>(), leftCity, rightCity, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("P"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("C"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("C"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("CC"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("PC"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("PC"), new List<ResourceEffect>(), new List<ResourceEffect> { papyrus, cloth }, new List<ResourceEffect> { cloth, glass }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.Marketplace, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.WestTradingPost, expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.WestTradingPost, expectedResult);

            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.WestTradingPost, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.WestTradingPost, expectedResult);

            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("SSS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.WestTradingPost, expectedResult);

            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("SSS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.WestTradingPost | ResourceManager.CommerceEffects.EastTradingPost, expectedResult);

            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect> { stone_1 }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.WestTradingPost | ResourceManager.CommerceEffects.EastTradingPost, expectedResult);


            // After we have a list of options for building a card, we can apply commercial effects,
            // then resolve the options into:
            // * minimal cost
            // * prefer to pay left neighbor (may be minimal cost, mat be higher than minimal)
            // * prefer to pay right neighbor (may be minimal cost, may be higher than minimal)
            // 

            Console.WriteLine("Resource Manager tests completed.  All unit tests passed.");
        }
    }
}
