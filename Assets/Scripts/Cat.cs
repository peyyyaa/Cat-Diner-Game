using System.Collections.Generic;  // needed for Queue
using UnityEngine;

// The cat waiter (the Cat class from Table 2 of the doc).
// Put this on ONE object named CatWaiter. It walks to customers and helps them.
public class Cat : MonoBehaviour
{
    public string CatName = "Penny";
    public float MoveSpeed = 5f;

    // Where the cat stands, relative to the customer it's helping.
    public Vector3 StandOffset = new Vector3(-0.9f, 0f, 0f);

    // Queue<T>: a to-do list where the first task added is the first one done.
    // Each task is "go to this customer and do whatever they need next".
    Queue<Customer> tasks = new Queue<Customer>();

    Customer currentTask;  // the customer the cat is walking to right now

    // Called by a Customer when the player clicks them.
    public void AddTask(Customer customer)
    {
        // Don't add the same customer twice if the player double-clicks.
        if (customer == currentTask || tasks.Contains(customer))
        {
            return;
        }

        tasks.Enqueue(customer);
        Debug.Log(CatName + " will go to " + customer.CustomerName + ". Tasks waiting: " + tasks.Count);
    }

    void Update()
    {
        // No current task? Take the next one from the queue, if there is one.
        // (A customer who already left counts as null, so they're skipped too.)
        if (currentTask == null)
        {
            if (tasks.Count > 0)
            {
                currentTask = tasks.Dequeue();
            }
            return;
        }

        // Walk toward the customer a little bit each frame.
        Vector3 target = currentTask.transform.position + StandOffset;
        transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * Time.deltaTime);

        // Close enough? Do the task and get ready for the next one.
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentTask.WaiterArrived();
            currentTask = null;
        }
    }
}
