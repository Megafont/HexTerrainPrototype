using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HexTerrainPrototype.Data
{
    [Serializable]
    public class TerrainGenRule : IComparable<TerrainGenRule>
    {
        [Tooltip("The climate type this rule applies to, if any. NOTE: Climate and TerrainType cannot both be set to \"None\".")]
        public Climates Climate;
        [Tooltip("The terrain type this rule applies to, if any. NOTE: Climate and TerrainType cannot both be set to \"None\".")]
        public TerrainTypes TerrainType;
        [FormerlySerializedAs("Weight")]
        [Tooltip("This is the weight given to the spawning of this terrain type. For example, if it has a +30% chance to spawn next to the parent tile, then you'd set this to 0.3f.")]
        [Range(0f, 1f)]
        public float Probability = 0.5f;
        
        
        
        public TerrainGenRule(Climates climate, TerrainTypes terrainType, float probability)
        {
            Climate = climate;
            TerrainType = terrainType;
            Probability = probability;
        }
            
        public TerrainGenRule(TerrainGenRule rule)
        {
            Climate = rule.Climate;
            TerrainType = rule.TerrainType;
            Probability = rule.Probability;
        }
        
        
        public int CompareTo(TerrainGenRule other)
        {
            if (other == null) return 1; // Current instance is greater than null

            // Compare by Weight
            int weightComparison = this.Probability.CompareTo(other.Probability);
            if (weightComparison != 0)
            {
                return weightComparison;
            }

            // If weights are equal, compare by Name
            return 1;
        }        
        
        public bool IsMatch(TerrainGenRule rule)
        {
            return Climate == rule.Climate && TerrainType == rule.TerrainType;
        }

        public void DEBUG_PrintRule()
        {
            Debug.Log($"TERRAIN GEN RULE  |  Climate: {Climate}, TerrainType: {TerrainType}, Probability: {Probability}");
        }
    }
}
