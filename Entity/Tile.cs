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

        public string SignText { get; set; } // XLAB 

        public Tile()
        {

        }

    }
}
