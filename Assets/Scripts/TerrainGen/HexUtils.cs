using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace HexTerrainPrototype
{
    public static class HexUtils
    {
        public enum SwizzleTypes
        {
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX,
        }


        public static Vector3Int GetRandomTileCoord(int xMinInclusive, int xMaxInclusive, int yMinInclusive, int yMaxInclusive, int zMinInclusive = 0, int zMaxInclusive = 0)
        {
            return new Vector3Int(Random.Range(xMinInclusive, xMaxInclusive),
                                  Random.Range(yMinInclusive, yMaxInclusive),
                                  Random.Range(zMinInclusive, zMaxInclusive));
        }
        
        /// <summary>
        /// This utility function is an extension method for Vector3Int so you can call it directly on any Vector3Int variable.
        /// It reorders the components of the vector based on the swizzle type.
        /// </summary>
        /// <param name="vector">The vector to reorder.</param>
        /// <param name="swizzleType">The swizzle type.</param>
        /// <returns></returns>
        public static Vector3Int Swizzle(this Vector3Int vector, SwizzleTypes swizzleType)
        {
            switch (swizzleType)
            {
                case SwizzleTypes.XYZ:
                    // Same as return new Vector3(vector.x, vector.y, vector.z);
                    return vector;
                case SwizzleTypes.XZY:
                    return new Vector3Int(vector.x, vector.z, vector.y);
                case SwizzleTypes.YXZ:
                    return new Vector3Int(vector.y, vector.x, vector.z);
                case SwizzleTypes.YZX:
                    return new Vector3Int(vector.y, vector.z, vector.x);
                case SwizzleTypes.ZXY:
                    return new Vector3Int(vector.z, vector.x, vector.y);
                case SwizzleTypes.ZYX:
                    return new Vector3Int(vector.z, vector.y, vector.x);
                default:
                    return vector;
            }
        }

        public static HexMapStyles GetHexMapStyle(Tilemap tileMap)
        {
            // NOTE: This comparison looks backwards at first, but it needs to be that way due to how hex maps work.
            //return tileMap.cellSize.y > tileMap.cellSize.x ? HexMapStyles.FLAT_TOP
            //                                               : HexMapStyles.POINTY_TOP;
            
            // Determine which type of hex map this is.
            if (tileMap.cellSwizzle == GridLayout.CellSwizzle.YXZ)
                return HexMapStyles.FLAT_TOP;
            else if (tileMap.cellSwizzle == GridLayout.CellSwizzle.XYZ)
                return HexMapStyles.POINTY_TOP;
            else
                throw new Exception("Cannot determine the style of this hex tile map! It has an unknown tile map style.");
        }
        
        /// <summary>
        /// Checks if the specified tile has been generated or not.
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        /// <returns>True if this tile has already been generated.</returns>
        public static bool TileHasBeenGenerated(HexTile tile)
        {
            //Debug.Log($"{tile != null}    {(tile != null ? tile.TileTypeData != null : "False")}");
            return tile != null && tile.TileTypeData != null;
        }
        
    }
}
