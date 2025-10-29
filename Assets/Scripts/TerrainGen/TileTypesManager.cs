using System;
using System.Collections.Generic;
using System.Linq;
using HexTerrainPrototype.Data;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HexTerrainPrototype
{
    public static class TileTypesManager
    {
        private static Dictionary<string, TileTypeDataSO> _AllTileTypesByNameLookup;

        private static Dictionary<Climates, List<TileTypeDataSO>> _TileTypesByClimateLookup;
        private static Dictionary<TerrainTypes, List<TileTypeDataSO>> _TileTypesByTerrainTypeLookup;


        public static TileTypeDataSO GetTileTypeData(int index) { return _AllTileTypesByNameLookup.Values.ElementAt(index); }
        public static int Count => _AllTileTypesByNameLookup.Count;
        
        
        public static void Initialize()
        {
            // Initialize the all tile types by name lookup dictionary.
            _AllTileTypesByNameLookup = new Dictionary<string, TileTypeDataSO>();
            
            // ----------------------------------------------------------------------------------------------------
            
            // Initialize the tile types by climate lookup dictionary.
            _TileTypesByClimateLookup = new Dictionary<Climates, List<TileTypeDataSO>>();
            
            // Create a list inside it for each climate type.
            foreach (Climates climate in Enum.GetValues(typeof(Climates)))
            {
                _TileTypesByClimateLookup.Add(climate, new List<TileTypeDataSO>());
            }

            // ----------------------------------------------------------------------------------------------------
            
            // Initialize the tile types by terrain type lookup dictionary.
            _TileTypesByTerrainTypeLookup = new Dictionary<TerrainTypes, List<TileTypeDataSO>>();
            
            // Create a list inside it for each terrain type.
            foreach (TerrainTypes terrainType in Enum.GetValues(typeof(TerrainTypes)))
            {
                _TileTypesByTerrainTypeLookup.Add(terrainType, new List<TileTypeDataSO>());
            }
            
            // ----------------------------------------------------------------------------------------------------
            
            // Load in the tile types.
            LoadTileTypes();
        }
        
        public static void LoadTileTypes()
        {
            List<TileTypeDataSO> tileTypes = Resources.LoadAll<TileTypeDataSO>("ScriptableObjects/TerrainTileTypes").ToList();
            
            
            // Add each tile type to the appropriate lookup dictionaries.
            foreach (TileTypeDataSO tileTypeData in tileTypes)
            {
                _AllTileTypesByNameLookup.Add(tileTypeData.name, tileTypeData);
                
                _TileTypesByClimateLookup[tileTypeData.Climate].Add(tileTypeData);
                _TileTypesByTerrainTypeLookup[tileTypeData.TerrainType].Add(tileTypeData);
                
            } // end foreach
            
            
            Debug.Log($"Loaded tile types data:    Total Types: {tileTypes.Count}");
        }

        /// <summary>
        /// Gets a random tile of the given type or types.
        /// </summary>
        /// <param name="climate">The climate of the tile to get. You can set this to 'None' if you don't care about the climate type. Then you'll get a random tile of the specified terrain type./</param>
        /// <param name="terrainType">The terrain type of the tile to get. You can set this to 'None' if you don't care about the terrain type. Then you'll get a random tile of the specified climate type.</param>
        /// <returns>A random tile of the specified climate, terrain type, or both.</returns>
        public static TileTypeDataSO GetRandomTileOfType(Climates climate, TerrainTypes terrainType)
        {
            List<TileTypeDataSO> tileTypes = new List<TileTypeDataSO>();

            if (climate == Climates.None && terrainType == TerrainTypes.None)
            {
                // This one is using AddRange() on purpose since there can't be any duplicates in this case anyway. So the extra check is not needed.
                tileTypes.AddRange(_AllTileTypesByNameLookup);
            }
            else
            {
                tileTypes.AddRange(_TileTypesByClimateLookup[climate]);
            }


            return tileTypes[Random.Range(0, tileTypes.Count - 1)];
            
            int attempts = 0;
            while (true)
            {
                TileTypeDataSO tileType = tileTypes[Random.Range(0, tileTypes.Count - 1)];
                if (climate == Climates.None)
                    return tileType;

                
                int diff = climate - tileType.Climate;

                // Check that the randomly selected tile type is not more than one climate type away from the specified climate. This prevents a snowy tile from spawning next to a desert tile, for example.
                if (diff >= -1 && diff <= 1)
                    return tileType;

                attempts++;
                if (attempts >= 100)
                {
                    Debug.LogError("Failed to pick a valid tile type after 100 attempts. The currently selected tile type will be used.");
                    return tileType;
                }
            }
        }
        
        /// <summary>
        /// Returns a list of all tiles of the specified climate type.
        /// </summary>
        /// <param name="climate">The climate type to find tiles for.</param>
        /// <returns>A list of tiles of the specified climate type.</returns>
        public static List<TileTypeDataSO> GetTileTypesByClimate(Climates climate)
        {
            if (climate == Climates.None)
                return new List<TileTypeDataSO>();
            
            return _TileTypesByClimateLookup[climate];
        }
        
        /// <summary>
        /// Returns a list of all tiles of the specified terrain type.
        /// </summary>
        /// <param name="terrainType">The terrain type to find tiles for.</param>
        /// <returns>A list of tiles of the specified terrain type.</returns>
        public static List<TileTypeDataSO> GetTileTypesByTerrainType(TerrainTypes terrainType)
        {
            if (terrainType == TerrainTypes.None)
                return new List<TileTypeDataSO>();

            return _TileTypesByTerrainTypeLookup[terrainType];
        }
        
    }
}
