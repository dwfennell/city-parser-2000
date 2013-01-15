using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000.Utility
{
    /// <summary>
    /// The <c>CityTileIterator</c> iterates through city tile coordinates.
    /// </summary>
    class CityTileIterator
    {
        /// <summary>
        /// X coordinate.
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        /// Y coordinate.
        /// </summary>
        public int Y { get; private set; }
        /// <summary>
        /// Number of tiles for every side.
        /// </summary>
        public int TilesPerSide { get; private set; }

        /// <summary>
        /// Numerical identifier for the current tile.
        /// </summary>
        public int TileNumber
        {
            get { return X + Y * TilesPerSide; }
            private set { TileNumber = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tilesPerCitySide">The number of tiles on each city side.</param>
        public CityTileIterator (int tilesPerCitySide) 
        {
            TilesPerSide = tilesPerCitySide;
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Increments the current tile. If on the final tile roll over to the first tile.
        /// </summary>
        public void IncrementCurrentTile()
        {
            if (X < TilesPerSide - 1)
            {
                X++;
            }
            else if (Y < TilesPerSide - 1)
            {
                Y++;
                X = 0;
            }
            else
            {
                // We are on the last tile; loop back to the start.
                Reset();
            }
        }

        /// <summary>
        /// Decrements the interator. If on the first tile roll over to the final tile.
        /// </summary>
        public void DecrementCurrentTile()
        {
            if (X != 0)
            {
                X--;
            }
            else if (Y != 0) 
            {    
                X = TilesPerSide - 1;
                Y--;
            }
            else
            {
                // We are on the first tile; loop backwards to the last tile.
                ResetToLastTile();
            }
        }

        /// <summary>
        /// Set X and Y coordinates to 0.
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
        }

        /// <summary>
        /// Set X and Y coordinates to their maximum value.
        /// </summary>
        public void ResetToLastTile()
        {
            X = TilesPerSide - 1;
            Y = TilesPerSide - 1;
        }

    }
}
