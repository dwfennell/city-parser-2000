using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CityParser2000
{
    class CityValidator
    {
        public static bool validate(Stream fileStream)
        {
            using (System.IO.BinaryReader reader = new BinaryReader(fileStream))
            {
                // Read 12-byte header. 

                // Check for ridiculous file lengths. 
                if (reader.BaseStream.Length <= 12)
                {
                    // Very small file is not a sc2 file (and will crash things later).
                    return false;
                }
                else if (reader.BaseStream.Length > 307200)
                {
                    // Over 300kb. Impossible for Sim City 2000 files?
                    return false;
                }

                // Confirm Interchange File Format (iff) filetype.
                string iffType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                reader.ReadBytes(4); // Unimportant bytes.
                var fileType = Encoding.ASCII.GetString(reader.ReadBytes(4));
                if (!iffType.Equals("FORM") || !fileType.Equals("SCDH"))
                {
                    // This is not a Sim City 2000 file.
                    return false;
                }

                return true;
            }
        }


    }
}
