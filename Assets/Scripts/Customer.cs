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

    // How long they look at the menu after sitting down (a random time between these).
    public float MinMenuTime = 2f;
    public float MaxMenuTime = 4f;
    float menuTimer;

    // Optional: a little menu card in front of the cat while it decides.
    public GameObject MenuProp;

    // How many seconds the customer spends eating before paying.
    public float EatingTime = 3f;
    float eatingTimer;

    // The tip is up to this part of the bill (0.2 = 20%):
    // the more patience left when the food arrives, the bigger the tip.
    public float MaxTipPercent = 0.2f;
    float patienceWhenServed;

    // The color the customer turns as they run out of patience.
    public Color AngryColor = Color.red;

    // Optional child objects (drag them in on the prefab):
    public TMP_Text StatusLabel;          // text in the speech bubble
    public SpriteRenderer PatienceBar;    // the colored bar
    public Transform PatienceBarAnchor;   // optional: the bar shrinks toward this point
    public GameObject PatienceMeter;      // optional: the whole meter, hidden once served
    float barFullWidth;

    // List<T>: the food items this party ordered (one per cat).
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

        Transform bar = BarToScale();
        if (bar != null)
        {
            barFullWidth = bar.localScale.x;
        }

        ShowMenuProps(false);
    }

    // The thing that shrinks: the anchor if there is one, otherwise the bar itself.
    Transform BarToScale()
    {
        if (PatienceBarAnchor != null)
        {
            return PatienceBarAnchor;
        }
        if (PatienceBar != null)
        {
            return PatienceBar.transform;
        }
        return null;
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
        else if (State == CustomerState.ReadingMenu)
        {
            // Happily reading the menu: no patience lost.
            menuTimer -= Time.deltaTime;

            if (menuTimer <= 0f)
            {
                ReadyToOrder();
            }
        }
        else if (State == CustomerState.Eating)
        {
            // Happy customers don't lose patience while eating.
            eatingTimer -= Time.deltaTime;

            if (eatingTimer <= 0f)
            {
                FinishEating();
            }
        }
    }

    // Called by the GameManager right after the party is seated.
    public void SitDown()
    {
        State = CustomerState.ReadingMenu;
        menuTimer = Random.Range(MinMenuTime, MaxMenuTime);

        // Getting a seat makes them happy again.
        Patience = MaxPatience;
        UpdateColor();

        ShowMenuProps(true);
    }

    void ReadyToOrder()
    {
        State = CustomerState.Seated;  // paw up: ready to order
        ShowMenuProps(false);
        Debug.Log(DisplayName + " is ready to order!");
    }

    void ShowMenuProps(bool visible)
    {
        if (MenuProp != null)
        {
            MenuProp.SetActive(visible);
        }

        if (HasBuddy && Buddy.MenuProp != null)
        {
            Buddy.MenuProp.SetActive(visible);
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
        Transform bar = BarToScale();
        if (bar != null)
        {
            Vector3 scale = bar.localScale;
            scale.x = barFullWidth * Mathf.Max(patienceLeft, 0f);
            bar.localScale = scale;
        }

        if (PatienceBar != null)
        {
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
                // Only the cat at the front asks for a table; the rest wait their turn.
                bool isFirst = CustomerSpawner.Instance == null ||
                               !CustomerSpawner.Instance.FirstComeFirstServed ||
                               CustomerSpawner.Instance.IsFirstInLine(this);
                need = isFirst ? "Table, please!" : "Waiting in line...";
                break;
            case CustomerState.ReadingMenu:
                need = "Hmm, let me see...";
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
        SoundManager.Play(Sfx.Order);
    }

    void ServeFood()
    {
        SoundManager.Play(Sfx.Serve);

        State = CustomerState.Eating;
        eatingTimer = EatingTime;

        // Remember how happy they were when the food arrived (for the tip).
        patienceWhenServed = Mathf.Clamp01(Patience / MaxPatience);

        // Back to their normal color while they enjoy the food.
        spriteRenderer.color = startColor;
        if (HasBuddy)
        {
            buddyRenderer.color = buddyStartColor;
        }

        // Happy customers don't need a patience bar.
        if (PatienceMeter != null)
        {
            PatienceMeter.SetActive(false);
        }
        else if (PatienceBar != null)
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

    // The faster they were served, the bigger the tip.
    decimal CalculateTip(decimal bill)
    {
        decimal percent = (decimal)(MaxTipPercent * patienceWhenServed);  // cast float -> decimal
        return decimal.Round(bill * percent);  // round to whole pesos
    }

    // Done eating: leave the money (bill + tip) on the table and go home.
    void FinishEating()
    {
        decimal bill = GetOrderTotal();
        decimal tip = CalculateTip(bill);

        GameManager.Instance.CustomerFinished(this);

        if (AssignedTable != null)
        {
            // The money waits on the table until the waiter collects it.
            AssignedTable.LeaveMoney(bill, tip);
        }
        else
        {
            GameManager.Instance.CollectMoney(bill, tip);
        }

        State = CustomerState.Leaving;
        Destroy(gameObject);  // remove the customer (and their buddy) from the scene
    }

    void LeaveUnhappy()
    {
        GameManager.Instance.CustomerLeftUnhappy(this);
        SoundManager.Play(Sfx.Angry);
        Effects.Float(transform.position + Vector3.up * 1.2f, "Hmph!", new Color(0.85f, 0.2f, 0.25f));
        State = CustomerState.Leaving;

        // No money left behind: the table is free right away.
        if (AssignedTable != null)
        {
            AssignedTable.ClearDishes();
            AssignedTable.Free();
        }

        Destroy(gameObject);
    }

    // Once seated, the cat waiter goes to the table's waiter spot instead of
    // standing next to one cat. "override" replaces WaiterTarget's version.
    public override Vector3 StandPosition
    {
        get
        {
            if (AssignedTable != null)
            {
                return AssignedTable.WaiterPosition;
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
                    Order order = cat.HandOver();

                    // Put the plates on the table in front of the cats.
                    if (AssignedTable != null)
                    {
                        AssignedTable.PlaceDishes(order.Plates);
                    }

                    ServeFood();
                }
                break;

                // Any other state (e.g. still reading the menu): nothing to do.
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

            case CustomerState.ReadingMenu:
                Debug.Log(DisplayName + " is still looking at the menu.");
                break;

            case CustomerState.Seated:
                // Taking orders is the cat waiter's job.
                GameManager.Instance.Waiter.AddTask(this);
                break;

            case CustomerState.Ordering:
                Debug.Log(DisplayName + "'s " + OrderText() +
                          " is in the kitchen. Click the plates on the counter when they're ready.");
                break;

            default:
                Debug.Log(DisplayName + " is currently " + State + ".");
                break;
        }
    }
}