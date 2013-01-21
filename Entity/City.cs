using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
    /// <summary>
    /// The <c>City</c> type is a representation of a simulated city.
    /// </summary>
    public class City
    {
        #region properties and fields

        /// <summary>
        /// The city's name.
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// The mayor of the city.
        /// </summary>
        public string MayorName { get; set; }

        #endregion

        #region public constants

        /// <summary>
        /// <para>Indicates the number of city tiles along each edge of the city.</para>
        /// <para>Cities are always square.</para>
        /// </summary>
        public const int TilesPerSide = 128;

        /// <summary>
        /// Enumerates underground structures.
        /// </summary>
        public enum UndergroundItem { SubwayAndPipe, Tunnel, SubwayStation, Subway, Pipe };

        /// <summary>
        /// Enumerates city zones.
        /// </summary>
        public enum Zone { LightResidential, DenseResidential, LightCommercial, DenseCommercial, LightIndustrial, DenseIndustrial, MilitaryBase, Airport, Seaport };

        public enum MiscStatistic
        {
            CitySize, AvailableFunds, WorkforcePercentage, LifeExpectancy, EducationQuotent, 
            YearOfFounding, DaysSinceFounding, 
            SteelMiningDemand, TextilesDemand, PetrochemicalDemand, FoodDemand, ConstructionDemand, AutomotiveDemand, AerospaceDemand, FinanceDemand, MediaDemand, ElectronicsDemand, TourismDemand,
            SteelMiningTaxRate, TextilesTaxRate, PetrochemicalTaxRate, FoodTaxRate, ConstructionTaxRate, AutomotiveTaxRate, AerospaceTaxRate, FinanceTaxRate, MediaTaxRate, ElectronicsTaxRate, TourismTaxRate,
            SteelMiningRatio, TextilesRatio, PetrochemcalRatio, FoodRatio, ConstructionRatio, AutomotiveRatio, AerospaceRatio, FinanceRatio, MediaRatio, ElectronicsRatio, TourismRatio,
            NeighborSize1, NeighborSize2, NeighborSize3, NeighborSize4
        }

        public enum Map { PolicePower, FirePower, Pollution, Traffic, Crime, Altitude, PropertyValue, PopulationDensity, PopulationGrowth }

        #endregion

        #region local variables

        // Track known miscellaneous integer statistics. 
        private Dictionary<MiscStatistic, int> miscStatistics = new Dictionary<MiscStatistic, int>();
        private Dictionary<Map, List<int>> cityMaps = new Dictionary<Map, List<int>>();
        private List<string> signs = new List<string>();
        private Tile[,] tiles = new Tile[TilesPerSide, TilesPerSide];

        #endregion

        #region constructors and initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public City()
        {
            initializeTiles();
            CityName = "";
            MayorName = "";
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

        /// <summary>
        /// Set the zone for the city tile at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Tile x-coordinate.</param>
        /// <param name="y">Tile y-coordinate.</param>
        /// <param name="zone">Zone code (residential, commercial, etc).</param>
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

        /// <summary>
        /// Set the above-ground structure (<see cref="Building"/>) for the city tile at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Tile x-coordinate.</param>
        /// <param name="y">Tile y-coordinate.</param>
        /// <param name="buildingCode"></param>
        public void SetBuilding(int x, int y, Building.BuildingCode buildingCode)
        {
            tiles[x, y].SetBuilding(buildingCode);
        }


        /// <summary>
        /// Mark a building corner for the city tile at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Tile x-coordinate.</param>
        /// <param name="y">Tile y-coordinate.</param>
        /// <param name="cornerCode">Indicates which corner of the tile is the building corner.</param>
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

        /// <summary>
        /// Add data for a given city map.
        /// </summary>
        /// <param name="mapType">Enum value specifies type of map data</param>
        /// <param name="mapData"></param>
        public void SetMap(Map mapType, List<int> mapData)
        {
            cityMaps[mapType] = new List<int>(mapData);
        }

        /// <summary>
        /// Record user-generated sign text.
        /// </summary>
        /// <param name="signText"></param>
        public void AddSignText(string signText)
        {
            signs.Add(signText);
        }

        /// <summary>
        /// Add a known integer statistic.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMiscStatistic(MiscStatistic key, int value)
        {
            miscStatistics.Add(key, value);
        }

        /// <summary>
        /// Set a series of boolean flags for the city tile at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Tile x-coordinate.</x></param>
        /// <param name="y">Tile y-coordinate.</param>
        /// <param name="isSalty">True if this tile would be salt water.</param>
        /// <param name="isWaterCovered">True if this tile is covered in water.</param>
        /// <param name="isWaterSupplied">True if this tile is connected to the city's water system</param>
        /// <param name="isPiped">True if this tile can convey water.</param>
        /// <param name="isPowered">True if this tile has access to the electric grid.</param>
        /// <param name="isConductive">True if this tile can conduct electricity.</param>
        public void SetTileFlags(int x, int y, bool isSalty, bool isWaterCovered, bool isWaterSupplied, bool isPiped, bool isPowered, bool isConductive)
        {
            tiles[x, y].IsSalty = isSalty;
            tiles[x, y].IsWaterCovered = isWaterCovered;
            tiles[x, y].IsWaterSupplied = isWaterSupplied;
            tiles[x, y].IsPiped = isPiped;
            tiles[x, y].IsPowered = isPowered;
            tiles[x, y].IsConductive = isConductive;
        }

        /// <summary>
        /// Set what is underground of city tile at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">Tile x-coordinate.</param>
        /// <param name="y">Tile y-coordinate.</param>
        /// <param name="undergroundItem">Underground structure code (pipe, subway, etc).</param>
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

        #endregion // setters

        #region getters

        /// <summary>
        /// Get miscellaneous integer stat.
        /// </summary>
        /// <param name="statCode"></param>
        /// <returns></returns>
        public int GetMiscStatistic(MiscStatistic statCode) 
        {
            return miscStatistics[statCode];
        }

        /// <summary>
        /// Tests for the existance of miscellaneous integer stat.
        /// </summary>
        /// <param name="statCode"></param>
        /// <returns></returns>
        public bool HasMiscStatistic(MiscStatistic statCode)
        {
            return miscStatistics.ContainsKey(statCode);
        }

        /// <summary>
        /// Get a map representing a particular city aspect.
        /// </summary>
        /// <param name="mapCode"></param>
        /// <returns>A copy of the specified map's data.</returns>
        public List<int> GetMap(Map mapCode)
        {
            return new List<int>(cityMaps[mapCode]);
        }

        /// <summary>
        /// Tests for the existance of a given city map.
        /// </summary>
        /// <param name="mapCode"></param>
        /// <returns></returns>
        public bool HasMap(Map mapCode)
        {
            return cityMaps.ContainsKey(mapCode);
        }

        #endregion

    }
}
