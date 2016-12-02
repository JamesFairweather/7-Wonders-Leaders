using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public class ResourceManager
    {
        // Maintain the resource list in an order that minimizes the search paths.
        // Simplest types at the top so those cards are used up first/ when calculating whether a structure is affordable.
        // Those are RawMaterials which do not offer a choice, Goods and single-choice Resources come first.  Next come
        // double RawMaterial cards.  They don't offer a choice but are better than single Raw materials.  Next would come
        // first-age resource cards that have a choice of two.  There's a beter chance that by the time those cards are
        // considered, one of the needed resources has already been taken care of by a single-resource card.  Last are the
        // cards that offer a choice of all 3 goods or all 4 raw materials (Forum/Caravansery/Alexandria stages as they are
        // the most flexible).  Leader effects such as Bilkis, Imhotep, Archimedes are not added to this chain but are dealt
        // with by the reduction algorithm.  Same with the Secret Warehouse, Black Market, and Clandestine Docks.
        List<ResourceEffect> resources = new List<ResourceEffect>();

        public void add(ResourceEffect s)
        {
            // resource effect list is ordered.  First are resources which provide a single resource.
            // Then double resource structures, then either/or with a choice of two, and lastly one of
            // 3, 4 or 7.
            int nResources = s.resourceTypes.Length;

            int insertionIndex = -1;

            if (resources.Count != 0)
            {
                if (nResources == 2)
                {
                    // if the card has 2 resources, check whether it's an either/or or a double.
                    // put double resources ahead of either/or ones.
                    if (s.resourceTypes[0] == s.resourceTypes[1])
                    {
                        insertionIndex = resources.FindLastIndex(x => x.IsDoubleResource());
                    }
                    else
                    {
                        insertionIndex = resources.FindLastIndex(x => x.resourceTypes.Length == nResources);
                    }
                }
                else
                {
                    // resource has 1, 3, 4, or 7 possibilities.
                    insertionIndex = resources.FindLastIndex(x => x.resourceTypes.Length == nResources);
                }

                while (insertionIndex == -1)
                {
                    if (nResources == 0)
                    {
                        // This resource card has fewer resources than any other card already in the resource
                        // list, so it goes at the top.
                        break;
                    }
                    // no entries found with this number of resources.  Add the card after the last level 
                    // where there are some found.

                    --nResources;

                    insertionIndex = resources.FindLastIndex(x => x.resourceTypes.Length == nResources);
                }
            }

            resources.Insert(insertionIndex+1, s);
        }

        [Flags]
        public enum CommerceEffects
        {
            None = 0,                   // No special commerce effects: All resources purchased from neighbors cost 2 coins.
            Marketplace = 1,            // Grey resources (Papyrus, Cloth, Glass), cost 1 when purchased from either neighbor
            WestTradingPost = 2,        // Raw Materials (Wood, Stone, Brick, Ore), cost 1 when purchased from the left neighbor
            EastTradingPost = 4,        // Raw Materials (Wood, Stone, Brick, Ore), cost 1 when purchased from the right neighbor
            Bilkis = 8,                 // Can buy any resource, once per turn, by paying 1 coin to the bank
            ClandestineDockWest = 16,   // First resource purchased from the left neighbor is discounted by 1 coin (cumulative with Marketplace/Trading Post)
            ClandestineDockEast = 32,   // First resource purchased from the right neighbor is discounted by 1 coin (cumulative with Marketplace/Trading Post)
            SecretWarehouse = 64,       // Secret Warehouse is active
            BlackMarket1 = 128,         // Black Market card OR China B's wonder stage
            BlackMarket2 = 256,         // both Black Market card AND China B's wonder stage 
        }

        CommerceEffects marketEffects = CommerceEffects.None;

        public void SetCommerceEffect(CommerceEffects me)
        {
            this.marketEffects = me;
        }

        public void AddCommerceEffect(CommerceEffects me)
        {
            SetCommerceEffect(this.marketEffects | me);
        }

        public List<ResourceEffect> getResourceList(bool isSelf)
        {
            // remove resources that cannot be used by neighbors.
            return isSelf ? resources : resources.FindAll(x => x.canBeUsedByNeighbors == true);
        }

        enum ResourceOwner
        {
            Self,
            Left,
            Right,
        };

        class ResourceUsed
        {
            public ResourceUsed(ResourceOwner o, int cost)
            {
                this.owner = o;
                this.cost = cost;
                this.usedDoubleResource = false;
            }

            public ResourceOwner owner { get; private set; }    // who owns this resource

            public int index { get; private set; }              // index into the owner's resource list

            public int cost { get; private set; }               // how much does this resource cost (0/1/2)

            /// <summary>
            /// If both resources from a double are used, it is noted here.
            /// </summary>
            public bool usedDoubleResource { get; set; }
        }

        enum SpecialTrait
        {
            Unavailable = 0,
            Unused = 1,
            Used = 2,
        };

        class ResourceCost
        {
            public ResourceCost()
            {
                bank = 0;
                left = 0;
                right = 0;
            }

            public int bank;
            public int left;
            public int right;
        };

        struct ReduceState
        {
            // These are static fields, they are set when the recursion begins.

            // Effects can only be added to this field.
            public CommerceEffects marketEffects;

            // preferences can change from one run to the next.
            public CommercePreferences pref;

            public List<ResourceEffect> myResources;
            public List<ResourceEffect> leftResources;
            public List<ResourceEffect> rightResources;

            // One of these fields is updated with each level of recursion
            public int myResourceIndex;
            public int leftResourceIndex;
            public int rightResourceIndex;

            public int leftResourcesAvailable;
            public int rightResourcesAvailable;

            public Stack<ResourceUsed> currentResourceStack;        // current resource stack
            public List<ResourceUsed> lowestCostResourceStack;      // output of a successful trace

            public SpecialTrait wildResource;                       // Imhotep, Archimedes
            public SpecialTrait bilkis;
            public SpecialTrait secretWarehouse;

            public int nBlackMarketIndex;
            public int nBlackMarketAvailable;

            public ResourceEffect wildResourceEffect;
            public ResourceEffect blackMarketResource;
        };

        /// <summary>
        /// The recursive reduction function.  It searches every possible resource path until a good one is found,
        /// or if doing LowestCost, it searches every possible resource path and returns the cheapest one.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="lowCost"></param>
        /// <param name="remainingCost"></param>
        static void ReduceRecursively(ReduceState state, ref ResourceCost lowCost, string remainingCost)
        {
            if (remainingCost == string.Empty)
            {
                // success!  This combination of resources reduced the cost to zero.

                ResourceCost rc = new ResourceCost();

                foreach (ResourceUsed ru in state.currentResourceStack)
                {
                    if (ru.owner == ResourceOwner.Left)
                    {
                        rc.left += ru.cost * (ru.usedDoubleResource ? 2 : 1);
                    }
                    else if (ru.owner == ResourceOwner.Right)
                    {
                        rc.right += ru.cost * (ru.usedDoubleResource ? 2 : 1);
                    }
                    else
                    {
                        rc.bank += ru.cost;
                    }
                }

                if (state.marketEffects.HasFlag(CommerceEffects.ClandestineDockWest) && rc.left != 0) --rc.left;
                if (state.marketEffects.HasFlag(CommerceEffects.ClandestineDockEast) && rc.right != 0) --rc.right;

                bool replaceResourceStack = false;

                if (state.lowestCostResourceStack.Count == 0)
                {
                    replaceResourceStack = true;
                }
                else
                {
                    // When I started doing LowestCost, my initial implementation of this was to compare each
                    // resource used in this stack to those in the existing good stack, and replace any resources
                    // in the existing stack with those in the new stack, if the new stack had a lower cost.  However,
                    // this broke when there were choices (e.g. a forum), and this resource was being used twice.
                    // e.g. this test case was failing:
                    // expectedResult.leftCoins = 2;
                    // expectedResult.rightCoins = 1;
                    // Verify2(new Cost("WSBPG"), new List<ResourceEffect> { stone_1 /* putting a wood here works, stone does not */, caravansery, forum },
                    //     new List<ResourceEffect> { papyrus, glass, }, new List<ResourceEffect> { wood_clay },
                    //     ResourceManager.CommercePreferences.LowestCost | ResourceManager.CommercePreferences.BuyFromLeftNeighbor,
                    //     ResourceManager.CommerceEffects.EastTradingPost,
                    //    expectedResult);
                    //
                    // An analysis indicated what was happening was the Forum was being used twice to fulfill the Papyrus
                    // resource, once from the first match (when glass was purchased), then again from a later match,
                    // (when Papyrus was purchased)

                    if ((rc.left + rc.right + rc.bank) < (lowCost.left + lowCost.right + lowCost.bank))
                    {
                        // This is a cheaper overall stack than the existing one, so use it
                        replaceResourceStack = true;
                    }
                    else if ((rc.left + rc.right) == (lowCost.left + lowCost.right))
                    {
                        // This branch indicates the amount paid to each neighbor is the same in this stack compared with the
                        // existing good stack, so we do a secondary comparison for the costs to each neighbor and pay the
                        // preferred one if the cost to the non-preferred one is lower.  Note, that we intentionally do not
                        // the bank cost in this secondary comparison as we would prefer to pay for Bilkis' resource than pay
                        // a neighbor for theirs.
                        replaceResourceStack =
                            (state.pref.HasFlag(CommercePreferences.BuyFromLeftNeighbor) && lowCost.left < rc.left) ||
                            (state.pref.HasFlag(CommercePreferences.BuyFromRightNeighbor) && lowCost.right < rc.right);
                    }
                }

                if (replaceResourceStack)
                {
                    // replace the existing used resource list with this cheaper one.
                    state.lowestCostResourceStack.Clear();

                    foreach (ResourceUsed r in state.currentResourceStack)
                    {
                        state.lowestCostResourceStack.Add(r);
                    }

                    lowCost = rc;
                }

               // do not continue down this search path once we have a completed path.  The algorithm
               // searches every possible combination and if there's a cheaper good path, it will be found
               return;
            }

            // we need to check every possible path, starting at the current recursion level, going from cheapest
            // possible resource to the most expensive.  This should return the cheapest possible coin cost for
            // the structure under consideration.
            while (state.myResourceIndex < state.myResources.Count ||
                state.leftResourceIndex < state.leftResourcesAvailable ||
                state.rightResourceIndex < state.rightResourcesAvailable ||
                state.nBlackMarketIndex < state.nBlackMarketAvailable ||
                state.wildResource == SpecialTrait.Unused ||
                state.bilkis == SpecialTrait.Unused)
            {
                ResourceEffect res = null;

                if (state.lowestCostResourceStack.Count != 0 && !state.pref.HasFlag(CommercePreferences.LowestCost))
                    break;

                int myInc= 0;
                int leftInc = 0;
                int rightInc = 0;
                bool usedWildResource = false;
                bool usedBilkis = false;
                ResourceUsed resUsed;

                if (state.myResourceIndex < state.myResources.Count)
                {
                    // my city's resources are free, use them up first.
                    res = state.myResources[state.myResourceIndex];
                    myInc = 1;
                    resUsed = new ResourceUsed(ResourceOwner.Self, 0);
                }
                else if (state.nBlackMarketIndex < state.nBlackMarketAvailable)
                {
                    res = state.blackMarketResource;
                    resUsed = new ResourceUsed(ResourceOwner.Self, 0);
                }
                else if (state.wildResource == SpecialTrait.Unused)
                {
                    // Archimedes, Imhotep, Leonidas and Hammurabi.  These resources have no cost
                    usedWildResource = true;
                    res = state.wildResourceEffect;
                    resUsed = new ResourceUsed(ResourceOwner.Self, 0);
                }
                else if (state.bilkis == SpecialTrait.Unused)
                {
                    // Bilkis costs one coin but that's better than paying a neighbor (unless they have a Clandestine Dock)
                    usedBilkis = true;
                    res = state.wildResourceEffect;
                    resUsed = new ResourceUsed(ResourceOwner.Self, 1);
                }
                else
                {
                    // try to find the remaining resources via commerce.  By default, we prefer to purchase
                    // from our left neighbor, but this is controllable.

                    bool buyingFromLeft = true;

                    if (state.pref.HasFlag(CommercePreferences.BuyFromLeftNeighbor))
                    {
                        if (state.leftResourceIndex < state.leftResourcesAvailable)
                        {
                            buyingFromLeft = true;
                        }
                        else if (state.rightResourceIndex < state.rightResourcesAvailable)
                        {
                            buyingFromLeft = false;
                        }
                    }
                    else if (state.pref.HasFlag(CommercePreferences.BuyFromRightNeighbor))
                    {
                        if (state.rightResourceIndex < state.rightResourcesAvailable)
                        {
                            buyingFromLeft = false;
                        }
                        else if (state.leftResourceIndex < state.leftResourcesAvailable)
                        {
                            buyingFromLeft = true;
                        }
                    }
                    else
                    {
                        // not handling this situation yet.
                        throw new NotImplementedException();
                    }

                    ResourceOwner ro;
                    int resourceIndex;

                    // search using the left neighbor's resources before the right one.
                    if (buyingFromLeft)
                    {
                        resourceIndex = state.leftResourceIndex;
                        res = state.leftResources[resourceIndex];
                        leftInc = 1;
                        ro = ResourceOwner.Left;
                    }
                    else
                    {
                        resourceIndex = state.rightResourceIndex;
                        res = state.rightResources[resourceIndex];
                        rightInc = 1;
                        ro = ResourceOwner.Right;
                    }

                    if (res == null)
                    {
                        // logic error
                        throw new NotImplementedException();
                    }

                    int resCost = 2;

                    if (res.IsManufacturedGood())
                    {
                        if (state.marketEffects.HasFlag(CommerceEffects.Marketplace))
                        {
                            resCost--;
                        }
                    }
                    else
                    {
                        if (buyingFromLeft && state.marketEffects.HasFlag(CommerceEffects.WestTradingPost))
                        {
                            resCost--;
                        }
                        else if (!buyingFromLeft && state.marketEffects.HasFlag(CommerceEffects.EastTradingPost))
                        {
                            resCost--;
                        }
                    }

                    resUsed = new ResourceUsed(ro, resCost);
                }

                state.currentResourceStack.Push(resUsed);

                for (int resIndex = 0; resIndex < res.resourceTypes.Length; resIndex++)
                {
                    if (state.lowestCostResourceStack.Count != 0 && !state.pref.HasFlag(CommercePreferences.LowestCost))
                        break;

                    char resType = res.resourceTypes[resIndex];

                    // incomplete set of resources to buy this structure so far.
                    int ind = remainingCost.IndexOf(resType);

                    if (ind != -1)
                    {
                        int nResourceCostsToRemove = res.IsDoubleResource() && ((remainingCost.Length - ind) > 1) && (remainingCost[ind] == remainingCost[ind + 1]) ? 2 : 1;

                        if (nResourceCostsToRemove == 2)
                        {
                            resIndex++;

                            // note that both resources provided by this
                            // double-resource card were used (for cost-calculating purposes).
                            // don't need to worry about this for my city as there's no cost to use them.
                            state.currentResourceStack.Peek().usedDoubleResource = true;
                        }

                        // Secret Warehouse.  Must be considered _after_ double-type resources.  Only applies to our city's resources
                        // and only if they are a single, double, or either/or.  No Forum/Caravansery.
                        if (state.secretWarehouse == SpecialTrait.Unused && myInc == 1 && res.resourceTypes.Length <= 2 && res != state.blackMarketResource)
                        {
                            if (((remainingCost.Length - ind) > nResourceCostsToRemove) && (remainingCost[ind] == remainingCost[ind + nResourceCostsToRemove]))
                            {
                                // turn a single into a double or a double into a triple
                                ++nResourceCostsToRemove;
                                state.secretWarehouse = SpecialTrait.Used;
                            }
                        }

                        state.myResourceIndex += myInc;
                        state.leftResourceIndex += leftInc;
                        state.rightResourceIndex += rightInc;

                        if (usedWildResource)
                            state.wildResource = SpecialTrait.Used;

                        if (usedBilkis)
                            state.bilkis = SpecialTrait.Used;

                        if (res == state.blackMarketResource)
                            state.nBlackMarketIndex++;

                        ReduceRecursively(state, ref lowCost, remainingCost.Remove(ind, nResourceCostsToRemove));

                        state.myResourceIndex -= myInc;
                        state.leftResourceIndex -= leftInc;
                        state.rightResourceIndex -= rightInc;

                        if (usedWildResource)
                            state.wildResource = SpecialTrait.Unused;

                        if (usedBilkis)
                            state.bilkis = SpecialTrait.Unused;

                        if (state.secretWarehouse == SpecialTrait.Used)
                            state.secretWarehouse = SpecialTrait.Unused;

                        if (res == state.blackMarketResource)
                            state.nBlackMarketIndex--;
                    }
                }

                // pop the last ResourceEffect off, then move on to the next resource choice for this structure
                state.currentResourceStack.Pop();

                // increment the resource counter (only one of these is ever set to 1, the other two will be 0.
                state.myResourceIndex += myInc;
                state.leftResourceIndex += leftInc;
                state.rightResourceIndex += rightInc;

                if (usedWildResource)
                    state.wildResource = SpecialTrait.Used;

                if (usedBilkis)
                    state.bilkis = SpecialTrait.Used;

                if (res == state.blackMarketResource)
                    state.nBlackMarketIndex++;
            }
        }

        // These flags can change with each call to GetCommerceOptions.
        [Flags]
        public enum CommercePreferences
        {
            /// <summary>
            /// buy from the neighbor which will keep the cost to minimum (i.e. consider the effects of trading posts and Clandestine Docks)
            /// </summary>
            LowestCost = 1,
            /// <summary>
            /// prefer to buy from the left neighbor (because they are losing or maybe because they have a Trading Post or
            /// Marketplace pointed at your city and are therefore more likely to buy from you when they have a chance).
            /// </summary>
            BuyFromLeftNeighbor = 2,
            /// <summary>
            /// prefer to buy from the right neighbor (because they are losing or maybe because they have a Trading Post or
            /// Marketplace pointed at your city and are therefore more likely to buy from you when they have a chance).
            /// </summary>
            BuyFromRightNeighbor = 4,
            /// <summary>
            /// This structure costs 1 fewer grey or brown resources.  (Imhotep, Archimedes, Leonidas, Hammurabi)
            /// </summary>
            OneResourceDiscount = 8,
            /// <summary>
            /// Try to buy a resource from each neighbor (to get extra coins from Hatshepsut) (not implemented yet)
            /// </summary>
            BuyOneFromEachNeighbor = 16,
        };

        /// <summary>
        /// The entry point to the reduction algorithm
        /// </summary>
        /// <param name="cost">The cost in coins and resources of the structure</param>
        /// <param name="leftResources">Resources available for my city to purchase (Browns/Greys only)</param>
        /// <param name="rightResources">Resources available for my city to purchase (Browns/Greys only)</param>
        /// <param name="pref">Flags indicating how the search should be performed (Buy from Left or Right), find LowestCost, etc.</param>
        /// <returns></returns>
        public CommerceOptions CanAfford(Cost cost, List<ResourceEffect> leftResources, List<ResourceEffect> rightResources,
            CommercePreferences pref = CommercePreferences.LowestCost | CommercePreferences.BuyFromLeftNeighbor)
        {
            CommerceOptions commOptions = new CommerceOptions();
            ReduceState rs = new ReduceState();

            rs.myResources = resources;
            rs.leftResources = leftResources;
            rs.rightResources = rightResources;
            rs.leftResourcesAvailable = leftResources.Count();
            rs.rightResourcesAvailable = rightResources.Count();
            rs.currentResourceStack = new Stack<ResourceUsed>();
            rs.marketEffects = this.marketEffects;
            rs.pref = pref;
            rs.lowestCostResourceStack = new List<ResourceUsed>();
            ResourceCost lowCost = new ResourceCost();

            rs.wildResource = pref.HasFlag(CommercePreferences.OneResourceDiscount) ? SpecialTrait.Unused : SpecialTrait.Unavailable;
            rs.bilkis = marketEffects.HasFlag(CommerceEffects.Bilkis) ? SpecialTrait.Unused : SpecialTrait.Unavailable;

            if (rs.wildResource == SpecialTrait.Unused || rs.bilkis == SpecialTrait.Unused)
                rs.wildResourceEffect = new ResourceEffect(false, "WSBOCGP");

            rs.secretWarehouse = marketEffects.HasFlag(CommerceEffects.SecretWarehouse) ? SpecialTrait.Unused : SpecialTrait.Unavailable;
            rs.nBlackMarketIndex = 0;
            rs.nBlackMarketAvailable = 0;

            if (marketEffects.HasFlag(CommerceEffects.BlackMarket1))
                ++rs.nBlackMarketAvailable;

            if (marketEffects.HasFlag(CommerceEffects.BlackMarket2))
                ++rs.nBlackMarketAvailable;

            if (rs.nBlackMarketAvailable > 0)
            {
                string strBlackMarket = "WSBOCGP";

                foreach (ResourceEffect re in resources)
                {
                    // The Black Market only excludes resources produced by this
                    // city's brown or grey structures, which only have 1 or 2 resource types
                    // So this excludes the Caravansery, Forum, and any Wonder stages.
                    if (re.resourceTypes.Length <= 2)
                    {
                        foreach (char c in re.resourceTypes)
                        {
                            int index = strBlackMarket.IndexOf(c);

                            if (index >= 0)
                                strBlackMarket = strBlackMarket.Remove(index, 1);
                        }
                    }
                }

                rs.blackMarketResource = new ResourceEffect(false, strBlackMarket);
            }

            if (cost.resources != string.Empty)
            {
                // kick off a recursive reduction of the resource cost.  Paths that completely eliminate the cost
                // are returned in the requiredResourcesLists.
                ReduceRecursively(rs, ref lowCost, cost.resources);

                commOptions.bAreResourceRequirementsMet = rs.lowestCostResourceStack.Count != 0;
            }
            else
            {
                commOptions.bAreResourceRequirementsMet = true;
            }

            if (commOptions.bAreResourceRequirementsMet)
            {
                commOptions.bankCoins = lowCost.bank + cost.coin;
                commOptions.leftCoins = lowCost.left;
                commOptions.rightCoins = lowCost.right;
            }

            return commOptions;
        }
    }
}
