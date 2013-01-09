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

        public bool IncrementCurrentTile()
        {
            if (X < TilesPerSide - 1)
            {
                X++;
                return true;
            }
            else if (Y < TilesPerSide - 1)
            {
                Y++;
                X = 0;
                return true;
            }
            else
            {
                // We are on the last tile; cannot increment.
                return false;
            }
        }

        public bool DecrementCurrentTile()
        {
            if (X != 0)
            {
                X--;
                return true;
            }
            else if (Y != 0) 
            {    
                X = TilesPerSide - 1;
                Y--;
                return true;
            }
            else
            {
                // We are on the first tile; cannot decrement.
                return false;
            }
        }

        public void Reset()
        {
            X = 0;
            Y = 0;
        }
    }
}
