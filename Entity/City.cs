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

        //enum miscKeys { foundingYear, moneySupply, simNationPop, neighborPop1, neighborPop2, neighborPop3, neighborPop4 };
        
        #endregion

        #region local constants
        private const int tilesPerSide = 128;
        #endregion

        #region local variables
        private Tile[,] tiles = new Tile[tilesPerSide, tilesPerSide];

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

        #region constructors

        public City()
        {
            initializeTiles();
        }

        #endregion

        #region getters and setters

        public Tile getTile(int x, int y)
        {
            return tiles[x, y];
        }

        // This method may be tempoarary, but is useful during testing.
        public void addMiscValue(int value)
        {
            miscValues.Add(value);
        }

        public void setPoliceMap(List<int> mapData)
        {
            policeMap = new List<int>(mapData);
        }

        public void setCrimeMap(List<int> mapData)
        {
            crimeMap = new List<int>(mapData);
        }

        public void setFirefighterMap(List<int> mapData)
        {
            firefigherMap = new List<int>(mapData);
        }

        public void setPollutionMap(List<int> mapData)
        {
            pollutionMap = new List<int>(mapData);
        }

        public void setPopulationMap(List<int> mapData)
        {
            populationMap = new List<int>(mapData);
        }

        public void setPopulationGrowthMap(List<int> mapData)
        {
            populationGrowthMap = new List<int>(mapData);
        }

        public void setTrafficMap(List<int> mapData)
        {
            trafficMap = new List<int>(mapData);
        }

        public void setPropertyValueMap(List<int> mapData)
        {
            propertyValueMap = new List<int>(mapData);
        }
        
        #endregion

        private void initializeTiles()
        {
            for (int i = 0; i < tilesPerSide; i++) 
            {
                for (int j = 0; j < tilesPerSide; j++)
                {
                    tiles[i, j] = new Tile();
                }
            }
        }

    }
}
