﻿using System;
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

        static CommerceOptions Verify2(Cost cost, List<ResourceEffect> cityResources, List<ResourceEffect> leftResources, List<ResourceEffect> rightResources)
        {
            ResourceManager resMan = new ResourceManager();

            cityResources.ForEach(x =>
            {
                resMan.add(x);
            });

            return resMan.GetCommerceOptions(cost, leftResources, rightResources);
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

            CommerceOptions costResult;

            costResult = Verify2(new Cost(), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>());
            Verify(costResult.buildable == Buildable.True);
            Verify(costResult.commerceOptions.Count == 0);

            costResult = Verify2(new Cost("3"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>());

            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 3);
            Verify(costResult.commerceOptions[0].leftCoins == 0);
            Verify(costResult.commerceOptions[0].rightCoins == 0);
            // Verify(costResult.commerceOptions[0].purchasedResourceFromLeftNeighbor == false);
            // Verify(costResult.commerceOptions[0].purchasedResourceFromRightNeighbor == false);

            costResult = Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>());

            Verify(costResult.buildable == Buildable.InsufficientResources);
            Verify(costResult.commerceOptions.Count == 0);

            // Given a list of available resources (i.e. this city's resources and those of its neighbors),
            // we want to know a list of commere options for building it.

            // Can we build a single-resource card with that resource missing?
            Verify(Verify2(new Cost("S"), new List<ResourceEffect> { wood_1, }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);

            // Can we build a single-resource card with that resource present?
            Verify(Verify2(new Cost("S"), new List<ResourceEffect> { stone_1, }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            costResult = Verify2(new Cost("5C"), new List<ResourceEffect> { cloth, }, new List<ResourceEffect>(), new List<ResourceEffect>());
            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 5);
            Verify(costResult.commerceOptions[0].leftCoins == 0);
            Verify(costResult.commerceOptions[0].rightCoins == 0);

            // Check that last one again, to confirm it's not buildable if the resource required is missing.
            Verify(Verify2(new Cost("5C"), new List<ResourceEffect> { papyrus, }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);

            Verify(Verify2(new Cost("WW"), new List<ResourceEffect> { wood_2, }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            costResult = Verify2(new Cost("4WW"), new List<ResourceEffect> { wood_2, }, new List<ResourceEffect>(), new List<ResourceEffect>());
            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 4);
            Verify(costResult.commerceOptions[0].leftCoins == 0);
            Verify(costResult.commerceOptions[0].rightCoins == 0);

            Verify(Verify2(new Cost("OWW"), new List<ResourceEffect> { wood_2, ore_1 }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("OWW"), new List<ResourceEffect> { wood_2, ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            Verify(Verify2(new Cost("WW"), new List<ResourceEffect> { wood_1, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWB"), new List<ResourceEffect> { wood_1, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);
            Verify(Verify2(new Cost("WWB"), new List<ResourceEffect> { wood_ore, stone_wood, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            // here's a more intersesting case: the structure costs wood and 2 ore, with two flexes and a caravansery.
            // it should be buildable but the algorithm must realize that it must take the ore from the wood/ore option.
            Verify(Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, stone_wood, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            Verify(Verify2(new Cost("OOO"), new List<ResourceEffect> { ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);
            Verify(Verify2(new Cost("OOO"), new List<ResourceEffect> { ore_1, ore_2 }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            // Make sure it's not a problem to have an unused resource in the middle of the string
            Verify(Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WBOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("BSOO"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWBS"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay, papyrus }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWOOPPG"), new List<ResourceEffect> { papyrus, papyrus, stone_wood, clay_ore, wood_ore, wood_clay, forum, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWOOPPG"), new List<ResourceEffect> { papyrus, papyrus, stone_wood, clay_ore, stone_clay, wood_ore, wood_clay, forum }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWSOO"), new List<ResourceEffect> { stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);
            Verify(Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { clay_1, stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.True);

            Verify(Verify2(new Cost("WWSS"), new List<ResourceEffect> { wood_ore, stone_clay, clay_ore, caravansery }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);
            Verify(Verify2(new Cost("WWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);
            Verify(Verify2(new Cost("PWWOOP"), new List<ResourceEffect> { stone_wood, clay_ore, wood_ore, wood_clay, papyrus }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);

            // This one has more than one success path as it contains more resources than requirements.
            Verify(Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { stone_wood, clay_ore, stone_clay, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);
            Verify(Verify2(new Cost("WWSBOO"), new List<ResourceEffect> { clay_2, stone_wood, clay_ore, wood_ore, wood_clay }, new List<ResourceEffect>(), new List<ResourceEffect>()).buildable == Buildable.InsufficientResources);

            ///////////////////////////
            // Start of commerce tests
            ///////////////////////////

            costResult = Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_1, }, new List<ResourceEffect>());
            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 0);
            Verify(costResult.commerceOptions[0].leftCoins == 2);
            Verify(costResult.commerceOptions[0].rightCoins == 0);

            costResult = Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 });
            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 0);
            Verify(costResult.commerceOptions[0].leftCoins == 0);
            Verify(costResult.commerceOptions[0].rightCoins == 2);

            costResult = Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { clay_1, }, new List<ResourceEffect>() { stone_2 });
            Verify(costResult.buildable == Buildable.CommerceRequired);
            Verify(costResult.commerceOptions.Count == 1);
            Verify(costResult.commerceOptions[0].bankCoins == 0);
            Verify(costResult.commerceOptions[0].leftCoins == 0);
            Verify(costResult.commerceOptions[0].rightCoins == 4);

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
