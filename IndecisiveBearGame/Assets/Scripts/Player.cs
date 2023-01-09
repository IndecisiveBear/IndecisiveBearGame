using UnityEngine;

public class Player : MonoBehaviour
{
    const float WalkSpeed = 0.01f;
    public float PlayerSpeed = WalkSpeed;
    public bool InCollision = false;
    public Vector2 Position;
    private float GridWidth;
    private float GridHeight;
    private float GridSize;
    private int[] GridLocation;
    private GameObject[,] LightGrid;
    private GameObject[,] WallGrid;

    GameObject[] Walls;
    BoxCollider2D Body;

    void Start()
    {
        Walls = GameObject.FindGameObjectsWithTag("Wall");
        Body = gameObject.GetComponent<BoxCollider2D>();

        WallGrid = new GameObject[(int)GridWidth, (int)GridHeight];
        GridLocation = FindGridPlacement();
        GenerateLight();
    }

    void Update()
    {
        
        Position = transform.position;
        Move();
        transform.position = Position;

        if (Input.GetKey(KeyCode.Q)) // Warp to origin, for debugging 
        {
            Position = Vector2.zero;
            transform.position = Position;
        }
    }

    /// <summary>
    /// <c>Move</c> handles all player movement using `wasd` keys.
    /// </summary>
    public void Move()
    {
        int upMoveCount = 0;
        int rightMoveCount = 0;

        if (Input.GetKeyDown(KeyCode.LeftShift)) PlayerSpeed = (PlayerSpeed == WalkSpeed) ? 2 * WalkSpeed : WalkSpeed;
        if (Input.GetKey(KeyCode.W)) upMoveCount += 1;
        if (Input.GetKey(KeyCode.S)) upMoveCount -= 1;
        if (Input.GetKey(KeyCode.A)) rightMoveCount -= 1;
        if (Input.GetKey(KeyCode.D)) rightMoveCount += 1;
 
        if (upMoveCount != 0 || rightMoveCount != 0)
        {
            PlayerCollision(Body, Walls);
            var magnitude = Mathf.Sqrt(rightMoveCount * rightMoveCount + upMoveCount * upMoveCount);
            var resultant = rightMoveCount * Vector2.right + upMoveCount * Vector2.up;
            Position += (resultant / magnitude) * PlayerSpeed;

            bool changedPlacement = false;
            
            if (GridLocation != null)
            {
                if (GridLocation[0] != FindGridPlacement()[0] || GridLocation[1] != FindGridPlacement()[1])
                {
                    changedPlacement = true;
                }
            }
            GridLocation = FindGridPlacement();
            //Debug.Log(GridLocation[0]);
            if (changedPlacement)
            {
                GenerateLight();
            }
        }
    }

    /// <summary>
    /// <c>DetectCollision</c> handles collision detection logic between two BoxColliders. 
    /// It also warps player to nearest acceptable location if they are inside of a wall.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollision(BoxCollider2D body1, BoxCollider2D body2)
    {

        if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y + body2.bounds.extents.y)
        {
            // Detect the edge with which we are colliding.
            if (body1.bounds.center.x > body2.bounds.center.x && 
                Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
            {
                // Colliding with right edge of body2
                Position.x = body2.bounds.center.x + body2.bounds.extents.x + body1.bounds.extents.x;
                transform.position = Position;
            }
            if (body1.bounds.center.x < body2.bounds.center.x && 
                Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
            {
                // Colliding with left edge of body2
                Position.x = body2.bounds.center.x - body2.bounds.extents.x - body1.bounds.extents.x;
                transform.position = Position;
            }
            if (body1.bounds.center.y > body2.bounds.center.y && 
                Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
            {
                // Colliding with top edge of body2
                Position.y = body2.bounds.center.y + body2.bounds.extents.y + body1.bounds.extents.y;
                transform.position = Position;
            }
            if (body1.bounds.center.y < body2.bounds.center.y && 
                Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
            {
                // Colliding with bottom edge of body2
                Position.y = body2.bounds.center.y - body2.bounds.extents.y - body1.bounds.extents.y;
                transform.position = Position;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// <c>PlayerCollision</c> sets `Player.InCollision` to `true` if player is inside one or more `obstacles`.
    /// </summary>
    private void PlayerCollision(BoxCollider2D playerBody, GameObject[] obstacles)
    { 
        foreach (GameObject obstacle in obstacles)
        {
            BoxCollider2D box = obstacle.GetComponent<BoxCollider2D>();
            if (DetectCollision(playerBody, box))
            {
                InCollision = true;
            }
        }
        InCollision = false;
    }

    /// <summary>
    /// <c>SetGridInformation</c> transfers the grid information from GridGenerator.cs to Player.cs.
    /// </summary>
    public void SetGridInformation(string[,] gridString, float gridSize, GameObject[,] lightGrid)
    {
        GridHeight = gridString.GetLength(0);
        GridWidth = gridString.GetLength(1);
        GridSize = gridSize;
        LightGrid = lightGrid;
    }

    /// <summary>
    /// <c>ConvertToGridPosition</c> is a helper function to quickly convert world positions to grid locations.
    /// </summary>
    private int ConvertToGridPosition(float pos, string direction)
    {
        if (direction == "x")
        {
            return (int)(Mathf.Round(pos / GridSize) * GridSize);
        }
        if (direction == "y")
        {
            return (int)(GridHeight * GridSize - (Mathf.Round(pos / GridSize) * GridSize));
        }
        return -1;
    }

    /// <summary>
    /// <c>FindGridPlacement</c> returns the player position in grid coordinates.
    /// </summary>
    private int[] FindGridPlacement()
    {
        
        int[] gridLocation = new int[2];
        gridLocation[0] = ConvertToGridPosition(transform.position.x, "x");

        gridLocation[1] = ConvertToGridPosition(transform.position.y, "y");

        if (gridLocation[0] < 0)
        {
            gridLocation[0] = 0;
        }
        if (gridLocation[0] >= GridWidth)
        {
            gridLocation[0] = (int)GridWidth - 1;
        }
        if (gridLocation[1] < 0)
        {
            gridLocation[1] = 0;
        }
        if (gridLocation[1] >= GridHeight)
        {
            gridLocation[1] = (int)GridHeight - 1;
        }

        return gridLocation;
    }

    /// <summary>
    /// <c>FindGridWalls</c> stores the updated wall gameobjects in grid coordinates in 'WallGrid'. 
    /// </summary>
    private void FindGridWalls()
    {
        for (int i = 0; i < WallGrid.GetLength(0); i += 1)
        {
            for (int j = 0; j < WallGrid.GetLength(1); j += 1)
            {
                WallGrid[i, j] = null;
            }
        }
        foreach (GameObject wall in Walls)
        {
            try
            {
                WallGrid[ConvertToGridPosition(wall.transform.position.x, "x"), ConvertToGridPosition(wall.transform.position.y, "y")] = wall;
            }
            catch (System.IndexOutOfRangeException)
            {

            }
        }
    }

    /// <summary>
    /// <c>GenerateLight</c> lights up the tiles next to the player, to some 'depthSize' distance.
    /// </summary>
    private void GenerateLight()
    {
        for (int i = 0; i < LightGrid.GetLength(0); i += 1)
        {
            for (int j = 0; j < LightGrid.GetLength(1); j += 1)
            {
                LightGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 1f);
            }
        }

        FindGridWalls();

        int depthSize = 9;

        GenerateLightHelper(GridLocation[0] + 1, GridLocation[1] + 1, depthSize, depthSize);
        GenerateLightHelper(GridLocation[0] - 1, GridLocation[1] + 1, depthSize, depthSize);
        GenerateLightHelper(GridLocation[0] + 1, GridLocation[1] - 1, depthSize, depthSize);
        GenerateLightHelper(GridLocation[0] - 1, GridLocation[1] - 1, depthSize, depthSize);

        GenerateLightHelper(GridLocation[0] - 1, GridLocation[1], depthSize, depthSize);
        GenerateLightHelper(GridLocation[0] + 1, GridLocation[1], depthSize, depthSize);
        GenerateLightHelper(GridLocation[0], GridLocation[1] - 1, depthSize, depthSize);
        GenerateLightHelper(GridLocation[0], GridLocation[1] + 1, depthSize, depthSize);

        GenerateLightHelper(GridLocation[0], GridLocation[1], depthSize, depthSize);
        
        
    }

    /// <summary>
    /// <c>GenerateLightHelper</c> is a helper function for 'GenerateLight', which recursively calls itself 'maxDepth' times. 
    /// </summary>
    private void GenerateLightHelper(int x, int y, int depth, int maxDepth)
    {
        
        if (depth == 0)
        {
            return;
        }
        if (x < 0 || x >= GridWidth * GridSize || y < 0 || y >= GridHeight * GridSize)
        {
            return;
        }

        if (LightGrid[y, x].GetComponent<SpriteRenderer>().color.a > (1f - ((float)depth / (float)maxDepth)))
        {
            LightGrid[y, x].GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 1f - ((float)depth / (float)maxDepth));
            if (WallGrid[x, y] == null)
            {
                GenerateLightHelper(x - 1, y, depth - 1, maxDepth);
                GenerateLightHelper(x + 1, y, depth - 1, maxDepth);
                GenerateLightHelper(x, y - 1, depth - 1, maxDepth);
                GenerateLightHelper(x, y + 1, depth - 1, maxDepth);
            } else
            {
                return;
            }
        } else
        {
            return;
        }
    }
}
