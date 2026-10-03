using System.Collections.Generic;  // needed for List
using UnityEngine;

// Attach this to each table object in the scene.
public class Table : MonoBehaviour
{
    // bool from Table 1 of the doc: is this table in use?
    // (It stays in use until the waiter collects the money left on it.)
    public bool IsOccupied = false;

    // Which customer (party) is sitting here (empty when nobody is seated).
    public Customer SeatedCustomer;

    // Empty child objects that mark where each cat sits (e.g. Seat_L and Seat_R).
    // The number of seats decides how big a party this table can hold.
    public Transform[] Seats;

    // Optional: empty child objects on the tabletop where each cat's plate goes
    // (one per seat, in the same order as Seats).
    public Transform[] DishSpots;

    // Optional: where the money is left after eating (defaults to the table's center).
    public Transform MoneySpot;

    // Where the cat waiter stands when visiting this table, relative to its center.
    public Vector3 WaiterOffset = new Vector3(0f, -1f, 0f);

    // Only used if Seats is empty (the old one-customer behavior).
    public Vector3 SeatOffset = new Vector3(0f, 0.8f, 0f);

    // The plates currently on this table.
    List<ServingPlate> dishes = new List<ServingPlate>();

    // List<T>: only the seat slots that were actually filled in the Inspector
    // (empty slots are skipped instead of causing an error).
    List<Transform> UsableSeats()
    {
        List<Transform> usable = new List<Transform>();

        if (Seats != null)
        {
            foreach (Transform seat in Seats)
            {
                if (seat != null)
                {
                    usable.Add(seat);
                }
            }
        }

        return usable;
    }

    // Property: how many cats fit at this table.
    public int SeatCount
    {
        get
        {
            int count = UsableSeats().Count;
            return count == 0 ? 1 : count;  // no seat markers = the old single seat
        }
    }

    public void Seat(Customer customer)
    {
        IsOccupied = true;
        SeatedCustomer = customer;

        List<Transform> seats = UsableSeats();

        // No seat markers set up: use the old offset.
        if (seats.Count == 0)
        {
            customer.transform.position = transform.position + SeatOffset;
            return;
        }

        // The first cat of the party takes seat 1...
        PlaceAt(customer.transform, customer.GetComponent<CustomerAnimator>(), seats[0]);

        // ...and their buddy (if they came as a pair) takes seat 2.
        if (customer.HasBuddy && seats.Count > 1)
        {
            PlaceAt(customer.Buddy.transform, customer.Buddy.GetComponent<CustomerAnimator>(), seats[1]);
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

    // Puts the delivered plates on the tabletop, one in front of each cat.
    public void PlaceDishes(List<ServingPlate> plates)
    {
        for (int i = 0; i < plates.Count; i++)
        {
            ServingPlate plate = plates[i];
            if (plate == null)
            {
                continue;
            }

            Vector3 position;
            if (DishSpots != null && i < DishSpots.Length && DishSpots[i] != null)
            {
                position = DishSpots[i].position;
            }
            else
            {
                // No dish spots: spread the plates across the middle of the table.
                float offsetX = (i - (plates.Count - 1) / 2f) * 0.4f;
                position = transform.position + new Vector3(offsetX, 0.1f, 0f);
            }

            plate.transform.position = position;
            plate.SetOrderInLayer(3);  // on top of the tabletop
            dishes.Add(plate);
        }
    }

    // Removes the plates from the table.
    public void ClearDishes()
    {
        foreach (ServingPlate plate in dishes)
        {
            if (plate != null)
            {
                Destroy(plate.gameObject);
            }
        }
        dishes.Clear();
    }

    // Called by a Customer who finished eating: leaves the money on the table.
    // The table stays in use until the waiter collects it.
    public void LeaveMoney(decimal bill, decimal tip)
    {
        SeatedCustomer = null;

        TableMoney moneyPrefab = GameManager.Instance.MoneyPrefab;

        // No money prefab set up: count the money straight away, like before.
        if (moneyPrefab == null)
        {
            GameManager.Instance.CollectMoney(bill, tip);
            ClearDishes();
            Free();
            return;
        }

        Vector3 position = (MoneySpot != null) ? MoneySpot.position : transform.position;
        TableMoney money = Instantiate(moneyPrefab, position, Quaternion.identity);
        money.Setup(this, bill, tip);
    }

    public void Free()
    {
        IsOccupied = false;
        SeatedCustomer = null;
    }
}