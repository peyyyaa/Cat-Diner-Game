using System.Collections.Generic;  // needed for Queue, List and SortedSet
using UnityEngine;

// The cat waiter (the Cat class from Table 2 of the doc).
// Put this on ONE object named CatWaiter. It walks to customers, plates, money, tables and the counter.
public class Cat : MonoBehaviour
{
    public string CatName = "Penny";
    public float MoveSpeed = 5f;

    // Tick this if your cat picture faces right; untick if it faces left.
    // The cat flips to face the direction it's walking.
    public bool ArtFacesRight = true;
    SpriteRenderer spriteRenderer;

    // Optional: a small child object shown while carrying food (only used if there are no plates).
    public GameObject CarryIcon;

    // Where carried plates sit, relative to the cat.
    public Vector3 CarryOffset = new Vector3(0f, 0.6f, 0f);

    [Header("Walking around furniture")]
    // Put the tables and the counter on a layer named "Obstacle", then choose it here.
    public LayerMask Obstacles;

    // The floor is split into a grid of small squares this size.
    // Smaller = more precise paths through narrow gaps, but a bit more work.
    public float GridCellSize = 0.2f;

    // How wide the cat is: squares closer than this to furniture count as blocked.
    // If the cat can't fit through an aisle it should fit through, lower this.
    public float BodyRadius = 0.15f;

    // Tick to see the blocked squares (red) and the current path (cyan)
    // in the Scene view while the game runs and CatWaiter is selected.
    public bool ShowGridGizmos = true;

    // The dish the cat is holding (null = paws are empty).
    public Order CarriedOrder;

    // Queue<T>: a to-do list where the first task added is the first one done.
    Queue<WaiterTarget> tasks = new Queue<WaiterTarget>();

    WaiterTarget currentTask;                 // where the cat is going right now
    List<Vector3> path = new List<Vector3>(); // the points to walk through to get there

    DepthSort depth;  // optional: draws the cat in front of / behind furniture

    // The navigation grid: true = a square the cat can't stand on.
    bool[,] blocked;
    Vector2 gridOrigin;   // world position of the bottom-left corner of the grid
    int gridWidth;
    int gridHeight;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        depth = GetComponent<DepthSort>();
        SetCarryIcon(false);
        BuildGrid();
    }

    // Called when the player clicks a customer, a plate, money, a table, or the counter.
    public void AddTask(WaiterTarget target)
    {
        // Don't add the same place twice if the player double-clicks.
        if (target == currentTask || tasks.Contains(target))
        {
            return;
        }

        tasks.Enqueue(target);
        Debug.Log(CatName + " will go to " + target.name + ". Tasks waiting: " + tasks.Count);
    }

    void Update()
    {
        KeepPlatesInFront();

        // No current task (or it disappeared)? Decide what to do next.
        if (currentTask == null)
        {
            PickNextTask();
            return;
        }

        if (path.Count == 0)
        {
            PlanPath();
        }

        // Walk toward the next point on the path, a little bit each frame.
        Vector3 next = path[0];
        next.z = transform.position.z;  // stay on the same 2D plane

        Face(next);
        transform.position = Vector3.MoveTowards(transform.position, next, MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, next) < 0.05f)
        {
            path.RemoveAt(0);

            // That was the last point: we've arrived.
            if (path.Count == 0)
            {
                WaiterTarget arrivedAt = currentTask;
                currentTask = null;
                arrivedAt.WaiterArrived(this);
            }
        }
    }

    void PickNextTask()
    {
        path.Clear();

        if (CarriedOrder != null)
        {
            // Carrying food always comes first: deliver it straight away.
            if (CarriedOrder.Customer != null)
            {
                currentTask = CarriedOrder.Customer;
            }
            else
            {
                Debug.Log("The customer left, so the " + CarriedOrder.ItemNames() + " went to waste.");
                DropFood();
            }
        }
        else if (tasks.Count > 0)
        {
            // Something that no longer exists counts as null and is skipped.
            currentTask = tasks.Dequeue();
        }

        if (currentTask != null)
        {
            PlanPath();
        }
    }

    void PlanPath()
    {
        path = FindPath(transform.position, currentTask.StandPosition);

        if (path.Count == 0)
        {
            path.Add(transform.position);  // already there
        }
    }

    // ================= Navigation grid =================

    // Covers the camera's view with squares and marks the ones touching furniture.
    public void BuildGrid()
    {
        Camera cam = Camera.main;
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;

        // One extra unit around the edges, just in case.
        viewWidth += 2f;
        viewHeight += 2f;

        gridOrigin = (Vector2)cam.transform.position - new Vector2(viewWidth, viewHeight) / 2f;
        gridWidth = Mathf.CeilToInt(viewWidth / GridCellSize);
        gridHeight = Mathf.CeilToInt(viewHeight / GridCellSize);
        blocked = new bool[gridWidth, gridHeight];

        // Nested for loops: check every square of the grid.
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Blocked if a cat-sized circle here would touch any furniture.
                blocked[x, y] = Obstacles.value != 0 &&
                                Physics2D.OverlapCircle(CellCenter(x, y), BodyRadius, Obstacles) != null;
            }
        }
    }

    Vector3 CellCenter(int x, int y)
    {
        return new Vector3(gridOrigin.x + (x + 0.5f) * GridCellSize,
                           gridOrigin.y + (y + 0.5f) * GridCellSize,
                           transform.position.z);
    }

    Vector2Int CellAt(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / GridCellSize);
        int y = Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / GridCellSize);
        return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), Mathf.Clamp(y, 0, gridHeight - 1));
    }

    bool InsideGrid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
    }

    // If a spot is on a blocked square (e.g. a stand spot inside a table's collider),
    // find the closest open square, searching outward ring by ring.
    Vector2Int NearestOpenCell(Vector2Int cell)
    {
        if (!blocked[cell.x, cell.y])
        {
            return cell;
        }

        for (int ring = 1; ring < 20; ring++)
        {
            Vector2Int best = cell;
            float bestDistance = float.MaxValue;

            for (int dx = -ring; dx <= ring; dx++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    int x = cell.x + dx;
                    int y = cell.y + dy;
                    if (InsideGrid(x, y) && !blocked[x, y])
                    {
                        float distance = dx * dx + dy * dy;
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            best = new Vector2Int(x, y);
                        }
                    }
                }
            }

            if (bestDistance < float.MaxValue)
            {
                return best;
            }
        }
        return cell;  // nothing open nearby: give up and use the original square
    }

    // ================= Pathfinding =================

    // The full route from start to goal: A* on the grid, then smoothed into straight lines.
    List<Vector3> FindPath(Vector3 start, Vector3 goal)
    {
        if (blocked == null)
        {
            BuildGrid();
        }

        Vector2Int startCell = NearestOpenCell(CellAt(start));
        Vector2Int goalCell = NearestOpenCell(CellAt(goal));

        List<Vector2Int> cells = FindCellPath(startCell, goalCell);
        if (cells == null)
        {
            Debug.LogWarning(CatName + " can't find a way there, so it walks straight. " +
                             "Check that the aisles aren't fully blocked (try a smaller Body Radius).");
            List<Vector3> straight = new List<Vector3>();
            straight.Add(goal);
            return straight;
        }

        // Turn the squares into world points, starting from where the cat is now.
        List<Vector3> points = new List<Vector3>();
        points.Add(start);
        for (int i = 1; i < cells.Count; i++)
        {
            points.Add(CellCenter(cells[i].x, cells[i].y));
        }

        // Finish exactly on the spot if it's on open floor
        // (otherwise stop on the closest open square next to it).
        Vector2Int exactGoal = CellAt(goal);
        if (!blocked[exactGoal.x, exactGoal.y])
        {
            points.Add(new Vector3(goal.x, goal.y, transform.position.z));
        }

        return Smooth(points);
    }

    // A* search: always explore the square with the lowest
    // "distance walked so far + straight-line distance still to go".
    List<Vector2Int> FindCellPath(Vector2Int start, Vector2Int goal)
    {
        float[,] walked = new float[gridWidth, gridHeight];
        Vector2Int[,] cameFrom = new Vector2Int[gridWidth, gridHeight];
        bool[,] done = new bool[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                walked[x, y] = float.MaxValue;
            }
        }

        // SortedSet keeps the squares to explore sorted by score, so the best one is always .Min.
        // Each entry is (score, square number); the square number keeps entries unique.
        SortedSet<(float, int)> toExplore = new SortedSet<(float, int)>();

        walked[start.x, start.y] = 0f;
        cameFrom[start.x, start.y] = start;
        toExplore.Add((Estimate(start, goal), start.x + start.y * gridWidth));

        while (toExplore.Count > 0)
        {
            int number = toExplore.Min.Item2;   // the square with the best score
            toExplore.Remove(toExplore.Min);

            Vector2Int current = new Vector2Int(number % gridWidth, number / gridWidth);
            if (done[current.x, current.y])
            {
                continue;
            }
            done[current.x, current.y] = true;

            if (current == goal)
            {
                break;
            }

            // Try all 8 neighbours (up, down, left, right and the diagonals).
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = current.x + dx;
                    int ny = current.y + dy;
                    if (!InsideGrid(nx, ny) || blocked[nx, ny] || done[nx, ny]) continue;

                    // No squeezing diagonally between two blocked squares (cutting corners).
                    bool diagonal = dx != 0 && dy != 0;
                    if (diagonal && (blocked[current.x + dx, current.y] || blocked[current.x, current.y + dy])) continue;

                    float stepCost = diagonal ? 1.414f : 1f;
                    float newWalked = walked[current.x, current.y] + stepCost;

                    if (newWalked < walked[nx, ny])
                    {
                        walked[nx, ny] = newWalked;
                        cameFrom[nx, ny] = current;
                        Vector2Int neighbour = new Vector2Int(nx, ny);
                        toExplore.Add((newWalked + Estimate(neighbour, goal), nx + ny * gridWidth));
                    }
                }
            }
        }

        if (!done[goal.x, goal.y])
        {
            return null;  // the goal can't be reached
        }

        // Follow cameFrom backward from the goal to build the route.
        List<Vector2Int> route = new List<Vector2Int>();
        Vector2Int step = goal;
        while (step != start)
        {
            route.Insert(0, step);
            step = cameFrom[step.x, step.y];
        }
        route.Insert(0, start);
        return route;
    }

    // Straight-line distance between two squares (A* uses it to aim toward the goal).
    float Estimate(Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    // Removes the zig-zag: from each point, jump to the farthest later point
    // that can be reached in a straight line without touching a blocked square.
    List<Vector3> Smooth(List<Vector3> points)
    {
        List<Vector3> result = new List<Vector3>();
        int from = 0;

        while (from < points.Count - 1)
        {
            int farthest = from + 1;
            for (int i = points.Count - 1; i > from + 1; i--)
            {
                if (ClearLine(points[from], points[i]))
                {
                    farthest = i;
                    break;
                }
            }

            result.Add(points[farthest]);
            from = farthest;
        }
        return result;
    }

    // Walks along the line in small steps and checks every square it crosses.
    bool ClearLine(Vector3 a, Vector3 b)
    {
        float length = Vector3.Distance(a, b);
        int steps = Mathf.CeilToInt(length / (GridCellSize * 0.5f));

        for (int i = 0; i <= steps; i++)
        {
            Vector3 point = Vector3.Lerp(a, b, steps == 0 ? 0f : (float)i / steps);
            Vector2Int cell = CellAt(point);
            if (blocked[cell.x, cell.y])
            {
                return false;
            }
        }
        return true;
    }

    // Draws the grid and path in the Scene view (only when CatWaiter is selected).
    void OnDrawGizmosSelected()
    {
        if (!ShowGridGizmos || blocked == null)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (blocked[x, y])
                {
                    Gizmos.DrawCube(CellCenter(x, y), Vector3.one * GridCellSize * 0.9f);
                }
            }
        }

        Gizmos.color = Color.cyan;
        Vector3 previous = transform.position;
        foreach (Vector3 point in path)
        {
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
    }

    // ================= Depth =================

    // Carried plates are drawn just in front of the cat, wherever the cat is.
    void KeepPlatesInFront()
    {
        if (CarriedOrder == null)
        {
            return;
        }

        int baseOrder = (depth != null) ? depth.BaseOrder : 0;

        for (int i = 0; i < CarriedOrder.Plates.Count; i++)
        {
            if (CarriedOrder.Plates[i] != null)
            {
                CarriedOrder.Plates[i].SetOrderInLayer(baseOrder + 6 + i * 2);
            }
        }
    }

    // Flip to face the way we're walking (only if moving sideways noticeably).
    void Face(Vector3 point)
    {
        float sideways = point.x - transform.position.x;
        if (Mathf.Abs(sideways) > 0.01f)
        {
            bool movingLeft = sideways < 0f;
            spriteRenderer.flipX = ArtFacesRight ? movingLeft : !movingLeft;
        }
    }

    // ================= Carrying food =================

    // Takes an order from the kitchen and carries its plates.
    public void PickUp(Order order)
    {
        CarriedOrder = order;

        for (int i = 0; i < order.Plates.Count; i++)
        {
            ServingPlate plate = order.Plates[i];
            if (plate == null) continue;

            plate.SetClickable(false);
            plate.transform.SetParent(transform);  // the plate now moves with the cat
            plate.transform.localPosition = CarryOffset + new Vector3(0f, i * 0.15f, 0f);
        }

        // No plate art? Show the simple carry icon instead.
        SetCarryIcon(order.Plates.Count == 0);

        Debug.Log(CatName + " picked up " + order.ItemNames() + " for " + order.Customer.DisplayName + ".");
    }

    // Gives the carried order to the customer (the plates go to their table).
    public Order HandOver()
    {
        Order order = CarriedOrder;
        CarriedOrder = null;

        if (order != null)
        {
            foreach (ServingPlate plate in order.Plates)
            {
                if (plate != null)
                {
                    plate.transform.SetParent(null);  // stop moving with the cat
                }
            }
        }

        SetCarryIcon(false);
        return order;
    }

    // Throws the carried food away (the customer left before it arrived).
    public void DropFood()
    {
        if (CarriedOrder != null)
        {
            foreach (ServingPlate plate in CarriedOrder.Plates)
            {
                if (plate != null)
                {
                    Destroy(plate.gameObject);
                }
            }
        }

        CarriedOrder = null;
        SetCarryIcon(false);
    }

    void SetCarryIcon(bool visible)
    {
        if (CarryIcon != null)
        {
            CarryIcon.SetActive(visible);
        }
    }
}