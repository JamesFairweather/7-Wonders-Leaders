using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public class ResourceManager
    {
        // this list needs to be sorted in a particular order.  Simplest types at the top so those cards are used up first
        // when calculating whether a structure is affordable.  Those are RawMaterials which do not offer a choice
        // Goods and single-choice Resources come first.  Next come double RawMaterial cards.  They don't offer a
        // choice but are better than single Raw materials.  Next would come first-age resource cards that have a choice of
        // two.  There's a beter chance that by the time those cards are considered, one of the needed resources has
        // already been taken care of by a single-resource card.  Last are the cards that offer a choice of all 3 goods
        // or all 4 raw materials (Forum/Caravansery/Alexandria stages as they are the most flexible).  After those are
        // considered, we look at Bilkis and other leaders who provide a -1 discount on certain structure classes.
        // Also the Secret Warehouse and Black Market are in there somewhere.
        List<ResourceEffect> resources = new List<ResourceEffect>();

        //Add an OR resource
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
            None = 0,
            Marketplace = 1,
            WestTradingPost = 2,
            EastTradingPost = 4,
            Bilkis = 8,
            ClandestineDockWest = 16,
            ClandestineDockEast = 32,
            SecretWarehouse = 64,
            // BlackMarket1 = 128,     // Black Market card or China B's wonder stage
            // BlackMarket2 = 256,     // Black Market card and China B's wonder stage 
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

        public IEnumerable<ResourceEffect> getResourceList(bool isSelf)
        {
            if (isSelf)
            {
                return resources;
            }
            else
            {
                // remove resources that cannot be used by neighbors.
                return resources.Where(x => x.canBeUsedByNeighbors == true);
            }
        }

       /**
         * Remove all letters that appear in B FROM A, then return the newly trimmed A
         * The interpretation of this, with respect to this program, is that given a Cost A, and available resources B
         * the return value represents unpaid Costs after using the B resources
         * For example, if the return value is "", then we know that with A was affordable with resources B
         * If the return value is "W", then we know that a Wood still must be paid.
         * @param A = COST
         * @param B = RESOURCES
         * 
         * Note, there's a major bug with this: for the flex resource structures, only the first resource
         * is considered, which means that some structures that are affordable using the 2nd option are
         * returned as Not Buildable.
         */

        public Cost eliminate(Cost structureCost, string resourceString)
        {
            // interesting.  structs do not need to be instantiated.  Classes do.  But structs
            // can only be PoD types, they cannot contain functions.

            Cost c = structureCost;

            foreach (char ch in resourceString)
            {
                switch (ch)
                {
                    case 'W':
                        if (c.wood != 0)
                        {
                            --c.wood;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'S':
                        if (c.stone != 0)
                        {
                            --c.stone;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'B':
                        if (c.clay != 0)
                        {
                            --c.clay;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'O':
                        if (c.ore != 0)
                        {
                            --c.ore;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'C':
                        if (c.cloth != 0)
                        {
                            --c.cloth;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'G':
                        if (c.glass != 0)
                        {
                            --c.glass;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'P':
                        if (c.papyrus != 0)
                        {
                            --c.papyrus;
                            // if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    default:
                        throw new Exception();
                }
            }

            return c;
        }

        /**
         * Given a resource DAG graph, determine if a cost is affordable
         * @return
         */
        public bool canAfford(Cost cost, int nWildResources)
        {
            List<ResourceEffect> leftResourcesRequired = new List<ResourceEffect>();
            List<ResourceEffect> rightResourcesRequired = new List<ResourceEffect>();

            CommerceOptions co = GetCommerceOptions(cost, leftResourcesRequired, rightResourcesRequired, CommercePreferences.BuyFromLeftNeighbor);

            if (nWildResources != 0)
            {
                // TODO: make the Resource Manager aware of this special resource.
                throw new NotImplementedException();
            }

            if (co.bAreResourceRequirementsMet)
            {
                return leftResourcesRequired.Count == 0 && rightResourcesRequired.Count == 0;
            }
            /*
            foreach (ResourceEffect e in resources)
            {
                if (eliminate(cost, true, e.resourceTypes).IsZero())
                    return true;

                if (e.IsDoubleResource())
                {
                    // this is a double-resource card (i.e. Sawmill/Quarry/Brickyard/Foundry).
                    // See if there's another cost entry that can be eliminated with the 2nd resource.
                    // All other ResourceEffect cards can only be used once.
                    if (eliminate(cost, true, e.resourceTypes).IsZero())
                        return true;
                }
            }
            */

            // TODO: implement this
            // If the number of wild resources (i.e. Bilkis/Archimedes/Leonidas/Imhotep/Hammurabi)
            // is greater than or equal to the remaining cost after all other resource options have
            // been spent, the structure is afforable.  I'll remove Bilkis from the list of wilds
            // if the player doesn't have a coin.
            // if (nWildResources >= (leftResourcesRequired.Count + rightResourcesRequired.Count))
            //    return true;

            return false;
        }

        /*
        /// <summary>
        /// Combine the player's resource list with those of his neighboring cities into a single ResourceList, to see whether a card
        /// could be afforded using commerce.
        /// </summary>
        /// <param name="A">Left DAG</param>
        /// <param name="B">Centre DAG</param>
        /// <param name="C">Right DAG</param>
        /// <returns>A Mega DAG that consists of A, B, C combined</returns>
        public static ResourceManager addThreeDAGs(ResourceManager A, ResourceManager B, ResourceManager C)
        {
            ResourceManager returnedList = new ResourceManager();

            IEnumerable<ResourceEffect> rA = A.getResourceList(false);
            IEnumerable<ResourceEffect> rB = B.getResourceList(true);
            IEnumerable<ResourceEffect> rC = C.getResourceList(false);

            foreach (ResourceEffect e in rA.Where(x => x.resourceTypes.Length == 1))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rB.Where(x => x.resourceTypes.Length == 1))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rC.Where(x => x.resourceTypes.Length == 1))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rA.Where(x => (x.IsDoubleResource())))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rB.Where(x => (x.IsDoubleResource())))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rC.Where(x => (x.IsDoubleResource())))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rA.Where(x => (x.resourceTypes.Length == 2) && (x.resourceTypes[0] != x.resourceTypes[1])))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rB.Where(x => (x.resourceTypes.Length == 2) && (x.resourceTypes[0] != x.resourceTypes[1])))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rC.Where(x => (x.resourceTypes.Length == 2) && (x.resourceTypes[0] != x.resourceTypes[1])))
            {
                returnedList.add(e);
            }

            foreach (ResourceEffect e in rB.Where(x => x.resourceTypes.Length > 2))
            {
                returnedList.add(e);
            }

            return returnedList;
        }
        */

        enum ResourceOwner
        {
            Self,
            Left,
            Right,
        };

        struct ResourceUsed
        {
            public ResourceUsed(ResourceOwner o, int i)
            {
                this.owner = o;
                this.index = i;
                this.usedDoubleResource = false;
            }

            public ResourceOwner owner { get; private set; }   // who owns this resource

            public int index { get; private set; }             // index into the owner's resource list

            public bool usedDoubleResource;
        }

        struct ReduceState
        {
            // These are static fields, they are set when the recursion begins.

            // preferences can change from one run to the next.
            public CommercePreferences pref;
            public List<ResourceEffect> myResources;
            public List<ResourceEffect> leftResources;
            public List<ResourceEffect> rightResources;

            // Effects can only be added to this field.
            public CommerceEffects marketEffects;

            // One of these fields is updated with each level of recursion
            public Stack<ResourceUsed> usedResources;       // current resource stack
            public int myResourceIndex;
            public int leftResourceIndex;
            public int rightResourceIndex;

            // Contains the successful result.
            public List<ResourceUsed> outputResourceList;   // output of a successful trace

            public bool hasWildResource;             // Imhotep, Archimedes
            public bool usedWildResource;
            public bool hasBilkis;
            public bool usedBilkis;
            public bool hasSecretWarehouse;
            public bool usedSecretWarehouse;
        };

        const int BilkisResourceIndex = -1;

        static bool ReduceRecursively(ReduceState state, string remainingCost)
        {
            bool retVal = false;

            /*
             * This is an alternate way to implement the wild-card resource.  Unforunately it has a bug with
             * double-resources, which would need to be handled by changing the nResourceCostsToRemove, below.
             * Instead, the one resource discount is handled by adding a wild card resource for this structure
             * and then removing it afterwards.  I've left this code in here for now in case I change my mind
             * again and decide it's better to do it this way than the temporary resource.
            if (((pref & CommercePreferences.OneResourceDiscount) == CommercePreferences.OneResourceDiscount) &&
                (remainingCost.Length == 1))
            {
                // if the 1-resource discount is in effect (Imhotep, Archimedes, etc.), and there's only a single
                // resource left to match, we're done.
                remainingCost = string.Empty;
            }
            */

            if (remainingCost == string.Empty)
            {
                // success!  This combination of resources reduced the cost to zero.

                // copy the resource stack to the output
                foreach (ResourceUsed r in state.usedResources)
                {
                    state.outputResourceList.Add(r);
                }

                // end
                return true;
            }

            // we need to check every possible path, starting at the current recursion level, going from cheapest
            // possible resource to the most expensive.  This should return the cheapest possible coin cost for
            // the structure under consideration.
            while ((state.myResourceIndex < state.myResources.Count ||
                state.leftResourceIndex < state.leftResources.Count ||
                state.rightResourceIndex < state.rightResources.Count ||
                state.hasWildResource && !state.usedWildResource ||
                state.hasBilkis && !state.usedBilkis) &&
                (state.outputResourceList.Count == 0))
            {
                ResourceEffect res = null;

                int myInc= 0;
                int leftInc = 0;
                int rightInc = 0;

                bool usedSecretWarehouse = false;

                if (state.myResourceIndex < state.myResources.Count)
                {
                    // my city's resources are free, use them up first.
                    res = state.myResources[state.myResourceIndex];
                    myInc = 1;
                    state.usedResources.Push(new ResourceUsed(ResourceOwner.Self, state.myResourceIndex));
                }
                else if (state.hasWildResource && !state.usedWildResource)
                {
                    // Archimedes, Imhotep, Leonidas and Hammurabi.  Take the first resource off the list.
                    state.usedWildResource = true;
                    state.usedResources.Push(new ResourceUsed(ResourceOwner.Self, 0 /* for my city resources, the index doesn't matter (except Bilkis) */));
                    retVal |= ReduceRecursively(state, remainingCost.Remove(0, 1));
                    state.usedWildResource = false;
                    state.usedResources.Pop();

                    return retVal;
                }
                else if (state.hasBilkis && !state.usedBilkis)
                {
                    // Bilkis costs one coin but that's better than paying a neighbor
                    state.usedBilkis = true;
                    state.usedResources.Push(new ResourceUsed(ResourceOwner.Self, BilkisResourceIndex));
                    retVal |= ReduceRecursively(state, remainingCost.Remove(0, 1));
                    state.usedBilkis = false;
                    state.usedResources.Pop();

                    return retVal;
                }
                else
                {
                    // this block means the structure cannot be afforded using only our city's resources,
                    // so we will try using our neighbors' resources.
                    if ((state.pref & CommercePreferences.BuyFromLeftNeighbor) == CommercePreferences.BuyFromLeftNeighbor)
                    {
                        // search using the left neighbor's resources before the right one.
                        if (state.leftResourceIndex < state.leftResources.Count)
                        {
                            res = state.leftResources[state.leftResourceIndex];
                            leftInc = 1;
                            state.usedResources.Push(new ResourceUsed(ResourceOwner.Left, state.leftResourceIndex));
                        }
                        else if (state.rightResourceIndex < state.rightResources.Count)
                        {
                            res = state.rightResources[state.rightResourceIndex];
                            rightInc = 1;
                            state.usedResources.Push(new ResourceUsed(ResourceOwner.Right, state.rightResourceIndex));
                        }
                    }
                    else if ((state.pref & CommercePreferences.BuyFromRightNeighbor) == CommercePreferences.BuyFromRightNeighbor)
                    {
                        // search using the right neighbor's resources before the left one.
                        if (state.rightResourceIndex < state.rightResources.Count)
                        {
                            res = state.rightResources[state.rightResourceIndex];
                            rightInc = 1;
                            state.usedResources.Push(new ResourceUsed(ResourceOwner.Right, state.rightResourceIndex));
                        }
                        else if (state.leftResourceIndex < state.leftResources.Count)
                        {
                            res = state.leftResources[state.leftResourceIndex];
                            leftInc = 1;
                            state.usedResources.Push(new ResourceUsed(ResourceOwner.Left, state.leftResourceIndex));
                        }
                    }

                    if (res == null)
                    {
                        // logic error
                        throw new NotImplementedException();
                    }
                }

                for (int resIndex = 0; resIndex < res.resourceTypes.Length && (state.outputResourceList.Count == 0); resIndex++)
                {
                    char resType = res.resourceTypes[resIndex];
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
                            ResourceUsed ru = state.usedResources.Pop();
                            ru.usedDoubleResource = true;
                            state.usedResources.Push(ru);
                        }

                        // Secret Warehouse.  Must be considered _after_ double-type resources.  Only applies to our city's resources
                        // and only if they are a single, double, or either/or.  No Forum/Caravansery.  TODO: also check this doesn't
                        // apply to a Black Market.
                        if (state.hasSecretWarehouse && !state.usedSecretWarehouse && myInc == 1 && res.resourceTypes.Length <= 2)
                        {
                            if (((remainingCost.Length - ind) > nResourceCostsToRemove) && (remainingCost[ind] == remainingCost[ind + nResourceCostsToRemove]))
                            {
                                // turn a single into a double or a double into a triple
                                ++nResourceCostsToRemove;
                                state.usedSecretWarehouse = true;
                                usedSecretWarehouse = true;
                            }
                        }

                        // This resource matches one (or more) of the required resources.  Remove the matched
                        // resource from the remainingCost and move down a level of recusion.
                        state.myResourceIndex += myInc;
                        state.leftResourceIndex += leftInc;
                        state.rightResourceIndex += rightInc;

                        retVal |= ReduceRecursively(state, remainingCost.Remove(ind, nResourceCostsToRemove));

                        state.myResourceIndex -= myInc;
                        state.leftResourceIndex -= leftInc;
                        state.rightResourceIndex -= rightInc;

                        if (usedSecretWarehouse)
                            state.usedSecretWarehouse = false;
                    }

                    // if this resource isn't in the cost string, move on to the next option in the resource
                    // choices for this ResourceEffect.
                }

                // pop the last ResourceEffect off, then move on to the next resource choice for this structure
                state.usedResources.Pop();

                // increment the resource counter (only one of these is ever set to 1, the other two will be 0.
                state.myResourceIndex += myInc;
                state.leftResourceIndex += leftInc;
                state.rightResourceIndex += rightInc;
            }

            return retVal;
        }

        // I don't want to find every possible resource path.  There could be hundreds of valid combinations.
        // Instead, I want to have biases: prefer to buy from left, prefer to buy from right, prefer even,
        // prefer cheapest option, buy 1 from each before buying a 2nd one (i.e. to get coins from Hatshepsut).
        // This will guide the search path.  Of course, if the preferred option cannot be satisfied, we
        // will return the first valid path we encounter.
        [Flags]
        public enum CommercePreferences
        {
            BuyFromCheaperNeighbor = 0,     // buy from the neighbor which will keep the cost to minimum (i.e. consider the effects of trading posts and Clandestine Docks)
            BuyFromLeftNeighbor = 1,        // prefer to buy from the left neighbor (maybe they have a trading post pointed at you, or they're losing)
            BuyFromRightNeighbor = 2,       // prefer to buy from the right neighbor (maybe they have a trading post pointed at you, or they're losing)
            // BuyOneFromEachNeighbor = 4,     // after buying a resource from the preferred neighbor, buy one from the other neighbor (to get extra coins from Hatshepsut) (not implemented yet)
            OneResourceDiscount = 8,        // Imhotep, Archimedes, Leonidas, Hammurabi (not implemented yet)
        };

        // minimal cost (i.e. buy from neighbor you get discounts from
        // Hatshepsut - try to buy one from each neighbor.

        public CommerceOptions GetCommerceOptions(Cost cost, List<ResourceEffect> leftResources, List<ResourceEffect> rightResources, CommercePreferences pref = CommercePreferences.BuyFromCheaperNeighbor)
        {
            CommerceOptions commOptions = new CommerceOptions();
            ReduceState rs = new ReduceState();

            rs.myResources = resources;
            rs.leftResources = leftResources;
            rs.rightResources = rightResources;
            rs.usedResources = new Stack<ResourceUsed>();
            rs.marketEffects = this.marketEffects;
            rs.pref = pref;
            rs.outputResourceList = new List<ResourceUsed>();

            rs.hasWildResource = (pref & CommercePreferences.OneResourceDiscount) == CommercePreferences.OneResourceDiscount;
            rs.hasBilkis = (marketEffects & CommerceEffects.Bilkis) == CommerceEffects.Bilkis;
            rs.hasSecretWarehouse = (marketEffects & CommerceEffects.SecretWarehouse) == CommerceEffects.SecretWarehouse;

            // kick off a recursive reduction of the resource cost.  Paths that completely eliminate the cost
            // are returned in the requiredResourcesLists.
            commOptions.bAreResourceRequirementsMet = ReduceRecursively(rs, cost.CostAsString());

            if (commOptions.bAreResourceRequirementsMet)
            {
                commOptions.bankCoins += cost.coin;
            }

            // Now go through the list of resources used and tabulate the total for each neighbor.
            int neighborManufacturedGoodCost = ((marketEffects & CommerceEffects.Marketplace) == CommerceEffects.Marketplace) ? 1 : 2;
            int leftNeighborRawMaterialsCost = ((marketEffects & CommerceEffects.WestTradingPost) == CommerceEffects.WestTradingPost) ? 1 : 2;
            int rightNeighborRawMaterialsCost = ((marketEffects & CommerceEffects.EastTradingPost) == CommerceEffects.EastTradingPost) ? 1 : 2;

            foreach (ResourceUsed res in rs.outputResourceList)
            {
                if (res.owner == ResourceOwner.Left)
                {
                    ResourceEffect re = leftResources[res.index];

                    if (re.IsManufacturedGood())
                    {
                        commOptions.leftCoins += neighborManufacturedGoodCost;
                    }
                    else
                    {
                        commOptions.leftCoins += leftNeighborRawMaterialsCost;
                        if (res.usedDoubleResource)
                            commOptions.leftCoins += leftNeighborRawMaterialsCost;
                    }
                }
                else if (res.owner == ResourceOwner.Right)
                {
                    ResourceEffect re = rightResources[res.index];
                    if (re.IsManufacturedGood())
                    {
                        commOptions.rightCoins += neighborManufacturedGoodCost;
                    }
                    else
                    {
                        commOptions.rightCoins += rightNeighborRawMaterialsCost;
                        if (res.usedDoubleResource)
                            commOptions.rightCoins += rightNeighborRawMaterialsCost;
                    }
                }
                else if (res.owner == ResourceOwner.Self && res.index == BilkisResourceIndex)
                {
                    commOptions.bankCoins += 1;     // Add one coin for Bilkis
                }
            }

            if (((marketEffects & CommerceEffects.ClandestineDockWest) == CommerceEffects.ClandestineDockWest) &&
                commOptions.leftCoins != 0)
            {
                commOptions.leftCoins -= 1;
            }

            if (((marketEffects & CommerceEffects.ClandestineDockEast) == CommerceEffects.ClandestineDockEast) &&
                commOptions.rightCoins != 0)
            {
                commOptions.rightCoins -= 1;
            }

            return commOptions;
        }
    }
}
