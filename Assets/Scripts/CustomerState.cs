// The fixed set of stages a customer can be in (enum, from Table 1 of the doc).
public enum CustomerState
{
    Waiting,      // standing in the waiting line
    Seated,       // at the table with a paw up: ready to order
    Ordering,     // order taken, waiting for the food
    Eating,
    Leaving,
    ReadingMenu   // just sat down, looking at the menu (added last so older saved values don't shift)
}