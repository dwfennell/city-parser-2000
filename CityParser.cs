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

        // Binary codes that indicate what is underground in a tile. Multiples distinguish slope and direction.
        // Used when decoding XUND segment.
        private enum undergroundCode { 
            nothing = 0x00,
            subway1 = 0x01,
            subway2 = 0x02,
            subway3 = 0x03,
            subway4 = 0x04,
            subway5 = 0x05,
            subway6 = 0x06,
            subway7 = 0x07,
            subway8 = 0x08,
            subway9 = 0x09,
            subwayA = 0x0A,
            subwayB = 0x0B,
            subwayC = 0x0C,
            subwayD = 0x0D,
            subwayE = 0x0E,
            subwayF = 0x0F,
            pipe1 = 0x10,
            pipe2 = 0x11,
            pipe3 = 0x12,
            pipe4 = 0x13,
            pipe5 = 0x14,
            pipe6 = 0x15,
            pipe7 = 0x16,
            pipe8 = 0x17,
            pipe9 = 0x18,
            pipeA = 0x19,
            pipeB = 0x1A,
            pipeC = 0x1B,
            pipeD = 0x1C,
            pipeE = 0x1D,
            pipeF = 0x1E,
            pipeAndSubway1 = 0x1F,
            pipeAndSubway2 = 0x20,
            tunnel1 = 0x21,
            tunnel2 = 0x22,
            subwayStationOrSubRail = 0x23
        };

        // Zones. Order is important as this is used in decoding binary data.
        private enum zoneCode { none, lightResidential, denseResidential, lightCommercial, denseCommercial, lightIndustrial, denseIndustrial, military, airport, seaport };

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

        // TODO: It seems as though we could remove a fair amount of 
        //  repetition in the "parseAndStore<segment_name>Map" functions
        //  with some more advanced OO and/or a good design pattern.
        //  That seems like a very good idea for some later refactoring. -dustin

        private static City parseAndStoreComplexMap(City city, BinaryReader reader, string segmentName, int segmentLength)
        {
            if ("XBIT".Equals(segmentName))
            {
                city = parseAndStoreXbitMap(city, reader, segmentLength);
            }
            else if ("XUND".Equals(segmentName))
            {
                city = parseAndStoreXundMap(city, reader, segmentLength);
            }
            else if ("XZON".Equals(segmentName))
            {
                city = parseAndStoreXzonMap(city, reader, segmentLength);
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

        private static City parseAndStoreXundMap(City city, BinaryReader reader, int segmentLength)
        {
            // Parse XUND segment.
            // This segment indicates what exists underground in each tile, given by a one-byte integer code.

            undergroundCode tileCode;

            // Tile coordinates within the city.
            int xCoord = 0;
            int yCoord = 0;
            int citySideLength = City.TilesPerSide;

            using (var decompressedReader = new BinaryReader(decompressSegment(reader, segmentLength)))
            {
                while (decompressedReader.BaseStream.Position < decompressedReader.BaseStream.Length)
                {
                    tileCode = (undergroundCode)reader.ReadByte();

                    switch (tileCode)
                    {
                        case undergroundCode.nothing:
                            // This tile doesn't have anything under the ground.
                            break;
                        case undergroundCode.pipeAndSubway1:
                        case undergroundCode.pipeAndSubway2:
                            city.SetUndergroundItem(xCoord, yCoord, City.UndergroundItem.SubwayAndPipe);
                            break;
                        case undergroundCode.subwayStationOrSubRail:
                            city.SetUndergroundItem(xCoord, yCoord, City.UndergroundItem.SubwayStation);
                            break;
                        case undergroundCode.tunnel1:
                        case undergroundCode.tunnel2:
                            // TODO: confirm this code is actually for tunnels.
                            city.SetUndergroundItem(xCoord, yCoord, City.UndergroundItem.Tunnel);
                            break;
                        case undergroundCode.subway1:
                        case undergroundCode.subway2:
                        case undergroundCode.subway3:
                        case undergroundCode.subway4:
                        case undergroundCode.subway5:
                        case undergroundCode.subway6:
                        case undergroundCode.subway7:
                        case undergroundCode.subway8:
                        case undergroundCode.subway9:
                        case undergroundCode.subwayA:
                        case undergroundCode.subwayB:
                        case undergroundCode.subwayC:
                        case undergroundCode.subwayD:
                        case undergroundCode.subwayE:
                        case undergroundCode.subwayF:
                            city.SetUndergroundItem(xCoord, yCoord, City.UndergroundItem.Subway);
                            break;
                        case undergroundCode.pipe1:
                        case undergroundCode.pipe2:
                        case undergroundCode.pipe3:
                        case undergroundCode.pipe4:
                        case undergroundCode.pipe5:
                        case undergroundCode.pipe6:
                        case undergroundCode.pipe7:
                        case undergroundCode.pipe8:
                        case undergroundCode.pipe9:
                        case undergroundCode.pipeA:
                        case undergroundCode.pipeB:
                        case undergroundCode.pipeC:
                        case undergroundCode.pipeD:
                        case undergroundCode.pipeE:
                        case undergroundCode.pipeF:
                            city.SetUndergroundItem(xCoord, yCoord, City.UndergroundItem.Pipe);
                            break;
                        default:
                            // Note: Hex codes over 0x23 are likely unused, but if they are used we would end up here.
                            break;
                    }

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

        private static City parseAndStoreXzonMap(City city, BinaryReader reader, int segmentLength)
        {
            // Tile coordinates within the city.
            int xCoord = 0;
            int yCoord = 0;
            int citySideLength = City.TilesPerSide;

            // b00001111. The zone information is encoded in bits 0-3
            byte zoneMask = 15;
            byte rawByte;
            zoneCode tileCode;

            using (var decompressedReader = new BinaryReader(decompressSegment(reader, segmentLength)))
            {
                while (decompressedReader.BaseStream.Position < decompressedReader.BaseStream.Length)
                {
                    rawByte = reader.ReadByte();

                    // A little bit-wise arithmetic to extract our 4-bit zone code.
                    tileCode = (zoneCode)(rawByte & zoneMask);

                    switch (tileCode)
                    {
                        case zoneCode.lightResidential:
                            city.SetZone(xCoord, yCoord, City.Zone.LightResidential);
                            break;
                        case zoneCode.denseResidential:
                            city.SetZone(xCoord, yCoord, City.Zone.DenseResidential);
                            break;
                        case zoneCode.lightCommercial:
                            city.SetZone(xCoord, yCoord, City.Zone.LightCommercial);
                            break;
                        case zoneCode.denseCommercial:
                            city.SetZone(xCoord, yCoord, City.Zone.DenseCommercial);
                            break;
                        case zoneCode.lightIndustrial:
                            city.SetZone(xCoord, yCoord, City.Zone.LightIndustrial);
                            break;
                        case zoneCode.denseIndustrial:
                            city.SetZone(xCoord, yCoord, City.Zone.DenseIndustrial);
                            break;
                        case zoneCode.military:
                            city.SetZone(xCoord, yCoord, City.Zone.MilitaryBase);
                            break;
                        case zoneCode.airport:
                            city.SetZone(xCoord, yCoord, City.Zone.Airport);
                            break;
                        case zoneCode.seaport:
                            city.SetZone(xCoord, yCoord, City.Zone.Seaport);
                            break;
                    }

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
                city.SetPoliceMap(mapData);
            }
            else if ("XFIR".Equals(segmentName))
            {
                city.SetFirefighterMap(mapData);
            }
            else if ("XPOP".Equals(segmentName))
            {
                city.SetPopulationMap(mapData);
            }
            else if ("XROG".Equals(segmentName))
            {
                city.SetPopulationGrowthMap(mapData);
            }
            else if ("XTRF".Equals(segmentName))
            {
                city.SetTrafficMap(mapData);
            }
            else if ("XPLT".Equals(segmentName))
            {
                city.SetPollutionMap(mapData);
            }
            else if ("XVAL".Equals(segmentName))
            {
                city.SetPropertyValueMap(mapData);
            }
            else if ("XCRM".Equals(segmentName))
            {
                city.SetCrimeMap(mapData);
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
                    city.AddMiscValue(miscValue);
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

