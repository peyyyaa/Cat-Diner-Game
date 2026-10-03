using UnityEngine;

// One cat's full set of animation frames.
// [System.Serializable] lets this plain class show up in the Inspector,
// so you can drag sprites into it.
[System.Serializable]
public class CatLook
{
    public string Name = "Cat";

    [Header("Front view")]
    public Sprite[] Idle;
    public Sprite[] Walk;
    public Sprite[] Order;
    public Sprite[] Eat;
    public Sprite[] Upset;

    // Used while the cat sits at a table. Draw these facing RIGHT;
    // the game flips them for seats on the other side.
    // Leave any of them empty and the front-view version is used instead.
    [Header("Side view at the table (optional, facing right)")]
    public Sprite[] SideSit;
    public Sprite[] SideOrder;
    public Sprite[] SideEat;
    public Sprite[] SideUpset;
}