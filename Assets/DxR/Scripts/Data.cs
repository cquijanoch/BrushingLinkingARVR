using System.Collections.Generic;
using GeoJSON.Net.Geometry;

namespace DxR
{
    public class Data
    {
        public string url;
        public List<Dictionary<string, string>> values;
        public List<string> fieldNames;
        public List<List<List<IPosition>>> polygons; // List 1: Each element corresponds to a feature (i.e., a data row)
                                                     // List 2: Each element corresponds to a polygon (only 1 for polygons, >1 for multipolygons)
                                                     // List 3: Each element corresponds to a lat/long position in a specific order
        public List<IPosition> centres;              // Each element corresponds to the centrepoint of a feature
    }
}
