using UnityEngine;

// Put this on the Customer prefab (next to the Customer component).
// It plays the right set of frames from Look based on the customer's state.
public class CustomerAnimator : MonoBehaviour
{
    // Filled in by the CustomerSpawner when the customer is created.
    public CatLook Look;

    // How fast the frames change.
    public float FramesPerSecond = 4f;

    // Below this fraction of patience, a waiting customer looks upset (0.3 = 30%).
    public float UpsetBelow = 0.3f;

    Customer customer;
    SpriteRenderer spriteRenderer;

    Sprite[] currentFrames;  // the animation playing right now
    int frameIndex;
    float frameTimer;

    void Start()
    {
        customer = GetComponent<Customer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Look == null)
        {
            return;  // no frames assigned: keep whatever sprite the prefab has
        }

        Sprite[] wanted = ChooseFrames();
        if (wanted == null || wanted.Length == 0)
        {
            return;  // that animation wasn't filled in, so leave the sprite alone
        }

        // Switched to a different animation? Start it from its first frame.
        if (wanted != currentFrames)
        {
            currentFrames = wanted;
            frameIndex = 0;
            frameTimer = 0f;
        }

        // Move to the next frame when enough time has passed, looping back to 0.
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / FramesPerSecond)
        {
            frameTimer -= 1f / FramesPerSecond;
            frameIndex = (frameIndex + 1) % currentFrames.Length;  // % = remainder, makes it loop
        }

        spriteRenderer.sprite = currentFrames[frameIndex];
    }

    // switch: pick the animation that matches what the customer is doing.
    Sprite[] ChooseFrames()
    {
        switch (customer.State)
        {
            case CustomerState.Seated:
                return Look.Order;   // paw up: "I'm ready to order!"

            case CustomerState.Eating:
                return Look.Eat;

            case CustomerState.Waiting:
            case CustomerState.Ordering:
                if (IsUpset() && Look.Upset != null && Look.Upset.Length > 0)
                {
                    return Look.Upset;
                }
                return Look.Idle;

            default:
                return Look.Idle;
        }
    }

    bool IsUpset()
    {
        return customer.Patience / customer.MaxPatience < UpsetBelow;
    }
}
