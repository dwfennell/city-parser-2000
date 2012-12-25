using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
    public class City
    {
        
        #region properties and fields

        public string cityName { get; set; }
        public string mayorName { get; set; }

        #endregion

        #region public constants

        // NOTE: I'm not sure if using enums is approprate here. My reasoning 
        //  is that allows me to set a variety of tile properties without 
        //  polluting the class namespace with a bunch of functions, but 
        //  something doesn't feel right. I guess I don't really need to 
        //  worry about maintainability too much in this case. -dustin
        // 
        // TODO: Remove. This functionality (the enum together with "SetTileProperty" was replaced with the simpler and more efficient "SetTileFlags". 
        //  Keeping it for now as a pattern for possible use later. SetTileFlags is perhaps less elegant, but it works better I think.
        //
        // NOTE (2nd!): I might just keep these around for a bit in case anyone has an opinion on which approach might be better, or can offer another solution for this sort of situation.
        public enum TileProperty { Salty, WaterCovered, WaterSupplied, Piped, Powered, Conductive };

        // These things are under the ground.
        // NOTE: Again, I'm unsure about enums for this sort of thing, but for now this seems OK. -dustin
        public enum UndergroundItem { SubwayAndPipe, Tunnel, SubwayStation, Subway, Pipe };

        public enum Zone { LightResidential, DenseResidential, LightCommercial, DenseCommercial, LightIndustrial, DenseIndustrial, MilitaryBase, Airport, Seaport };

        public const int TilesPerSide = 128;

        #endregion

        #region local variables

        private Tile[,] tiles = new Tile[TilesPerSide, TilesPerSide];

        // Keep track of misc integer stats. (We don't know what all of them represent atm).
        private List<int> miscValues = new List<int>(); // This one could be temporary.
        private Dictionary<string, int> miscStats = new Dictionary<string, int>();

        private List<int> policeMap;
        private List<int> firefigherMap;
        private List<int> crimeMap;
        private List<int> pollutionMap;
        private List<int> populationMap;
        private List<int> populationGrowthMap;
        private List<int> trafficMap;
        private List<int> propertyValueMap;

        #endregion

        #region constructors and initialization

        public City()
        {
            initializeTiles();
        }

        private void initializeTiles()
        {
            for (int i = 0; i < TilesPerSide; i++)
            {
                for (int j = 0; j < TilesPerSide; j++)
                {
                    tiles[i, j] = new Tile();
                }
            }
        }

        #endregion

        // TODO: Consider removing this. As we might want to move away from a tile-based implementation in the future.
        public Tile getTile(int x, int y)
        {
            return tiles[x, y];
        }

        #region setters

        public void SetTileFlags(int x, int y, bool isSalty, bool isWaterCovered, bool isWaterSupplied, bool isPiped, bool isPowered, bool isConductive) 
        {
            tiles[x, y].IsSalty = isSalty;
            tiles[x, y].IsWaterCovered = isWaterCovered;
            tiles[x, y].IsWaterSupplied = isWaterSupplied;
            tiles[x, y].IsPiped = isPiped;
            tiles[x, y].IsPowered = isPowered;
            tiles[x, y].IsConductive = isConductive;
        }

        public void SetUndergroundItem(int x, int y, UndergroundItem undergroundItem)
        {
            switch (undergroundItem) 
            {
                case UndergroundItem.Pipe:
                    tiles[x, y].HasPipe = true;
                    return;
                case UndergroundItem.Subway:
                    tiles[x, y].HasSubway = true;
                    return;
                case UndergroundItem.SubwayAndPipe:
                    tiles[x, y].HasPipe = true;
                    tiles[x, y].HasSubway = true;
                    return;
                case UndergroundItem.SubwayStation:
                    tiles[x, y].HasSubwayStation = true;
                    return;
                case UndergroundItem.Tunnel:
                    tiles[x, y].HasTunnel = true;
                    return;
            }
        }

        public void SetZone(int x, int y, Zone zone)
        {
            switch (zone)
            {
                case Zone.LightResidential:
                    tiles[x, y].IsLightResidential = true;
                    return;
                case Zone.DenseResidential:
                    tiles[x, y].IsDenseResidential = true;
                    return;
                case Zone.LightCommercial:
                    tiles[x, y].IsLightCommercial = true;
                    return;
                case Zone.DenseCommercial:
                    tiles[x, y].IsDenseCommerical = true;
                    return;
                case Zone.LightIndustrial:
                    tiles[x, y].IsLightIndustrial = true;
                    return;
                case Zone.DenseIndustrial:
                    tiles[x, y].IsDenseIndustrial = true;
                    return;
                case Zone.Airport:
                    tiles[x, y].IsAirport = true;
                    return;
                case Zone.Seaport:
                    tiles[x, y].IsSeaport = true;
                    return;
                case Zone.MilitaryBase:
                    tiles[x, y].IsMilitaryBase = true;
                    return;
            }
        }

        public void SetBuilding(int x, int y, int buildingCode)
        {
            tiles[x, y].SetBuilding(buildingCode);
        }

        // TODO: Remove later. Keeping this here for now in case I want to use this as a pattern later.
        // It has been replaced with "SetTileFlags".
        public void SetTileProperty(TileProperty tileProperty, int x, int y, bool value)
        {
            switch (tileProperty)
            {
                case TileProperty.Conductive:
                    tiles[x, y].IsConductive = value;
                    return;
                case TileProperty.Piped:
                    tiles[x, y].IsPiped = value;
                    return;
                case TileProperty.Powered:
                    tiles[x, y].IsPowered = value;
                    return;
                case TileProperty.Salty:
                    tiles[x, y].IsSalty = value;
                    return;
                case TileProperty.WaterCovered:
                    tiles[x, y].IsWaterCovered = value;
                    return;
                case TileProperty.WaterSupplied:
                    tiles[x, y].IsWaterSupplied = value;
                    return;
                default:
                    // TODO: Throw an exception here?
                    Console.WriteLine("WARNING: SetTileProperty (bool overload) default case used.");
                    return;
            }
        }

        public void SetPoliceMap(List<int> mapData)
        {
            policeMap = new List<int>(mapData);
        }

        public void SetCrimeMap(List<int> mapData)
        {
            crimeMap = new List<int>(mapData);
        }

        public void SetFirefighterMap(List<int> mapData)
        {
            firefigherMap = new List<int>(mapData);
        }

        public void SetPollutionMap(List<int> mapData)
        {
            pollutionMap = new List<int>(mapData);
        }

        public void SetPopulationMap(List<int> mapData)
        {
            populationMap = new List<int>(mapData);
        }

        public void SetPopulationGrowthMap(List<int> mapData)
        {
            populationGrowthMap = new List<int>(mapData);
        }

        public void SetTrafficMap(List<int> mapData)
        {
            trafficMap = new List<int>(mapData);
        }

        public void SetPropertyValueMap(List<int> mapData)
        {
            propertyValueMap = new List<int>(mapData);
        }

        // This method may be tempoarary, but is useful during testing.
        public void AddMiscValue(int value)
        {
            miscValues.Add(value);
        }

        #endregion // setters

    }
}
