using System.Collections.Generic;  // needed for List
using TMPro;                       // needed for TextMeshPro text
using UnityEngine;

// Attach this to the customer prefab. The object also needs a Collider 2D
// (e.g. Circle Collider 2D) or clicks won't be detected.
// It inherits from WaiterTarget, so the cat can walk to it.
public class Customer : WaiterTarget
{
    public string CustomerName = "Customer";            // string
    public CustomerState State = CustomerState.Waiting; // enum
    public Table AssignedTable;                         // filled in when seated

    // The second cat in a party of two (a child object on the prefab).
    // The spawner turns it on or off when the customer arrives.
    public PartyMember Buddy;
    public string BuddyName = "";

    // Properties: worked out each time they're read.
    public bool HasBuddy
    {
        get { return Buddy != null && Buddy.gameObject.activeSelf; }
    }

    public int PartySize
    {
        get { return HasBuddy ? 2 : 1; }
    }

    // "Mochi" for one cat, "Mochi & Tofu" for a pair.
    public string DisplayName
    {
        get { return HasBuddy ? CustomerName + " & " + BuddyName : CustomerName; }
    }

    // Patience in seconds (float, from Table 1 of the doc).
    public float MaxPatience = 10f;
    public float Patience;   // public so you can watch it count down in the Inspector

    // How many seconds the customer spends eating before paying.
    public float EatingTime = 3f;
    float eatingTimer;

    // The color the customer turns as they run out of patience.
    public Color AngryColor = Color.red;

    // Optional child objects (drag them in on the prefab):
    // a text above the head, and a bar that shrinks as patience runs out.
    public TMP_Text StatusLabel;
    public SpriteRenderer PatienceBar;
    float barFullWidth;

    // List<T>: the food items this customer ordered.
    // (Called OrderedItems so it doesn't clash with the Order class.)
    public List<FoodItem> OrderedItems = new List<FoodItem>();

    SpriteRenderer spriteRenderer;
    Color startColor;
    SpriteRenderer buddyRenderer;
    Color buddyStartColor;

    // Start runs once, when the game begins (or when this customer is created).
    void Start()
    {
        Patience = MaxPatience;
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;

        if (Buddy != null)
        {
            buddyRenderer = Buddy.GetComponent<SpriteRenderer>();
            buddyStartColor = buddyRenderer.color;
        }

        if (PatienceBar != null)
        {
            barFullWidth = PatienceBar.transform.localScale.x;
        }
    }

    // Update runs once every frame. This replaces the doc's while loop:
    // instead of looping until something is finished (which would freeze Unity),
    // we do a tiny bit of work each frame.
    void Update()
    {
        UpdateLabel();

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

        // The buddy shares the same patience, so it gets the same tint.
        if (HasBuddy)
        {
            buddyRenderer.color = Color.Lerp(AngryColor, buddyStartColor, patienceLeft);
        }

        // Shrink the patience bar and fade it from green to red.
        if (PatienceBar != null)
        {
            Vector3 scale = PatienceBar.transform.localScale;
            scale.x = barFullWidth * Mathf.Max(patienceLeft, 0f);
            PatienceBar.transform.localScale = scale;
            PatienceBar.color = Color.Lerp(Color.red, Color.green, patienceLeft);
        }
    }

    // switch: show a short message above the customer based on their state.
    void UpdateLabel()
    {
        if (StatusLabel == null)
        {
            return;
        }

        string need;
        switch (State)
        {
            case CustomerState.Waiting:
                need = "Table, please!";
                break;
            case CustomerState.Seated:
                need = "Ready to order!";
                break;
            case CustomerState.Ordering:
                need = OrderText();
                break;
            case CustomerState.Eating:
                need = "Yum!";
                break;
            default:
                need = "";
                break;
        }

        StatusLabel.text = DisplayName + "\n" + need;
    }

    // All ordered dishes as one string, e.g. "Milk Tea, Tuna Pasta".
    string OrderText()
    {
        string text = "";
        foreach (FoodItem item in OrderedItems)
        {
            if (text != "")
            {
                text += ", ";
            }
            text += item.Name;
        }
        return text;
    }

    void TakeOrder()
    {
        OrderedItems.Clear();

        // for loop: every cat in the party orders one dish.
        for (int i = 0; i < PartySize; i++)
        {
            OrderedItems.Add(Menu.GetRandomItem());
        }

        State = CustomerState.Ordering;  // now waiting for their food

        // Being attended to makes the customer happy again.
        Patience = MaxPatience;
        UpdateColor();

        Debug.Log(DisplayName + " ordered " + OrderText() +
                  ". Total: ₱" + GetOrderTotal());

        // Send the order to the kitchen.
        KitchenManager.Instance.StartCooking(this);
    }

    void ServeFood()
    {
        State = CustomerState.Eating;
        eatingTimer = EatingTime;

        // Back to their normal color while they enjoy the food.
        spriteRenderer.color = startColor;
        if (HasBuddy)
        {
            buddyRenderer.color = buddyStartColor;
        }

        // Happy customers don't need a patience bar.
        if (PatienceBar != null)
        {
            PatienceBar.gameObject.SetActive(false);
        }

        Debug.Log("Served " + OrderText() + " to " + DisplayName + ". Enjoy!");
    }

    // Adds up the price of everything in the order using the menu's price list.
    public decimal GetOrderTotal()
    {
        decimal total = 0m;  // local variable: only exists inside this method

        foreach (FoodItem item in OrderedItems)
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

    // Once seated, the cat waiter goes to the table's waiter spot instead of
    // standing next to one cat. "override" replaces WaiterTarget's version.
    public override Vector3 StandPosition
    {
        get
        {
            if (AssignedTable != null)
            {
                return AssignedTable.transform.position + AssignedTable.WaiterOffset;
            }
            return base.StandPosition;  // "base" = WaiterTarget's original version
        }
    }

    // Called by the Cat when it reaches this customer.
    // "override" = this is Customer's version of WaiterTarget's WaiterArrived.
    public override void WaiterArrived(Cat cat)
    {
        switch (State)
        {
            case CustomerState.Seated:
                TakeOrder();
                break;

            case CustomerState.Ordering:
                // Only serve if the cat is holding THIS customer's food.
                if (cat.CarriedOrder != null && cat.CarriedOrder.Customer == this)
                {
                    cat.DropFood();
                    ServeFood();
                }
                break;

                // Any other state (e.g. already eating): nothing to do.
        }
    }

    // Unity calls this automatically when the object is clicked.
    void OnMouseDown()
    {
        HandleClick();
    }

    // What a click does. Public so the Buddy can pass its clicks here too.
    public void HandleClick()
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
                // Taking orders is the cat waiter's job.
                GameManager.Instance.Waiter.AddTask(this);
                break;

            case CustomerState.Ordering:
                Debug.Log(DisplayName + "'s " + OrderText() +
                          " is in the kitchen. Click the counter when it turns yellow.");
                break;

            default:
                Debug.Log(CustomerName + " is currently " + State + ".");
                break;
        }
    }
}