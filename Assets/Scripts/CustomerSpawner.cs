using System.Collections.Generic;  // needed for List
using UnityEngine;

// Put this on an empty object named CustomerSpawner, placed where the FRONT
// of the waiting line should be (the end farthest from the door).
// The line grows from here in the Line Direction, toward the door.
public class CustomerSpawner : MonoBehaviour
{
    // So other scripts can ask "who is first in line?" (static: one per game).
    public static CustomerSpawner Instance;

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

    // Which way the line grows from the front (this object is the FRONT of the line).
    // Point it toward your door: (0,-1) = down, (0,1) = up, (-1,0) = left, (1,0) = right.
    public Vector2 LineDirection = new Vector2(0f, -1f);

    // How fast cats in the line walk forward when the line moves up.
    public float LineWalkSpeed = 3f;

    // When ticked, only the cat at the front of the line can be seated,
    // so nobody gets overtaken by a cat who arrived later.
    public bool FirstComeFirstServed = true;

    // Random names for customers (array of strings).
    public string[] CustomerNames = { "Mochi", "Tofu", "Biscuit", "Mango", "Pancake", "Nori", "Kiwi", "Sushi" };

    // Optional: different cat pictures. Each new customer gets a random one.
    // (Only used if Customer Looks below is empty.)
    public Sprite[] CustomerSprites;

    // Optional: full animated cats. Each new customer gets a random one.
    public CatLook[] CustomerLooks;

    // Optional: the entrance door. It opens when a customer arrives,
    // and new customers appear there and walk to the back of the line.
    public Door EntranceDoor;

    // Chance that a customer arrives with a buddy (0 = never, 1 = always).
    [Range(0f, 1f)]
    public float PairChance = 0.4f;

    // List<T> used as a queue: index 0 is the front of the line,
    // new customers are added at the end (the back).
    List<Customer> line = new List<Customer>();
    float spawnTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spawnTimer = 1f;  // first customer arrives after 1 second
    }

    void Update()
    {
        CleanUpLine();
        MoveLineForward();

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

    // Takes out anyone who isn't waiting any more (seated, or left unhappy).
    // Backward for loop, because we remove items while looping.
    void CleanUpLine()
    {
        for (int i = line.Count - 1; i >= 0; i--)
        {
            if (line[i] == null || line[i].State != CustomerState.Waiting)
            {
                line.RemoveAt(i);
            }
        }
    }

    // Each cat walks toward its spot: spot 0 is the front, spot 1 behind it, and so on.
    void MoveLineForward()
    {
        for (int i = 0; i < line.Count; i++)
        {
            Vector3 spot = SpotPosition(i);
            line[i].transform.position = Vector3.MoveTowards(
                line[i].transform.position, spot, LineWalkSpeed * Time.deltaTime);
        }
    }

    Vector3 SpotPosition(int index)
    {
        Vector3 direction = new Vector3(LineDirection.x, LineDirection.y, 0f).normalized;
        return transform.position + direction * SpotSpacing * index;
    }

    // Is this customer at the front of the line?
    public bool IsFirstInLine(Customer customer)
    {
        return line.Count > 0 && line[0] == customer;
    }

    // Who is at the front of the line (or null if nobody is waiting)?
    public Customer FirstInLine()
    {
        return line.Count > 0 ? line[0] : null;
    }

    void TrySpawnCustomer()
    {
        // if-else: only spawn if there's room in the waiting line.
        if (line.Count >= MaxWaiting)
        {
            Debug.Log("The waiting line is full. A customer walked past.");
            return;
        }

        // New customers appear at the door (if there is one), otherwise at their spot.
        Vector3 startPosition = (EntranceDoor != null) ? EntranceDoor.transform.position
                                                       : SpotPosition(line.Count);
        startPosition.z = transform.position.z;

        // Instantiate = make a copy of the prefab in the scene.
        Customer customer = Instantiate(CustomerPrefab, startPosition, Quaternion.identity);

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

        line.Add(customer);  // join at the BACK of the line

        if (EntranceDoor != null)
        {
            EntranceDoor.Open();
        }

        Debug.Log(customer.DisplayName + " arrived and joined the line (place " + line.Count + ").");
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
}