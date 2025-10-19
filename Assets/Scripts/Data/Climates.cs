using UnityEngine;

namespace HexTerrainPrototype.Data
{
    /// <summary>
    /// NOTE: This enum should be in order from coldest to hottest.
    /// </summary>
    public enum Climates
    {
        Any = 0, // This is just used as a default value when a climate isn't selected yet.
        Snowy,
        Temperate,
        Tropical,
        Arid,
    }
}
