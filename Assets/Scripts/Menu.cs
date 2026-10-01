using System.Collections.Generic;  // needed for Dictionary
using UnityEngine;                 // needed for Random

// The diner's menu. "static" means there's only one menu for the whole game,
// and any script can use it directly (Menu.GetRandomItem()) without
// attaching it to an object.
public static class Menu
{
    // array: a fixed collection of the dishes (MenuItems in the doc).
    // The "m" after each price tells C# it's a decimal; the "f" after
    // each cook time (in seconds) tells C# it's a float.
    public static FoodItem[] MenuItems =
    {
        new FoodItem("Fish Sandwich", 45m, 4f),
        new FoodItem("Tuna Pasta", 60m, 6f),
        new FoodItem("Milk Tea", 35m, 2f)
    };

    // Dictionary<TKey, TValue>: look up a price by dish name (MenuPrices in the doc).
    // Example: MenuPrices["Tuna Pasta"] gives 60.
    public static Dictionary<string, decimal> MenuPrices = BuildPriceList();

    static Dictionary<string, decimal> BuildPriceList()
    {
        Dictionary<string, decimal> prices = new Dictionary<string, decimal>();

        // foreach loop: go through every dish and record its price.
        foreach (FoodItem item in MenuItems)
        {
            prices[item.Name] = item.Price;
        }

        return prices;
    }

    public static FoodItem GetRandomItem()
    {
        // Random.Range with whole numbers: the max is NOT included,
        // so this gives 0, 1, or 2 for a 3-item menu.
        int index = Random.Range(0, MenuItems.Length);
        return MenuItems[index];
    }
}