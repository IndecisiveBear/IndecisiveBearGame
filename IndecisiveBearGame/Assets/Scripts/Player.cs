using System;
using UnityEngine;

public class Player : MonoBehaviour, Collideable
{
    const float WalkSpeed = 0.01f;
    public float PlayerSpeed = WalkSpeed;
    public bool InCollision = false;
    public Vector2 Position;
    private float _gridWidth;
    private float _gridHeight;
    private float _gridSize;
    private int[] _gridLocation;
    private GameObject[,] _lightGrid;
    private GameObject[,] _brightGrid;
    private GameObject[,] _darkGrid;
    private GameObject[][,] _gridLayers;
    private GameObject[,] _currentGrid;
    private int _currentLayer;
    private BoxCollider2D _startClimbingRamp = null;
    private float _rampFactor = 0f;
    private float _rampFactor2 = 0.9f;
    float[] _fogColor = new float[] { 0f, 0f, 0f };
    int _fogDepthSize = 9;
    BoxCollider2D _body;

    void Start()
    {
        _body = gameObject.GetComponent<BoxCollider2D>();
        _gridLocation = FindGridPlacement();
        SetFogParameters(dark: true);
        GenerateFog();
    }

    void Update()
    {
        Position = transform.position;
        Move();
        transform.position = Position;
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
            PlayerCollision(_body, new string[]{ "Wall", "RampN", "RampS", "RampW", "RampE"});
            if (_currentLayer > 0)
            {
                PlayerCollision(_body, new string[] { "null", "RampNDown", "RampSDown", "RampWDown", "RampEDown"});
            }
            var magnitude = Mathf.Sqrt(rightMoveCount * rightMoveCount + upMoveCount * upMoveCount);
            var resultant = rightMoveCount * Vector2.right + upMoveCount * Vector2.up;
            Position += (resultant / magnitude) * PlayerSpeed;

            bool changedPlacement = false;
            if (_gridLocation != null)
            {
                if (_gridLocation[0] != FindGridPlacement()[0] || _gridLocation[1] != FindGridPlacement()[1])
                {
                    changedPlacement = true;
                }
            }
            _gridLocation = FindGridPlacement();
            if (changedPlacement)
            {
                GenerateFog();
            }
        }
    }

    /// <summary>
    /// <c>DetectCollisionEmpty</c> handles collision detection logic between one BoxColliders and one location.
    /// It also warps player to nearest acceptable location if they are inside of this location.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionEmpty(BoxCollider2D body1, float x, float y, float size)
    {

        if (body1.bounds.center.x + body1.bounds.extents.x >= x - size / 2 &&
            body1.bounds.center.x - body1.bounds.extents.x <= x + size / 2 &&
            body1.bounds.center.y + body1.bounds.extents.y >= y - size / 2 &&
            body1.bounds.center.y - body1.bounds.extents.y <= y + size / 2)
        {
            // Detect the edge with which we are colliding.
            if (body1.bounds.center.x > x &&
                Mathf.Abs(body1.bounds.center.y - y) < 1.5f * size / 2)
            {
                // Colliding with right edge of body2
                Position.x = x + size / 2 + body1.bounds.extents.x;
                transform.position = Position;
            }
            if (body1.bounds.center.x < x &&
                Mathf.Abs(body1.bounds.center.y - y) < 1.5f * size / 2)
            {
                // Colliding with left edge of body2
                Position.x = x - size / 2 - body1.bounds.extents.x;
                transform.position = Position;
            }
            if (body1.bounds.center.y > y &&
                Mathf.Abs(body1.bounds.center.x - x) < 1.5f * size / 2)
            {
                // Colliding with top edge of body2
                Position.y = y + size / 2 + body1.bounds.extents.y;
                transform.position = Position;
            }
            if (body1.bounds.center.y < y &&
                Mathf.Abs(body1.bounds.center.x - x) < 1.5f * size / 2)
            {
                // Colliding with bottom edge of body2
                Position.y = y - size / 2 - body1.bounds.extents.y;
                transform.position = Position;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// <c>MoveUpGrid</c> updates the grid information to one grid layer above the current grid layer. 
    /// </summary>
    public void MoveUpGrid()
    {
        _currentLayer += 1;
        _currentGrid = _gridLayers[_currentLayer];
        GetComponent<SpriteRenderer>().sortingOrder += 1;

        ResetDepth();
    }

    /// <summary>
    /// <c>MoveDownGrid</c> updates the grid information to one grid layer below the current grid layer. 
    /// </summary>
    public void MoveDownGrid()
    {
        if (_currentLayer > 0)
        {
            _currentLayer -= 1;
            _currentGrid = _gridLayers[_currentLayer];
            GetComponent<SpriteRenderer>().sortingOrder -= 1;

            ResetDepth();
        }
    }

    /// <summary>
    /// <c>ResetDepth</c> recalculates the brightness and darkness levels of all layers above and beneath the player.
    /// </summary>
    public void ResetDepth()
    {
        GenerateFog();

        // Reset dimness for lower levels
        for (int i = 0; i < _gridLayers[0].GetLength(0); i++)
        {
            for (int j = 0; j < _gridLayers[0].GetLength(1); j++)
            {
                _darkGrid[j, i].GetComponent<SpriteRenderer>().color =
                                new Color(0f, 0f, 0f, 0f);
            }
        }
        for (int i = 0; i < _gridLayers[0].GetLength(0); i++)
        {
            for (int j = 0; j < _gridLayers[0].GetLength(1); j++)
            {
                if (_brightGrid[j, i].GetComponent<SpriteRenderer>().color.a == 0f)
                {
                    if (_currentLayer > 0 && _gridLayers[_currentLayer - 1][i, j] != null
                        && _gridLayers[_currentLayer - 1][i, j].tag == "Wall")
                    {
                        continue;
                    }
                    for (int k = _currentLayer - 1; k >= 0; k -= 1)
                    {
                        if ((_gridLayers[k][i, j] != null
                            || (k != 0 && _gridLayers[k - 1][i, j] != null
                            && _gridLayers[k - 1][i, j].tag == "Wall"))
                            || (_gridLayers[k][i, j] == null && k == 0))
                        {
                            float shiftPower = 1;
                            if (k != 0 && _gridLayers[k - 1][i, j] != null && _gridLayers[k - 1][i, j].tag == "Wall")
                            {
                                shiftPower = 2;
                            }
                            _darkGrid[j, i].GetComponent<SpriteRenderer>().color =
                            new Color(0f, 0f, 0f,
                            1f - 0.5f * (float)Mathf.Pow(0.8f, (-k + _currentLayer - shiftPower))
                            );
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// <c>Operation</c> is a delegate operation method meant to be overridden by a newly defined operation.
    /// </summary>
    delegate void Operation(int x, int y);

    /// <summary>
    /// <c>OperateOnNearbySquares</c> performs an operation (a method passed as a parameter) 
    /// on all nearby grid squares to a given location.
    /// </summary>
    private void OperateOnNearbySquares(float x, float y, Operation operation)
    {
        x = ConvertToGridPosition(x, "x");
        y = ConvertToGridPosition(y, "y");

        if (x >= 0 && x < _currentGrid.GetLength(0) &&
            y >= 0 && y < _currentGrid.GetLength(1))
        {
            operation((int)x, (int)y);

            if (x + 1 < _currentGrid.GetLength(0))
            {
                operation((int)(x + 1), (int)y);

                if (y + 1 < _currentGrid.GetLength(1))
                {
                    operation((int)(x + 1), (int)(y + 1));
                }
                if (y - 1 >= 0)
                {
                    operation((int)(x + 1), (int)(y - 1));
                }
            }
            if (x - 1 >= 0)
            {
                operation((int)(x - 1), (int)y);

                if (y + 1 < _currentGrid.GetLength(1))
                {
                    operation((int)(x - 1), (int)(y + 1));
                }
                if (y - 1 >= 0)
                {
                    operation((int)(x - 1), (int)(y - 1));
                }
            }
            if (y - 1 >= 0)
            {
                operation((int)x, (int)(y - 1));
            }
            if (y + 1 < _currentGrid.GetLength(1))
            {
                operation((int)x, (int)(y + 1));
            }
        }
    }

    /// <summary>
    /// <c>PlayerCollision</c> sets `Player.InCollision` to `true` if player is inside one or more `obstacles`.
    /// </summary>
    private void PlayerCollision(BoxCollider2D playerBody, string[] obstacles)
    {
        /// <summary>
        /// <c>PlayerCollisionInnerHelper</c> overrides Operation.
        /// </summary>
        void PlayerCollisionInnerHelper(int x, int y)
        {
            if (_currentGrid[x, y] != null && 
                Array.Exists(obstacles, element => element == _currentGrid[x, y].tag))
            {
                CollisionManager.DetectCollision(gameObject, _currentGrid[x, y]);
            }
            if (Array.Exists(obstacles, element => element == "null") && 
                _currentGrid[x, y] == null && _currentLayer > 0 &&
                _gridLayers[_currentLayer - 1][x, y] == null)
            {
                DetectCollisionEmpty(playerBody, 
                    ConvertToWorldPosition(x, "x"),
                    ConvertToWorldPosition(y, "y"), 
                    _gridSize);
            }
        }
        Operation helper = new Operation(PlayerCollisionInnerHelper);
        OperateOnNearbySquares(transform.position.x, transform.position.y, helper);
        InCollision = false;
    }

    /// <summary>
    /// <c>SetGridInformation</c> transfers the grid information from GridGenerator.cs to Player.cs.
    /// </summary>
    public void SetGridInformation(
        string[,] gridString,
        float gridSize,
        GameObject[,] lightGrid,
        GameObject[,,] gridLayers,
        int currentLayer,
        int maxLayer,
        GameObject[,] brightGrid,
        GameObject[,] darkGrid
    )
    {
        _gridHeight = gridString.GetLength(0);
        _gridWidth = gridString.GetLength(1);
        _gridSize = gridSize;
        _lightGrid = lightGrid;
        _brightGrid = brightGrid;
        _darkGrid = darkGrid;

        for (int i = 0; i < _lightGrid.GetLength(0); i += 1)
        {
            for (int j = 0; j < _lightGrid.GetLength(1); j += 1)
            {
                _lightGrid[i, j].tag = "Lights";
            }
        }
        _gridLayers = new GameObject[maxLayer][,];
        for (int layer = 0; layer < maxLayer; layer++)
        {
            _gridLayers[layer] = new GameObject[gridLayers.GetLength(1), gridLayers.GetLength(0)];
        }
        for (int i = 0; i < gridLayers.GetLength(0); i++)
        {
            for (int j = 0; j < gridLayers.GetLength(1); j++)
            {
                for (int k = 0; k < maxLayer; k++)
                {
                    _gridLayers[k][j, i] = gridLayers[i, j, k];
                }
            }
        }
        _currentLayer = currentLayer;
        _currentGrid = _gridLayers[_currentLayer];

        _gridLocation = FindGridPlacement();

        ResetDepth();
    }

    /// <summary>
    /// <c>PrintCurrentGrid</c> prints the current grid layout to the console for debugging purposes.
    /// </summary>
    public void PrintCurrentGrid()
    {
        string s = "";
        for (int i = 0; i < _currentGrid.GetLength(1); i += 1)
        {
            for (int j = 0; j < _currentGrid.GetLength(0); j += 1)
            {
                if (_currentGrid[j, i] != null)
                {
                    s += " " + _currentGrid[j, i].tag + " ";
                }
                else
                {
                    s += " null ";
                }
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    /// <summary>
    /// <c>ConvertToGridPosition</c> is a helper function to quickly convert world positions to grid locations.
    /// </summary>
    private int ConvertToGridPosition(float pos, string direction)
    {
        if (direction == "x")
        {
            return (int)(Mathf.Round(pos / _gridSize) * _gridSize);
        }
        if (direction == "y")
        {
            return (int)(_gridHeight * _gridSize - (Mathf.Round(pos / _gridSize) * _gridSize));
        }
        return -1;
    }

    /// <summary>
    /// <c>ConvertToWorldPosition</c> is a helper function to quickly convert grid positions to world locations.
    /// </summary>
    private float ConvertToWorldPosition(int pos, string direction)
    {
        if (direction == "x")
        {
            return (float)(pos * _gridSize);
        }
        if (direction == "y")
        {
            return (float)(_gridHeight * _gridSize - pos * _gridSize);
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
        if (gridLocation[0] >= _gridWidth)
        {
            gridLocation[0] = (int)_gridWidth - 1;
        }
        if (gridLocation[1] < 0)
        {
            gridLocation[1] = 0;
        }
        if (gridLocation[1] >= _gridHeight)
        {
            gridLocation[1] = (int)_gridHeight - 1;
        }
        return gridLocation;
    }

    /// <summary>
    /// <c>GenerateBrightness/c> brights up the tiles above the player's layer, 
    /// to some 'depthSize' distance.
    /// </summary>
    private void GenerateBrightness(int x, int y)
    {
        for (int k = _gridLayers.Length - 1; k >= _currentLayer; k -= 1)
        {
            if (_gridLayers[k][x, y] != null)
            {
                float shiftPower = 1;
                if (_gridLayers[k][x, y].tag == "Wall")
                {
                    shiftPower = 0;
                }
                if (k != _currentLayer || shiftPower == 0)
                {
                    _brightGrid[y, x].GetComponent<SpriteRenderer>().color =
                        new Color(1f, 1f, 1f,
                        1f - 0.7f * (float)Mathf.Pow(0.8f, (k - _currentLayer - shiftPower)));
                }
                break;
            }
        }
    }

    /// <summary>
    /// <c>GenerateFog</c> lights up the tiles next to the player, 
    /// to some 'depthSize' distance.
    /// </summary>
    private void GenerateFog(int depthSize = -1, float[] rgbColor = null)
    {
        float r = (rgbColor is null) ? _fogColor[0] : rgbColor[0];
        float g = (rgbColor is null) ? _fogColor[1] : rgbColor[1];
        float b = (rgbColor is null) ? _fogColor[2] : rgbColor[2];
        rgbColor = new float[] { r, g, b };
        depthSize = (depthSize == -1) ? _fogDepthSize : depthSize;
        if (rgbColor is not null && rgbColor.Length != 3)
            throw new ArgumentException("rgbColor should be an float list of RGB values.");

        for (int i = 0; i < _lightGrid.GetLength(0); i += 1)
        {
            for (int j = 0; j < _lightGrid.GetLength(1); j += 1)
            {
                _lightGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(r, g, b, 1f);
                _brightGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
            }
        }

        GenerateFogHelper(_gridLocation[0] + 1, _gridLocation[1] + 1, depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0] - 1, _gridLocation[1] + 1, depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0] + 1, _gridLocation[1] - 1, depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0] - 1, _gridLocation[1] - 1, depthSize, depthSize, rgbColor);

        GenerateFogHelper(_gridLocation[0] - 1, _gridLocation[1], depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0] + 1, _gridLocation[1], depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0], _gridLocation[1] - 1, depthSize, depthSize, rgbColor);
        GenerateFogHelper(_gridLocation[0], _gridLocation[1] + 1, depthSize, depthSize, rgbColor);

        GenerateFogHelper(_gridLocation[0], _gridLocation[1], depthSize, depthSize, rgbColor);
    }

    /// <summary>
    /// <c>GenerateFogHelper</c> is a helper function for 'GenerateFog', 
    /// which recursively calls itself 'maxDepth' times. 
    /// </summary>
    private void GenerateFogHelper(int x, int y, int depth, int maxDepth, float[] rgbColor = null)
    {
        float r = (rgbColor is not null) ? rgbColor[0] : 0f;
        float g = (rgbColor is not null) ? rgbColor[1] : 0f;
        float b = (rgbColor is not null) ? rgbColor[2] : 0f;
        if (depth == 0)
        {
            return;
        }
        if (x < 0 || x >= _gridWidth * _gridSize ||
            y < 0 || y >= _gridHeight * _gridSize)
        {
            return;
        }
        if (_lightGrid[y, x].GetComponent<SpriteRenderer>().color.a
            > (1f - ((float)depth / (float)maxDepth)))
        {
            _lightGrid[y, x].GetComponent<SpriteRenderer>().color
                = new Color(r, g, b, 1f - ((float)depth / (float)maxDepth));
            GenerateBrightness(x, y);
            if (_currentGrid[x, y] != null && _currentGrid[x, y].tag == "Wall")
            {
                return;
            }
            else
            {
                GenerateFogHelper(x - 1, y, depth - 1, maxDepth, rgbColor);
                GenerateFogHelper(x + 1, y, depth - 1, maxDepth, rgbColor);
                GenerateFogHelper(x, y - 1, depth - 1, maxDepth, rgbColor);
                GenerateFogHelper(x, y + 1, depth - 1, maxDepth, rgbColor);
            }
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// <c>SetFogParameters</c> sets the color and depth of the fog. Either
    /// pass in an int `depthSize` and a float[] `rgbColor` array of RGB channels
    /// as floats between 0 and 1, or choose one of the presets:
    /// `dark`, `opaqueDark`, `denseFog`, or `lightFog`. Defaults to black.
    /// </summary>
    public void SetFogParameters(
        int depthSize = -1,
        float[] rgbColor = null,
        bool opaqueDark = false,
        bool dark = false,
        bool denseFog = false,
        bool lightFog = false
    )
    {
        // Throw error if more than one bool
        if ((dark ? 1 : 0) + (opaqueDark ? 1 : 0) + (denseFog ? 1 : 0) + (lightFog ? 1 : 0) > 1)
            throw new ArgumentException("At most one bool parameter may be `true`.");

        // Throw error if bool + rgb
        if ((dark ? 1 : 0) + (opaqueDark ? 1 : 0) + (denseFog ? 1 : 0) + (lightFog ? 1 : 0) > 0)
        {
            if (rgbColor is not null)
                throw new ArgumentException("Cannot specify both `rgbColor` and booleans.");
        }

        // Throw error if rbg is not the correct size
        if (rgbColor is not null && rgbColor.Length != 3)
            throw new ArgumentException("rgbColor should be an float list of RGB values.");

        // Actual logic
        if (rgbColor is not null)
        {
            _fogColor = rgbColor;
            if (depthSize != -1) _fogDepthSize = depthSize;
        }

        if (dark)
        {
            _fogColor = new float[] { 0f, 0f, 0f };
            if (depthSize == -1) _fogDepthSize = 9;
        }

        if (opaqueDark)
        {
            _fogColor = new float[] { 0f, 0f, 0f };
            if (depthSize == -1) _fogDepthSize = 4;
        }

        if (denseFog)
        {
            _fogColor = new float[] { 0.3f, 0.3f, 0.3f };
            if (depthSize == -1) _fogDepthSize = 6;
        }

        if (lightFog)
        {
            _fogColor = new float[] { 0.5f, 0.5f, 0.5f };
            if (depthSize == -1) _fogDepthSize = 16;
        }
    }


    public Vector2 GetPosition()
    {
        return Position;
    }
    public void SetPositionX(float x)
    {
        Position.x = x;
    }
    public void SetPositionY(float y)
    {
        Position.y = y;
    }
    public void SetTransformPosition(Vector2 pos)
    {
        transform.position = pos;
    }
    public BoxCollider2D GetStartClimbingRamp()
    {
        return _startClimbingRamp;
    }
    public void SetStartClimbingRamp(BoxCollider2D body)
    {
        _startClimbingRamp = body;
    }
}
