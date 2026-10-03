using UnityEngine;

// Put this on the "Buddy" child of the Customer prefab.
// The buddy is the second cat in a party of two. It has no logic of its own:
// it shares the leader's patience, table, and bill, and a click on the buddy
// counts as a click on the whole party.
public class PartyMember : MonoBehaviour
{
    Customer leader;

    void OnMouseDown()
    {
        if (leader == null)
        {
            leader = GetComponentInParent<Customer>();  // the Customer on the parent object
        }

        leader.HandleClick();
    }
}
