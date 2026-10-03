using UnityEngine;

// Put this on the CatWaiter (next to the Cat component).
// It picks the right animation from how the waiter is moving and whether it carries food.
// Side frames are drawn facing RIGHT; the Cat script already flips the sprite for left.
public class WaiterAnimator : MonoBehaviour
{
    [Header("Standing still")]
    public Sprite[] Idle;
    public Sprite[] CarryIdle;

    [Header("Walking (side frames face right)")]
    public Sprite[] WalkSide;
    public Sprite[] WalkDown;   // toward the camera
    public Sprite[] WalkUp;     // away from the camera (back view)

    [Header("Walking while carrying plates")]
    public Sprite[] CarrySide;
    public Sprite[] CarryDown;
    public Sprite[] CarryUp;

    public float FramesPerSecond = 8f;

    Cat cat;
    SpriteRenderer spriteRenderer;
    Vector3 lastPosition;

    Sprite[] currentFrames;
    int frameIndex;
    float frameTimer;

    void Start()
    {
        cat = GetComponent<Cat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    // LateUpdate runs after every Update, so the cat has already moved this frame.
    void LateUpdate()
    {
        Vector3 movement = transform.position - lastPosition;
        lastPosition = transform.position;

        Sprite[] wanted = ChooseFrames(movement);
        if (wanted == null || wanted.Length == 0)
        {
            return;  // that animation wasn't filled in: keep the current sprite
        }

        // Switched to a different animation? Start it from its first frame.
        if (wanted != currentFrames)
        {
            currentFrames = wanted;
            frameIndex = 0;
            frameTimer = 0f;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / FramesPerSecond)
        {
            frameTimer -= 1f / FramesPerSecond;
            frameIndex = (frameIndex + 1) % currentFrames.Length;  // % makes it loop
        }

        spriteRenderer.sprite = currentFrames[frameIndex];
    }

    Sprite[] ChooseFrames(Vector3 movement)
    {
        bool carrying = cat != null && cat.CarriedOrder != null;
        bool moving = movement.magnitude > 0.0005f;

        if (!moving)
        {
            return carrying ? Pick(CarryIdle, Idle) : Idle;
        }

        // Mostly sideways, or mostly up/down?
        if (Mathf.Abs(movement.x) >= Mathf.Abs(movement.y))
        {
            return carrying ? Pick(CarrySide, WalkSide) : WalkSide;
        }
        if (movement.y < 0f)
        {
            return carrying ? Pick(CarryDown, WalkDown) : WalkDown;
        }
        return carrying ? Pick(CarryUp, WalkUp) : WalkUp;
    }

    // Use the first list if it has frames, otherwise fall back to the second.
    Sprite[] Pick(Sprite[] preferred, Sprite[] fallback)
    {
        return (preferred != null && preferred.Length > 0) ? preferred : fallback;
    }
}
