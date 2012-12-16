using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
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

        // TODO: Use this for a more friendly "building name" property?
        private int overgroundCode; // XBLD
        private int undergroundCode; // XUND
        // TODO: Make into propery?
        private int zoneCode;
        // TODO: Slope should maybe be dealt with at some point. Very low 
        //  priority, for now just store the code.
        private int slopeCode;

        private bool isWatered { get; set; } //XBIT
        private bool isPowered { get; set; } //XBIT
        private bool canPipeWater { get; set; } //XBIT
        private bool isWaterCovered { get; set; } //XBIT
        private bool isSalty { get; set; } //XBIT

        private string signText { get; set; } // XLAB 

        public Tile()
        {

        }

    }
}
