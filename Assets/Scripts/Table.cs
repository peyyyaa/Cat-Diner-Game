using UnityEngine;

// Attach this to each table object in the scene.
public class Table : MonoBehaviour
{
    // bool from Table 1 of the doc: is someone sitting here?
    public bool IsOccupied = false;

    // Which customer (party) is sitting here (empty when the table is free).
    public Customer SeatedCustomer;

    // Empty child objects that mark where each cat sits (e.g. Seat_L and Seat_R).
    // The number of seats decides how big a party this table can hold.
    public Transform[] Seats;

    // Where the cat waiter stands when visiting this table, relative to its center.
    public Vector3 WaiterOffset = new Vector3(0f, -1f, 0f);

    // Only used if Seats is empty (the old one-customer behavior).
    public Vector3 SeatOffset = new Vector3(0f, 0.8f, 0f);

    // Property: how many cats fit at this table.
    public int SeatCount
    {
        get
        {
            if (Seats == null || Seats.Length == 0)
            {
                return 1;
            }
            return Seats.Length;
        }
    }

    public void Seat(Customer customer)
    {
        IsOccupied = true;
        SeatedCustomer = customer;

        // No seat markers set up: use the old offset.
        if (Seats == null || Seats.Length == 0)
        {
            customer.transform.position = transform.position + SeatOffset;
            return;
        }

        // The first cat of the party takes seat 1...
        PlaceAt(customer.transform, customer.GetComponent<CustomerAnimator>(), Seats[0]);

        // ...and their buddy (if they came as a pair) takes seat 2.
        if (customer.HasBuddy && Seats.Length > 1)
        {
            PlaceAt(customer.Buddy.transform, customer.Buddy.GetComponent<CustomerAnimator>(), Seats[1]);
        }
    }

    // Moves one cat onto a seat and turns it to face the table.
    void PlaceAt(Transform cat, CustomerAnimator animator, Transform seat)
    {
        cat.position = seat.position;

        if (animator != null)
        {
            // A seat to the RIGHT of the table's center faces LEFT, toward the table.
            bool faceLeft = seat.position.x > transform.position.x;
            animator.SitSideways(faceLeft);
        }
    }

    public void Free()
    {
        IsOccupied = false;
        SeatedCustomer = null;
    }
}