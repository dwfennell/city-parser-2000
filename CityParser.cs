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
        
        // Binary segments describing city maps which contain solely integer values.
        private static readonly HashSet<string> integerMaps = new HashSet<string> { "XPLC", "XFIR", "XPOP", "XROG", "XTRF", "XPLT", "XVAL", "XCRM" };

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

        // Of the 1200 misc integer stats in the "MISC" segment, we know what these ones mean.
        private static readonly Dictionary<City.MiscStatistic, int> miscStatMap = new Dictionary<City.MiscStatistic, int>()
        {
            {City.MiscStatistic.AerospaceDemand, 109},
            {City.MiscStatistic.AerospaceRatio, 111},
            {City.MiscStatistic.AerospaceTaxRate, 110},
            {City.MiscStatistic.AutomotiveDemand, 106},
            {City.MiscStatistic.AutomotiveRatio, 108},
            {City.MiscStatistic.AutomotiveTaxRate, 107},
            {City.MiscStatistic.AvailableFunds, 5},
            {City.MiscStatistic.CitySize, 1035},
            {City.MiscStatistic.ConstructionDemand, 103},
            {City.MiscStatistic.ConstructionRatio, 105},
            {City.MiscStatistic.ConstructionTaxRate, 104},
            {City.MiscStatistic.DaysSinceFounding, 4},
            {City.MiscStatistic.EducationQuotent, 19},
            {City.MiscStatistic.ElectronicsDemand, 118},
            {City.MiscStatistic.ElectronicsRatio, 120},
            {City.MiscStatistic.ElectronicsTaxRate, 119},
            {City.MiscStatistic.FinanceDemand, 112},
            {City.MiscStatistic.FinanceRatio, 114},
            {City.MiscStatistic.FinanceTaxRate, 113},
            {City.MiscStatistic.FoodDemand, 100},
            {City.MiscStatistic.FoodRatio, 102},
            {City.MiscStatistic.FoodTaxRate, 101},
            {City.MiscStatistic.LifeExpectancy, 18},
            {City.MiscStatistic.MediaDemand, 115},
            {City.MiscStatistic.MediaRatio, 117},
            {City.MiscStatistic.MediaTaxRate, 116},
            {City.MiscStatistic.PetrochemcalRatio, 99},
            {City.MiscStatistic.PetrochemicalDemand, 97},
            {City.MiscStatistic.PetrochemicalTaxRate, 98},
            {City.MiscStatistic.SteelMiningDemand, 91},
            {City.MiscStatistic.SteelMiningRatio, 93},
            {City.MiscStatistic.SteelMiningTaxRate, 92},
            {City.MiscStatistic.TextilesDemand, 94},
            {City.MiscStatistic.TextilesRatio, 96},
            {City.MiscStatistic.TextilesTaxRate, 95},
            {City.MiscStatistic.TourismDemand, 121},
            {City.MiscStatistic.TourismRatio, 123},
            {City.MiscStatistic.TourismTaxRate, 122},
            {City.MiscStatistic.WorkforcePercentage, 17},
            {City.MiscStatistic.YearOfFounding, 3},
            {City.MiscStatistic.NeighborSize1, 439},
            {City.MiscStatistic.NeighborSize2, 443},
            {City.MiscStatistic.NeighborSize3, 447},
            {City.MiscStatistic.NeighborSize4, 451}
        };

        #endregion

        #region constructors 

        /// <summary>
        ///  The <c>CityParser</c> type reads and interprets SC2 (Sim City 2000) files.
        /// </summary>
        public CityParser () 
        {
            tileIterator = new Utility.CityTileIterator(City.TilesPerSide);
        }

        #endregion

        #region local variables

        private Utility.CityTileIterator tileIterator;

        #endregion

        #region parsing and storage

        /// <summary>
        ///   Parses binary data from <paramref name="binaryFilename"/> and stores it in a <see cref="City"/> object.
        /// </summary>
        /// <param name="inputStream">A stream containing binary data from a SC2 format file.</param>
        /// <param name="doQuickParse">If true only essencial data is parsed.</param>
        /// <returns></returns>
        public City ParseCityFile(Stream inputStream, bool doQuickParse)
        {
            var city = new City();

            if (doQuickParse)
            {
                // Do fast parse.
                city = parseCityFileFast(inputStream, city);
            }
            else
            {
                // Do full parse. Gathers all information.
                city = parseCityFileFull(inputStream, city);
            }

            return city;
        }

        /// <summary>
        ///   Parses binary data from <paramref name="binaryFilename"/> and stores it in a <see cref="City"/> object.
        /// </summary>
        /// <param name="inputStream">A stream containing binary data from a SC2 format file.</param>
        /// <returns>A <see cref="City"/> instance reflecting data from <paramref name="inputStream"/></returns>
        public City ParseCityFile(Stream inputStream)
        {
            return ParseCityFile(inputStream, false);
        }

        private City parseCityFileFast(Stream inputStream, City city)
        {
            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                // Skip 12-byte header.
                reader.BaseStream.Position += 12;

                string segmentName;
                Int32 segmentLength;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    segmentName = readString(reader, 4);
                    segmentLength = readInt32(reader);

                    if ("CNAM".Equals(segmentName))
                    {
                        city = parseCityName(city, reader, segmentLength);
                    }
                    else if ("MISC".Equals(segmentName))
                    {
                        city = parseMiscValues(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("XLAB".Equals(segmentName))
                    {
                        city = parse256Labels(city, getDecompressedReader(reader, segmentLength), true);
                    }
                    else
                    {
                        // Skip segment.
                        reader.BaseStream.Position += segmentLength;
                    }
                }
            }

            return city;
        }

        private City parseCityFileFull(Stream inputStream, City city)
        {
            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                // Read 12-byte header. 
                reader.BaseStream.Position += 12;

                // The rest of the file is divided into segments.
                // Each segment begins with a 4-byte segment name, followed by a 32-bit integer segment length.
                // Most segments are compressed using a simple run-length compression scheme, and must be 
                //  decompressed before they can be parsed correctly.
                string segmentName;
                Int32 segmentLength;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Parse segment data and store it in a City object. 
                    segmentName = readString(reader, 4);
                    segmentLength = readInt32(reader);

                    if ("CNAM".Equals(segmentName))
                    {
                        // City name (uncompressed).
                        city = parseCityName(city, reader, segmentLength);
                    }
                    else if ("MISC".Equals(segmentName))
                    {
                        // MISC contains a series of 32-bit integers.
                        city = parseMiscValues(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("ALTM".Equals(segmentName))
                    {
                        // Altitude map. (Not compressed)
                        city = parseAltitudeMap(city, reader, segmentLength);
                    }
                    else if ("XTER".Equals(segmentName))
                    {
                        // Terrain slope map. 
                        // Ignore for now. 
                        reader.BaseStream.Position += segmentLength;
                    }
                    else if ("XBLD".Equals(segmentName))
                    {
                        // Buildings map.
                        city = parseBuildingMap(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("XZON".Equals(segmentName))
                    {
                        // Zoning map (also specifies building corners).
                        city = parseZoningMap(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("XUND".Equals(segmentName))
                    {
                        // Underground structures map.
                        city = parseUndergroundMap(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("XTXT".Equals(segmentName))
                    {
                        // Sign information, of some sort. 
                        // Ignore for now. 
                        reader.BaseStream.Position += segmentLength;
                    }
                    else if ("XLAB".Equals(segmentName))
                    {
                        // 256 Labels. Mayor's name, then sign text.
                        city = parse256Labels(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if ("XMIC".Equals(segmentName))
                    {
                        // Microcontroller info.
                        // Ignore for now. 
                        reader.BaseStream.Position += segmentLength;
                    }
                    else if ("XTHG".Equals(segmentName))
                    {
                        // Segment contents unknown.
                        // Ignore for now. 
                        reader.BaseStream.Position += segmentLength;
                    }
                    else if ("XBIT".Equals(segmentName))
                    {
                        // One byte of flags for each city tile.
                        city = parseBinaryFlagMap(city, getDecompressedReader(reader, segmentLength));
                    }
                    else if (integerMaps.Contains(segmentName))
                    {
                        // Data in these segments are represented by integer values ONLY.
                        city = parseIntegerMap(city, segmentName, getDecompressedReader(reader, segmentLength));
                    }
                    else
                    {
                        // Unknown segment, ignore.
                        reader.BaseStream.Position += segmentLength;
                    }
                }
            }

            return city;
        }

        #region complex city map parsers

        private City parseBinaryFlagMap(City city, BinaryReader segmentReader)
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

                tileIterator.IncrementCurrentTile();
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseUndergroundMap(City city, BinaryReader segmentReader)
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

                tileIterator.IncrementCurrentTile();
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseZoningMap(City city, BinaryReader segmentReader)
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

                tileIterator.IncrementCurrentTile();
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseBuildingMap(City city, BinaryReader segmentReader)
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

                tileIterator.IncrementCurrentTile();
            }

            segmentReader.Dispose();
            return city;
        }

        private City parseAltitudeMap(City city, BinaryReader reader, int segmentLength)
        {
            // Altitude map.
            // This segment is NOT compressed. 
            // Each square gets two bytes.

            byte byteOne;
            byte byteTwo;
            int altitude;
            // b00011111. Altitude is stored in bits 0-4.
            byte altitudeMask = 31;

            List<int> mapData = new List<int>();
            long readerStopPosition = reader.BaseStream.Position + segmentLength;
            while (reader.BaseStream.Position < readerStopPosition)
            {
                // Don't do anything with the first byte (at least for now).
                byteOne = reader.ReadByte();
                byteTwo = reader.ReadByte();

                // In SC2000 the minimum altitude is 50 and the maximum is 3150, thus the 50's below.
                altitude = ((altitudeMask & byteTwo) * 50) + 50;
                mapData.Add(altitude);
            }

            city.SetMap(City.Map.Altitude, mapData);
            return city;
        }

        #endregion

        private City parseIntegerMap(City city, string segmentName, BinaryReader segmentReader)
        {
            List<int> mapData = new List<int>();

            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                mapData.Add((int)segmentReader.ReadByte());
            }

            city = storeIntegerMapData(city, mapData, segmentName);
            return city;
        }

        private City storeIntegerMapData(City city, List<int> mapData, string segmentName)
        {
            if ("XTRF".Equals(segmentName))
            {
                city.SetMap(City.Map.Traffic, mapData);
            }
            else if ("XPLT".Equals(segmentName))
            {
                city.SetMap(City.Map.Pollution, mapData);
            }
            else if ("XVAL".Equals(segmentName))
            {
                city.SetMap(City.Map.PropertyValue, mapData);
            }
            else if ("XCRM".Equals(segmentName))
            {
                city.SetMap(City.Map.Crime, mapData);
            }
            else if ("XPLC".Equals(segmentName))
            {
                city.SetMap(City.Map.PolicePower, mapData);
            }
            else if ("XFIR".Equals(segmentName))
            {
                city.SetMap(City.Map.FirePower, mapData);
            }
            else if ("XPOP".Equals(segmentName))
            {
                city.SetMap(City.Map.PopulationDensity, mapData);
            }
            else if ("XROG".Equals(segmentName))
            {
                city.SetMap(City.Map.PopulationGrowth, mapData);
            }        

            return city;
        }

        private City parseCityName(City city, BinaryReader reader, int segmentLength)
        {
            byte nameLength = reader.ReadByte();
            string cityName = readString(reader, nameLength);

            // Remove garbage characters that are at the end of the name.
            int gibbrishStart = cityName.IndexOf("\0");
            if (gibbrishStart >= 0)
            {
                cityName = cityName.Remove(gibbrishStart);
            }

            // City name is possibly padded. Ignore this padding.
            // NOTE: I yet to see a case where there actually is padding. I believe this is unrelated to the gibberish removal above, but I could be wrong.
            if (nameLength < segmentLength - 1)
            {
                reader.ReadBytes(segmentLength - nameLength - 1);
            }

            city.CityName = cityName;
            return city;
        }

        private City parseMiscValues(City city, BinaryReader segmentReader)
        {
            // The MISC segment contains ~1200 integer values.

            // TODO: Still a lot of work to be done on this segment. Aka: we don't know what most of these numbers mean, and are just recording them.
            List<int> miscValues = new List<int>();
            Int32 miscValue;
            while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
            {
                miscValue = readInt32(segmentReader);
                miscValues.Add(miscValue);
            }

            // Store values which we have identified.
            foreach (KeyValuePair<City.MiscStatistic, int> pair in miscStatMap)
            {
                city.SetMiscStatistic(pair.Key, miscValues[pair.Value]);
            }

            segmentReader.Dispose();
            return city;
        }

        private City parse256Labels(City city, BinaryReader segmentReader, bool mayorNameOnly)
        {
            // This segment describes 256 strings. String 0 is the mayor's name, the remaining are text from user-generated signs in the city.

            int labelLength;
            string label;
            const int maxLabelLength = 24;

            // Parse mayor's name.
            labelLength = segmentReader.ReadByte();
            label = readString(segmentReader, labelLength);
            if (maxLabelLength - labelLength > 0)
            {
                segmentReader.ReadBytes(maxLabelLength - labelLength);
            }
            city.MayorName = label;

            if (!mayorNameOnly)
            {
                while (segmentReader.BaseStream.Position < segmentReader.BaseStream.Length)
                {
                    // Parse sign-text strings.

                    // Each string is 24 bytes long, and is preceded by a 1-byte count. 
                    labelLength = segmentReader.ReadByte();
                    label = readString(segmentReader, labelLength);
                    city.AddSignText(label);

                    // Advance past any padding to next label.
                    if (maxLabelLength - labelLength > 0)
                    {
                        segmentReader.ReadBytes(maxLabelLength - labelLength);
                    }
                }
            }

            segmentReader.Dispose();
            return city;

        }

        private City parse256Labels(City city, BinaryReader segmentReader)
        {
            return parse256Labels(city, segmentReader, false);
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

        private bool hasCorner(byte b, byte cornerMask)
        {
            return (b & cornerMask) == (byte)1;
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

        #endregion
    }
}

