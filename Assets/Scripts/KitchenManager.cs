using System.Collections.Generic;  // needed for List
using TMPro;                       // needed for TextMeshPro text
using UnityEngine;

// The kitchen counter (KitchenManager from the doc).
// Put this on a counter object that has a Sprite Renderer and a Box Collider 2D.
// It inherits from WaiterTarget, so the cat can walk to it.
public class KitchenManager : WaiterTarget
{
    public static KitchenManager Instance;

    // The counter turns this color when at least one dish is ready.
    public Color ReadyColor = Color.yellow;

    // Optional text on the counter showing what's cooking and ready.
    public TMP_Text StatusLabel;

    // Optional: the plate that appears on the counter when a dish is ready.
    // Leave empty to keep the old behavior (counter turns yellow, click the counter).
    public ServingPlate PlatePrefab;

    // Empty child objects on the counter where ready plates are put down.
    public Transform[] PlateSpots;

    // Which food picture to show on the plate for each dish.
    public FoodSprite[] FoodSprites;

    // Gap between plates when one order has more than one dish.
    public float PlateSpacing = 0.4f;

    // List<Order>: every order currently cooking or waiting to be picked up.
    List<Order> orders = new List<Order>();

    // array: which order is sitting on each plate spot (null = spot is free).
    Order[] spotUsedBy;

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

        int spotCount = (PlateSpots == null) ? 0 : PlateSpots.Length;
        spotUsedBy = new Order[spotCount];
    }

    // Called by a Customer when the cat takes their order.
    public void StartCooking(Customer customer)
    {
        Order order = new Order(customer, customer.OrderedItems);
        orders.Add(order);
        Debug.Log("Kitchen is cooking " + order.ItemNames() + " for " +
                  customer.DisplayName + " (" + order.CookTimeLeft + "s).");
    }

    void Update()
    {
        bool anyReady = false;
        int cookingCount = 0;
        int readyCount = 0;

        // for loop going BACKWARD, because we might remove orders while looping
        // (removing while going forward would skip items).
        for (int i = orders.Count - 1; i >= 0; i--)
        {
            Order order = orders[i];

            // The customer gave up and left, so throw their order away.
            if (order.Customer == null)
            {
                RemoveOrder(order);
                continue;  // skip to the next order
            }

            if (!order.IsReady)
            {
                order.CookTimeLeft -= Time.deltaTime;

                if (order.CookTimeLeft <= 0f)
                {
                    order.IsReady = true;
                    ShowPlates(order);
                    Debug.Log(order.ItemNames() + " for " + order.Customer.DisplayName +
                              " is ready! Click the plates on the counter.");
                }
            }

            if (order.IsReady)
            {
                anyReady = true;
                readyCount++;
            }
            else
            {
                cookingCount++;
            }
        }

        // Ternary operator: a one-line if-else. "condition ? ifTrue : ifFalse"
        spriteRenderer.color = anyReady ? ReadyColor : normalColor;

        if (StatusLabel != null)
        {
            StatusLabel.text = "Kitchen\nCooking: " + cookingCount + "  Ready: " + readyCount;
        }
    }

    // Puts one plate per dish on a free spot of the counter.
    void ShowPlates(Order order)
    {
        if (PlatePrefab == null)
        {
            return;  // no plate prefab: the counter just turns yellow like before
        }

        Vector3 basePosition = transform.position;
        int spot = FindFreeSpot();

        if (spot != -1)
        {
            spotUsedBy[spot] = order;
            order.CounterSpot = spot;
            basePosition = PlateSpots[spot].position;
        }

        for (int i = 0; i < order.Items.Count; i++)
        {
            Vector3 position = basePosition + new Vector3(i * PlateSpacing, 0f, 0f);

            // Instantiate = make a copy of the plate prefab in the scene.
            ServingPlate plate = Instantiate(PlatePrefab, position, Quaternion.identity);
            plate.Order = order;
            plate.ShowFood(SpriteFor(order.Items[i]));
            plate.SetOrderInLayer(3);  // just above the counter
            plate.SetClickable(true);
            order.Plates.Add(plate);
        }
    }

    // for loop: the first plate spot with nothing on it, or -1 if all are full.
    int FindFreeSpot()
    {
        for (int i = 0; i < spotUsedBy.Length; i++)
        {
            if (spotUsedBy[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    void FreeSpot(Order order)
    {
        int spot = order.CounterSpot;

        if (spot >= 0 && spot < spotUsedBy.Length && spotUsedBy[spot] == order)
        {
            spotUsedBy[spot] = null;
        }

        order.CounterSpot = -1;
    }

    // Removes an order completely (its customer left): plates disappear too.
    void RemoveOrder(Order order)
    {
        orders.Remove(order);
        FreeSpot(order);

        foreach (ServingPlate plate in order.Plates)
        {
            if (plate != null)
            {
                Destroy(plate.gameObject);
            }
        }
        order.Plates.Clear();
    }

    // foreach: look up the picture for a dish by its name.
    public Sprite SpriteFor(FoodItem item)
    {
        if (FoodSprites == null)
        {
            return null;
        }

        foreach (FoodSprite foodSprite in FoodSprites)
        {
            if (foodSprite.FoodName == item.Name)
            {
                return foodSprite.Sprite;
            }
        }
        return null;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameFinished)
        {
            return;
        }

        GameManager.Instance.Waiter.AddTask(this);
    }

    // The counter itself was clicked: hand over the oldest ready order.
    public override void WaiterArrived(Cat cat)
    {
        Order readyOrder = FindReadyOrder();

        if (readyOrder == null)
        {
            Debug.Log("Nothing is ready yet.");
            return;
        }

        HandOrderToCat(readyOrder, cat);
    }

    // Gives a ready order (and its plates) to the waiter.
    public void HandOrderToCat(Order order, Cat cat)
    {
        if (cat.CarriedOrder != null)
        {
            Debug.Log(cat.CatName + "'s paws are full!");
            return;
        }

        if (order == null || !orders.Contains(order) || !order.IsReady)
        {
            Debug.Log("That order was already picked up.");
            return;
        }

        orders.Remove(order);
        FreeSpot(order);
        cat.PickUp(order);
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