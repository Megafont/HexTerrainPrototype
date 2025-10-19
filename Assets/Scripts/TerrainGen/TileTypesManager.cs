using System;
using System.Collections.Generic;
using System.Linq;
using HexTerrainPrototype.Data;
using UnityEngine;

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
        
    }
}
