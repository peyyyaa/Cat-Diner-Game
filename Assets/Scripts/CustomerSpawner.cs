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

    // Optional: different cat pictures. Each new customer gets a random one.
    // (Only used if Customer Looks below is empty.)
    public Sprite[] CustomerSprites;

    // Optional: full animated cats. Each new customer gets a random one.
    public CatLook[] CustomerLooks;

    // Optional: the entrance door, which opens when a customer arrives.
    public Door EntranceDoor;

    // Chance that a customer arrives with a buddy (0 = never, 1 = always).
    [Range(0f, 1f)]
    public float PairChance = 0.4f;

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

        int nameIndex = Random.Range(0, CustomerNames.Length);
        customer.CustomerName = CustomerNames[nameIndex];
        customer.MaxPatience = Random.Range(MinPatience, MaxPatience);
        GiveRandomLook(customer.gameObject);

        // Some customers arrive as a pair.
        bool isPair = customer.Buddy != null && Random.value < PairChance;
        if (customer.Buddy != null)
        {
            customer.Buddy.gameObject.SetActive(isPair);
        }

        if (isPair)
        {
            // The next name in the list, so the pair never share a name.
            customer.BuddyName = CustomerNames[(nameIndex + 1) % CustomerNames.Length];
            GiveRandomLook(customer.Buddy.gameObject);
        }

        waitingSpots[spot] = customer;

        if (EntranceDoor != null)
        {
            EntranceDoor.Open();
        }

        Debug.Log(customer.CustomerName + " arrived at the diner.");
    }

    // Give one cat (the customer or their buddy) a random look.
    // Animated cats win over still pictures.
    void GiveRandomLook(GameObject cat)
    {
        CustomerAnimator animator = cat.GetComponent<CustomerAnimator>();
        if (animator != null && CustomerLooks != null && CustomerLooks.Length > 0)
        {
            animator.Look = CustomerLooks[Random.Range(0, CustomerLooks.Length)];
        }
        else if (CustomerSprites != null && CustomerSprites.Length > 0)
        {
            Sprite look = CustomerSprites[Random.Range(0, CustomerSprites.Length)];
            cat.GetComponent<SpriteRenderer>().sprite = look;
        }
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