using System;
using System.Collections.Generic;
using System.Linq;
using HexTerrainPrototype.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace HexTerrainPrototype
{
    /// <summary>
    /// This class generates a random hex terrain based on the tile type data (scriptable objects).
    ///
    /// *** IMPORTANT ***
    /// If the tiles are rotated the wrong way, you probably need to turn off the RotateTiles90Degrees debug option, and/or make sure
    /// the HexTileMap field is set to the correct tile map for the type of tiles you're using.
    /// </summary>
    public class HexTerrainGenerator : MonoBehaviour
    {
        [Header("General")]

        [Tooltip("The style of hex tilemap to generate.")]
        [SerializeField]
        private HexMapStyles _HexMapStyle;
        
        [Tooltip("The width of the hex tilemap to be generated.")]
        [SerializeField]
        [Min(1)]
        private int _HexTileMapWidth;

        [Tooltip("The height of the hex tilemap to be generated.")]
        [SerializeField]
        [Min(1)]
        private int _HexTileMapHeight;

        [Tooltip("If enabled, the tilemap will be centered on the origin. This means that an 11x11 map would have tile coordinates ranging from (-5,-5) to (5, 5) rather than from (0,0) to (10,10).")]
        [SerializeField]
        private bool _CenterMapOnOrigin = false;

        [Tooltip("The flat-top tile map.")]
        [SerializeField]
        private Tilemap _HexTileMap_FlatTop;

        [Tooltip("The pointy-top tile map.")]
        [SerializeField]
        private Tilemap _HexTileMap_PointyTop;
        
        
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
        
        [Tooltip("The X position for the first tile to generate when the UseRandomFirstTilePosition option is disabled. After this tile is generated, it's neighbors will generated in clockwise order starting from the north or northeast neighbor depending on the tile map style. Then it will do the same for the neighbors of each of those tiles, and repeat until the map is done.")]
        [SerializeField]
        [Min(0)]
        private int _FirstTileXPos;

        [Tooltip("The Y position for the first tile to generate when the UseRandomFirstTilePosition option is disabled. After this tile is generated, it's neighbors will generated in clockwise order starting from the north or northeast neighbor depending on the tile map style. Then it will do the same for the neighbors of each of those tiles, and repeat until the map is done.")]
        [SerializeField]
        [Min(0)]
        private int _FirstTileYPos;

        [Space(10)]
        
        [Tooltip("If enabled, the type of the first tile will be selected at random.")]
        [SerializeField]
        private bool _UseRandomFirstTileType = true;
        
        [Tooltip("The tile type to use for the first tile when the UseRandomFirstTileType option is disabled.")]
        [SerializeField]
        private TileTypeDataSO _FirstTileType;

        [FormerlySerializedAs("_StackedRuleBonus")]
        [Space(10)] 
        
        [Header("Terrain Generation Settings")]
        
        [Tooltip("If the neighboring tiles contain duplicate terrain generation rules, this value will be added to boost the weight of that rule since we have a rule stacking on top of itself. This value is a percentage in the range 0f-1f. It is used to prevent the selection probablity of a given tile type from easily reaching 100%, which would effectively disable all following rules and generate a map that is almost entirely temperate, snowy, or arid.")]
        [SerializeField]
        [Range(0, 1)]
        private float _StackedRuleProbabilityBonus = 0.025f;

        [Space(10)] 
        
        [Header("Debug Settings")] 

        /*
        [Tooltip("When enabled, the tiles are rotated 90 degrees when placed to allow testing of pointy-top hex tilemaps with the flat-top placeholder tile set.")]
        [SerializeField]
        private bool _RotateTiles90Degrees = false;
    
        [Space(10)]
        */
        
        [Tooltip("If enabled, debug text will be displayed for each tile.")]
        [SerializeField]
        private bool _EnableTileDebugText;
        
        [Tooltip("This is the object all tile debug text objects will be parented to.")]
        [SerializeField]
        private Transform _TileDebugTextsParent;
        
        [Tooltip("The prefab to use for tile debug text.")]
        [SerializeField]
        private TextMeshPro _TileDebugTextPrefab;

        [SerializeField]
        private Color _FirstTileDebugTextColor = Color.red;
        
        [SerializeField]
        private Color _TileDebugTextColor = Color.yellow;
        


        /// <summary>
        /// Returns the total number of tiles in the tile map.
        /// </summary>
        public int TotalTiles => _HexTileMapWidth * _HexTileMapHeight;

        
        private Tilemap _HexTileMap;
        
        private int _TileNum = 0;
        private List<TextMeshPro> _TileDebugTextList = new List<TextMeshPro>();

        
        private Rect _HexMapBounds;

        private List<Vector3Int> _NeighborTiles;

        /// <summary>
        /// This just tracks the tile being generated.
        /// NOTE: We're using 3D coordinates since Unity's Tilemap component does so, since the Z-component can be used for a height value for example.
        /// </summary>
        private Vector3Int _CurTilePos;
        
    

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
            
            Debug.Log($"FIRST TILE POS: {_CurTilePos}    FIRST TILE TYPE: {firstTileTypeData.name}");
            
            // Set the sprite for this tile.
            PlaceTile(_CurTilePos,
                      firstTileTypeData,
                      true);
            
            
            // Set the starting tile as the first tile to check the neighbors of to find more tiles that
            // need to be generated.
            nextTilesToCheck.Add(_CurTilePos);

            tileCount++;


            int rounds = 0;
            while (true)
            {
                curTilesToCheck.Clear();
                curTilesToCheck.AddRange(nextTilesToCheck);
                nextTilesToCheck.Clear();


                for (int i = 0; i < curTilesToCheck.Count; i++)
                {
                    // Find all neighbors of this tile that have not been generated yet.
                    // We are using a private member var here, so we don't create a new list every time we get the neighbor tiles of a given tile.
                    List<Vector3Int> nullNeighborTiles = GetAllNullNeighborTiles(curTilesToCheck[i]);
                    if (nullNeighborTiles == null || nullNeighborTiles.Count <= 0)
                        continue;

                    
                    for (int j = 0; j < nullNeighborTiles.Count; j++)
                    {
                        // Get the position of the tile to generate.
                        Vector3Int tileToGenPos = nullNeighborTiles[j];
                        
                        // Skip this tile if it is outside the bounds of the map.
                        if (!TileIsInBounds(tileToGenPos))
                            continue;
                        
                        
                        // Get the actual tile at that position in the tilemap, if there is one.
                        HexTile tileToGen = (HexTile) _HexTileMap.GetTile(tileToGenPos);
                        
                        // If this tile has already been generated, then do not add it into the list of tiles to check, as it has already had its neighbors checked.
                        if (!HexUtils.TileHasBeenGenerated(tileToGen))
                            nextTilesToCheck.Add(tileToGenPos);
                        
                        // Determine the tile type of this tile.
                        TileTypeDataSO tileType = GenerateTile(tileToGenPos);
                        Debug.Log($"TILE POS: {tileToGenPos}    TILE TYPE: {(tileType != null ? tileType.name : "NULL")}");

                        // Set the sprite of the tile.
                        PlaceTile(tileToGenPos, tileType);
                        
                        tileCount++;
                        
                    } // end for j
                    
                } // end for i

                
                // If we're done, exit this loop.
                if (nextTilesToCheck.Count <= 0)
                    break;
                
            } // end while
            
            Debug.Log($"TOTAL TILES GENERATED: {tileCount}");
        }
        
        private TileTypeDataSO GenerateTile(Vector3Int tilePos)
        {
            // Compile the list of terrain generation rules for this tile.
            List<TerrainGenRule> tileGenRules = CompileGenerationRulesForTile(tilePos, out Climates coldestNeighborClimate, out Climates warmestNeighborClimate, out Climates avgClimate);     
            
            // Figure out the climate for this tile.
            Climates climate = Climates.None;
            // If this tile has both snowy and arid neighbors, then force the climate to be temperate.
            /*
            if (coldestNeighborClimate == Climates.Snowy && warmestNeighborClimate == Climates.Arid)
                climate = Climates.Temperate;
            else */
                climate = avgClimate;
            
            Debug.Log($"CLIMATE:    Final: {climate}    Avg: {avgClimate}    Coldest: {coldestNeighborClimate}    Warmest: {warmestNeighborClimate}");
            
            // Determine the type of this tile.
            TileTypeDataSO tileType = SelectRandomTile(tileGenRules, coldestNeighborClimate, warmestNeighborClimate, avgClimate);
            
            return tileType;
        }
        
        private void PlaceTile(Vector3Int tilePos, TileTypeDataSO tileType, bool isFirstTile = false)
        {
            // Set the sprite of the tile.
            HexTile tileVisual = tileType.GetTileVisual();
            
            //if (_RotateTiles90Degrees)
            //    tileVisual.Rotation = 90f;
                        
            _HexTileMap.SetTile(tilePos, tileVisual);
                        
            //if (_RotateTiles90Degrees) // Set the rotation back to 0 so it doesn't get saved as 90 degrees in the scriptable object. This is only an issue in the editor, and not in builds.
            //    tileVisual.Rotation = 0f;

            
            if (_EnableTileDebugText)
                SpawnTileDebugText(tilePos, tileType, isFirstTile);
        }
        
        private void SpawnTileDebugText(Vector3Int tilePos, TileTypeDataSO tileTypeData, bool isFirstTile = false)
        {
            TextMeshPro tileDebugText = Instantiate(_TileDebugTextPrefab, tilePos, Quaternion.identity, _TileDebugTextsParent);
            tileDebugText.transform.position = _HexTileMap.CellToWorld(tilePos);
            tileDebugText.GetComponent<RectTransform>().sizeDelta = new Vector2(_HexTileMap.cellSize.x, _HexTileMap.cellSize.y);
            
            tileDebugText.text = $"#{_TileNum}\n{tilePos}\n{tileTypeData.name}";
            
            tileDebugText.color = isFirstTile ? _FirstTileDebugTextColor : _TileDebugTextColor;
            
            _TileNum++;
            
            _TileDebugTextList.Add(tileDebugText);
        }

        /// <summary>
        /// Draws a random tile based on the terrain generation rules from the all neighboring tiles that are already generated.
        /// </summary>
        /// <param name="tileGenRules">A list of the terrain generation rules from neighboring tiles.</param>
        /// <param name="climate">The climate value to use for rules that don't have one.</param>
        /// <returns>The selected tile type.</returns>
        private TileTypeDataSO SelectRandomTile(List<TerrainGenRule> tileGenRules,  Climates coldestNeighborClimate, Climates warmestNeighborClimate, Climates avgClimate)
        {
            /*
            if (climate == Climates.None)
            {
                Debug.LogWarning("NONE!!!");
                climate = Climates.Temperate;
            }
            */
            
            TileTypeDataSO tileTypeData = null;
            
            Climates reasonableClimate = Climates.Temperate;
            Climates baseClimate = Climates.Temperate;
            if (coldestNeighborClimate == warmestNeighborClimate)
            {
                baseClimate = coldestNeighborClimate;
            }
            reasonableClimate = (Climates)Random.Range(Mathf.Max((int) baseClimate - 1, 1), // The Max() function makes sure this value is not less than 1.
                                                       Mathf.Min((int) baseClimate + 1, Enum.GetValues(typeof(Climates)).Length - 1)); // The Min() function makes sure this value is not greater than the last Climates enum value.
            
            if (coldestNeighborClimate == Climates.Snowy && warmestNeighborClimate == Climates.Arid)
                reasonableClimate = Climates.Temperate;
            
            
            // Populate the weighted item list.
            foreach (TerrainGenRule rule in tileGenRules)
            {
                //rule.DEBUG_PrintRule();
                
                if (Random.Range(0f, 1f) <= rule.Probability)
                {
                    tileTypeData = TileTypesManager.GetRandomTileOfType(rule.Climate != Climates.None ? rule.Climate : reasonableClimate, 
                                                                        rule.TerrainType);
                    if (tileTypeData != null)
                        return tileTypeData;
                }
                
            } // foreach TerrainGenRule


            // None of the rules generated a tile this time, so pick one randomly with an appropriate climate, but no required terrain type.
            int attempts = 100;
            for (int i = attempts; i > 0; i--)
            {
                
                i--;
                
            } // end for i

            
            Debug.LogWarning($"Failed to select a valid tile after {attempts} attempts! The currently selected tile will be used so generation can continue.");
            tileTypeData = TileTypesManager.GetRandomTileOfType(reasonableClimate, 
                                                      TerrainTypes.None);
            return tileTypeData;
        }

        private List<TerrainGenRule> CompileGenerationRulesForTile(Vector3Int tilePos, out Climates coldestNeighborClimate, out Climates warmestNeighborClimate, out Climates avgClimate)
        {
            float climatesSum = 0;
            coldestNeighborClimate = (Climates) Enum.GetValues(typeof(Climates)).Cast<int>().Max();
            warmestNeighborClimate = (Climates) Enum.GetValues(typeof(Climates)).Cast<int>().Min();
            
            
            // Create a list to store the relevant terrain generation rules in.
            List<TerrainGenRule> tileGenRules = new List<TerrainGenRule>();
                
            // Get all neighboring tiles that are already generated.
            List<Vector3Int> generatedNeighbors = GetAllGeneratedNeighborTiles(tilePos);
            //Debug.Log("GENERATED NEIGHBORS COUNT: " + generatedNeighbors.Count);
            
            // Get the climate limits of the neighors we just found.
            GetClimateLimits(generatedNeighbors, out coldestNeighborClimate, out warmestNeighborClimate, out avgClimate);
            
            
            // For each of those already existing neighbor tiles, apply its terrain generation rules to the tile type weights lookup.
            foreach (Vector3Int neighborPos in generatedNeighbors)
            {
                // Get the tile at this position from the tile map.
                HexTile neighborTile = (HexTile) _HexTileMap.GetTile(neighborPos);

                /*
                if (neighborTile == null)
                    Debug.LogError($"{neighborPos}    neighborTile==NULL");
                else if (neighborTile.TileTypeData == null)
                    Debug.LogError($"{neighborPos}    neighborTile.TerrainTypeData==NULL");
                else if (neighborTile.TileTypeData.NeighborGenerationRules == null)
                    Debug.LogError($"{neighborPos}    neighborTile.TerrainTypeData.NeighborGenerationRules==NULL");
                else if (neighborTile.TileTypeData.NeighborGenerationRules.Count == 0)
                    Debug.LogError($"{neighborPos}    neighborTile.TerrainTypeData.NeighborGenerationRules.Count==0");
                */
                
                // Iterate through each of its terrain generation rules for neighboring tiles.
                foreach (TerrainGenRule rule in neighborTile.TileTypeData.NeighborGenerationRules)
                {
                    // Check if this rule already exists in the rules list we're building for the tile we're generating.
                    bool foundMatch = false;
                    for (int i = 0; i < tileGenRules.Count; i++)
                    {
                        TerrainGenRule ruleInfo = tileGenRules[i];
                        if (ruleInfo.IsMatch(rule))
                        {
                            foundMatch = true;

                            if (rule.Probability > 0)
                                ruleInfo.Probability = Mathf.Clamp(ruleInfo.Probability + _StackedRuleProbabilityBonus, 0f, 1f);
                            else if (rule.Probability == 0)
                                ruleInfo.Probability = 0;
                            
                            if (rule.Probability != 0)
                                tileGenRules[i] = ruleInfo; // We have to copy the whole object back into the list since it is a struct.
                            
                            break;
                        }
                        
                    } // end for i

                    // If not match was found, then we may be able to add this rule to the list.
                    if (!foundMatch)
                    {
                        rule.DEBUG_PrintRule();
                        // If this rule is arid, but there is a snowy neighboring tile, or this rule is snowy but there is an arid neighboring tile,
                        // then we should skip this rule, as it could result in desert and snow tiles being right next to each other.
                        if (!(coldestNeighborClimate == Climates.Snowy && rule.Climate == Climates.Arid) &&
                            !(warmestNeighborClimate == Climates.Arid && rule.Climate == Climates.Snowy))
                        {
                            tileGenRules.Add(new TerrainGenRule(rule));
//                            rule.DEBUG_PrintRule();
                        }
                    }
                    
                    
                } // foreach terrain gen rule
                
            } // foreach neighbor tile
            
            
            // Now that we've compiled a list of terrain generation rules for this tile based on all neighboring tiles that are already generated,
            // we will sort the list by probability from highest to lowest. We can just use Sort() here, because the TerrainGenRuleInfos implement IComparable<TerrainGenRuleInfo>.
            tileGenRules.Sort();
            
            // Remove any entries that have a probability of <= 0.
            for (int i = tileGenRules.Count - 1; i >= 0; i--)
            {
                if (tileGenRules[i].Probability <= 0)
                    tileGenRules.RemoveAt(i);
                
            } // end for i

            
            
            Debug.Log($"GENERATING TILE:    {tilePos}    ColdestNeighbor: {coldestNeighborClimate}    WarmestNeighbor: {warmestNeighborClimate}    AvgNeighbor: {avgClimate}");
            return tileGenRules;
        }

        private void GetClimateLimits(List<Vector3Int> tilesToCheck, out Climates coldestNeighborClimate, out Climates warmestNeighborClimate, out Climates avgClimate)
        {
            coldestNeighborClimate = (Climates) Enum.GetValues(typeof(Climates)).Cast<int>().Max();
            warmestNeighborClimate = (Climates) Enum.GetValues(typeof(Climates)).Cast<int>().Min();
            
            // Check the climate of every tile in the list.
            foreach (Vector3Int tilePos in tilesToCheck)
            {
                // Get the tile at this position from the tile map.
                HexTile neighborTile = (HexTile)_HexTileMap.GetTile(tilePos);

                // Add the climate value to a running total. This will be turned into the average climate of the neighboring tiles, which we will use for rules that have no climate value.
                Climates curTileClimate = neighborTile.TileTypeData.Climate;
                
                // If the climate is set to None for some reason, just ignore it.
                if (curTileClimate != Climates.None)
                {
                    if (curTileClimate < coldestNeighborClimate)
                        coldestNeighborClimate = curTileClimate;
                    if (curTileClimate > warmestNeighborClimate)
                        warmestNeighborClimate = curTileClimate;
                }
                
            } // end foreach tile

            avgClimate = (Climates) Mathf.RoundToInt(((int) coldestNeighborClimate + (int) coldestNeighborClimate) / 2f);
        }
        
        private void InitializeTerrainGeneration()
        {
            // Seed the random number generator.
            Random.InitState(_UseTimeAsSeed ? (int) DateTime.Now.TimeOfDay.Ticks
                                            : _Seed);
            
            // Set our reference to the tilemap with the specified style.
            // This way all the code will just use whichever one is appropriate.
            _HexTileMap = _HexMapStyle == HexMapStyles.FLAT_TOP ? _HexTileMap_FlatTop 
                                                                : _HexTileMap_PointyTop;
            
            // Enable the appropriate hex tilemap, and disable the other one.
            _HexTileMap_FlatTop.gameObject.SetActive(_HexMapStyle == HexMapStyles.FLAT_TOP);
            _HexTileMap_PointyTop.gameObject.SetActive(_HexMapStyle == HexMapStyles.POINTY_TOP);
            
            
            // Determine the bounds of the tilemap based on the size settings.
            // First, check the width and height. If they are even, just make them odd by subtracting 1. This way we always have an even number of tiles left/right and above/below the origin tile (0,0,0).
            if (_CenterMapOnOrigin)
            {
                int width = _HexTileMapWidth % 2 == 0 ? _HexTileMapWidth - 1 : _HexTileMapWidth;
                int height = _HexTileMapHeight % 2 == 0 ? _HexTileMapHeight - 1 : _HexTileMapHeight;
                int halfWidth = width / 2;
                int halfHeight = height / 2;
                _HexMapBounds = new Rect(-halfWidth, -halfHeight, width, height);

                //_HexTileMap.transform.position = new Vector3(_HexGridWidth / 2f, _HexGridHeight / 2f, 0);

                // Determine the starting tile position.
                // If the UserRandomFirstTilePosition option is enabled, select a random starting tile position.
                // Otherwise, use the FirstTileXPos and FirstTileYPos settings.
                _CurTilePos = _UseRandomFirstTilePosition ? HexUtils.GetRandomTileCoord(-halfWidth, halfWidth - 1, -halfHeight, halfHeight - 1)
                                                          : new Vector3Int(Mathf.Clamp(_FirstTileXPos, 0, _HexTileMapWidth - 1), 
                                                                           Mathf.Clamp(_FirstTileYPos, 0, _HexTileMapHeight - 1),
                                                                           0);
            }
            else // _CenterMapOnOrigin is off, so the origin cell will be at the lower-left corner of the map rather than in the center.
            {
                _HexMapBounds = new Rect(0, 0, _HexTileMapWidth, _HexTileMapHeight);
                
                // Determine the starting tile position.
                // If the UserRandomFirstTilePosition option is enabled, select a random starting tile position.
                // Otherwise, use the FirstTileXPos and FirstTileYPos settings.
                _CurTilePos = _UseRandomFirstTilePosition ? HexUtils.GetRandomTileCoord(0, _HexTileMapWidth - 1, 0, _HexTileMapHeight - 1)
                                                          : new Vector3Int(Mathf.Clamp(_FirstTileXPos, 0, _HexTileMapWidth - 1), 
                                                                           Mathf.Clamp(_FirstTileYPos, 0, _HexTileMapHeight - 1),
                                                                           0);                
            }
            
            
            if (!TileIsInBounds(_CurTilePos))
                throw new Exception($"The starting tile position {_CurTilePos} is outside the bounds of the map: ({_HexMapBounds.xMin},{_HexMapBounds.xMax}) to ({_HexMapBounds.yMin},{_HexMapBounds.yMax})! Change it in the inspector, or enable the UseRandomFirstTilePosition option there.");
        }

        /// <summary>
        /// Returns a list of all neighboring tiles for the given tile position.
        /// </summary>
        /// <param name="curTilePos">The position of the tile to get the neighbors for.</param>
        /// <returns>A list of neighboring tile positions.</returns>
        private List<Vector3Int> GetAllNeighborTiles(Vector3Int curTilePos)
        {
            List<Vector3Int> neighborTilesList = new List<Vector3Int>();
            
        
            for (int i = 0; i < 6; i++)
            {
                Vector3Int nextNeighborCoord = _HexMapStyle == HexMapStyles.FLAT_TOP ? HexDirections.GetNeighborPos_FlatTop(curTilePos, (HexDirections.Directions_FlatTop) i)
                                                                                     : HexDirections.GetNeighborPos_PointyTop(curTilePos, (HexDirections.Directions_PointyTop) i);
                
                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                
                // Check that this tile position is within the tilemap bounds. If not, then just skip it.
                if (!TileIsInBounds(nextNeighborCoord))
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
            
        
            for (int i = 0; i < 6; i++)
            {
                Vector3Int nextNeighborCoord = _HexMapStyle == HexMapStyles.FLAT_TOP ? HexDirections.GetNeighborPos_FlatTop(curTilePos, (HexDirections.Directions_FlatTop) i)
                                                                                     : HexDirections.GetNeighborPos_PointyTop(curTilePos, (HexDirections.Directions_PointyTop) i);
                
                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                HexTile tile = (HexTile) _HexTileMap.GetTile(nextNeighborCoord);

                bool isGenerated = HexUtils.TileHasBeenGenerated(tile);

                if (isGenerated)
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
            
        
            for (int i = 0; i < 6; i++)
            {
                Vector3Int nextNeighborCoord = _HexMapStyle == HexMapStyles.FLAT_TOP ? HexDirections.GetNeighborPos_FlatTop(curTilePos, (HexDirections.Directions_FlatTop) i)
                                                                                     : HexDirections.GetNeighborPos_PointyTop(curTilePos, (HexDirections.Directions_PointyTop) i);

                // If this tile position is outside the bounds of the map, skip it.
                if (!_HexMapBounds.Contains(nextNeighborCoord))
                    continue;
                
                
                HexTile tile = (HexTile)_HexTileMap.GetTile(nextNeighborCoord);

                bool isGenerated = HexUtils.TileHasBeenGenerated(tile);
                
                if (!isGenerated)
                    neighborTilesList.Add(nextNeighborCoord);                            

            } // end for i

        
            return neighborTilesList;
        }

        public bool TileIsInBounds(Vector3Int tilePos)
        {
            return tilePos.x >= _HexMapBounds.xMin && tilePos.x <= _HexMapBounds.xMax &&
                   tilePos.y >= _HexMapBounds.yMin && tilePos.y <= _HexMapBounds.yMax;
        }

    }
}
