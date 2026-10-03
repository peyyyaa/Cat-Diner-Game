using System.Collections.Generic;  // needed for Queue
using UnityEngine;

// The cat waiter (the Cat class from Table 2 of the doc).
// Put this on ONE object named CatWaiter. It walks to customers, plates, money and the kitchen.
public class Cat : MonoBehaviour
{
    public string CatName = "Penny";
    public float MoveSpeed = 5f;

    // Tick this if your cat picture faces right; untick if it faces left.
    // The cat flips to face the direction it's walking.
    public bool ArtFacesRight = true;
    SpriteRenderer spriteRenderer;

    // Optional: a small child object shown while carrying food (only used if there are no plates).
    public GameObject CarryIcon;

    // Where carried plates sit, relative to the cat.
    public Vector3 CarryOffset = new Vector3(0f, 0.6f, 0f);

    // Walking around things: put the counter (and anything else solid) on a layer
    // named "Obstacle", then choose that layer here.
    public LayerMask Obstacles;

    // Empty objects placed around the obstacles (e.g. near the counter's corners).
    // When the straight path is blocked, the cat walks via one of these.
    public Transform[] WalkPoints;

    // The dish the cat is holding (null = paws are empty).
    public Order CarriedOrder;

    // Queue<T>: a to-do list where the first task added is the first one done.
    Queue<WaiterTarget> tasks = new Queue<WaiterTarget>();

    WaiterTarget currentTask;  // where the cat is walking right now
    Transform detour;          // a walk point to visit first, to get around something

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetCarryIcon(false);
    }

    // Called when the player clicks a customer, a plate, money, or the kitchen counter.
    public void AddTask(WaiterTarget target)
    {
        // Don't add the same place twice if the player double-clicks.
        if (target == currentTask || tasks.Contains(target))
        {
            return;
        }

        tasks.Enqueue(target);
        Debug.Log(CatName + " will go to " + target.name + ". Tasks waiting: " + tasks.Count);
    }

    void Update()
    {
        // No current task? Decide what to do next.
        if (currentTask == null)
        {
            detour = null;

            if (CarriedOrder != null)
            {
                // Carrying food always comes first: deliver it straight away.
                if (CarriedOrder.Customer != null)
                {
                    currentTask = CarriedOrder.Customer;
                }
                else
                {
                    Debug.Log("The customer left, so the " + CarriedOrder.ItemNames() + " went to waste.");
                    DropFood();
                }
            }
            else if (tasks.Count > 0)
            {
                // Something that no longer exists counts as null and is skipped next frame.
                currentTask = tasks.Dequeue();
            }
            return;
        }

        Vector3 destination = currentTask.StandPosition;

        // Something solid in the way? Walk via a walk point first.
        if (detour == null && IsBlocked(transform.position, destination))
        {
            detour = FindDetour(destination);
        }

        Vector3 next = (detour != null) ? detour.position : destination;
        next.z = transform.position.z;  // stay on the same 2D plane

        Face(next);
        transform.position = Vector3.MoveTowards(transform.position, next, MoveSpeed * Time.deltaTime);

        // Close enough?
        if (Vector3.Distance(transform.position, next) < 0.05f)
        {
            if (detour != null)
            {
                detour = null;  // reached the walk point: carry on toward the real destination
                return;
            }

            // Reached the destination: let the target decide what happens.
            WaiterTarget arrivedAt = currentTask;
            currentTask = null;
            arrivedAt.WaiterArrived(this);
        }
    }

    // Flip to face the way we're walking (only if moving sideways noticeably).
    void Face(Vector3 point)
    {
        float sideways = point.x - transform.position.x;
        if (Mathf.Abs(sideways) > 0.01f)
        {
            bool movingLeft = sideways < 0f;
            spriteRenderer.flipX = ArtFacesRight ? movingLeft : !movingLeft;
        }
    }

    // Is there an obstacle on the straight line between two points?
    bool IsBlocked(Vector3 from, Vector3 to)
    {
        if (Obstacles.value == 0)
        {
            return false;  // no obstacle layer chosen: always walk straight
        }

        return Physics2D.Linecast(from, to, Obstacles).collider != null;
    }

    // Picks the walk point that gets around the obstacle.
    Transform FindDetour(Vector3 destination)
    {
        if (WalkPoints == null)
        {
            return null;
        }

        Transform best = null;
        float bestDistance = float.MaxValue;

        // First choice: a point with a clear path from here AND a clear path to the destination.
        foreach (Transform point in WalkPoints)
        {
            if (point == null) continue;

            if (!IsBlocked(transform.position, point.position) && !IsBlocked(point.position, destination))
            {
                float distance = Vector3.Distance(transform.position, point.position) +
                                 Vector3.Distance(point.position, destination);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }
        }

        if (best != null)
        {
            return best;
        }

        // Otherwise: any point we can reach that gets us closer to the destination.
        foreach (Transform point in WalkPoints)
        {
            if (point == null) continue;

            bool notWhereWeStand = Vector3.Distance(transform.position, point.position) > 0.1f;
            if (notWhereWeStand && !IsBlocked(transform.position, point.position))
            {
                float distance = Vector3.Distance(point.position, destination);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }
        }

        return best;  // may be null: then the cat just walks straight
    }

    // Takes an order from the kitchen and carries its plates.
    public void PickUp(Order order)
    {
        CarriedOrder = order;

        for (int i = 0; i < order.Plates.Count; i++)
        {
            ServingPlate plate = order.Plates[i];
            if (plate == null) continue;

            plate.SetClickable(false);
            plate.transform.SetParent(transform);  // the plate now moves with the cat
            plate.transform.localPosition = CarryOffset + new Vector3(0f, i * 0.15f, 0f);
            plate.SetOrderInLayer(6 + i * 2);       // in front of the waiter
        }

        // No plate art? Show the simple carry icon instead.
        SetCarryIcon(order.Plates.Count == 0);

        Debug.Log(CatName + " picked up " + order.ItemNames() + " for " + order.Customer.DisplayName + ".");
    }

    // Gives the carried order to the customer (the plates go to their table).
    public Order HandOver()
    {
        Order order = CarriedOrder;
        CarriedOrder = null;

        if (order != null)
        {
            foreach (ServingPlate plate in order.Plates)
            {
                if (plate != null)
                {
                    plate.transform.SetParent(null);  // stop moving with the cat
                }
            }
        }

        SetCarryIcon(false);
        return order;
    }

    // Throws the carried food away (the customer left before it arrived).
    public void DropFood()
    {
        if (CarriedOrder != null)
        {
            foreach (ServingPlate plate in CarriedOrder.Plates)
            {
                if (plate != null)
                {
                    Destroy(plate.gameObject);
                }
            }
        }

        CarriedOrder = null;
        SetCarryIcon(false);
    }

    void SetCarryIcon(bool visible)
    {
        if (CarryIcon != null)
        {
            CarryIcon.SetActive(visible);
        }
    }
}