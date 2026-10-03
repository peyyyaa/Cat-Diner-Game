using UnityEngine;

// Put this on ONE empty object named GameManager.
public class GameManager : MonoBehaviour
{
    // A shortcut so any script can reach the GameManager by writing
    // GameManager.Instance (a static member: it belongs to the class, not an object).
    public static GameManager Instance;

    // Drag your table objects into this array in the Inspector.
    public Table[] Tables;

    // Drag your CatWaiter object into this slot in the Inspector.
    public Cat Waiter;

    // Optional: the money customers leave on the table (drag the Money prefab here).
    // Leave empty and money is counted straight away when customers finish eating.
    public TableMoney MoneyPrefab;

    // How long one game session lasts, in seconds.
    public float SessionLength = 90f;
    public float TimeLeft;

    // bool: once true, the session is over (gameFinished in the doc's do-while example).
    public bool GameFinished = false;

    // decimal for precise money values. Unity's Inspector can't display
    // decimals, but it now shows on screen through the UIManager.
    public decimal Money = 0m;

    // How much of the money came from tips.
    public decimal TotalTips = 0m;

    // How many customers paid and left happy.
    public int CustomersServed = 0;

    // How many customers walked out because they waited too long.
    public int LostCustomers = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        TimeLeft = SessionLength;
        Time.timeScale = 1f;  // make sure time is running when the game starts
    }

    // This is the doc's do-while game loop, done the Unity way:
    // Unity keeps calling Update every frame, and we check whether
    // the session should end, instead of writing our own loop.
    void Update()
    {
        if (GameFinished)
        {
            return;  // nothing left to do
        }

        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            EndGame();
        }
    }

    void EndGame()
    {
        GameFinished = true;

        // Freezes everything that uses Time.deltaTime: patience, eating, spawning.
        Time.timeScale = 0f;

        Debug.Log("Game over! Money: ₱" + Money + " | Served: " + CustomersServed +
                  " | Lost: " + LostCustomers);
    }

    public void TrySeatCustomer(Customer customer)
    {
        // First come, first served: only the cat at the front of the line can be seated.
        CustomerSpawner spawner = CustomerSpawner.Instance;
        if (spawner != null && spawner.FirstComeFirstServed && !spawner.IsFirstInLine(customer))
        {
            Customer first = spawner.FirstInLine();
            Debug.Log("Please wait your turn! " + (first != null ? first.DisplayName : "Someone") +
                      " is first in line.");
            return;
        }

        Table freeTable = FindFreeTable(customer.PartySize);

        // if-else: seat the customer, or tell them to keep waiting.
        if (freeTable != null)
        {
            freeTable.Seat(customer);
            customer.AssignedTable = freeTable;
            customer.SitDown();  // they look at the menu first
            Debug.Log(customer.DisplayName + " was seated at " + freeTable.name + ".");
        }
        else
        {
            Debug.Log("No free table for a party of " + customer.PartySize + ". " +
                      customer.DisplayName + " keeps waiting.");
        }
    }

    // Called by a Customer when they finish eating and leave their money on the table.
    public void CustomerFinished(Customer customer)
    {
        CustomersServed++;
        Debug.Log(customer.DisplayName + " finished eating and left money on the table. Served: " + CustomersServed);
    }

    // Called when the waiter collects the money from a table (like Figure 3 in the doc).
    // Only now is the money added to the total.
    public void CollectMoney(decimal bill, decimal tip)
    {
        Money += bill + tip;
        TotalTips += tip;
        Debug.Log("Collected ₱" + bill + " + ₱" + tip + " tip. Money: ₱" + Money);
    }

    // Called by a Customer right before it leaves unhappy.
    public void CustomerLeftUnhappy(Customer customer)
    {
        LostCustomers++;
        Debug.Log(customer.CustomerName + " ran out of patience and left! Lost customers: " + LostCustomers);
    }

    // for loop: check every table until we find one that's free
    // AND has enough seats for the whole party.
    Table FindFreeTable(int partySize)
    {
        for (int i = 0; i < Tables.Length; i++)
        {
            if (Tables[i] == null)
            {
                continue;  // empty slot in the Inspector: skip it
            }

            if (!Tables[i].IsOccupied && Tables[i].SeatCount >= partySize)
            {
                return Tables[i];
            }
        }
        return null; // every table is taken
    }
}