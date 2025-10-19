using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HexTerrainPrototype.Data
{
    [CreateAssetMenu(fileName = "TileTypeData", menuName = "Scriptable Objects/Tile Type Data")]
    public class TileTypeDataSO : ScriptableObject
    {
        [Tooltip("The climate of this tile type.")]
        public Climates Climate;
        [Tooltip("The terrain type of this tile type.")]
        public TerrainTypes TerrainType;

        [Tooltip("A list of the visuals that can be used for this tile type when placing it in the tile map. For each tile of this tile type, one of these is selected at random. NOTE: If the tile sprite appears way too big or small in the map, make sure in the sprite's import settings that the Pixels Per Unit option is set to the largest dimension of the sprite.")]
        [SerializeField]
        private List<HexTile> TileVisuals;

        [Tooltip("Specifies which visual in the TileVisuals list to use for this tile type. The default value is -1, which means one is chosen at random for every tile of this type.")]
        public int TileVisualIndex = -1;
        
        [Tooltip("These are the rules that help determine what can generate next to this tile type. Note that all tile types have a weight of 1f by default so everything has an equal chance to spawn. In this list, you add any tile types that have a higher or lower chance than that. For example, a tile with +30% chance to spawn next to this one, should have its weight set to 1.3 (aka 130%). Also note that you can set TerrainType to \"None\" to make a rule that only applies to a certain climate. Likewise, setting Climate to \"None\" will make a rule that only applies to a certain terrain type. Finally, setting the Weight value of a rule to 0 will make it so the specified terrain and//or climate can never spawn next to this tile type. This weight value is multiplied into the default terrain gen probability values for the tile being generated.")]
        public List<TerrainGenRule> NeighborGenerationRules;
        
        
        public int TileVisualsCount { get { return TileVisuals.Count; } }

        public HexTile GetTileVisual()
        {
            return TileVisualIndex >= 0 ? TileVisuals[TileVisualIndex]
                                        : TileVisuals[Random.Range(0, TileVisuals.Count)];
        }
    }
}
