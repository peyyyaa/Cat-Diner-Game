using System.Collections.Generic;  // needed for List

// One customer's order while it's in the kitchen (Order class from the doc).
// A plain C# class: it isn't attached to any object, it's just data.
public class Order
{
    public Customer Customer;      // who ordered it
    public List<FoodItem> Items;   // what they ordered
    public float CookTimeLeft;     // seconds until it's ready
    public bool IsReady;

    // The plates that appear on the counter when it's ready (one per dish).
    public List<ServingPlate> Plates = new List<ServingPlate>();

    // Which spot on the counter the plates are sitting on (-1 = none).
    public int CounterSpot = -1;

    public Order(Customer customer, List<FoodItem> items)
    {
        Customer = customer;
        Items = new List<FoodItem>(items);  // copy the list

        // Cooking time = the cook times of all items added together.
        CookTimeLeft = 0f;
        foreach (FoodItem item in Items)
        {
            CookTimeLeft += item.CookTime;
        }

        IsReady = false;
    }

    // Returns the item names as one string, e.g. "Milk Tea, Tuna Pasta".
    public string ItemNames()
    {
        string names = "";
        foreach (FoodItem item in Items)
        {
            if (names != "")
            {
                names += ", ";
            }
            names += item.Name;
        }
        return names;
    }
}