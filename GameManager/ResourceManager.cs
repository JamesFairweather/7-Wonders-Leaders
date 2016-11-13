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
        public Cost eliminate(Cost structureCost, bool stopAfterAMatchIsFound, string resourceString)
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
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'S':
                        if (c.stone != 0)
                        {
                            --c.stone;
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'B':
                        if (c.clay != 0)
                        {
                            --c.clay;
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'O':
                        if (c.ore != 0)
                        {
                            --c.ore;
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'C':
                        if (c.cloth != 0)
                        {
                            --c.cloth;
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'G':
                        if (c.glass != 0)
                        {
                            --c.glass;
                            if (stopAfterAMatchIsFound) return c;
                        }
                        break;

                    case 'P':
                        if (c.papyrus != 0)
                        {
                            --c.papyrus;
                            if (stopAfterAMatchIsFound) return c;
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

            // If the number of wild resources (i.e. Bilkis/Archimedes/Leonidas/Imhotep/Hammurabi)
            // is greater than or equal to the remaining cost after all other resource options have
            // been spent, the structure is afforable.  I'll remove Bilkis from the list of wilds
            // if the player doesn't have a coin.
            if (nWildResources >= cost.Total())
                return true;

            return false;
        }

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

        static void ReduceRecursively(string strCost, List<ResourceEffect> availableResources, int startResource, Stack<int> thisList, List<List<int>> resourceOptions )
        {
            if (strCost == string.Empty)
            {
                // success!  This combination of resources reduced the cost to zero.

                // if we already have this combination, don't add it again.
                // This will happen if there's more resources available than
                // required.  e.g. a cost string has both Wood and Ore and the
                // resources include the flex card with both wood and ore on it,
                // or the Caravansery.
                bool alreadyHaveIt = false;
                foreach (List<int> resList in resourceOptions)
                {
                    List<int> dup = new List<int>(resList.Count);
                    resList.ForEach(x => { dup.Add(x); });
                    foreach (int re in thisList)
                    {
                        dup.Remove(re);
                    }

                    if (dup.Count == 0)
                    {
                        alreadyHaveIt = true;
                        break;
                    }
                }

                if (!alreadyHaveIt)
                {
                    // Clone this resource stack option and add it to the final list.
                    List<int> validResourceList = new List<int>(thisList.Count);
                    foreach (int r in thisList)
                    {
                        validResourceList.Add(r);
                    }
                    resourceOptions.Add(validResourceList);
                }

                // we have reached the end of this recursion path.
                return;
            }

            for (int i = startResource; i < availableResources.Count; ++i)
            {
                thisList.Push(i);

                ResourceEffect res = availableResources[i];

                foreach (char resType in res.resourceTypes)
                {
                    int ind = strCost.IndexOf(resType);

                    if (ind != -1)
                    {
                        // This resource matches on (or more) of the required resources.  Put it on the
                        // good resource stack and move down a level of recursion.  We increment the start
                        // recursion level so we don't consider this resource again in this chain.



                        ReduceRecursively(strCost.Remove(ind, 1), availableResources, i + 1, thisList, resourceOptions);   // doesn't matter whether it leads to a good path or not

                        // pop the last ResourceEffect off, then move on to the next resource choice for this structure
                    }

                    // if this resource isn't in the cost string, move on to the next option in the resource string.
                }

                thisList.Pop();

            }
        }

        public CommerceOptions GetCommerceOptions(Cost cost)
        {
            CommerceOptions commOptions = new CommerceOptions();
            commOptions.commerceOptions = new List<CommerceOptions.CommerceCost>();

            if (cost.coin == 0 && cost.wood == 0 && cost.stone == 0 && cost.clay == 0 &&
                cost.ore == 0 && cost.cloth == 0 && cost.glass == 0 && cost.papyrus == 0)
            {
                // No resource or coin cost
                commOptions.buildable = Buildable.True;
                return commOptions;
            }

            string strCost = cost.CostAsString();

            if (strCost != string.Empty)
            {
                // Clone the resourceList
                List<ResourceEffect> resrcList = new List<ResourceEffect>(resources.Count);
                resources.FindAll(x => x.IsSimpleResource()).ForEach(item =>
                {
                    resrcList.Add(item);
                });

                int resIndex = 0;

                // The card has a resource (non-coin) cost.  Start by looking at this city's resources
                while (resIndex < strCost.Length)
                {
                    ResourceEffect re = resrcList.Find(x => x.IsSimpleResource() && x.resourceTypes.Contains(strCost[resIndex]));

                    if (re != null)
                    {
                        // if this is a double-resource (Sawmill/Quarry/Foundry/Brickyard), and the next two resources
                        // we are trying to find are the same type, count this match as two instead of one.
                        int nResourcesMatched = re.IsDoubleResource() && strCost.Length > 1 && strCost[resIndex] == strCost[resIndex+1] ? 2 : 1;

                        strCost = strCost.Remove(resIndex, nResourcesMatched);

                        // this resource structure has been used, remove it from the available list.
                        resrcList.Remove(re);
                    }
                    else
                    {
                        resIndex++;
                    }
                }

                if (strCost != string.Empty)
                {
                    // now we have to eliminate the cost using complex resources:
                    // flex cards, Forum, Caravansery, and Black Market
                    resrcList.Clear();
                    resources.FindAll(x => x.IsSimpleResource() == false).ForEach(item =>
                    {
                        resrcList.Add(item);
                    });

                    List<List<int>> requiredResourcesLists = new List<List<int>>();
                    Stack<int> myList = new Stack<int>();

                    ReduceRecursively(strCost, resrcList, 0, myList, requiredResourcesLists);

                    if (requiredResourcesLists.Count > 0)
                    {
                        // at least one of these paths returned a complete elimination of the cost string
                        strCost = string.Empty;

                        // now go through the requiredResourcesLists and see if any did not use any neighbor's resources
                    }
                }
            }

            if (strCost.Length == 0)
            {
                if (cost.coin != 0)
                {
                    CommerceOptions.CommerceCost co = new CommerceOptions.CommerceCost();
                    co.bankCoins = cost.coin;

                    commOptions.commerceOptions.Add(co);
                    commOptions.buildable = Buildable.CommerceRequired;
                }
                else
                {
                    commOptions.buildable = Buildable.True;
                }

                return commOptions;
            }
            else
            {
                commOptions.buildable = Buildable.InsufficientResources;
            }

            // Look at other resources

            return commOptions;
        }
    }
}
