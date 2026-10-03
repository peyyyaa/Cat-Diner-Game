using UnityEngine;

// Put this on the Customer prefab AND on its Buddy child.
// It plays the right set of frames from Look based on the party's state.
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

    // Set by the Table when this cat sits down.
    bool sittingSideways = false;
    bool faceLeft = false;

    Vector3 lastPosition;    // where the cat was last frame
    bool walking;            // did it move since last frame?

    Sprite[] currentFrames;  // the animation playing right now
    int frameIndex;
    float frameTimer;

    void Start()
    {
        // GetComponentInParent also checks this object itself, so this finds the
        // Customer on the leader AND on the buddy (whose Customer is on the parent).
        customer = GetComponentInParent<Customer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    // Called by the Table when this cat is seated.
    public void SitSideways(bool shouldFaceLeft)
    {
        sittingSideways = true;
        faceLeft = shouldFaceLeft;
    }

    void Update()
    {
        if (customer == null)
        {
            return;
        }

        // Side art is drawn facing right, so flip it for seats facing left.
        spriteRenderer.flipX = sittingSideways && faceLeft;

        if (Look == null)
        {
            return;  // no frames assigned: keep whatever sprite the prefab has
        }

        // Walking up the waiting line? (moved since last frame)
        walking = (transform.position - lastPosition).magnitude > 0.0005f;
        lastPosition = transform.position;

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

    // switch: pick the animation that matches what the party is doing.
    Sprite[] ChooseFrames()
    {
        switch (customer.State)
        {
            case CustomerState.Seated:
                return Pick(Look.SideOrder, Look.Order);   // paw up: "We're ready to order!"

            case CustomerState.Eating:
                return Pick(Look.SideEat, Look.Eat);

            case CustomerState.ReadingMenu:
                return Pick(Look.SideSit, Look.Idle);   // calm, looking at the menu

            case CustomerState.Waiting:
            case CustomerState.Ordering:
                // Shuffling forward in the line: use the walk frames.
                if (walking && customer.State == CustomerState.Waiting && Look.Walk != null && Look.Walk.Length > 0)
                {
                    return Look.Walk;
                }

                if (IsUpset())
                {
                    Sprite[] upset = Pick(Look.SideUpset, Look.Upset);
                    if (upset != null && upset.Length > 0)
                    {
                        return upset;
                    }
                }
                return Pick(Look.SideSit, Look.Idle);

            default:
                return Look.Idle;
        }
    }

    // Use the side-view frames while sitting at a table (if they exist),
    // otherwise fall back to the front-view frames.
    Sprite[] Pick(Sprite[] side, Sprite[] front)
    {
        if (sittingSideways && side != null && side.Length > 0)
        {
            return side;
        }
        return front;
    }

    bool IsUpset()
    {
        return customer.Patience / customer.MaxPatience < UpsetBelow;
    }
}