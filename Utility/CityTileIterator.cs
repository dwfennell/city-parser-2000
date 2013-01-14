using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000.Utility
{
    class CityTileIterator
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int TilesPerSide { get; private set; }

        public int TileNumber
        {
            get { return X + Y * TilesPerSide; }
            private set { TileNumber = value; }
        }

        public CityTileIterator (int tilesPerCitySide) 
        {
            TilesPerSide = tilesPerCitySide;
            X = 0;
            Y = 0;
        }

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

        public void Reset()
        {
            X = 0;
            Y = 0;
        }

        public void ResetToLastTile()
        {
            X = TilesPerSide - 1;
            Y = TilesPerSide - 1;
        }

    }
}
