using UnityEngine;

// Attach this to each table object in the scene.
public class Table : MonoBehaviour
{
    // bool from Table 1 of the doc: is someone sitting here?
    public bool IsOccupied = false;

    // Which customer is sitting here (empty when the table is free).
    public Customer SeatedCustomer;

    // Where the customer appears, relative to the table's center.
    // You can tweak this in the Inspector.
    public Vector3 SeatOffset = new Vector3(0f, 0.8f, 0f);

    public void Seat(Customer customer)
    {
        IsOccupied = true;
        SeatedCustomer = customer;

        // Move the customer to the seat. No walking animation yet; they just appear there.
        customer.transform.position = transform.position + SeatOffset;
    }

    public void Free()
    {
        IsOccupied = false;
        SeatedCustomer = null;
    }
}
