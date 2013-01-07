using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
    // Note: I wonder is setting data for individual tiles is really the best thing to do. 
    //  Constructing a series of city "maps" (aka, power map, water system map) might be more useful. 

    public class Tile
    {
        // For now I'm doing the simplest thing possible, which is to just make this class be a bunch of boolean flags (and ints?). 
        // Someday a more complex scheme may come together, but for the moment this works fine. -dustin

        private Building building { get; set; }

        // TODO: Private properties should become 'real' public properties at some point.
        private int traffic { get; set; }
        private int police { get; set; }
        private int fire { get; set; }
        private int population { get; set; }
        private int populationGrowthRate { get; set; }
        private int propertyValue { get; set; }
        private int pollution { get; set; }

        public int Altitude { get; set; }

        public bool IsWaterSupplied { get; set; }
        public bool IsPowered { get; set; }
        public bool IsConductive { get; set; }
        public bool IsPiped { get; set; }
        public bool IsWaterCovered { get; set; }
        public bool IsSalty { get; set; }

        // Underground items.
        public bool HasPipe { set; get; }
        public bool HasSubway { set; get; }
        public bool HasTunnel { set; get; }
        public bool HasSubwayStation { set; get; }

        // Zones
        public bool IsDenseResidential { set; get; }
        public bool IsLightResidential { set; get; }
        public bool IsLightCommercial { set; get; }
        public bool IsDenseCommerical { set; get; }
        public bool IsLightIndustrial { set; get; }
        public bool IsDenseIndustrial { set; get; }
        public bool IsMilitaryBase { set; get; }
        public bool IsAirport { set; get; }
        public bool IsSeaport { set; get; }
        public bool IsResidential
        {
            get
            {
                return IsDenseResidential || IsLightResidential;
            }
            private set { }
        }
        public bool IsCommerical
        {
            get
            {
                return IsDenseCommerical || IsLightCommercial;
            }
            private set { }
        }
        public bool IsIndustrial 
        {
            get
            {
                return IsDenseIndustrial || IsLightIndustrial;
            }
            private set {}
        }

        public string SignText { get; set; } // XLAB 

        public Tile()
        {
            IsWaterSupplied = false;
            IsPowered = false;
            IsConductive = false;
            IsPiped = false;
            IsWaterCovered = false;
            IsSalty = false;

            HasPipe = false;
            HasSubway = false;
            HasTunnel = false;
            HasSubwayStation = false;
        }

        public void SetBuilding(int code)
        {
            building = new Building((Building.BuildingCode)code);
        }

    }
}
