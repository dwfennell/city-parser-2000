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
        private static readonly HashSet<string> integerMaps = new HashSet<string> { "XPLC", "XFIR", "XPOP", "XROG", "XTRF", "XPLT", "XVAL", "XCRM" };

        // Binary segments describing city maps in which the byte data is uniqure to each segment.
        private static readonly HashSet<string> complexMaps = new HashSet<string> { "XTER", "XBLD", "XZON", "XUND", "XTXT", "XBIT", "ALTM" };

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

        #region constructors 
        
        static void Main()
        {
            CityParser parser = new CityParser();

            City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\dustropolis.sc2");
            //City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\new city.sc2");
            //City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\dustropolis.sc2");
            //City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\altTest2.sc2");
            //City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\zoneTest.sc2");
            //City ourCity = parser.ParseBinaryFile("C:\\Users\\Owner\\Desktop\\CitiesSC2000\\underground_test.sc2");
        }

        private CityParser () 
        {
            tileIterator = new Utility.CityTileIterator(City.TilesPerSide);
        }

        #endregion

        #region local variables

        private Utility.CityTileIterator tileIterator;

        #endregion

        #region parsing and storage

        public City ParseBinaryFile(string binaryFilename)
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

        private City parseAndStoreComplexMap(City city, BinaryReader reader, string segmentName, int segmentLength)
        {
            if ("XBIT".Equals(segmentName))
            {
                city = parseAndStoreXbitMap(city, getDecompressedReader(reader, segmentLength));
            }
            else if ("XBLD".Equals(segmentName))
            {
                city = parseAndStoreXbldMap(city, getDecompressedReader(reader, segmentLength));
            }
            else if ("XUND".Equals(segmentName))
            {
                city = parseAndStoreXundMap(city, getDecompressedReader(reader, segmentLength));
            }
            else if ("XZON".Equals(segmentName))
            {
                city = parseAndStoreXzonMap(city, getDecompressedReader(reader, segmentLength));
            }
            else if ("ALTM".Equals(segmentName))
            {
                // Altitude map. (Not compressed)
                city = parseAndStoreAltmMap(city, reader, segmentLength);
            }
            else
            {
                // TODO: Segment parsing not yet implemented. 
                reader.ReadBytes(segmentLength);
            }

            return city;
        }

        private City parseAndStoreXbitMap(City city, BinaryReader segmentReader)
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

            tileIterator.Reset();
            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                // TODO: Possible bug. Test data "new city.sc2" does not seem to be decompressing this segment correctly.
                tileByte = segmentReader.ReadByte();

                saltyFlag = (tileByte & saltyMask) != 0;
                waterCoveredFlag = (tileByte & waterCoveredMask) != 0;
                waterSuppliedFlag = (tileByte & waterSuppliedMask) != 0;
                pipedFlag = (tileByte & pipedMask) != 0;
                poweredFlag = (tileByte & poweredMask) != 0;
                conductiveFlag = (tileByte & conductiveMask) != 0;

                city.SetTileFlags(tileIterator.X, tileIterator.Y, saltyFlag, waterCoveredFlag, waterSuppliedFlag, pipedFlag, poweredFlag, conductiveFlag);

                // Update tile coodinates.
                if (!tileIterator.IncrementCurrentTile())
                {
                    // Error: Incremented past last tile.
                    // TODO: Throw exception.
                }
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseAndStoreXundMap(City city, BinaryReader segmentReader)
        {
            // Parse XUND segment.
            // This segment indicates what exists underground in each tile, given by a one-byte integer code.

            undergroundCode tileCode;
            tileIterator.Reset();

            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                tileCode = (undergroundCode) segmentReader.ReadByte();

                switch (tileCode)
                {
                    case undergroundCode.nothing:
                        // This tile doesn't have anything under the ground.
                        break;
                    case undergroundCode.pipeAndSubway1:
                    case undergroundCode.pipeAndSubway2:
                        city.SetUndergroundItem(tileIterator.X, tileIterator.Y, City.UndergroundItem.SubwayAndPipe);
                        break;
                    case undergroundCode.subwayStationOrSubRail:
                        city.SetUndergroundItem(tileIterator.X, tileIterator.Y, City.UndergroundItem.SubwayStation);
                        break;
                    case undergroundCode.tunnel1:
                    case undergroundCode.tunnel2:
                        // NOTE: These codes appear to have not been used... nor does there appear to be any underground code at all for tunnels. 
                        //  Perhaps these codes were meant to be tunnels but were never implemented as such, or possibly these codes indicate some other non-tunnel underground object.
                        // TODO: Log if we ever get here? 
                        city.SetUndergroundItem(tileIterator.X, tileIterator.Y, City.UndergroundItem.Tunnel);
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
                        city.SetUndergroundItem(tileIterator.X, tileIterator.Y, City.UndergroundItem.Subway);
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
                        city.SetUndergroundItem(tileIterator.X, tileIterator.Y, City.UndergroundItem.Pipe);
                        break;
                    default:
                        // Note: Hex codes over 0x23 are likely unused, but if they are used we would end up here.
                        break;
                }

                // Update tile coodinates.
                if (!tileIterator.IncrementCurrentTile())
                {
                    // Error: Incremented past last tile.
                    // TODO: Throw exception.
                }
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseAndStoreXzonMap(City city, BinaryReader segmentReader)
        {
            // Parse zoning and "building corner" information.

            // b00001111. The zone information is encoded in bits 0-3
            byte zoneMask = 15;
            // b0001000. Set if building has a corner in the 'top right'.
            byte cornerMask1 = 16;
            // b00100000. Set if building has a corner in the 'bottom right'.
            byte cornerMask2 = 32;
            // b01000000. Set if building has a corner in the 'bottom left'.
            byte cornerMask3 = 64;
            // b10000000. Set if building has a corner in the 'top left'.
            byte cornerMask4 = 128;
            zoneCode tileZoneCode;

            tileIterator.Reset();
            byte rawByte;
            
            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                rawByte = segmentReader.ReadByte();

                // A little bit-wise arithmetic to extract our 4-bit zone code.
                tileZoneCode = (zoneCode) (rawByte & zoneMask);

                switch (tileZoneCode)
                {
                    case zoneCode.lightResidential:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.LightResidential);
                        break;
                    case zoneCode.denseResidential:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.DenseResidential);
                        break;
                    case zoneCode.lightCommercial:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.LightCommercial);
                        break;
                    case zoneCode.denseCommercial:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.DenseCommercial);
                        break;
                    case zoneCode.lightIndustrial:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.LightIndustrial);
                        break;
                    case zoneCode.denseIndustrial:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.DenseIndustrial);
                        break;
                    case zoneCode.military:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.MilitaryBase);
                        break;
                    case zoneCode.airport:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.Airport);
                        break;
                    case zoneCode.seaport:
                        city.SetZone(tileIterator.X, tileIterator.Y, City.Zone.Seaport);
                        break;
                }

                if (hasCorner(rawByte, cornerMask1))
                {
                    city.SetBuildingCorner(tileIterator.X, tileIterator.Y, Building.CornerCode.TopRight);
                }
                if (hasCorner(rawByte, cornerMask2))
                {
                    city.SetBuildingCorner(tileIterator.X, tileIterator.Y, Building.CornerCode.BottomRight);
                }
                if (hasCorner(rawByte, cornerMask3))
                {
                    city.SetBuildingCorner(tileIterator.X, tileIterator.Y, Building.CornerCode.BottomLeft);
                }
                if (hasCorner(rawByte, cornerMask4))
                {
                    city.SetBuildingCorner(tileIterator.X, tileIterator.Y, Building.CornerCode.TopLeft);
                }

                // Update tile coodinates.
                if (!tileIterator.IncrementCurrentTile())
                {
                    // Error: Incremented past last tile.
                    // TODO: Throw exception.
                }
            }

            segmentReader.Dispose();
            return city;
        }

        private bool hasCorner(byte b, byte cornerMask) 
        {
            return (b & cornerMask) == (byte) 1;
        }

        private City parseAndStoreXbldMap(City city, BinaryReader segmentReader)
        {
            // This segment indicates what is above ground in each square.

            // TODO: Shouldn't be relying on "Building.BuildingCode" order like this. BAD.
            tileIterator.Reset();
            byte rawByte;
            Building.BuildingCode buildingCode;
            
            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                // This map contains on 'building code' for each square. 
                // The building code is a one-byte integer value.
                rawByte = segmentReader.ReadByte();
                buildingCode = (Building.BuildingCode) rawByte;
                city.SetBuilding(tileIterator.X, tileIterator.Y, buildingCode);

                // Update tile coodinates.
                if (!tileIterator.IncrementCurrentTile())
                {
                    // Error: Incremented past last tile.
                    // TODO: Throw exception.
                }
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseAndStoreAltmMap(City city, BinaryReader reader, int segmentLength)
        {
            // Altitude map.
            // This segment is NOT compressed. 
            // Each square gets two bytes.

            byte byteOne;
            byte byteTwo;
            int altitude;
            // b00011111. Altitude is stored in bits 0-4.
            byte altitudeMask = 31;

            tileIterator.Reset();
            long readerStopPosition = reader.BaseStream.Position + segmentLength;
            while (reader.BaseStream.Position < readerStopPosition)
            {
                // Don't do anything with the first byte (at least for now).
                byteOne = reader.ReadByte();
                byteTwo = reader.ReadByte();

                // In SC2000 the minimum altitude is 50 and the maximum is 3150, thus the 50's below.
                altitude = ((altitudeMask & byteTwo) * 50) + 50;
                city.setAltitude(tileIterator.X, tileIterator.Y, altitude);

                // Update tile coodinates.
                if (!tileIterator.IncrementCurrentTile())
                {
                    // Error: Incremented past last tile.
                    // TODO: Throw exception.
                }
            }
            return city;
        }

        #endregion

        private List<int> parseIntegerMap(BinaryReader reader, int segmentLength)
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

        private City storeIntegerMapData(City city, List<int> mapData, string segmentName)
        {
            if ("XPLC".Equals(segmentName))
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

        private City parseAndStoreCityName(City city, BinaryReader reader, int segmentLength)
        {
            // TODO: there is still some excess junk at the end of the city name, it begins with a "/0" (null character).
            
            byte nameLength = reader.ReadByte();
            string cityName = readString(reader, nameLength);

            if (nameLength < segmentLength - 1)
            {
                // Ignore padding at the end cityname.
                reader.ReadBytes(segmentLength - nameLength - 1);
            }

            city.CityName = cityName;
            return city;
        }

        private City parseAndStoreMiscValues(City city, BinaryReader reader, int segmentLength)
        {
            // The MISC segment contains ~1200 integer values.

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

        private MemoryStream decompressSegment(BinaryReader compressed, int compressedLength)
        {
            // Data is compressed using a simple run-length encoding.
            var decompressed = new MemoryStream();
            var writer = new BinaryWriter(decompressed);
            int segmentPos = 0;
            byte chunkCode;
            byte repeatByte;

            while (segmentPos < compressedLength)
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

        private BinaryReader getDecompressedReader(BinaryReader compressedReader, int compressedLength)
        {
            return new BinaryReader(decompressSegment(compressedReader, compressedLength));
        }

        private string readString(BinaryReader reader, int length)
        {
            byte[] buffer = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(buffer);
        }

        private Int16 readInt16(BinaryReader reader)
        {
            Int16 i = reader.ReadInt16();
            return BitConverter.IsLittleEndian ? toLittleEndian(i) : i;
        }

        private Int32 readInt32(BinaryReader reader)
        {
            Int32 i = reader.ReadInt32();
            return BitConverter.IsLittleEndian ? toLittleEndian(i) : i;
        }

        private Int32 toLittleEndian(Int32 bigEndian)
        {
            // IPAddress happens to have a function what does what we want.
            // This obviously has nothing to do with networking.
            return IPAddress.HostToNetworkOrder(bigEndian);
        }

        private Int16 toLittleEndian(Int16 bigEndian)
        {
            // IPAddress happens to have a function what does what we want.
            // This obviously has nothing to do with networking.
            return IPAddress.HostToNetworkOrder(bigEndian);
        }

        private byte toLittleEndian(byte bigEndian)
        {
            return ReverseWithLookupTable(bigEndian);
        }

        private static readonly byte[] BitReverseTable =
        {
            0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0,
            0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0,
            0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8,
            0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8,
            0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4,
            0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4,
            0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec,
            0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc,
            0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2,
            0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2,
            0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea,
            0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa,
            0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6,
            0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6,
            0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee,
            0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe,
            0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1,
            0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1,
            0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9,
            0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9,
            0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5,
            0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5,
            0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed,
            0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd,
            0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3,
            0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3,
            0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb,
            0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb,
            0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7,
            0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7,
            0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef,
            0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff
        };

        private byte ReverseWithLookupTable(byte toReverse)
        {
            return BitReverseTable[toReverse];
        }


        #endregion
    }
}

