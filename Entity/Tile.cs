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
        // TODO: Private properties? Presumably I'm going to modify these at some point.
        private int altitude { get; set; }
        private int traffic { get; set; }
        private int police { get; set; }
        private int fire { get; set; }
        private int population { get; set; }
        private int populationGrowthRate { get; set; }
        private int propertyValue { get; set; }
        private int pollution { get; set; }

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

    }
}
