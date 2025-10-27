using UnityEngine;

namespace HexTerrainPrototype
{
    public static class HexDirections
    {
        /// <summary>
        /// These are the hex directions for flat-top hex grids.
        /// </summary>
        public enum Directions_FlatTop
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
        public enum Directions_PointyTop
        {
            NORTH_EAST = 0,
            EAST,
            SOUTH_EAST,
            SOUTH_WEST,
            WEST,
            NORTH_WEST
        }
        
        
        // Define neighbor offsets for odd rows in a pointy top hex grid.
        // The offsets depend on whether the row (y) is even or odd.
        public static readonly Vector3Int[] OddRowNeighbors_FlatTop = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0),  // NORTH_EAST
            new Vector3Int( 1,  1, 0),  // EAST
            new Vector3Int( 0,  1, 0),  // SOUTH_EAST       
            new Vector3Int(-1,  0, 0),  // SOUTH_WEST
            new Vector3Int( 0, -1, 0),  // WEST
            new Vector3Int( 1, -1, 0),  // NORTH_WEST
        };

        // Define neighbor offsets for even rows in a pointy top hex grid.
        // The offsets depend on whether the row (y) is even or odd.
        public static readonly Vector3Int[] EvenRowNeighbors_FlatTop = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0),  // NORTH_EAST
            new Vector3Int( 0,  1, 0),  // EAST
            new Vector3Int(-1,  1, 0),  // SOUTH_EAST
            new Vector3Int(-1,  0, 0),  // SOUTH_WEST
            new Vector3Int(-1, -1, 0),  // WEST
            new Vector3Int( 0, -1, 0),  // NORTH_WEST
        };
        
        // Define neighbor offsets for odd rows in a flat top hex grid.
        // The offsets depend on whether the row (y) is even or odd.
        public static readonly Vector3Int[] OddRowNeighbors_PointyTop = new Vector3Int[]
        {
            new Vector3Int( 1,  1, 0),  // NORTH_EAST
            new Vector3Int( 1,  0, 0),  // EAST
            new Vector3Int( 1, -1, 0),  // SOUTH_EAST       
            new Vector3Int( 0, -1, 0),  // SOUTH_WEST
            new Vector3Int(-1,  0, 0),  // WEST
            new Vector3Int( 0,  1, 0),  // NORTH_WEST
        };

        // Define neighbor offsets for even rows in a flat top hex grid.
        // The offsets depend on whether the row (y) is even or odd.
        public static readonly Vector3Int[] EvenRowNeighbors_PointyTop = new Vector3Int[]
        {
            new Vector3Int( 0,  1, 0),  // NORTH_EAST
            new Vector3Int( 1,  0, 0),  // EAST
            new Vector3Int( 0, -1, 0),  // SOUTH_EAST
            new Vector3Int(-1, -1, 0),  // SOUTH_WEST
            new Vector3Int(-1,  0, 0),  // WEST
            new Vector3Int(-1,  1, 0),  // NORTH_WEST
        };        
        
     
        
        public static Vector3Int GetNeighborPos_FlatTop(Vector3Int tilePos, Directions_FlatTop direction)
        {
            Vector3Int result = Vector3Int.zero;

            //Debug.Log($"IS_EVEN (FLAT-TOP): {(tilePos.y % 2 == 0)}    {tilePos}");
            if (tilePos.y % 2 == 0)
                return tilePos + EvenRowNeighbors_FlatTop[(int) direction];
            else
                return tilePos + OddRowNeighbors_FlatTop[(int) direction];
        }
        
        public static Vector3Int GetNeighborPos_PointyTop(Vector3Int tilePos, Directions_PointyTop direction)
        {
            Vector3Int result = Vector3Int.zero;

            //Debug.Log($"IS_EVEN (POINTY-TOP): {(tilePos.y % 2 == 0)}    {tilePos}");
            if (tilePos.y % 2 == 0)
                return tilePos + EvenRowNeighbors_PointyTop[(int) direction];
            else
                return tilePos + OddRowNeighbors_PointyTop[(int) direction];
        }        
    }

}
