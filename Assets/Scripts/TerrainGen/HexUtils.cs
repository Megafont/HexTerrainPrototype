using UnityEngine;
using UnityEngine.Tilemaps;

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
            // Determine which type of hex map this is.
            // NOTE: This comparison looks backwards at first, but it needs to be that way due to how hex maps work.
            return tileMap.cellSize.y > tileMap.cellSize.x ? HexMapStyles.FLAT_TOP
                                                           : HexMapStyles.POINTY_TOP;
        }

        public static Vector2 HexToWorldCoord(Tilemap tileMap, Vector3Int hexCoord)
        {
            // TODO: IMPLEMENT THIS FUNCTION
            return Vector2.zero;
        }

        public static Vector3Int WorldToHexCoord(Tilemap tileMap, Vector2 worldCoord)
        {
            // TODO: IMPLEMENT THIS FUNCTION
            return Vector3Int.zero;
        }
        
        public static bool HexCoordIsInMapBounds(Tilemap tileMap, Vector3Int hexPos)
        {
            return false;
        }
        
        public static bool TileHasBeenGenerated(HexTile tile)
        {
            Debug.Log($"{tile != null}    {(tile != null ? tile.TileTypeData != null : "false")}");
            return tile != null;
        }
        
    }
}
