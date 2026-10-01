using System.Collections.Generic;  // needed for List
using UnityEngine;

// Attach this to each customer object. The object also needs a Collider 2D
// (e.g. Circle Collider 2D) or clicks won't be detected.
public class Customer : MonoBehaviour
{
    public string CustomerName = "Customer";            // string
    public CustomerState State = CustomerState.Waiting; // enum
    public Table AssignedTable;                         // filled in when seated

    // Patience in seconds (float, from Table 1 of the doc).
    public float MaxPatience = 10f;
    public float Patience;   // public so you can watch it count down in the Inspector

    // How many seconds the customer spends eating before paying.
    public float EatingTime = 3f;
    float eatingTimer;

    // The color the customer turns as they run out of patience.
    public Color AngryColor = Color.red;

    // List<T>: the food items this customer ordered (Order in the doc).
    public List<FoodItem> Order = new List<FoodItem>();

    SpriteRenderer spriteRenderer;
    Color startColor;

    // Start runs once, when the game begins (or when this customer is created).
    void Start()
    {
        Patience = MaxPatience;
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
    }

    // Update runs once every frame. This replaces the doc's while loop:
    // instead of looping until something is finished (which would freeze Unity),
    // we do a tiny bit of work each frame.
    void Update()
    {
        // Patience drains while the customer is waiting for a table,
        // waiting to order, or waiting for their food.
        if (State == CustomerState.Waiting ||
            State == CustomerState.Seated ||
            State == CustomerState.Ordering)
        {
            // Time.deltaTime = seconds since the last frame, so this drains 1 per second.
            Patience -= Time.deltaTime;
            UpdateColor();

            if (Patience <= 0f)
            {
                LeaveUnhappy();
            }
        }
        else if (State == CustomerState.Eating)
        {
            // Happy customers don't lose patience while eating.
            eatingTimer -= Time.deltaTime;

            if (eatingTimer <= 0f)
            {
                PayAndLeave();
            }
        }
    }

    // Blend from the starting color toward AngryColor as patience runs out.
    void UpdateColor()
    {
        float patienceLeft = Patience / MaxPatience;  // 1 = full, 0 = empty
        spriteRenderer.color = Color.Lerp(AngryColor, startColor, patienceLeft);
    }

    void TakeOrder()
    {
        Order.Clear();
        Order.Add(Menu.GetRandomItem());

        State = CustomerState.Ordering;  // now waiting for their food

        // Being attended to makes the customer happy again.
        Patience = MaxPatience;
        UpdateColor();

        Debug.Log(CustomerName + " ordered " + Order[0].Name +
                  ". Total: ₱" + GetOrderTotal());
    }

    void ServeFood()
    {
        State = CustomerState.Eating;
        eatingTimer = EatingTime;

        // Back to their normal color while they enjoy the food.
        spriteRenderer.color = startColor;

        Debug.Log("Served " + Order[0].Name + " to " + CustomerName + ". Enjoy!");
    }

    // Adds up the price of everything in the order using the menu's price list.
    public decimal GetOrderTotal()
    {
        decimal total = 0m;  // local variable: only exists inside this method

        foreach (FoodItem item in Order)
        {
            total += Menu.MenuPrices[item.Name];
        }

        return total;
    }

    void PayAndLeave()
    {
        GameManager.Instance.ReceivePayment(this, GetOrderTotal());
        Leave();
    }

    void LeaveUnhappy()
    {
        GameManager.Instance.CustomerLeftUnhappy(this);
        Leave();
    }

    // Shared by both ways of leaving: give the table back and disappear.
    void Leave()
    {
        State = CustomerState.Leaving;

        if (AssignedTable != null)
        {
            AssignedTable.Free();
        }

        Destroy(gameObject);  // remove the customer from the scene
    }

    // Unity calls this automatically when the object is clicked.
    void OnMouseDown()
    {
        // Ignore clicks once the game is over.
        if (GameManager.Instance.GameFinished)
        {
            return;
        }

        // switch statement: what a click does depends on the customer's state
        // (same idea as Figure 4 in the doc).
        switch (State)
        {
            case CustomerState.Waiting:
                GameManager.Instance.TrySeatCustomer(this);
                break;

            case CustomerState.Seated:
                TakeOrder();
                break;

            case CustomerState.Ordering:
                ServeFood();
                break;

            default:
                Debug.Log(CustomerName + " is currently " + State + ".");
                break;
        }
    }
}