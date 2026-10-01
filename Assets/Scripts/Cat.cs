using System.Collections.Generic;  // needed for Queue
using UnityEngine;

// The cat waiter (the Cat class from Table 2 of the doc).
// Put this on ONE object named CatWaiter. It walks to customers and the kitchen.
public class Cat : MonoBehaviour
{
    public string CatName = "Penny";
    public float MoveSpeed = 5f;

    // Optional: a small child object shown while the cat is carrying food.
    public GameObject CarryIcon;

    // The dish the cat is holding (null = paws are empty).
    public Order CarriedOrder;

    // Queue<T>: a to-do list where the first task added is the first one done.
    // It holds WaiterTargets, so it can contain customers AND the kitchen.
    Queue<WaiterTarget> tasks = new Queue<WaiterTarget>();

    WaiterTarget currentTask;  // where the cat is walking right now

    void Start()
    {
        SetCarryIcon(false);
    }

    // Called when the player clicks a customer or the kitchen counter.
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
                // A customer who already left counts as null and is skipped next frame.
                currentTask = tasks.Dequeue();
            }
            return;
        }

        // Walk toward the target a little bit each frame.
        Vector3 target = currentTask.StandPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * Time.deltaTime);

        // Close enough? Let the target decide what happens.
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            WaiterTarget arrivedAt = currentTask;
            currentTask = null;
            arrivedAt.WaiterArrived(this);
        }
    }

    public void PickUp(Order order)
    {
        CarriedOrder = order;
        SetCarryIcon(true);
        Debug.Log(CatName + " picked up " + order.ItemNames() + " for " + order.Customer.CustomerName + ".");
    }

    // Empties the cat's paws (after serving, or if the customer left).
    public void DropFood()
    {
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