using System;
using UnityEngine;

namespace HexTerrainPrototype.Data
{
    [Serializable]
    public class TerrainGenRule
    {
        [Tooltip("The climate type this rule applies to.")]
        public Climates Climate;
        [Tooltip("The terrain type this rule applies to.")]
        public TerrainTypes TerrainType;
        [Tooltip("This is the weight given to the spawning of this terrain type. For example, if it has +30% chance to spawn next to the parent tile, then you'd set this to 1.3. The default value of this field is 1 (aka 100%).")]
        [Min(0f)]
        public float Weight = 1f;
    }
}
