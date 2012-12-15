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
        /*
         * -- Notes on SC2 file parsing --
         * 
         * We have two main sections of the file: the header, and the body. 
         * 
         * The header is 12 bytes long. 
         *  1-4  : 'FORM' (in ASCII-compatable encoding). This is particular to IFF files.
         *  5-8  : Total number of bytes in the file, excluding the header.
         *  9-12 : File type "SCDH"
         *  
         * 
         * The body is in a series of segments. 
         *   1-4 : type of segment,
         *   5-8 : # of bytes in segment (except this 8 bytes)
         *   ... rest is data.
         *   
         * All but two segments are encoded using run-length encoding. 
         * Exceptions: The altitude map and the city name.
         * 
         * Run-length encoding:
         *   Two different 'chunks'.
         *   Chunk type 1 contains uncompressed data.
         *     The first byte will be less than 128 and represents the number of bytes in this chunk.
         *   Chunk type 2 describes compressed data. 
         *     The first byte will be between 129 and 255. Subtracting 127 from the first byte indicates how many times the second byte is to be repeated.
         *     (Chunk type 2 only ever contains 2 bytes)
         * 
         * The order of segments is known.
         * 
         * 
         * -- Strategy -- 
         * 
         * Read header.
         * Read segments, decompressing as necessary as we go.
         *   Automate this as much as possible... but need to be careful about uncompressed segments.
         * 
         * Separate segments and store in a dict-like data structure referenced by their segment codes.
         * 
         * Convert byte data into a more useable form... this will vary depending on segment.
         * 
         * 
         **/

        // Holds decompressed binary file input separated into its segments.
        private Dictionary<string, byte[]> rawDataSegments;

        public CityParser ()
        {
            
        }

        static void Main()
        {
            CityParser cp = new CityParser();
            cp.ParseBinaryFile("C:\\Users\\Owner\\Development\\Projects\\SimCityParser2000\\dustropolis.sc2");
        }

        public City ParseBinaryFile(string binaryFilename)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(binaryFilename, FileMode.Open)))
            {
                // Read (12 byte) header. 

                // Interchange File Format type.
                string iffType = readString(reader, 4);

                // Don't need: 32-bit integer remaining file length.
                reader.ReadBytes(4);
                var fileType = readString(reader, 4);

                // All Sim City 2000 files will have iffType "FORM" and fileType "SCDH".
                if (!iffType.Equals("FORM") || !fileType.Equals("SCDH"))
                {
                    // This is not a Sim City 2000 file.
                    // TODO: Throw an exception? Return blank city?
                    return new City();
                }

                // Finished with header. The rest of the file is divided into segments.

                // Read segment data.
                string segmentName;
                Int32 segmentLength;
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // All segments start with a name and length.
                    segmentName = readString(reader, 4);
                    segmentLength = readInt32(reader);

                    if ("CNAM".Equals(segmentName))
                    {
                        // Read city name. This section is not compressed.
                        
                        byte nameLength = reader.ReadByte();
                        string cityName = readString(reader, nameLength);
                        // The rest is padding. 
                        // NOTE: During testing the name length was '31' with segment length '32', while the city name was not that long. 
                        if (nameLength < segmentLength - 1)
                        {
                            reader.ReadBytes(segmentLength - nameLength - 1);
                        }
                    }
                    else if ("MISC".Equals(segmentName))
                    {
                        // MISC contains a series of 32-bit integers.

                        // Decompress segment, then parse it.
                        MemoryStream decompressedBinaryStream = decompressSegment(reader, segmentLength);
                        using (var decompressedReader = new BinaryReader(decompressedBinaryStream))
                        {
                            int decompressedLength = (int)decompressedReader.BaseStream.Length;

                            Int32 miscValue;
                            // TODO: Remove this.
                            var file = new StreamWriter("c:\\simcity\\misc_nums.txt");
                            while (decompressedReader.BaseStream.Position < decompressedLength)
                            {
                                miscValue = readInt32(decompressedReader);

                                // TODO: Remove this.
                                file.WriteLine(miscValue);
                            }
                            file.Close();
                        }
                    }
                    else
                    {
                        // Unknown segment, ignore.
                        Console.WriteLine("Skipping segment:");
                        Console.WriteLine(segmentName);

                        reader.ReadBytes(segmentLength);
                    }
                }
                reader.Close();
            }
            
            // TODO: Change return value;
            return new City();
        }

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
    }
}

