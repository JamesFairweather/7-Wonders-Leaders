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

            CommerceOptions co = resMan.GetCommerceOptions(cost, leftResources, rightResources, pref);

            Verify(co.bAreResourceRequirementsMet == expectedResult.bAreResourceRequirementsMet);
            Verify(co.bankCoins == expectedResult.bankCoins);
            Verify(co.leftCoins == expectedResult.leftCoins);
            Verify(co.rightCoins == expectedResult.rightCoins);
        }

        // Resource structures
        static ResourceEffect wood_1 = new ResourceEffect(true, "W");
        static ResourceEffect stone_1 = new ResourceEffect(true, "S");
        static ResourceEffect clay_1 = new ResourceEffect(true, "B");
        static ResourceEffect ore_1 = new ResourceEffect(true, "O");

        // Resources (either/or)
        static ResourceEffect wood_clay = new ResourceEffect(true, "WB");
        static ResourceEffect stone_clay = new ResourceEffect(true, "SB");
        static ResourceEffect clay_ore = new ResourceEffect(true, "BO");
        static ResourceEffect stone_wood = new ResourceEffect(true, "SW");
        static ResourceEffect wood_ore = new ResourceEffect(true, "WO");
        static ResourceEffect stone_ore = new ResourceEffect(true, "OS");

        // Resources (double)
        static ResourceEffect wood_2 = new ResourceEffect(true, "WW");
        static ResourceEffect stone_2 = new ResourceEffect(true, "SS");
        static ResourceEffect clay_2 = new ResourceEffect(true, "BB");
        static ResourceEffect ore_2 = new ResourceEffect(true, "OO");

        // Goods
        static ResourceEffect cloth = new ResourceEffect(true, "C");
        static ResourceEffect glass = new ResourceEffect(true, "G");
        static ResourceEffect papyrus = new ResourceEffect(true, "P");

        // Choose any, but can only be used by player, not neighbors
        static ResourceEffect forum = new ResourceEffect(false, "CGP");
        static ResourceEffect caravansery = new ResourceEffect(false, "WSBO");

        static void Main(string[] args)
        {
            BasicTest();
            ComplexTests();
        }

        /// <summary>
        /// More complex tests.  These are for verifying the LowestCost algorithm implementation,
        /// which tries every possible resource path for fulfilling the resource requirements
        /// and it returns the cheapest possible commmerce option.
        /// </summary>
        static void ComplexTests()
        {
            // Most of these tests are checking for minimal cost
            CommerceOptions expectedResult = new CommerceOptions();

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 0;

            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_1 }, new List<ResourceEffect> { stone_2 },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_1 }, new List<ResourceEffect> { stone_2 },
                ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            expectedResult.rightCoins = 4;
            Verify2(new Cost("SSSS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_1, stone_1, stone_1, stone_1 }, new List<ResourceEffect> { stone_1, stone_1, stone_1, stone_1 },
                ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            expectedResult.rightCoins = 3;
            Verify2(new Cost("WSO"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_wood, stone_1, ore_2 }, new List<ResourceEffect> { wood_clay, wood_ore, stone_2 },
                ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            expectedResult.leftCoins = 4;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("WSB"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_wood, stone_1 }, new List<ResourceEffect> { wood_clay },
                ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            // Nov. 15, 2016 I had a good one today.  Babylon B, Glassworks, Caravansery,
            // trading posts in each direction, Marketplace, Clandestine Dock West.
            // West neighbor: China A, Press, Glassworks, Forest Cave, Brickyard.
            // East neighbor: Olympia B, Timber Yard, single stone, double ore.

            expectedResult.bankCoins = 0;
            expectedResult.leftCoins = 6;   // papyrus, cloth + ore
            expectedResult.rightCoins = 1;  // clay

            // The key to this one is that the Caravansery must be used for a Clay resource, not an Ore.
            // That way the right neighbor can be used to fill the clay resource and the left neighbor only
            // needs to provide 1 clay (in addition to the Paper and Cloth).  The current implementation
            // won't work because after a successful path is found, the algorithm continues looking for
            // cheaper alternatives from the remaining resources.  It does not go back up the chain and reconsider
            // resource sources that were already used.  I am thinking the algorithm should be changed when we're
            // going for LowestCost to start with 
            Verify2(new Cost("OOOBCP"), new List<ResourceEffect> { ore_1, glass, caravansery, },
                new List<ResourceEffect> { cloth, papyrus, clay_2, ore_2, wood_ore, }, new List<ResourceEffect> { glass, wood_1, clay_1, wood_2, stone_2, },
                 ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                 ResourceManager.CommerceEffects.EastTradingPost,
                 expectedResult);
        }

        /// <summary>
        /// This suite passes only if the search is stopped after the first successful path to paying for the
        /// structure is found.
        /// </summary>
        static void BasicTest()
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

            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("P"), new List<ResourceEffect>(), new List<ResourceEffect> { forum }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.WestTradingPost | ResourceManager.CommerceEffects.EastTradingPost, expectedResult);

            Verify2(new Cost("BP"), new List<ResourceEffect> { papyrus }, new List<ResourceEffect> { }, new List<ResourceEffect> { caravansery },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.WestTradingPost | ResourceManager.CommerceEffects.EastTradingPost, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, caravansery },
                new List<ResourceEffect> { stone_2, wood_2, clay_2, }, new List<ResourceEffect> { papyrus, clay_2, ore_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.EastTradingPost, expectedResult);

            // In this test, we are looking for 3 resources, but for two of them, the only source is in my own
            // resource stack.  The wood has to be purchased from a neighbor
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("WOO"), new List<ResourceEffect> { wood_ore, caravansery }, new List<ResourceEffect> { stone_2, wood_2, clay_2, }, new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 }, ResourceManager.CommercePreferences.BuyFromRightNeighbor, ResourceManager.CommerceEffects.EastTradingPost, expectedResult);

            // In this test, we are looking for 4 resources, but for two of them, the only source is in my own
            // resource stack.  The wood has to be purchased from the non-preferred neighbor while the papyrus
            // is purchased for a discounted amount from the preferred neighbor
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("PWOO"), new List<ResourceEffect> { wood_ore, caravansery },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.Marketplace | ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            // Test for special flags.
            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.None, expectedResult);

            // 1-resource discount

            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("S"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.None, expectedResult);

            Verify2(new Cost("SSS"), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.None, expectedResult);

            Verify2(new Cost("SSSS"), new List<ResourceEffect> { stone_1, stone_2 }, new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.None, expectedResult);

            // same test as above, but now we have a one-resource discount due to a leader effect, so we
            // no longer have to buy the wood from our left neighbor
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("PWOO"), new List<ResourceEffect> { wood_ore, caravansery },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.Marketplace | ResourceManager.CommerceEffects.EastTradingPost,
                expectedResult);

            // Test double-resources.  Only one of the double-resources is needed from the left neighbor due to the discount.
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("SS"), new List<ResourceEffect>(), new List<ResourceEffect> { stone_2 }, new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.None, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 6;
            expectedResult.rightCoins = 4;
            Verify2(new Cost("WWWSPC"), new List<ResourceEffect> { }, new List<ResourceEffect> { cloth, wood_2, }, new List<ResourceEffect> { papyrus, wood_1, },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor | ResourceManager.CommercePreferences.OneResourceDiscount,
                ResourceManager.CommerceEffects.None, expectedResult);

            // Bilkis
            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.bankCoins = 1;
            expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("G"), new List<ResourceEffect>(), new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.Bilkis, expectedResult);

            expectedResult.bankCoins = 1;
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("PWOO"), new List<ResourceEffect> { wood_ore, caravansery },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.Marketplace | ResourceManager.CommerceEffects.EastTradingPost | ResourceManager.CommerceEffects.Bilkis,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.bankCoins = 1;
            expectedResult.leftCoins = 6;
            expectedResult.rightCoins = 4;
            Verify2(new Cost("WWWSPC"), new List<ResourceEffect> { }, new List<ResourceEffect> { cloth, wood_2, }, new List<ResourceEffect> { papyrus, wood_1, },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.Bilkis, expectedResult);

            // Secret Warehouse tests

            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("GG"), new List<ResourceEffect> { glass, }, new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.SecretWarehouse, expectedResult);

            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("OOO"), new List<ResourceEffect> { ore_2, }, new List<ResourceEffect>(), new List<ResourceEffect>(),
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.SecretWarehouse, expectedResult);

            // Secret Warehouse only applies to our city resources
            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.bankCoins = expectedResult.leftCoins = expectedResult.rightCoins = 0;
            Verify2(new Cost("OOO"), new List<ResourceEffect> { }, new List<ResourceEffect> { ore_1, }, new List<ResourceEffect> { ore_1 },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor, ResourceManager.CommerceEffects.SecretWarehouse, expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("PWOO"), new List<ResourceEffect> { wood_ore, caravansery },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.Marketplace | ResourceManager.CommerceEffects.SecretWarehouse,
                expectedResult);

            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("WWOO"), new List<ResourceEffect> { wood_ore },
                new List<ResourceEffect> { papyrus, ore_1, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, ore_1, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.SecretWarehouse,
                expectedResult);

            // Check that the algorithm realizes that by doubling the 2nd of an either/or, the
            // resource cost can be fulfilled
            expectedResult.leftCoins = 4;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("WWOO"), new List<ResourceEffect> { wood_ore },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2 },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.SecretWarehouse,
                expectedResult);

            // Make sure the Secret Warehouse is used only one time.
            expectedResult.leftCoins = 3;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("OOOOPG"), new List<ResourceEffect> { ore_2 },
                new List<ResourceEffect> { papyrus, stone_2, wood_2, clay_2, stone_ore, },
                new List<ResourceEffect> { papyrus, glass, stone_2, clay_2, clay_ore },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.Marketplace | ResourceManager.CommerceEffects.SecretWarehouse,
                expectedResult);

            // Check that Forums and Caravansery resources are not doubled with the Secret Warehouse.
            expectedResult.bAreResourceRequirementsMet = false;
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("CC"), new List<ResourceEffect> { forum, },
                new List<ResourceEffect> { },
                new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.SecretWarehouse,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("C"), new List<ResourceEffect> { },
                new List<ResourceEffect> { cloth, papyrus, },
                new List<ResourceEffect> { cloth, },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.ClandestineDockWest,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 3;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("CP"), new List<ResourceEffect> { },
                new List<ResourceEffect> { cloth, papyrus, },
                new List<ResourceEffect> { cloth, },
                ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                ResourceManager.CommerceEffects.ClandestineDockWest,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 1;
            expectedResult.rightCoins = 2;
            Verify2(new Cost("CP"), new List<ResourceEffect> { },
                new List<ResourceEffect> { cloth, papyrus, },
                new List<ResourceEffect> { cloth, },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.ClandestineDockWest,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 2;
            expectedResult.rightCoins = 1;
            Verify2(new Cost("CP"), new List<ResourceEffect> { },
                new List<ResourceEffect> { cloth, papyrus, },
                new List<ResourceEffect> { cloth, },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.ClandestineDockEast,
                expectedResult);

            // Black Market tests
            expectedResult.bAreResourceRequirementsMet = true;
            expectedResult.leftCoins = 0;
            expectedResult.rightCoins = 0;
            Verify2(new Cost("C"), new List<ResourceEffect> { },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.BlackMarket1,
                expectedResult);

            Verify2(new Cost("WC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.BlackMarket1,
                expectedResult);

            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("WWC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.BlackMarket1,
                expectedResult);

            // Verify the wood in our normal resource stack is removed from the Black Market(s).
            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("WWC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.BlackMarket1 | ResourceManager.CommerceEffects.BlackMarket2,
                expectedResult);

            // But when we build a different structure, with both Black Markets, it's successful.
            expectedResult.bAreResourceRequirementsMet = true;
            Verify2(new Cost("WPC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.BlackMarket1 | ResourceManager.CommerceEffects.BlackMarket2,
                expectedResult);

            // Secret Warehouse doubles the single wood...
            Verify2(new Cost("WWC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.SecretWarehouse | ResourceManager.CommerceEffects.BlackMarket1,
                expectedResult);

            // ... but not the Black Market
            expectedResult.bAreResourceRequirementsMet = false;
            Verify2(new Cost("WCC"), new List<ResourceEffect> { wood_1 },
                new List<ResourceEffect> { }, new List<ResourceEffect> { },
                ResourceManager.CommercePreferences.BuyFromRightNeighbor,
                ResourceManager.CommerceEffects.SecretWarehouse | ResourceManager.CommerceEffects.BlackMarket1,
                expectedResult);

            // After we have a list of options for building a card, we can apply commercial effects,
            // then resolve the options into:
            // * minimal cost
            // * prefer to pay left neighbor (may be minimal cost, mat be higher than minimal)
            // * prefer to pay right neighbor (may be minimal cost, may be higher than minimal)
            // 

            // What's the best way to handle wild-card resources?  Adding a check at the end works
            // except for buying doubled resources from neighbors :(
            // I could also put a new resource source on the list (before Bilkis)

            // Secret Warehouse.  How am I going to implement this?  The way the implementation has
            // been done so far, is for the search algorithm to return the first resource stack that
            // is able to build the card that meets a certain search criteria (i.e. prefer left or
            // right neighbors).  It may be that when I try to do minimal cost this whole scheme
            // completely breaks down and I have to go back to doing an (cost string length)^(resource combinations)
            // search, then sort the options by cost.  The problem is knowing which resource to double.
            // For example, suppose you're building PWWOO, and you have W/O.  Both neighbors have wood
            // but not ore.  You have a trading post or dock pointed at one.  It's better to use the
            // Secret Warehouse to build the Ore and buy the wood rather than the other way around.
            // Hmm, it may be that I'll have to note when I use an flex card and go back and do it
            // the other way.  I may have to do the same thing for the Black Market anyway, actually.
            // using one of its resources may be more efficient than using another.

            // I have to figure out how to best use wild resources (including Bilkis),
            // Secret Warehouse, and Black Market.  It'll get complicated when I am
            // trying for minimal cost and there are multiple paths for success.
            // If we use these special resources first, a successful path may be found,
            // but it may not be the cheapest possible successful path.  I think the
            // correct algorithm will be to use my city's non-choice resources, then
            // those of my neighbors, then my own city's choice resources.  If I have
            // any used choice resources after a match is found, we go back up the
            // stack, and starting with the least desirable resources purchased from
            // neighbors, see if they can be replaced with resources that have not
            // be used yet in my city.

            // Nov. 15, 2016 I had a good one today.  Babylon B, Glassworks, Caravansery,
            // trading posts in each direction, Marketplace, Clandestine Dock West.
            // West neighbor: China A, Press, Glassworks, Forest Cave, Brickyard.
            // East neighbor: Olympia B, Timber Yard, single stone, double ore.

            Console.WriteLine("Resource Manager tests completed.  All unit tests passed.");
        }
    }
}
