using UnityEngine;

// Put this on an empty object named CustomerSpawner, placed where the
// waiting line should start. New customers line up downward from here.
public class CustomerSpawner : MonoBehaviour
{
    // Drag your Customer PREFAB (from the Project window) into this slot.
    public Customer CustomerPrefab;

    // Seconds between new customers.
    public float SpawnInterval = 5f;

    // Each new customer gets a random patience between these two values.
    public float MinPatience = 12f;
    public float MaxPatience = 20f;

    // Size of the waiting line, and the gap between spots in it.
    public int MaxWaiting = 4;
    public float SpotSpacing = 1.2f;

    // Random names for customers (array of strings).
    public string[] CustomerNames = { "Mochi", "Tofu", "Biscuit", "Mango", "Pancake", "Nori", "Kiwi", "Sushi" };

    // array: which customer is standing in each waiting spot (empty = free spot).
    Customer[] waitingSpots;
    float spawnTimer;

    void Start()
    {
        waitingSpots = new Customer[MaxWaiting];
        spawnTimer = 1f;  // first customer arrives after 1 second
    }

    void Update()
    {
        if (GameManager.Instance.GameFinished)
        {
            return;  // no new customers after the game ends
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnCustomer();
            spawnTimer = SpawnInterval;
        }
    }

    void TrySpawnCustomer()
    {
        int spot = FindFreeSpot();

        // if-else: only spawn if there's room in the waiting line.
        if (spot == -1)
        {
            Debug.Log("The waiting line is full. A customer walked past.");
            return;
        }

        // Instantiate = make a copy of the prefab in the scene.
        Vector3 position = transform.position + Vector3.down * SpotSpacing * spot;
        Customer customer = Instantiate(CustomerPrefab, position, Quaternion.identity);

        customer.CustomerName = CustomerNames[Random.Range(0, CustomerNames.Length)];
        customer.MaxPatience = Random.Range(MinPatience, MaxPatience);

        waitingSpots[spot] = customer;
        Debug.Log(customer.CustomerName + " arrived at the diner.");
    }

    // for loop: find the first spot with nobody waiting in it.
    // Returns -1 if every spot is taken.
    int FindFreeSpot()
    {
        for (int i = 0; i < waitingSpots.Length; i++)
        {
            // A spot is free if it's empty, the customer there was destroyed
            // (left), or they've already been seated.
            if (waitingSpots[i] == null || waitingSpots[i].State != CustomerState.Waiting)
            {
                return i;
            }
        }
        return -1;
    }
}
