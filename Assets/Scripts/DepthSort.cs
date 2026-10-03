using UnityEngine;

// Y-sorting: things lower on the screen are drawn in front of things higher up,
// like a 3D room seen from above at an angle.
//
// Put this on the CatWaiter, the Customer prefab, every table, and the kitchen counter.
// Each sprite keeps the Order in Layer you gave it in the Inspector (booth 0, cat 1,
// tabletop 2...), and this script adds a "base" number on top based on how low
// the object stands on the screen.
public class DepthSort : MonoBehaviour
{
    // Tick for things that move (waiter, customers). Furniture can leave it unticked.
    public bool UpdateEveryFrame = false;

    // Where this object touches the floor.
    // Furniture: tick Use Collider Bottom (the bottom edge of its collider = its front edge).
    // Characters: untick it and set Feet Offset Y to where their feet are (e.g. -0.4).
    public bool UseColliderBottom = false;
    public float FeetOffsetY = 0f;

    // Property with a private set: other scripts can read it but only this script changes it.
    public int BaseOrder { get; private set; }

    // A seated customer follows its table's depth instead of its own.
    DepthSort following;

    Renderer[] renderers;  // every sprite and text on this object and its children
    int[] offsets;         // the Order in Layer each one had in the Inspector

    const int Middle = 10000;   // keeps every number positive and far above the floor (-20)
    const int PerUnit = 100;    // how many Order in Layer steps per 1 unit of height

    void Awake()
    {
        // Remember each renderer's own Order in Layer before changing anything.
        renderers = GetComponentsInChildren<Renderer>(true);
        offsets = new int[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            offsets[i] = renderers[i].sortingOrder;
        }
    }

    void Start()
    {
        Apply();
    }

    void LateUpdate()
    {
        if (UpdateEveryFrame || following != null)
        {
            Apply();
        }
    }

    public void Apply()
    {
        if (following != null)
        {
            BaseOrder = following.BaseOrder;
        }
        else
        {
            BaseOrder = OrderForHeight(FloorY());
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sortingOrder = BaseOrder + offsets[i];
            }
        }
    }

    float FloorY()
    {
        if (UseColliderBottom)
        {
            Collider2D area = GetComponent<Collider2D>();
            if (area != null)
            {
                return area.bounds.min.y;
            }
        }
        return transform.position.y + FeetOffsetY;
    }

    // Lower on screen (smaller y) = bigger number = drawn in front.
    public static int OrderForHeight(float y)
    {
        return Middle - Mathf.RoundToInt(y * PerUnit);
    }

    // Used by a Table: the seated customer is drawn at the table's depth.
    public void Follow(DepthSort other)
    {
        following = other;
        Apply();
    }
}
