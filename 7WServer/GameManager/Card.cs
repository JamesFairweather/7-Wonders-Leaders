using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public enum CardId
    {
        Lumber_Yard,
        Stone_Pit,
        Clay_Pool,
        Ore_Vein,
        Tree_Farm,
        Excavation,
        Clay_Pit,
        Timber_Yard,
        Forest_Cave,
        Mine,
        Loom,
        Glassworks,
        Press,
        Pawnshop,
        Baths,
        Altar,
        Theatre,
        Tavern,
        East_Trading_Post,
        West_Trading_Post,
        Marketplace,
        Stockade,
        Barracks,
        Guard_Tower,
        Apothecary,
        Workshop,
        Scriptorium,
        Sawmill,
        Quarry,
        Brickyard,
        Foundry,
        Aqueduct,
        Temple,
        Statue,
        Courthouse,
        Forum,
        Caravansery,
        Vineyard,
        Bazar,
        Walls,
        Training_Ground,
        Stables,
        Archery_Range,
        Dispensary,
        Laboratory,
        Library,
        School,
        Pantheon,
        Gardens,
        Town_Hall,
        Palace,
        Senate,
        Haven,
        Lighthouse,
        Chamber_of_Commerce,
        Arena,
        Fortifications,
        Circus,
        Arsenal,
        Siege_Workshop,
        Lodge,
        Observatory,
        University,
        Academy,
        Study,
        Workers_Guild,
        Craftsmens_Guild,
        Traders_Guild,
        Philosophers_Guild,
        Spies_Guild,
        Strategists_Guild,
        Shipowners_Guild,
        Scientists_Guild,
        Magistrates_Guild,
        Builders_Guild,
        Alexandria_A_Board,
        Alexandria_A_s1,
        Alexandria_A_s2,
        Alexandria_A_s3,
        Alexandria_B_Board,
        Alexandria_B_s1,
        Alexandria_B_s2,
        Alexandria_B_s3,
        Babylon_A_Board,
        Babylon_A_s1,
        Babylon_A_s2,
        Babylon_A_s3,
        Babylon_B_Board,
        Babylon_B_s1,
        Babylon_B_s2,
        Babylon_B_s3,
        Ephesos_A_Board,
        Ephesos_A_s1,
        Ephesos_A_s2,
        Ephesos_A_s3,
        Ephesos_B_Board,
        Ephesos_B_s1,
        Ephesos_B_s2,
        Ephesos_B_s3,
        Giza_A_Board,
        Giza_A_s1,
        Giza_A_s2,
        Giza_A_s3,
        Giza_B_Board,
        Giza_B_s1,
        Giza_B_s2,
        Giza_B_s3,
        Giza_B_s4,
        Halikarnassos_A_Board,
        Halikarnassos_A_s1,
        Halikarnassos_A_s2,
        Halikarnassos_A_s3,
        Halikarnassos_B_Board,
        Halikarnassos_B_s1,
        Halikarnassos_B_s2,
        Halikarnassos_B_s3,
        Olympia_A_Board,
        Olympia_A_s1,
        Olympia_A_s2,
        Olympia_A_s3,
        Olympia_B_Board,
        Olympia_B_s1,
        Olympia_B_s2,
        Olympia_B_s3,
        Rhodos_A_Board,
        Rhodos_A_s1,
        Rhodos_A_s2,
        Rhodos_A_s3,
        Rhodos_B_Board,
        Rhodos_B_s1,
        Rhodos_B_s2,
        Roma_A_Board,
        Roma_A_s1,
        Roma_A_s2,
        Roma_B_Board,
        Roma_B_s1,
        Roma_B_s2,
        Roma_B_s3,
        Alexander,
        Amytis,
        Archimedes,
        Aristotle,
        Bilkis,
        Caesar,
        Cleopatra,
        Croeseus,
        Euclid,
        Hammurabi,
        Hannibal,
        Hatshepsut,
        Hiram,
        Hypatia,
        Imhotep,
        Justinian,
        Leonidas,
        Maecenas,
        Midas,
        Nebuchadnezzar,
        Nefertiti,
        Nero,
        Pericles,
        Phidias,
        Plato,
        Praxiteles,
        Ptolemy,
        Pythagoras,
        Ramses,
        Sappho,
        Solomon,
        Tomyris,
        Varro,
        Vitruvius,
        Xenophon,
        Zenobia,
        Gamers_Guild,
        Courtesans_Guild,
        Diplomats_Guild,
        Architects_Guild,
        Pigeon_Loft,
        Militia,
        Hideout,
        Residence,
        Gambling_Den,
        Clandestine_Dock_West,
        Clandestine_Dock_East,
        Secret_Warehouse,
        Gates_of_the_City,
        Spy_Ring,
        Mercenaries,
        Lair,
        Consulate,
        Gambling_House,
        Black_Market,
        Sepulcher,
        Architect_Cabinet,
        Tabularium,
        Builders_Union,
        Torture_Chamber,
        Contingent,
        Brotherhood,
        Embassy,
        Cenotaph,
        Secret_Society,
        Slave_Market,
        Capitol,
        Bernice,
        Darius,
        Caligula,
        Aspasia,
        Diocletian,
        Semiramis,
        Counterfeiters_Guild,
        Guild_of_Shadows,
        Mourners_Guild,
    };

    // will be used for Wonder stages as well as card structures
    public class Cost
    {
        public int coin;
        public int wood;
        public int stone;
        public int clay;
        public int ore;
        public int cloth;
        public int glass;
        public int papyrus;

        public Cost()
        {
            coin = wood = stone = clay = ore = cloth = glass = papyrus = 0;
        }

        public Cost(string initStr)
        {
            coin = wood = stone = clay = ore = cloth = glass = papyrus = 0;

            foreach (char c in initStr)
            {
                switch (c)
                {
                    case '$': ++coin; break;
                    case 'W': ++wood; break;
                    case 'S': ++stone; break;
                    case 'B': ++clay; break;
                    case 'O': ++ore; break;
                    case 'C': ++cloth; break;
                    case 'G': ++glass; break;
                    case 'P': ++papyrus; break;
                    default:
                        throw new Exception();
                }
            }
        }

        public bool IsZero()
        {
            return coin == 0 && wood == 0 && stone == 0 && clay == 0 && ore == 0 && cloth == 0 && glass == 0 && papyrus == 0;
        }

        public Cost Copy()
        {
            Cost c = new Cost();

            c.coin = this.coin;
            c.wood = this.wood;
            c.stone = this.stone;
            c.clay = this.clay;
            c.ore = this.ore;
            c.cloth = this.cloth;
            c.glass = this.glass;
            c.papyrus = this.papyrus;

            return c;
        }

        public int Total()
        {
            return coin + wood + stone + clay + ore + cloth + glass + papyrus;
        }
    };

    public enum Buildable
    {
        True,
        CommerceRequired,
        InsufficientResources,
        InsufficientCoins,
        StructureAlreadyBuilt,      // for Wonder stages, this means all wonders stages have been built already.
    };

    public enum ExpansionSet
    {
        Original,
        Leaders,
        Cities,
    };

    public enum StructureType
    {
        // Basic cards
        RawMaterial,
        Goods,
        Civilian,
        Commerce,
        Military,
        Science,
        Guild,

        // Tavern, Ephesos (A) stage 2, Ephesos (B), all 3 stages.  The number of coins and points is NOT dependent on external factors.
        Constant,

        // These cards are not played, but they are used in some effects to determine coins and/or points
        MilitaryLosses,
        WonderStage,

        // Expansions
        Leader,
        City,

        // Tokens or classes that are considered for some cards in the expansion sets
        ConflictToken,
        ThreeCoins,
    };

    public abstract class Effect
    {
        public enum Type
        {
            // Cost,            // Spend coins.  Not used in the card list
            Military,           // All military cards, no others
            Resource,           // All browns, greys, plus the Forum, Caravansery, Alexendria wonder effects
            Science,            // All science cards, not including Scientist's Guild or Babylon wonder
            Commerce,           // Marketplace, Trading Posts, Olympia B stage 1
            CoinsPoints,        // All civilian, most guilds, all age 3 commerce, Vineyard, Bazar, most wonder stages

            // These cards don't fit into one of the above categories
            //ShipOwnersGuild,                // ShipOwners Guild
            ScienceWild,                    // Science wild card

            // Special wonder stages
            // PlayLastCardInAge,              // Babylon (B) 2nd stage
            PlayDiscardedCardForFree,       // Halikarnassos (A) 2nd stage
            //PlayDiscardedCardForFree_1VP,   // Halikarnassos (B) 2nd stage
            //PlayDiscardedCardForFree_2VP,   // Halikarnassos (B) 1st stage
            PlayACardForFreeOncePerAge,     // Olympia (A) 2nd stage
            //CopyGuildFromNeighbor,          // Olympia (B) 3rd stage
            //Rhodos_B_Stage1,                // Rhodos (B) 1st stage
            //Rhodos_B_Stage2,                // Rhodos (B) 2nd stage

            // From the Leaders expansion pack
            FreeLeaders,                    // Roma (A) board effect, Maecenas effect
            // DraftFourNewLeaders_5Coins,
            // PlayALeader_3VP,
            StructureDiscount,
            /*
            Aristotle,
            Bilkis,
            Hatshepsut,
            Justinian,
            Plato,
            Ramses,
            Tomyris,
            Vitruvius,
            Courtesan,
            */

            // From the Cities expansion pack
            CopyScienceSymbolFromNeighbor,
            CoinsLossPoints,
            Diplomacy,
            // Gambling_Den,
            //Clandestine_Dock_West,
            //Clandestine_Dock_East,
            //Secret_Warehouse,
            //Gambling_House,
            //Black_Market,
            CoinsLossPerMilitaryPoints,
            //Architect_Cabinet,
            //Builders_Union,
            //Bernice,
            //PlayABlackCardForFreeOncePerAge,
            //Semiramis,
        };
    };

    public class CoinEffect : Effect
    {
        public int coins;

        public CoinEffect(int coins)
        {
            this.coins = coins;
        }
    }

    public class MilitaryEffect : Effect
    {
        public int nShields { get; }

        public MilitaryEffect(int nShields)
        {
            this.nShields = nShields;
        }
    }

    public class ResourceEffect : Effect
    {
        public bool canBeUsedByNeighbors;
        public string resourceTypes { get; }

        public ResourceEffect(bool canBeUsedByNeighbors, string resourceTypes)
        {
            this.canBeUsedByNeighbors = canBeUsedByNeighbors;
            this.resourceTypes = resourceTypes;
        }

        public bool IsDoubleResource()
        {
            return resourceTypes.Length == 2 && resourceTypes[0] == resourceTypes[1];
        }
    }

    public class ScienceEffect : Effect
    {
        public enum Symbol
        {
            Compass,
            Gear,
            Tablet,
        };

        public Symbol symbol {
            get
            {
                switch (chSymbol)
                {
                    case 'C': return Symbol.Compass;
                    case 'G': return Symbol.Gear;
                    case 'T': return Symbol.Tablet;
                    default: throw new Exception();
                }
            }
        }

        char chSymbol;

        public ScienceEffect(string symbol)
        {
            this.chSymbol = symbol[0];
        }
    };

    public class CommercialDiscountEffect : Effect
    {
        public enum RawMaterials
        {
            None,
            LeftNeighbor,
            RightNeighbor,
            BothNeighbors,
        };

        public enum Goods
        {
            None,
            BothNeighbors,
        };

        public string effectString { get; }

        public CommercialDiscountEffect(string effectString)
        {
            this.effectString = effectString;
        }
    };

    public class CoinsAndPointsEffect : Effect
    {
        public enum CardsConsidered
        {
            None,
            Player,
            Neighbors,
            PlayerAndNeighbors,
        };

        public CardsConsidered cardsConsidered;
        public StructureType classConsidered;
        public int coinsGrantedAtTimeOfPlayMultiplier;
        public int victoryPointsAtEndOfGameMultiplier;

        public CoinsAndPointsEffect(CardsConsidered cardsConsidered, StructureType classConsidered, int coinsGrantedAtTimeOfPlayMultiplier, int victoryPointsAtEndOfGameMultiplier)
        {
            this.cardsConsidered = cardsConsidered;
            this.classConsidered = classConsidered;
            this.coinsGrantedAtTimeOfPlayMultiplier = coinsGrantedAtTimeOfPlayMultiplier;
            this.victoryPointsAtEndOfGameMultiplier = victoryPointsAtEndOfGameMultiplier;
        }
    };

    //public class ShipOwnersGuildEffect : Effect
   // {
   // }

    public class ScienceWildEffect : Effect
    {
    }

    //public class PlayLastCardInAgeEffect : Effect
   // {
   // }

    public class PlayDiscardedCardForFreeEffect : Effect
    {
    }

    //public class PlayDiscardedCardForFree_1VPEffect : Effect
    //{
    //}

    //public class PlayDiscardedCardForFree_2VPEffect : Effect
    //{
    //}

    public class PlayACardForFreeOncePerAgeEffect : Effect
    {
    }

    //public class CopyGuildFromNeighborEffect : Effect
    //{
    //}

    //public class Rhodos_B_Stage1Effect : Effect
    //{
    //}

    //public class Rhodos_B_Stage2Effect : Effect
    //{
    //}

    public class FreeLeadersEffect : Effect
    {
        // Roma (A) board effect, Maecenas
    }

    //public class RomaBBoardEffect : Effect
    //{
        // Roma (B) board effect (leaders cost is reduced by 2 for the player, and 1 for the players' neighbors.
    //}

    //public class DraftFourNewLeaders_5CoinsEffect : Effect
    //{
    //}

    //public class PlayALeader_3VPEffect : Effect
   // {
    //}

    public class StructureDiscountEffect : Effect
    {
        public StructureType discountedStructureType;

        public StructureDiscountEffect(StructureType s)
        {
            discountedStructureType = s;
        }
    }

    /*
    public class AristotleEffect : Effect
    {
    }

    public class BilkisEffect : Effect
    {
    }

    public class HatshepsutEffect : Effect
    {
    }

    public class JustinianEffect : Effect
    {
    }

    public class PlatoEffect : Effect
    {
    }

    public class RamsesEffect : Effect
    {
    }

    public class TomyrisEffect : Effect
    {
    }

    public class VitruviusEffect : Effect
    {
    }

    public class CourtesanEffect : Effect
    {
    }
    */

    // From the Cities expansion pack
    public class CopyScienceSymbolFromNeighborEffect : Effect
    {
    }

    public class CoinsLossPointsEffect : Effect
    {
    }

    public class DiplomacyEffect : Effect
    {
    }

    /*
    public class Gambling_DenEffect : Effect
    {
    }

    public class Clandestine_Dock_WestEffect : Effect
    {
    }

    public class Clandestine_Dock_EastEffect : Effect
    {
    }

    public class Secret_WarehouseEffect : Effect
    {
    }

    public class Gambling_HouseEffect : Effect
    {
    }

    public class Black_MarketEffect : Effect
    {
    }
    */

    public class CoinsLossPerMilitaryPointsEffect : Effect
    {
    }

    /*
    public class Architect_CabinetEffect : Effect
    {
    }

    public class Builders_UnionEffect : Effect
    {
    }

    public class BerniceEffect : Effect
    {
    }

    public class PlayABlackCardForFreeOncePerAgeEffect : Effect
    {
    }

    public class SemiramisEffect : Effect
    {
    }
    */

    public class Card
    {
        public ExpansionSet expansion;

        public CardId Id { get; private set; }

        public string strName { get; private set; }

        public StructureType structureType { get; private set; }

        public int age;

        public int wonderStage;

        public string description { get; private set; }
        public string iconName { get; private set; }
        int[] numAvailableByNumPlayers = new int[5];
        public Cost cost = new Cost();      // TODO: is it possible to make this immutable?
        public string[] chain = new string[2];
        public Effect effect;

        public bool isLeader { get { return structureType == StructureType.Leader; } }

        public Card(CardId cardId, string name, Effect effect)
        {
            this.Id = cardId;
            this.strName = name;
            this.effect = effect;
        }

        public Card(string[] createParams)
        {
            expansion = (ExpansionSet)Enum.Parse(typeof(ExpansionSet), createParams[0]);
            strName = createParams[1];

            structureType = (StructureType)Enum.Parse(typeof(StructureType), createParams[2]);

            switch (structureType)
            {
                case StructureType.WonderStage:
                    age = 0;
                    wonderStage = int.Parse(createParams[30]);
                    break;

                default:
                    age = int.Parse(createParams[3]);
                    wonderStage = 0;
                    for (int i = 0, j = 6; i < numAvailableByNumPlayers.Length; ++i, ++j)
                        numAvailableByNumPlayers[i] = int.Parse(createParams[j]);
                    break;
            }

            Id = CardNameFromStringName(strName, wonderStage);

            description = createParams[4];
            iconName = createParams[5];

            // Structure cost
            int.TryParse(createParams[11], out cost.coin);
            int.TryParse(createParams[12], out cost.wood);
            int.TryParse(createParams[13], out cost.stone);
            int.TryParse(createParams[14], out cost.clay);
            int.TryParse(createParams[15], out cost.ore);
            int.TryParse(createParams[16], out cost.cloth);
            int.TryParse(createParams[17], out cost.glass);
            int.TryParse(createParams[18], out cost.papyrus);

            // build chains (Cards that can be built for free in the following age)
            chain[0] = createParams[19];
            chain[1] = createParams[20];

            if (createParams[21] != string.Empty)
            {
                var effectType = (Effect.Type)Enum.Parse(typeof(Effect.Type), createParams[21]);

                switch (effectType)
                {
                    case Effect.Type.Military:
                        effect = new MilitaryEffect(int.Parse(createParams[22]));
                        break;

                    case Effect.Type.Resource:
                        effect = new ResourceEffect(structureType == StructureType.RawMaterial || structureType == StructureType.Goods,
                            createParams[23]);
                        break;

                    case Effect.Type.Science:
                        effect = new ScienceEffect(createParams[24]);
                        break;

                    case Effect.Type.Commerce:
                        effect = new CommercialDiscountEffect(createParams[25]);
                        break;

                    case Effect.Type.CoinsPoints:
                        CoinsAndPointsEffect.CardsConsidered cardsConsidered = (CoinsAndPointsEffect.CardsConsidered)
                            Enum.Parse(typeof(CoinsAndPointsEffect.CardsConsidered), createParams[26]);

                        StructureType classConsidered =
                            (StructureType)Enum.Parse(typeof(StructureType), createParams[27]);

                        int coinsGranted = 0;
                        int.TryParse(createParams[28], out coinsGranted);

                        int pointsAwarded = 0;
                        int.TryParse(createParams[29], out pointsAwarded);

                        effect = new CoinsAndPointsEffect(cardsConsidered, classConsidered, coinsGranted, pointsAwarded);
                        break;

                    case Effect.Type.ScienceWild:
                        effect = new ScienceWildEffect();
                        break;

                    //case Effect.Type.PlayLastCardInAge:
                    //    effect = new PlayLastCardInAgeEffect();
                    //    break;

                    case Effect.Type.PlayDiscardedCardForFree:
                        effect = new PlayDiscardedCardForFreeEffect();
                        break;

                    //case Effect.Type.PlayDiscardedCardForFree_1VP:
                    //    effect = new PlayDiscardedCardForFree_1VPEffect();
                    //    break;

                    //case Effect.Type.PlayDiscardedCardForFree_2VP:
                    //    effect = new PlayDiscardedCardForFree_2VPEffect();
                    //    break;

                    case Effect.Type.PlayACardForFreeOncePerAge:
                        effect = new PlayACardForFreeOncePerAgeEffect();
                        break;

                    //case Effect.Type.CopyGuildFromNeighbor:
                    //    effect = new CopyGuildFromNeighborEffect();
                    //    break;

                    //case Effect.Type.Rhodos_B_Stage1:
                    //    effect = new Rhodos_B_Stage1Effect();
                    //    break;

                    //case Effect.Type.Rhodos_B_Stage2:
                    //    effect = new Rhodos_B_Stage2Effect();
                    //    break;

                    // From the Leaders expansion pack
                    case Effect.Type.FreeLeaders:                    // Roma (A) board effect: Maecenas effect
                        effect = new FreeLeadersEffect();
                        break;

                    //case Effect.Type.DraftFourNewLeaders_5Coins:
                    //    effect = new DraftFourNewLeaders_5CoinsEffect();
                    //    break;

                    //case Effect.Type.PlayALeader_3VP:
                    //    effect = new PlayALeader_3VPEffect();
                     //   break;

                    case Effect.Type.StructureDiscount:
                        effect = new StructureDiscountEffect((StructureType)Enum.Parse(typeof(StructureType), createParams[31]));
                        break;

                        /*
                    case Effect.Type.Aristotle:
                        effect = new AristotleEffect();
                        break;

                    case Effect.Type.Bilkis:
                        effect = new BilkisEffect();
                        break;

                    case Effect.Type.Hatshepsut:
                        effect = new HatshepsutEffect();
                        break;

                    case Effect.Type.Justinian:
                        effect = new JustinianEffect();
                        break;

                    case Effect.Type.Plato:
                        effect = new PlatoEffect();
                        break;

                    case Effect.Type.Ramses:
                        effect = new RamsesEffect();
                        break;

                    case Effect.Type.Tomyris:
                        effect = new TomyrisEffect();
                        break;

                    case Effect.Type.Vitruvius:
                        effect = new VitruviusEffect();
                        break;

                    case Effect.Type.Courtesan:
                        effect = new CourtesanEffect();
                        break;
                        */

                    // From the Cities expansion pack
                    case Effect.Type.CopyScienceSymbolFromNeighbor:
                        effect = new CopyScienceSymbolFromNeighborEffect();
                        break;

                    case Effect.Type.CoinsLossPoints:
                        effect = new CoinsLossPointsEffect();
                        break;

                    case Effect.Type.Diplomacy:
                        effect = new DiplomacyEffect();
                        break;

                        /*
                    case Effect.Type.Gambling_Den:
                        effect = new Gambling_DenEffect();
                        break;

                    case Effect.Type.Clandestine_Dock_West:
                        effect = new Clandestine_Dock_WestEffect();
                        break;

                    case Effect.Type.Clandestine_Dock_East:
                        effect = new Clandestine_Dock_EastEffect();
                        break;

                    case Effect.Type.Secret_Warehouse:
                        effect = new Secret_WarehouseEffect();
                        break;

                    case Effect.Type.Gambling_House:
                        effect = new Gambling_HouseEffect();
                        break;

                    case Effect.Type.Black_Market:
                        effect = new Black_MarketEffect();
                        break;
                        */
                    case Effect.Type.CoinsLossPerMilitaryPoints:
                        effect = new CoinsLossPerMilitaryPointsEffect();
                        break;
                        /*
                    case Effect.Type.Architect_Cabinet:
                        effect = new Architect_CabinetEffect();
                        break;

                    case Effect.Type.Builders_Union:
                        effect = new Builders_UnionEffect();
                        break;

                    case Effect.Type.Bernice:
                        effect = new BerniceEffect();
                        break;

                    case Effect.Type.PlayABlackCardForFreeOncePerAge:
                        effect = new PlayABlackCardForFreeOncePerAgeEffect();
                        break;

                    case Effect.Type.Semiramis:
                        effect = new SemiramisEffect();
                        break;
                        */
                    default:
                        throw new Exception(string.Format("No effect class for this effect: {0}", effectType.ToString()));
                }
            }
        }


        public int GetNumCardsAvailable(int numPlayers)
        {
            return numAvailableByNumPlayers[numPlayers - 3];
        }

        public static CardId CardNameFromStringName(string nameAsString, int wonderStage = 0)
        {
            nameAsString = nameAsString.Replace("(", string.Empty);
            nameAsString = nameAsString.Replace(")", string.Empty);
            nameAsString = nameAsString.Replace(" ", "_");

            if (wonderStage != 0)
            {
                nameAsString += "_s" + wonderStage.ToString();
            }

            return (CardId)Enum.Parse(typeof(CardId), nameAsString);
        }
    }
}
