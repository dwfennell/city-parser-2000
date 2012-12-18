using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CityParser2000
{
    public class CityParser
    {
        #region local constants
        
        // Binary segments describing city maps which are solely integer values.
        private static HashSet<string> integerMaps = new HashSet<string> { "XLPC", "XFIR", "XPOP", "XROG", "XTRF", "XPLT", "XVAL", "XCRM" };

        // Binary segments describing city maps in which the byte data is uniqure to each segment.
        private static HashSet<string> complexMaps = new HashSet<string> { "XTER", "XBLD", "XZON", "XUND", "XTXT", "XBIT", "ALTM" };

        #endregion
        
        static void Main()
        {
            //City ourCity = CityParser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\new city.sc2");
            City ourCity = CityParser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\dustropolis.sc2");
        }

        public CityParser () {}

        #region parsing and storage

        public static City ParseBinaryFile(string binaryFilename)
        {
            var city = new City();

            using (BinaryReader reader = new BinaryReader(File.Open(binaryFilename, FileMode.Open)))
            {
                // Read 12-byte header. 

                string iffType = readString(reader, 4);
                reader.ReadBytes(4);
                var fileType = readString(reader, 4);

                // All Sim City 2000 files will have iffType "FORM" and fileType "SCDH".
                if (!iffType.Equals("FORM") || !fileType.Equals("SCDH"))
                {
                    // This is not a Sim City 2000 file.
                    // TODO: Throw an exception? Return blank city? Null?
                    return null;
                }

                // The rest of the file is divided into segments.
                // Each segment begins with a 4-byte segment name, followed by a 32-bit integer segment length.
                // Most segments are compressed using a simple run-length compression scheme, and must be 
                //  decompressed before they can be parsed correctly.
                string segmentName;
                Int32 segmentLength;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Parse and store segment data in a City object. 
                    // NOTE: This loop can and probably should be optimized at some point,
                    //  but right now the focus is on code clarity and semantic functionality.
                    segmentName = readString(reader, 4);
                    segmentLength = readInt32(reader);

                    if ("CNAM".Equals(segmentName))
                    {
                        // City name (uncompressed).
                        city = parseAndStoreCityName(city, reader, segmentLength);
                    }
                    else if ("MISC".Equals(segmentName))
                    {
                        // MISC contains a series of 32-bit integers (compressed). 
                        city = parseAndStoreMiscValues(city, reader, segmentLength);
                    } 
                    else if (integerMaps.Contains(segmentName)) 
                    {
                        List<int> mapData = parseIntegerMap(reader, segmentLength);
                        city = storeIntegerMapData(city, mapData, segmentName);
                        
                    }
                    else if (complexMaps.Contains(segmentName))
                    {
                        city = parseAndStoreComplexMap(city, reader, segmentName, segmentLength);
                    }
                    else
                    {
                        // Unknown segment, ignore.
                        reader.ReadBytes(segmentLength);
                    }
                }
            }
            return city;
        }

        #region complex city map parsers

        private static City parseAndStoreComplexMap(City city, BinaryReader reader, string segmentName, int segmentLength)
        {
            if ("XBIT".Equals(segmentName))
            {
                city = parseAndStoreXbitMap(city, reader, segmentLength);
            }
            else
            {
                // TODO: Segment parsing not yet implemented. 
                reader.ReadBytes(segmentLength);
            }

            return city;
        }

        private static City parseAndStoreXbitMap(City city, BinaryReader reader, int segmentLength)
        {
            // Parse XBIT segment. 
            // XBIT contains one byte of binary flags for each city tile.
            //
            // The flags for each bit are:
            // 0: Salt water. (If true and this tile has water it will be salt water)
            // 1: (unknown)
            // 2: Water covered.
            // 3: (unknown)
            // 4: Supplied with water from city water-system.
            // 5: Conveys water-system water. (Building and pipes convey water)
            // 6: Has electricty.
            // 7: Conducts electricity.

            using (var decompressedReader = new BinaryReader(decompressSegment(reader, segmentLength)))
            {
                bool saltyFlag;
                bool waterCoveredFlag;
                bool waterSuppliedFlag;
                bool pipedFlag;
                bool poweredFlag;
                bool conductiveFlag;

                // These will be used to set the bool flags.
                const byte saltyMask = 1;
                // Unknown flag in 1 << 1 position.
                const byte waterCoveredMask = 1 << 2;
                // Unknown flag in 1 << 3 position.
                const byte waterSuppliedMask = 1 << 4;
                const byte pipedMask = 1 << 5;
                const byte poweredMask = 1 << 6;
                const byte conductiveMask = 1 << 7;
                byte tileByte;

                // Tile coordinates within the city.
                int xCoord = 0;
                int yCoord = 0;
                int citySideLength = City.TilesPerSide;

                while (decompressedReader.BaseStream.Position < decompressedReader.BaseStream.Length)
                {
                    // TODO: Possible bug. Test data "new city.sc2" does not seem to be decompressing this segment correctly.
                    tileByte = decompressedReader.ReadByte();

                    saltyFlag = (tileByte & saltyMask) != 0;
                    waterCoveredFlag = (tileByte & waterCoveredMask) != 0;
                    waterSuppliedFlag = (tileByte & waterSuppliedMask) != 0;
                    pipedFlag = (tileByte & pipedMask) != 0;
                    poweredFlag = (tileByte & poweredMask) != 0;
                    conductiveFlag = (tileByte & conductiveMask) != 0;

                    city.SetTileFlags(xCoord, yCoord, saltyFlag, waterCoveredFlag, waterSuppliedFlag, pipedFlag, poweredFlag, conductiveFlag);

                    // Update tile coodinates.
                    xCoord++;
                    if (xCoord >= citySideLength)
                    {
                        yCoord++;
                        xCoord = 0;
                    }

                }
            }

            return city;
        }

        #endregion

        private static List<int> parseIntegerMap(BinaryReader reader, int segmentLength)
        {
            List<int> mapData = new List<int>();

            using (var decompressedReader = new BinaryReader(decompressSegment(reader, segmentLength)))
            {
                while (decompressedReader.BaseStream.Position < decompressedReader.BaseStream.Length)
                {
                    mapData.Add((int) decompressedReader.ReadByte());
                }
            }

            return mapData;
        }

        private static City storeIntegerMapData(City city, List<int> mapData, string segmentName)
        {
            if ("XLPC".Equals(segmentName))
            {
                city.setPoliceMap(mapData);
            }
            else if ("XFIR".Equals(segmentName))
            {
                city.setFirefighterMap(mapData);
            }
            else if ("XPOP".Equals(segmentName))
            {
                city.setPopulationMap(mapData);
            }
            else if ("XROG".Equals(segmentName))
            {
                city.setPopulationGrowthMap(mapData);
            }
            else if ("XTRF".Equals(segmentName))
            {
                city.setTrafficMap(mapData);
            }
            else if ("XPLT".Equals(segmentName))
            {
                city.setPollutionMap(mapData);
            }
            else if ("XVAL".Equals(segmentName))
            {
                city.setPropertyValueMap(mapData);
            }
            else if ("XCRM".Equals(segmentName))
            {
                city.setCrimeMap(mapData);
            }

            return city;
        }

        private static City parseAndStoreCityName(City city, BinaryReader reader, int segmentLength)
        {
            // TODO: there is still some excess junk at the end of the city name, it begins with a "/0" (null character).
            
            byte nameLength = reader.ReadByte();
            string cityName = readString(reader, nameLength);

            if (nameLength < segmentLength - 1)
            {
                // Ignore padding at the end cityname.
                reader.ReadBytes(segmentLength - nameLength - 1);
            }

            return city;
        }

        private static City parseAndStoreMiscValues(City city, BinaryReader reader, int segmentLength)
        {
            // TODO: Still a lot of work to be done on this segment. Aka: we don't know what most of these numbers mean, and are just recording them.
            using (var decompressedReader = new BinaryReader(decompressSegment(reader, segmentLength)))
            {
                int decompressedLength = (int)decompressedReader.BaseStream.Length;
                Int32 miscValue;
                while (decompressedReader.BaseStream.Position < decompressedLength)
                {
                    miscValue = readInt32(decompressedReader);
                    city.addMiscValue(miscValue);
                }
            }
            return city;
        }

        #endregion

        #region utility functions

        private static MemoryStream decompressSegment(BinaryReader compressed, int length)
        {
            // Data is compressed using a simple run-length encoding.
            var decompressed = new MemoryStream();
            var writer = new BinaryWriter(decompressed);
            int segmentPos = 0;
            byte chunkCode;
            byte repeatByte;

            while (segmentPos < length)
            {
                chunkCode = compressed.ReadByte();
                segmentPos++;

                // The chunkCode value determines how it should be interpreted...
                if (chunkCode < 127)
                {
                    // Chunk code describes how many (already not compressed) bytes are in this chunk.
                    writer.Write(compressed.ReadBytes(chunkCode));
                    segmentPos += chunkCode;
                }
                else
                {
                    // Chunk code describes how many times the following byte should be repeated.
                    // Subtract to obtain number of repetitions.
                    chunkCode -= 127;

                    repeatByte = compressed.ReadByte();
                    segmentPos++;
                    for (int i = 0; i < chunkCode; i++)
                    {
                        writer.Write(repeatByte);
                    }
                }
            }
            decompressed.Position = 0;
            return decompressed;
        }

        private static int toLittleEndian(int bigEndian)
        {
            // Convert from big-endian to little-endian.
            return IPAddress.HostToNetworkOrder(bigEndian);
        }

        private static string readString(BinaryReader reader, int length)
        {
            byte[] buffer = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(buffer);
        }

        private static Int32 readInt32(BinaryReader reader)
        {
            Int32 i = reader.ReadInt32();
            return BitConverter.IsLittleEndian ? toLittleEndian(i) : i;
        }

        #endregion
    }
}

