using UnityEngine;

// Put this on the Plate prefab: a plate sprite with a Box Collider 2D,
// plus a child sprite (FoodRenderer) that shows the dish on top.
// Clicking a plate on the counter sends the waiter to pick up that order.
public class ServingPlate : WaiterTarget
{
    public SpriteRenderer FoodRenderer;   // the child that shows the food

    // Which order this plate belongs to (set by the KitchenManager).
    public Order Order;

    // The waiter picks plates up from its usual spot at the counter.
    public override Vector3 StandPosition
    {
        get { return KitchenManager.Instance.StandPosition; }
    }

    public void ShowFood(Sprite food)
    {
        if (FoodRenderer != null && food != null)
        {
            FoodRenderer.sprite = food;
        }
    }

    // Draws the plate (and the food on it) at this Order in Layer.
    public void SetOrderInLayer(int orderInLayer)
    {
        GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;

        if (FoodRenderer != null)
        {
            FoodRenderer.sortingOrder = orderInLayer + 1;
        }
    }

    // Plates can only be clicked while they sit on the counter.
    public void SetClickable(bool clickable)
    {
        Collider2D clickArea = GetComponent<Collider2D>();
        if (clickArea != null)
        {
            clickArea.enabled = clickable;
        }
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameFinished)
        {
            return;
        }

        GameManager.Instance.Waiter.AddTask(this);
    }

    // The waiter reached the counter: hand over this plate's whole order.
    public override void WaiterArrived(Cat cat)
    {
        KitchenManager.Instance.HandOrderToCat(Order, cat);
    }
}
