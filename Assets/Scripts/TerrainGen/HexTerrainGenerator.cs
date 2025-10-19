using System;
using System.Collections.Generic;
using HexTerrainPrototype.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace HexTerrainPrototype
{
    public class HexTerrainGenerator : MonoBehaviour
    {
        [Header("General")]
        
        [Tooltip("The tile map to generate the random terrain in.")]
        [SerializeField]
        private Tilemap _HexTileMap;
        
        [Tooltip("The width of the tilemap to be generated.")]
        [SerializeField]
        [Min(1)]
        private int _HexTileMapWidth;

        [Tooltip("The height of the tilemap to be generated.")]
        [SerializeField]
        [Min(1)]
        private int _HexTileMapHeight;

        
        [Header("Seed Settings")]
        
        [Tooltip("If enabled, the current time will be used as the seed for the random number generator.")]
        [SerializeField]
        private bool _UseTimeAsSeed = true;
        
        [Tooltip("The seed to use in the random number generator when the UseRandomSeed option is disabled.")]
        [SerializeField]
        private int _Seed;
        
        [Space(10)]
        
        [Header("First Tile Settings")]
        
        [Tooltip("If enabled, the position of the first tile will be random.")]
        [SerializeField]
        private bool _UseRandomFirstTilePosition;
        
        [Tooltip("The X position for the first tile when the UseRandomFirstTilePosition option is disabled.")]
        [SerializeField]
        [Min(0)]
        private int _FirstTileXPos;

        [Tooltip("The Y position for the first tile when the UseRandomFirstTilePosition option is disabled.")]
        [SerializeField]
        [Min(0)]
        private int _FirstTileYPos;

        [Space(10)]
        
        [Tooltip("If enabled, the type of the first tile will be selected at random.")]
        [SerializeField]
        private bool _UseRandomFirstTileType = true;
        
        [Tooltip("The tile type for the first tile when the UseRandomFirstTileType option is disabled.")]
        [SerializeField]
        private TileTypeDataSO _FirstTileType;
        
        
        
        [SerializeField]
        private HexTile _GrassTile;
        
        [SerializeField]
        private HexTile _MountainTile;



        /// <summary>
        /// Returns the total number of tiles in the tile map.
        /// </summary>
        public int TotalTiles => _HexTileMapWidth * _HexTileMapHeight;
        
        
        
        /// <summary>
        /// These are the hex coordinate offsets for each neighbor tile of a given tile (when using flat-top hex tiles).
        /// NOTE: We're using 3D coordinates since Unity's Tilemap component does so, since the Z-component can be used for a height value for example.
        /// </summary>
        private Vector3Int[] _NeighborTileOffsets_FlatTop = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0),  // NORTH
            new Vector3Int( 0,  1, 0),  // NORTH_EAST
            new Vector3Int(-1,  1, 0),  // SOUTH_EAST
            new Vector3Int(-1,  0, 0),  // SOUTH
            new Vector3Int(-1, -1, 0),  // SOUTH_WEST
            new Vector3Int( 0, -1, 0)   // NORTH_WEST
        };

        /// <summary>
        /// These are the hex coordinate offsets for each neighbor tile of a given tile (when using pointy-top hex tiles).
        /// NOTE: We're using 3D coordinates since Unity's Tilemap component does so, since the Z-component can be used for a height value for example.
        /// </summary>
        private Vector3Int[] _NeighborTileOffsets_PointyTop = new Vector3Int[]
        {
            new Vector3Int( 0,  1, 0),  // NORTH_EAST
            new Vector3Int( 1,  0, 0),  // EAST
            new Vector3Int( 1, -1, 0),  // SOUTH_EAST
            new Vector3Int( 0, -1, 0),  // SOUTH_WEST
            new Vector3Int(-1, -1, 0),  // WEST
            new Vector3Int(-1,  0, 0)   // NORTH_WEST
        };        

        
        private Rect _HexMapBounds;
        private HexMapStyles _HexMapStyle;
        
        /// <summary>
        /// This dictionary is used for selecting a random tile based on all tile type weights according to the rules of the neighboring tiles.
        /// </summary>
        private Dictionary<TileTypeDataSO, float> _TileTypeWeightsLookup;

        private List<Vector3Int> _NeighborTiles;

        /// <summary>
        /// This just tracks the tile being generated.
        /// NOTE: We're using 3D coordinates since Unity's Tilemap component does so, since the Z-component can be used for a height value for example.
        /// </summary>
        private Vector3Int _CurTilePos;

        /// <summary>
        /// This just holds a reference to whichever of the _NeighborTileOffsets_... arrays we are using. They are defined above.
        /// </summary>
        private Vector3Int[] _NeighborTileOffsets;
        
    

        void Awake()
        {
            
        }
        
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            TileTypesManager.Initialize();
            
            if (_FirstTileType == null)
                throw new NullReferenceException("The FirstTileType property has not been set in the inspector!");
            
            //WeightedRandom.DEBUG_RunWeightedRandomTests(10000);
            GenerateTerrain();
        }
        
        
        public void GenerateTerrain()
        {
            int tileCount = 0;

            
            InitializeTerrainGeneration();

            List<Vector3Int> curTilesToCheck = new List<Vector3Int>();
            List<Vector3Int> nextTilesToCheck = new List<Vector3Int>();
            
            // Set the first tile's type.
            TileTypeDataSO firstTileTypeData = _UseRandomFirstTileType ? TileTypesManager.GetTileTypeData(Random.Range(0, TileTypesManager.Count)) 
                                                                       : _FirstTileType;
            
            Debug.Log($"FIRST TILE POS: {new Vector3Int(_FirstTileXPos, _FirstTileYPos)}    FIRST TILE TYPE: {firstTileTypeData.name}");
            
            // Set the sprite for this tile.
            _HexTileMap.SetTile(new Vector3Int(_FirstTileXPos, _FirstTileYPos), 
                                firstTileTypeData.GetTileVisual());
            
            // Cache the first tile's position.
            Vector3Int firstTilePos = _CurTilePos;
            
            // Set the starting tile as the first tile to check the neighbors of to find more tiles that
            // need to be generated.
            nextTilesToCheck.Add(_CurTilePos);

            tileCount++;
            

            
            while (true)
            {
                curTilesToCheck.Clear();
                curTilesToCheck.AddRange(nextTilesToCheck);
                nextTilesToCheck.Clear();


                for (int i = 0; i < curTilesToCheck.Count; i++)
                {
                    // Find all neighbors of this tile that have not been generated yet.
                    // We are using a private member var here, so we don't create a new list every time we get the neighbor tiles of a given tile.
                    List<Vector3Int> neighborTiles = GetAllNullNeighborTiles(curTilesToCheck[i]);
                    if (neighborTiles == null || neighborTiles.Count <= 0)
                        continue;
                    
                    for (int j = 0; j < neighborTiles.Count; j++)
                    {
                        // Get the position of the tile to generate.
                        Vector3Int tileToGenPos = neighborTiles[j];
                        // Skip this tile if it is outside the bounds of the map.
                        if (!_HexMapBounds.Contains(tileToGenPos))
                            continue;
                        
                        // Add this tile to the list of tiles to check on the next iteration of this loop.
                        nextTilesToCheck.Add(tileToGenPos);
                        
                        // Get the actual tile at that position in the tilemap, if there is one.
                        HexTile tileToGen = (HexTile) _HexTileMap.GetTile(tileToGenPos);
                        
                        // Determine the tile type of this tile.
                        TileTypeDataSO tileType = GenerateTile(tileToGenPos);
                        Debug.Log($"TILE POS: {tileToGenPos}    TILE TYPE: {tileType.name}");
                        
                        // Store the tile type data in the tile.
                        if (tileToGen != null)
                            tileToGen.TileTypeData = tileType;
                        
                        // Set the sprite of the tile.
                        _HexTileMap.SetTile(tileToGenPos, tileType.GetTileVisual());

                        
                        tileCount++;
                        
                        Debug.Log("NEIGHBOR: " + tileToGenPos);
                        
                    } // end for j
                    
                } // end for i

                
                // If we're done, exit this loop.
                if (nextTilesToCheck.Count <= 0)
                    break;
                
            } // end while
            
            Debug.Log($"Total tiles generated: {tileCount}");
        }

        private TileTypeDataSO GenerateTile(Vector3Int tilePos)
        {
            InitTileTypeWeightsLookup();
            
            // Get all neighboring tiles that are already generated.
            List<Vector3Int> generatedNeighbors = GetAllGeneratedNeighborTiles(tilePos);
            
            
            // Foreach each of those already existing neighbor tiles, apply its terrain generation rules to the tile type weights lookup.
            foreach (Vector3Int neighborPos in generatedNeighbors)
            {
                // Get the tile at this position from the tile map.
                HexTile neighborTile = (HexTile) _HexTileMap.GetTile(neighborPos);

                // Iterate through each of its terrain generation rules for neighboring tiles.
                foreach (TerrainGenRule rule in neighborTile.TileTypeData.NeighborGenerationRules)
                {
                    // Apply this terrain generation rule to all relevant tile types in the tile type weights lookup.
                    for (int i = 0; i < TileTypesManager.Count; i++)
                    {
                        // Get the next tile type from the manager
                        TileTypeDataSO tileTypeData = TileTypesManager.GetTileTypeData(i);

                        
                        // Next we need to check if we should apply this terrain gen rule's effects to this tile type in the tile type weights lookup dictionary.
                        bool doApply = false;
                        
                        // Does this rule only apply to a certain climate?
                        if (rule.TerrainType == TerrainTypes.Any && rule.Climate != Climates.Any)
                        {
                            if (tileTypeData.Climate == rule.Climate)
                                doApply = true;
                        }
                        // Does this rule only apply to a certain terrain type?
                        else if (rule.TerrainType != TerrainTypes.Any && rule.Climate == Climates.Any)
                        {
                            if (tileTypeData.TerrainType == rule.TerrainType)
                                doApply = true;
                        }
                        // Does this rule apply to both a terrain type and a climate?
                        else if (rule.TerrainType != TerrainTypes.Any && rule.Climate != Climates.Any)
                        {
                            if (tileTypeData.TerrainType == rule.TerrainType && tileTypeData.Climate == rule.Climate)
                                doApply = true;
                        }
                        // If we get here, it means rule.TerrainType and rule.Climate are both set to None.
                        else
                        {
                            Debug.LogWarning($"TileTypeDataSO for tile type {tileTypeData.name} contains a rule that applies to nothing since Terrain Type and Climate are both set to \"None\"!");
                        }
                        
                        
                        // Apply the effects of this rule if needed.                        
                        if (doApply)
                            _TileTypeWeightsLookup[tileTypeData] *= rule.Weight;

                         
                    } // for int i
                    
                } // foreach terrain gen rule
                
            } // foreach neighbor tile
            
            
            
            return DrawRandomTile();
        }

        private TileTypeDataSO DrawRandomTile()
        {
            // Create a weighted items list to pass into the WeightedRandom.Draw() function.
            List<WeightedItem<TileTypeDataSO>> tileTypeDataList = new List<WeightedItem<TileTypeDataSO>>();

            // Populate the weighted item list.
            for (int i = 0; i < TileTypesManager.Count; i++)
            {
                tileTypeDataList.Add(new WeightedItem<TileTypeDataSO>() { Item = TileTypesManager.GetTileTypeData(i), 
                                                                          Weight = _TileTypeWeightsLookup[TileTypesManager.GetTileTypeData(i)] });
            } // for int i


            bool result = WeightedRandom.Draw(tileTypeDataList, out TileTypeDataSO selectedItem);
            return result ? selectedItem : null;
        }
        
        private void InitializeTerrainGeneration()
        {
            _TileTypeWeightsLookup = new Dictionary<TileTypeDataSO, float>();
            
            
            // Seed the random number generator.
            Random.InitState(_UseTimeAsSeed ? (int) DateTime.Now.TimeOfDay.Ticks
                                            : _Seed);
            
            // Determine which type of hex map this is.
            _HexMapStyle = HexUtils.GetHexMapStyle(_HexTileMap);
            //Debug.Log("STYLE: " + _HexMapStyle);
            
            // Determine which neighbor cell offsets array we need to use.
            // If the tiles are wider than they are tall, then we know this is a flat-top hex grid. Otherwise it's a pointy-top hex grid.
            _NeighborTileOffsets = _HexMapStyle == HexMapStyles.FLAT_TOP ? _NeighborTileOffsets_FlatTop
                                                                         : _NeighborTileOffsets_PointyTop;
            
            // Determine the bounds of the tilemap based on the size settings.
            // First, check the width and height. If they are even, just make them odd by subtracting 1. This way we always have an even number of tiles left/right and above/below the origin tile (0,0,0).
            int width = _HexTileMapWidth % 2 == 0 ? _HexTileMapWidth - 1 : _HexTileMapWidth;
            int height = _HexTileMapHeight % 2 == 0 ? _HexTileMapHeight - 1 : _HexTileMapHeight;
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            _HexMapBounds = new Rect(-halfWidth, -halfHeight, width, height);
            
            // Determine the starting tile position.
            // If the UserRandomFirstTilePosition option is enabled, select a random starting tile position.
            // Otherwise, use the FirstTileXPos and FirstTileYPos settings.
            _CurTilePos = _UseRandomFirstTilePosition ? HexUtils.GetRandomTileCoord(-halfWidth, halfWidth, -halfHeight, halfHeight)
                                                      : new Vector3Int(Mathf.Clamp(_FirstTileXPos, 0, _HexTileMapWidth), 
                                                                       Mathf.Clamp(_FirstTileYPos, 0, _HexTileMapHeight),
                                                                       0);
        }
        
        /// <summary>
        /// Initializes the tile type weights lookup dictionary, by adding every tile type with a weight of 1f to start with.
        /// </summary>
        private void InitTileTypeWeightsLookup()
        {
            _TileTypeWeightsLookup.Clear();
            
            for (int i = 0; i < TileTypesManager.Count; i++)
            {
                TileTypeDataSO tileTypeData = TileTypesManager.GetTileTypeData(i);
                _TileTypeWeightsLookup.Add(tileTypeData, 1f);
            }
        }
        
        /// <summary>
        /// Returns a list of all neighboring tiles for the given tile position.
        /// </summary>
        /// <param name="curTilePos">The position of the tile to get the neighbors for.</param>
        /// <returns>A list of neighboring tile positions.</returns>
        private List<Vector3Int> GetAllNeighborTiles(Vector3Int curTilePos)
        {
            List<Vector3Int> neighborTilesList = new List<Vector3Int>();
            
        
            for (int i = 0; i < _NeighborTileOffsets.Length; i++)
            {
                Vector3Int nextNeighborCoord = curTilePos + _NeighborTileOffsets[i];

                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                
                // Check that this tile position is within the tilemap bounds. If not, then just skip it.
                if (nextNeighborCoord.x < _HexMapBounds.xMin || nextNeighborCoord.x > _HexMapBounds.xMax ||
                    nextNeighborCoord.y < _HexMapBounds.yMin || nextNeighborCoord.y > _HexMapBounds.yMax)
                {
                    continue;
                }
                
                
                HexTile tile = (HexTile)_HexTileMap.GetTile(nextNeighborCoord);
                
                neighborTilesList.Add(nextNeighborCoord);                            

            } // end for i

        
            return neighborTilesList;
        }
        
        /// <summary>
        /// Returns a list of all neighboring tiles for the given tile position that have already been generated.
        /// </summary>
        /// <param name="curTilePos">The position of the tile to get the neighbors for.</param>
        /// <returns>A list of neighboring tile positions.</returns>
        private List<Vector3Int> GetAllGeneratedNeighborTiles(Vector3Int curTilePos)
        {
            List<Vector3Int> neighborTilesList = new List<Vector3Int>();
            
        
            for (int i = 0; i < _NeighborTileOffsets.Length; i++)
            {
                Vector3Int nextNeighborCoord = curTilePos + _NeighborTileOffsets[i];
                
                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                
                HexTile tile = (HexTile)_HexTileMap.GetTile(nextNeighborCoord);

                if (HexUtils.TileHasBeenGenerated(tile))
                    neighborTilesList.Add(nextNeighborCoord);                            

            } // end for i

        
            return neighborTilesList;
        }        
        
        /// <summary>
        /// Returns a list of all neighboring tiles for the given tile position that have not yet been generated.
        /// </summary>
        /// <param name="curTilePos">The position of the tile to get the neighbors for.</param>
        /// <returns>A list of neighboring tile positions.</returns>
        private List<Vector3Int> GetAllNullNeighborTiles(Vector3Int curTilePos)
        {
            List<Vector3Int> neighborTilesList = new List<Vector3Int>();
            
        
            for (int i = 0; i < _NeighborTileOffsets.Length; i++)
            {
                Vector3Int nextNeighborCoord = curTilePos + _NeighborTileOffsets[i];
                
                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                
                HexTile tile = (HexTile)_HexTileMap.GetTile(nextNeighborCoord);

                if (!HexUtils.TileHasBeenGenerated(tile))
                    neighborTilesList.Add(nextNeighborCoord);                            

            } // end for i

        
            return neighborTilesList;
        }    
    }
}
