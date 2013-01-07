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

        public string CityName { get; set; }
        public string MayorName { get; set; }

        #endregion

        #region public constants

        // These things are under the ground.
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

        public void SetBuilding(int x, int y, Building.BuildingCode buildingCode)
        {
            tiles[x, y].SetBuilding(buildingCode);
        }

        public void SetBuildingCorner(int x, int y, Building.CornerCode cornerCode)
        {
            switch (cornerCode)
            {
                case Building.CornerCode.BottomRight:
                    tiles[x, y].HasBuildingCornerBottomRight = true;
                    return;
                case Building.CornerCode.BottomLeft:
                    tiles[x, y].HasBuildingCornerBottomLeft = true;
                    return;
                case Building.CornerCode.TopLeft:
                    tiles[x, y].HasBuildingCornerTopLeft = true;
                    return;
                case Building.CornerCode.TopRight:
                    tiles[x, y].HasBuildingCornerTopRight = true;
                    return;
            }
        }

        public void setAltitude(int x, int y, int altitude)
        {
            tiles[x, y].Altitude = altitude;
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
