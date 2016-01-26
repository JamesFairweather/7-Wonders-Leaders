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
        Petra_A_Board,
        Petra_A_s1,
        Petra_A_s2,
        Petra_A_s3,
        Petra_B_Board,
        Petra_B_s1,
        Petra_B_s2,
        Byzantium_A_Board,
        Byzantium_A_s1,
        Byzantium_A_s2,
        Byzantium_A_s3,
        Byzantium_B_Board,
        Byzantium_B_s1,
        Byzantium_B_s2,
        Alexander,
        Amytis,
        Archimedes,
        Aristotle,
        Bilkis,
        Caesar,
        Cleopatra,
        Croesus,
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
            ScienceWild,                    // Science wild card

            // Special wonder stages
            PlayACardForFreeOncePerAge,     // Olympia (A) 2nd stage

            // From the Leaders expansion pack
            FreeLeaders,                    // Roma (A) board, Maecenas
            StructureDiscount,

            // From the Cities expansion pack
            CopyScienceSymbolFromNeighbor,
            LossOfCoins,
            Diplomacy,
            CoinsLossPerMilitaryPoints,
        };
    };

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

    public class ScienceWildEffect : Effect
    {
    }

    public class PlayACardForFreeOncePerAgeEffect : Effect
    {
    }

    public class FreeLeadersEffect : Effect
    {
        // Roma (A) board effect, Maecenas
    }

    public class StructureDiscountEffect : Effect
    {
        public StructureType discountedStructureType;

        public StructureDiscountEffect(StructureType s)
        {
            discountedStructureType = s;
        }
    }

    // From the Cities expansion pack
    public class CopyScienceSymbolFromNeighborEffect : Effect
    {
    }

    public class LossOfCoinsEffect : Effect
    {
        public enum LossCounter
        {
            Constant,
            ConflictToken,
            WonderStage,
        };

        public LossCounter lc;
        public int coinsLost;
        public int victoryPoints;

        public LossOfCoinsEffect(LossCounter l, int coinsLost, int victoryPoints)
        {
            this.lc = l;
            this.coinsLost = coinsLost;
            this.victoryPoints = victoryPoints;
        }
    }

    public class DiplomacyEffect : Effect
    {
    }

    public class CoinsLossPerMilitaryPointsEffect : Effect
    {
    }

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

                    case Effect.Type.PlayACardForFreeOncePerAge:
                        effect = new PlayACardForFreeOncePerAgeEffect();
                        break;

                    // From the Leaders expansion pack
                    case Effect.Type.FreeLeaders:                    // Roma (A) board effect: Maecenas effect
                        effect = new FreeLeadersEffect();
                        break;

                    case Effect.Type.StructureDiscount:
                        effect = new StructureDiscountEffect((StructureType)Enum.Parse(typeof(StructureType), createParams[31]));
                        break;

                    // From the Cities expansion pack
                    case Effect.Type.CopyScienceSymbolFromNeighbor:
                        effect = new CopyScienceSymbolFromNeighborEffect();
                        break;

                    case Effect.Type.LossOfCoins:
                        LossOfCoinsEffect.LossCounter lc = (LossOfCoinsEffect.LossCounter)Enum.Parse(typeof(LossOfCoinsEffect.LossCounter), createParams[27]);
                        effect = new LossOfCoinsEffect(lc, int.Parse(createParams[28]), int.Parse(createParams[29]));
                        break;

                    case Effect.Type.Diplomacy:
                        effect = new DiplomacyEffect();
                        break;

                    case Effect.Type.CoinsLossPerMilitaryPoints:
                        effect = new CoinsLossPerMilitaryPointsEffect();
                        break;

                    default:
                        throw new Exception(string.Format("No effect class for this effect: {0}", effectType.ToString()));
                }
            }

            /*
            // Load the image associated with this card.  Commented-out for now.
            BitmapImage bmpImg = new BitmapImage();
            bmpImg.BeginInit();
            bmpImg.UriSource = new Uri("pack://application:,,,/7W;component/Resources/Images/icons/" + iconName + ".png");
            bmpImg.EndInit();
            */
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
