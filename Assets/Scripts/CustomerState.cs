// The fixed set of stages a customer can be in (enum, from Table 1 of the doc).
// Ordering and Eating aren't used yet; we'll need them in the next steps.
public enum CustomerState
{
    Waiting,   // standing in the waiting area
    Seated,    // sitting at a table
    Ordering,
    Eating,
    Leaving
}
