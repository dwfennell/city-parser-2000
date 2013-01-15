using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
    /// <summary>
    /// The <c>Tile</c> type describes a single tile in a simulated <see cref="City"/>.
    /// </summary>
    public class Tile
    {

        #region Public Properties

        /// <summary>
        /// Altitude in meters.
        /// </summary>
        public int Altitude { get; set; }

        /// <summary>
        /// True if tile is supplied with water from the city water system.
        /// </summary>
        public bool IsWaterSupplied { get; set; }
        /// <summary>
        /// True if tile is supplied with electricity.
        /// </summary>
        public bool IsPowered { get; set; }
        /// <summary>
        /// True if the tile can conduct electricty.
        /// </summary>
        public bool IsConductive { get; set; }
        /// <summary>
        /// True if the city can conduct water from the city water system.
        /// </summary>
        public bool IsPiped { get; set; }
        /// <summary>
        /// True if the tile is covered in water.
        /// </summary>
        public bool IsWaterCovered { get; set; }
        /// <summary>
        /// True if the tile is salt water, or would be salt water if it were water covered.
        /// </summary>
        public bool IsSalty { get; set; }

        // Building corner flags.
        // Set if a building has a corner in each respective tile corner.
        // Eg. A 1x1 building (such as a small park) has all flags set to 'true'.
        /// <summary>
        /// True if there is a building corner in the top-right corner of this tile.
        /// </summary>
        public bool HasBuildingCornerTopRight { get; set; }
        /// <summary>
        /// True if there is a building corner in the top-left corner of this tile.
        /// </summary>
        public bool HasBuildingCornerTopLeft { get; set; }
        /// <summary>
        /// True if there is a building corner in the bottom-right corner of this tile.
        /// </summary>
        public bool HasBuildingCornerBottomRight  { get; set; }
        /// <summary>
        /// True if there is a building corner in the bottom-left corner of this tile.
        /// </summary>
        public bool HasBuildingCornerBottomLeft  { get; set; }

        // Underground items.
        /// <summary>
        /// True if there is a pipe under this tile.
        /// </summary>
        public bool HasPipe { set; get; }
        /// <summary>
        /// True if there is a subway under this tile.
        /// </summary>
        public bool HasSubway { set; get; }
        /// <summary>
        /// True if there is a tunnel under this tile.
        /// </summary>
        public bool HasTunnel { set; get; }
        /// <summary>
        /// True if there is a subway station under this tile.
        /// </summary>
        public bool HasSubwayStation { set; get; }

        // Zones.
        /// <summary>
        /// True if this tile is zoned dense residential.
        /// </summary>
        public bool IsDenseResidential { set; get; }
        /// <summary>
        /// True if this tile is zoned light residential.
        /// </summary>
        public bool IsLightResidential { set; get; }
        /// <summary>
        /// True if this tile is zoned light commmercial.
        /// </summary>
        public bool IsLightCommercial { set; get; }
        /// <summary>
        /// True if this tile is zoned dense commercial.
        /// </summary>
        public bool IsDenseCommerical { set; get; }
        /// <summary>
        /// True if this tile is zoned light industrial.
        /// </summary>
        public bool IsLightIndustrial { set; get; }
        /// <summary>
        /// True if this tile is zoned dense industrial.
        /// </summary>
        public bool IsDenseIndustrial { set; get; }
        /// <summary>
        /// True if this tile is zoned for a military base.
        /// </summary>
        public bool IsMilitaryBase { set; get; }
        /// <summary>
        /// True if this tile is zoned for an airport.
        /// </summary>
        public bool IsAirport { set; get; }
        /// <summary>
        /// True if this tile is zoned for a seaport.
        /// </summary>
        public bool IsSeaport { set; get; }
        
        /// <summary>
        /// True if this tile is zoned residential.
        /// </summary>
        public bool IsResidential
        {
            get
            {
                return IsDenseResidential || IsLightResidential;
            }
            private set { }
        }

        /// <summary>
        /// True if this tile is zoned commerical.
        /// </summary>
        public bool IsCommerical
        {
            get
            {
                return IsDenseCommerical || IsLightCommercial;
            }
            private set { }
        }

        /// <summary>
        /// True if this tile is zoned industrial.
        /// </summary>
        public bool IsIndustrial 
        {
            get
            {
                return IsDenseIndustrial || IsLightIndustrial;
            }
            private set {}
        }

        /// <summary>
        /// The text for a user-generated sign, if it exists on this square.
        /// </summary>
        public string SignText { get; set; } 

        private Building building { get; set; }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
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

            HasBuildingCornerBottomLeft = false;
            HasBuildingCornerBottomRight = false;
            HasBuildingCornerTopLeft = false;
            HasBuildingCornerTopRight = false;
            Altitude = 0;

            IsDenseCommerical = false;
            IsDenseIndustrial = false;
            IsDenseResidential = false;
            IsLightCommercial = false;
            IsLightIndustrial = false;
            IsLightResidential = false;
            IsAirport = false;
            IsSeaport = false;
            IsMilitaryBase = false;

        }

        /// <summary>
        /// Set the building that exists on this tile.
        /// </summary>
        /// <param name="buildingCode"></param>
        public void SetBuilding(Building.BuildingCode buildingCode)
        {
            building = new Building(buildingCode);
        }

    }
}
