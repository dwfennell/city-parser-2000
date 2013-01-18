using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CityParser2000
{
    /// <summary>
    /// The <c>CityValidator</c> class deterines if a file conforms to the .sc2 format.
    /// </summary>
    public class CityValidator
    {
        /// <summary>
        /// Determine if <paramref name="fileStream"/> represents valid sc2 data.
        /// </summary>
        /// <param name="fileStream">A stream which may represent valid sc2 data.</param>
        /// <returns>True if <paramref name="fileStream" represents valid sc2 filedata./></returns>
        public static bool validate(Stream fileStream)
        {
            using (System.IO.BinaryReader reader = new BinaryReader(fileStream, new UTF8Encoding(), true))
            {   
                bool result = true;

                // Check for ridiculous file lengths. 
                if (reader.BaseStream.Length <= 12)
                {
                    // Very small file is not a sc2 file (and will crash things later).
                    result = false;
                }
                else if (reader.BaseStream.Length > 307200)
                {
                    // Over 300kb. Impossible for sc2 files?
                    result = false;
                }
                else
                {
                    // Confirm Interchange File Format (iff) filetype.
                    
                    // Read 12-byte header.
                    string iffType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    reader.ReadBytes(4); // Unimportant bytes.
                    var fileType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    
                    if (!iffType.Equals("FORM") || !fileType.Equals("SCDH"))
                    {
                        // This is not a Sim City 2000 file.
                        result = false;
                    }

                    // Reset stream position so no one knows we were here.
                    reader.BaseStream.Position = 0;
                }

                return result;
            }
        }
    }
}
