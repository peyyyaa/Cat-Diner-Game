// Links a dish name to the picture shown on its plate.
// [System.Serializable] lets you fill a list of these in the Inspector.
[System.Serializable]
public class FoodSprite
{
    public string FoodName;              // must match the name in Menu.cs exactly, e.g. "Milk Tea"
    public UnityEngine.Sprite Sprite;    // the food picture from your art
}
