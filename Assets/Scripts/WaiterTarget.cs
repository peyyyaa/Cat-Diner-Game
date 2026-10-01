using UnityEngine;

// Base class for anything the cat waiter can walk to (customers and the kitchen).
// "abstract" means you never attach WaiterTarget itself; you attach classes
// that INHERIT from it, like Customer and KitchenManager.
public abstract class WaiterTarget : MonoBehaviour
{
    // Where the cat stands, relative to this object. Adjust in the Inspector.
    public Vector3 StandOffset = new Vector3(-0.9f, 0f, 0f);

    // Property: works like a variable, but is calculated each time it's read.
    public Vector3 StandPosition
    {
        get { return transform.position + StandOffset; }
    }

    // Every class that inherits from WaiterTarget MUST say what happens
    // when the cat arrives. Each one does something different.
    public abstract void WaiterArrived(Cat cat);
}
