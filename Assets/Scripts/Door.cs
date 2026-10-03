using UnityEngine;

// Put this on the door object. It swaps to the "open" picture for a moment
// whenever Open() is called (the CustomerSpawner calls it when a customer arrives).
public class Door : MonoBehaviour
{
    public Sprite ClosedSprite;
    public Sprite OpenSprite;

    // How long the door stays open, in seconds.
    public float OpenTime = 0.6f;

    SpriteRenderer spriteRenderer;
    float openTimer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (ClosedSprite != null)
        {
            spriteRenderer.sprite = ClosedSprite;
        }
    }

    public void Open()
    {
        if (OpenSprite == null)
        {
            return;  // no open picture assigned: the door just stays as it is
        }

        spriteRenderer.sprite = OpenSprite;
        openTimer = OpenTime;
    }

    void Update()
    {
        if (openTimer > 0f)
        {
            openTimer -= Time.deltaTime;

            if (openTimer <= 0f && ClosedSprite != null)
            {
                spriteRenderer.sprite = ClosedSprite;
            }
        }
    }
}
