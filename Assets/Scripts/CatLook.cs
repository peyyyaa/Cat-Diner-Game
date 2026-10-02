using UnityEngine;

// One cat's full set of animation frames.
// [System.Serializable] lets this plain class show up in the Inspector,
// so you can drag sprites into it.
[System.Serializable]
public class CatLook
{
    public string Name = "Cat";
    public Sprite[] Idle;
    public Sprite[] Walk;
    public Sprite[] Order;
    public Sprite[] Eat;
    public Sprite[] Upset;
}
