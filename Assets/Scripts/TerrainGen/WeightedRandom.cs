using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

namespace HexTerrainPrototype
{
    /// <summary>
    /// This class makes a random selection from a list of weighted items.
    /// </summary>
    public static class WeightedRandom
    {
        public static bool Draw<T>(List<WeightedItem<T>> items, out T selectedItem)
        {
            // Calculate the total of all the item weights. This will be the upper bound for our random numbers.
            float totalWeight = GetTotalWeight(items);

            // Draw a random number from 0 to the upper bound we just determined.
            float randomValue = Random.Range(0, totalWeight);

            
            // Determine which item was selected.
            foreach (WeightedItem<T> item in items)
            {
                randomValue -= item.Weight;
                if (randomValue < 0)
                {
                    selectedItem = item.Item;
                    return true;
                }

            } // foreach WeightedItem


            selectedItem = default(T);
            return false;
        }

        /// <summary>
        /// Calculates the sum of the weights of all items in the passed in list. This value is used as the upper bound for random number generation in the Draw() function.
        /// </summary>
        /// <param name="items">The list of items to get the total weight of.</param>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <returns></returns>
        public static float GetTotalWeight<T>(List<WeightedItem<T>> items)
        {
            // Calculate the total of all the item weights. This will be the upper bound for our random numbers.
            float totalWeight = 0;
            foreach (WeightedItem<T> item in items)
            {
                totalWeight += item.Weight;
            }

            return totalWeight;
        }
        
        public static void DEBUG_ExecuteWeightedRandomTest<T>(List<WeightedItem<T>> items, int numDrawings = 10000)
        {
            Dictionary<T, WeightedItemTestData> itemTestDataLookup = new Dictionary<T, WeightedItemTestData>();
            
            // Calculate the total of all the item weights. This will be the upper bound for our random numbers.
            float totalWeight = GetTotalWeight(items);

            
            // Do a bunch of random number drawings.
            for (int i = 0; i < numDrawings; i++)
            {
                bool result = Draw<T>(items, out T selectedItem);

                if (result)
                {
                    if (!itemTestDataLookup.ContainsKey(selectedItem))
                        itemTestDataLookup.Add(selectedItem, new WeightedItemTestData());

                    WeightedItemTestData itemTestData = itemTestDataLookup[selectedItem];
                    itemTestData.TimesSelected++;
                    itemTestDataLookup[selectedItem] = itemTestData;
                }
            }
            
            
            // Calculate the overall percentage chance of each item based on the results, and display the test results.
            Debug.Log($"WEIGHTED RANDOM TEST RESULTS ({numDrawings} drawings): ");
            Debug.Log(new string('-', 256));
            
            int index = 0;
            foreach (WeightedItem<T> item in items)
            {
                WeightedItemTestData itemTestData = itemTestDataLookup[item.Item];
                
                itemTestData.OverallChancePercentage = ((float) itemTestData.TimesSelected / numDrawings) * 100f;
                Debug.Log($"Items[{index,5}]    Weight: {item.Weight,8:F3}    Overall Probablity: {itemTestData.OverallChancePercentage,8:F3}%    Times Selected: {itemTestData.TimesSelected, 4}");

                index++;
            }
            
            Debug.Log(new string('-', 256));
            
        }

        public static void DEBUG_RunWeightedRandomTests(int numDrawings = 10000)
        {
            List<WeightedItem<string>> items = new List<WeightedItem<string>>()
            {
                new WeightedItem<string>() { Item = "A", Weight = 1f },
                new WeightedItem<string>() { Item = "B", Weight = 1f },
                new WeightedItem<string>() { Item = "C", Weight = 1f },
                new WeightedItem<string>() { Item = "D", Weight = 1f },
                new WeightedItem<string>() { Item = "E", Weight = 1f },
            };

            DEBUG_ExecuteWeightedRandomTest(items, numDrawings);
            
            
            
            List<WeightedItem<string>> items2 = new List<WeightedItem<string>>()
            {
                new WeightedItem<string>() { Item = "A", Weight = 1f },
                new WeightedItem<string>() { Item = "B", Weight = 1.2f },
                new WeightedItem<string>() { Item = "C", Weight = 1f },
                new WeightedItem<string>() { Item = "D", Weight = 1f },
                new WeightedItem<string>() { Item = "E", Weight = 1f },
            };

            DEBUG_ExecuteWeightedRandomTest(items2, numDrawings);            
            
            
            
            List<WeightedItem<string>> items3 = new List<WeightedItem<string>>()
            {
                new WeightedItem<string>() { Item = "A", Weight = 1f },
                new WeightedItem<string>() { Item = "B", Weight = 0.8f },
                new WeightedItem<string>() { Item = "C", Weight = 1f },
                new WeightedItem<string>() { Item = "D", Weight = 1.2f },
                new WeightedItem<string>() { Item = "E", Weight = 1f },
            };

            DEBUG_ExecuteWeightedRandomTest(items3, numDrawings);               
        }
    }

    public struct WeightedItem<T>
    {
        public T Item;
        public float Weight;
    }

    public struct WeightedItemTestData
    {
        public float OverallChancePercentage;
        public int TimesSelected;
    }
}
