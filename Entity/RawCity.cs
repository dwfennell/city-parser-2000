using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This class represents data from the Sim City 2000 (SC2) file format in a  
// slightly more friendly format than binary. 
// It is used as an intermediary between the binary SC2 and the more 
// comprehensiable City.cs class. It is also used to more easily probe SC2.

namespace CityParser2000
{
    public class RawCity
    {
        private string _cityName;

        /**
         * Plan to represent each data in each SC2 segment: 
         * 
         * 
         * 
         * CNAM: City name. String.
         * MISC: A lot of misc integer stats. Store in array of integers, with a dict mapping to known names.
         * ALTM: Altitude map, also water flags. 2D integer array for altitude, 2d integer flag for 
         * 
         * ... this may be the wrong approach.
         * 
         **/

        public RawCity() 
        {

        }

        public string cityName
        {
            get { return this._cityName; }
            set { this._cityName = value; }
        }

    }
}
