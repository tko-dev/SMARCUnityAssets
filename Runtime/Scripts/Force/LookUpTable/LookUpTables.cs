using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Force.LookUpTable
{
    [Serializable]
    public class LookUpTables
    {
        public Double[][] xcp;
        public Double[][] cz;
        public Double[][] cy;
        public Double[][] cx;

        public static LookUpTables CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<LookUpTables>(jsonString);
        }
    }
}