using TMPro;        // needed for TextMeshPro text
using UnityEngine;

// Put this on the Money prefab (a coin or cash sprite with a Collider 2D).
// Customers leave it on their table after eating. Click it and the waiter
// collects it: only then is the money added to your total.
public class TableMoney : WaiterTarget
{
    // Optional: a small text showing how much is here.
    public TMP_Text AmountLabel;

    // decimal: precise money values (set by Setup, not in the Inspector).
    public decimal Bill;
    public decimal Tip;

    Table table;

    // Called by the Table right after the money is created.
    public void Setup(Table fromTable, decimal bill, decimal tip)
    {
        table = fromTable;
        Bill = bill;
        Tip = tip;

        // Draw on top of the table AND the plates (plates use top+1, their food top+2).
        int top = (fromTable != null) ? fromTable.TopOrder : 0;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = top + 3;
        }

        if (AmountLabel != null)
        {
            AmountLabel.GetComponent<Renderer>().sortingOrder = top + 4;
        }

        Effects.Pop(gameObject);  // appear with a little bounce

        if (AmountLabel != null)
        {
            AmountLabel.text = "PHP " + (bill + tip);

            if (tip > 0m)
            {
                AmountLabel.text += "\n+" + tip + " tip!";
            }
        }
    }

    // The waiter stands at the table's waiter spot to collect.
    public override Vector3 StandPosition
    {
        get
        {
            if (table != null)
            {
                return table.WaiterPosition;
            }
            return base.StandPosition;
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

    // The waiter reached the table: collect the money and clear the table.
    public override void WaiterArrived(Cat cat)
    {
        GameManager.Instance.CollectMoney(Bill, Tip);

        // Sounds and a floating "+PHP" so the player sees what they earned.
        SoundManager.Play(Sfx.Coin);
        Effects.Float(transform.position + Vector3.up * 0.5f, "+PHP " + (Bill + Tip), new Color(0.95f, 0.75f, 0.2f));
        if (Tip > 0m)
        {
            SoundManager.Play(Sfx.Tip);
            Effects.Float(transform.position + Vector3.up * 0.95f, "+" + Tip + " tip!", new Color(0.95f, 0.45f, 0.6f));
        }

        if (table != null)
        {
            table.ClearDishes();
            table.Free();  // the table can be used again
        }

        Destroy(gameObject);
    }
}