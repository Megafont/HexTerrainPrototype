using System.Collections.Generic;

using UnityEngine;


namespace HexTerrainPrototype
{
    /// <summary>
    /// This class contains extension methods for Unity's List class.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Adds the items of one list into another, but skips items that are already in the destination list.
        /// </summary>
        /// <param name="destination">The list we are adding items to.</param>
        /// <param name="rangeToAdd">The list of items to add to the destination list.</param>
        /// <typeparam name="T">The type of items in the lists.</typeparam>
        public static void AddRangeWithoutDuplicates<T>(this List<T> destination, List<T> rangeToAdd)
        {
            foreach (T item in rangeToAdd)
            {
                if (!destination.Contains(item))
                    destination.Add(item);
            } // end foreach
            
        }
        
    }
}
