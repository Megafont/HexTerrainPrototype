using HexTerrainPrototype.Data;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace HexTerrainPrototype
{
    [CreateAssetMenu(fileName = "CustomHexTile", menuName = "Tiles/Custom Hex Tile")]
    public class HexTile : Tile
    {
        [Tooltip("The TileTypeDataSO for this tile.")]
        public TileTypeDataSO TileTypeData;
        
        
        /// <summary>
        /// This field allows the tile to be rotated.
        /// </summary>
        [SerializeField]
        private Quaternion _Rotation = Quaternion.identity; // Default to no rotation

        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            // Get base tile data first
            base.GetTileData(position, tilemap, ref tileData);

            // Apply the custom rotation to the tile's transform matrix
            tileData.transform = Matrix4x4.TRS(Vector3.zero, _Rotation, Vector3.one);
        }

        
        /// <summary>
        /// Gets/sets the rotation of the tile in degrees.
        /// </summary>
        public float Rotation
        {
            get => _Rotation.eulerAngles.z;
            set => _Rotation = Quaternion.Euler(0, 0, value);
        }
    }
}
