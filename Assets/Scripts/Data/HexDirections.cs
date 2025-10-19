using UnityEngine;

namespace HexTerrainPrototype
{
    /// <summary>
    /// These are the hex directions for flat-top hex grids.
    /// </summary>
    public enum HexDirections_FlatTop
    {
        NORTH = 0,
        NORTH_EAST,
        SOUTH_EAST,
        SOUTH,
        SOUTH_WEST,
        NORTH_WEST
    }
    
    /// <summary>
    /// These are the hex directions for pointy-top hex grids.
    /// </summary>
    public enum HexDirections_PointyTop
    {
        NORTH_EAST = 0,
        EAST,
        SOUTH_EAST,
        SOUTH_WEST,
        WEST,
        NORTH_WEST
    }
    
}
