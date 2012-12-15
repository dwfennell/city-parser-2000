using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityParser2000
{
    public class City
    {
        private static const int tilesPerRow = 128;

        private string cityName;
        private string mayorName;

        private Tile[,] tiles = new Tile[tilesPerRow, tilesPerRow];

        public City()
        {
            initializeTiles();
        }

        private void initializeTiles()
        {
            for (int i = 0; i < tilesPerRow; i++) 
            {
                for (int j = 0; j < tilesPerRow; j++)
                {
                    tiles[i, j] = new Tile();
                }
            }
        }

    }
}
