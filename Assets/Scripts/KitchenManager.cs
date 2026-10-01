using System.Collections.Generic;  // needed for List
using UnityEngine;

// The kitchen counter (KitchenManager from the doc).
// Put this on a counter object that has a Sprite Renderer and a Box Collider 2D.
// It inherits from WaiterTarget, so the cat can walk to it.
public class KitchenManager : WaiterTarget
{
    public static KitchenManager Instance;

    // The counter turns this color when at least one dish is ready.
    public Color ReadyColor = Color.yellow;

    // List<Order>: every order currently cooking or waiting to be picked up.
    List<Order> orders = new List<Order>();

    SpriteRenderer spriteRenderer;
    Color normalColor;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor = spriteRenderer.color;
    }

    // Called by a Customer when the cat takes their order.
    public void StartCooking(Customer customer)
    {
        Order order = new Order(customer, customer.OrderedItems);
        orders.Add(order);
        Debug.Log("Kitchen is cooking " + order.ItemNames() + " for " +
                  customer.CustomerName + " (" + order.CookTimeLeft + "s).");
    }

    void Update()
    {
        bool anyReady = false;

        // for loop going BACKWARD, because we might remove orders while looping
        // (removing while going forward would skip items).
        for (int i = orders.Count - 1; i >= 0; i--)
        {
            Order order = orders[i];

            // The customer gave up and left, so throw their order away.
            if (order.Customer == null)
            {
                orders.RemoveAt(i);
                continue;  // skip to the next order
            }

            if (!order.IsReady)
            {
                order.CookTimeLeft -= Time.deltaTime;

                if (order.CookTimeLeft <= 0f)
                {
                    order.IsReady = true;
                    Debug.Log(order.ItemNames() + " for " + order.Customer.CustomerName +
                              " is ready! Click the counter.");
                }
            }

            if (order.IsReady)
            {
                anyReady = true;
            }
        }

        // Ternary operator: a one-line if-else. "condition ? ifTrue : ifFalse"
        spriteRenderer.color = anyReady ? ReadyColor : normalColor;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameFinished)
        {
            return;
        }

        GameManager.Instance.Waiter.AddTask(this);
    }

    // The cat reached the counter: hand over the oldest ready dish.
    public override void WaiterArrived(Cat cat)
    {
        if (cat.CarriedOrder != null)
        {
            Debug.Log(cat.CatName + "'s paws are full!");
            return;
        }

        Order readyOrder = FindReadyOrder();

        if (readyOrder == null)
        {
            Debug.Log("Nothing is ready yet.");
        }
        else
        {
            orders.Remove(readyOrder);
            cat.PickUp(readyOrder);
        }
    }

    // for loop: the first order that's finished cooking, or null if none.
    Order FindReadyOrder()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].IsReady && orders[i].Customer != null)
            {
                return orders[i];
            }
        }
        return null;
    }
}
