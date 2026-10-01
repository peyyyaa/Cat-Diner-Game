// A single dish on the menu (class from Table 2 of the doc).
// This is a plain C# class, not a MonoBehaviour, so it isn't attached
// to any object in the scene. It's just data.
public class FoodItem
{
    public string Name;
    public decimal Price;  // decimal = precise money values (FoodPrice in the doc)

    // Constructor: runs when we write new FoodItem("Name", 45m)
    public FoodItem(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
