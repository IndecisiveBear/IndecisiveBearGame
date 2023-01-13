using UnityEngine;

public class Player : MonoBehaviour
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

    BoxCollider2D Body;

    void Start()
    {
        Body = gameObject.GetComponent<BoxCollider2D>();

        _gridLocation = FindGridPlacement();
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
            PlayerCollision(Body, "Wall");
            PlayerCollision(Body, "RampN");
            PlayerCollision(Body, "RampS");
            PlayerCollision(Body, "RampW");
            PlayerCollision(Body, "RampE");
            if (_currentLayer > 0)
            {
                PlayerCollision(Body, "null");
                PlayerCollision(Body, "RampNDown");
                PlayerCollision(Body, "RampSDown");
                PlayerCollision(Body, "RampWDown");
                PlayerCollision(Body, "RampEDown");
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
    private bool DetectCollisionWall(BoxCollider2D body1, BoxCollider2D body2)
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
    /// <c>DetectCollisionRampS</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a south facing ramp, from a lower level to a higher level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampS(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    body1.bounds.center.y - body1.bounds.extents.y <
                    body2.bounds.center.y + body2.bounds.extents.y * _rampFactor2 &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1f * body2.bounds.extents.x)
                {
                    // Colliding with top edge of body2
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y - body2.bounds.extents.y)
            {
                // Exiting with bottom edge of body2
                MoveUpGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x * _rampFactor >=
            body2.bounds.center.x - body2.bounds.extents.x * _rampFactor &&
            body1.bounds.center.x - body1.bounds.extents.x * _rampFactor <=
            body2.bounds.center.x + body2.bounds.extents.x * _rampFactor &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                // Detect the edge with which we are exiting collision
                if (body1.bounds.center.x - body1.bounds.extents.x * _rampFactor >
                    body2.bounds.center.x + body2.bounds.extents.x * _rampFactor)
                {
                    // Exiting with right edge of body2
                    Position.x = body2.bounds.center.x + body2.bounds.extents.x * _rampFactor +
                        body1.bounds.extents.x * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.x + body1.bounds.extents.x * _rampFactor <
                    body2.bounds.center.x - body2.bounds.extents.x * _rampFactor)
                {
                    // Exiting with left edge of body2
                    Position.x = body2.bounds.center.x - body2.bounds.extents.x * _rampFactor -
                        body1.bounds.extents.x * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.y - body1.bounds.extents.y * 0.9 >
                    body2.bounds.center.y + body2.bounds.extents.y)
                {
                    // Exiting with top edge of body2
                    _startClimbingRamp = null;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampSDown</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a south facing ramp, from a higher level to a lower level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampSDown(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    _startClimbingRamp = body2;
                }
                return true;
            }
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Exiting with bottom edge of body2
                MoveDownGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                _startClimbingRamp = null;
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampN</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a north facing ramp, from a lower level to a higher level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampN(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    body1.bounds.center.y + body1.bounds.extents.y >
                    body2.bounds.center.y - body2.bounds.extents.y * _rampFactor2 &&
                    Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1f * body2.bounds.extents.x)
                {
                    // Colliding with bottom edge of body2
                    _startClimbingRamp = body2;
                }
                return true;
            }
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Exiting with top edge of body2
                MoveUpGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x * _rampFactor >=
            body2.bounds.center.x - body2.bounds.extents.x * _rampFactor &&
            body1.bounds.center.x - body1.bounds.extents.x * _rampFactor <=
            body2.bounds.center.x + body2.bounds.extents.x * _rampFactor &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                // Detect the edge with which we are exiting collision
                if (body1.bounds.center.x - body1.bounds.extents.x * _rampFactor >
                    body2.bounds.center.x + body2.bounds.extents.x * _rampFactor)
                {
                    // Exiting with right edge of body2
                    Position.x = body2.bounds.center.x + body2.bounds.extents.x * _rampFactor +
                        body1.bounds.extents.x * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.x + body1.bounds.extents.x * _rampFactor <
                    body2.bounds.center.x - body2.bounds.extents.x * _rampFactor)
                {
                    // Exiting with left edge of body2
                    Position.x = body2.bounds.center.x - body2.bounds.extents.x * _rampFactor -
                        body1.bounds.extents.x * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.y + body1.bounds.extents.y * 0.9 <
                    body2.bounds.center.y - body2.bounds.extents.y)
                {
                    // Exiting with bottom edge of body2
                    _startClimbingRamp = null;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampNDown</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a north facing ramp, from a higher level to a lower level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampNDown(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y - body2.bounds.extents.y)
            {
                // Exiting with top edge of body2
                MoveDownGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                _startClimbingRamp = null;
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampW</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a west facing ramp, from a lower level to a higher level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampW(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
        {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >= body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <= body2.bounds.center.y + body2.bounds.extents.y)
            {
                // Detect the edge with which we are colliding.
                if (body1.bounds.center.x > body2.bounds.center.x &&
                    body1.bounds.center.x - body1.bounds.extents.x <
                    body2.bounds.center.x + body2.bounds.extents.x * _rampFactor2 &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1f * body2.bounds.extents.y)
                {
                    // Colliding with right edge of body2
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x - body2.bounds.extents.x)
            {
                // Exiting with top edge of body2
                MoveUpGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y * _rampFactor >=
            body2.bounds.center.y - body2.bounds.extents.y * _rampFactor &&
            body1.bounds.center.y - body1.bounds.extents.y * _rampFactor <=
            body2.bounds.center.y + body2.bounds.extents.y * _rampFactor
            ))
            {
                // Detect the edge with which we are exiting collision
                if (body1.bounds.center.y - body1.bounds.extents.y * _rampFactor >
                    body2.bounds.center.y + body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with top edge of body2
                    Position.y = body2.bounds.center.y + body2.bounds.extents.y * _rampFactor +
                        body1.bounds.extents.y * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.y + body1.bounds.extents.y * _rampFactor <
                    body2.bounds.center.y - body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with bottom edge of body2
                    Position.y = body2.bounds.center.y - body2.bounds.extents.y * _rampFactor -
                        body1.bounds.extents.y * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.x - body1.bounds.extents.x * 0.9 >
                    body2.bounds.center.x + body2.bounds.extents.x)
                {
                    // Exiting with right edge of body2
                    _startClimbingRamp = null;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampWDown</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a west facing ramp, from a higher level to a lower level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampWDown(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x + body2.bounds.extents.x)
            {
                // Exiting with right edge of body2
                MoveDownGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                _startClimbingRamp = null;
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampE</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a east facing ramp, from a lower level to a higher level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampE(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    body1.bounds.center.x + body1.bounds.extents.x >
                    body2.bounds.center.x - body2.bounds.extents.x * _rampFactor2 &&
                    Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1f * body2.bounds.extents.y)
                {
                    // Colliding with left edge of body2
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.x + body1.bounds.extents.x >= body2.bounds.center.x + body2.bounds.extents.x)
            {
                // Exiting with top edge of body2
                MoveUpGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y * _rampFactor >=
            body2.bounds.center.y - body2.bounds.extents.y * _rampFactor &&
            body1.bounds.center.y - body1.bounds.extents.y * _rampFactor <=
            body2.bounds.center.y + body2.bounds.extents.y * _rampFactor
            ))
            {
                // Detect the edge with which we are exiting collision
                if (body1.bounds.center.y - body1.bounds.extents.y * _rampFactor >
                    body2.bounds.center.y + body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with top edge of body2
                    Position.y = body2.bounds.center.y + body2.bounds.extents.y * _rampFactor +
                        body1.bounds.extents.y * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.y + body1.bounds.extents.y * _rampFactor <
                    body2.bounds.center.y - body2.bounds.extents.y * _rampFactor)
                {
                    // Exiting with bottom edge of body2
                    Position.y = body2.bounds.center.y - body2.bounds.extents.y * _rampFactor -
                        body1.bounds.extents.y * _rampFactor;
                    transform.position = Position;
                }
                if (body1.bounds.center.x + body1.bounds.extents.x * 0.9 <
                    body2.bounds.center.x - body2.bounds.extents.x)
                {
                    // Exiting with right edge of body2
                    _startClimbingRamp = null;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// <c>DetectCollisionRampEDown</c> handles collision detection logic between two BoxColliders, one being 
    /// the player, and the other being a east facing ramp, from a higher level to a lower level. 
    /// It also warps player to nearest acceptable location if they are inside of a ramp.
    /// </summary>
    /// <returns>
    /// Whether or not a collision was detected.
    /// </returns>
    private bool DetectCollisionRampEDown(BoxCollider2D body1, BoxCollider2D body2)
    {
        if (_startClimbingRamp == null)
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
                    _startClimbingRamp = body2;
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
        }
        else if (_startClimbingRamp == body2)
        {
            if (body1.bounds.center.x - body1.bounds.extents.x <= body2.bounds.center.x - body2.bounds.extents.x)
            {
                // Exiting with right edge of body2
                MoveDownGrid();
                _startClimbingRamp = null;
            }
            if (
            !(
            body1.bounds.center.x + body1.bounds.extents.x >=
            body2.bounds.center.x - body2.bounds.extents.x &&
            body1.bounds.center.x - body1.bounds.extents.x <=
            body2.bounds.center.x + body2.bounds.extents.x &&
            body1.bounds.center.y + body1.bounds.extents.y >=
            body2.bounds.center.y - body2.bounds.extents.y &&
            body1.bounds.center.y - body1.bounds.extents.y <=
            body2.bounds.center.y + body2.bounds.extents.y
            ))
            {
                _startClimbingRamp = null;
            }
        }
        return false;
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
        GenerateLight();

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
    void OperateOnNearbySquares(float x, float y, Operation operation)
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
    private void PlayerCollision(BoxCollider2D playerBody, string obstacles)
    {
        /// <summary>
        /// <c>PlayerCollisionInnerHelper</c> overrides Operation.
        /// </summary>
        void PlayerCollisionInnerHelper(int x, int y)
        {
            if (_currentGrid[x, y] != null && _currentGrid[x, y].tag == obstacles)
            {
                BoxCollider2D box = _currentGrid[x, y].GetComponent<BoxCollider2D>();
                /// For a future commit, let's handle collisions for 
                /// different types of collidables differently. Right now we're just brute forcing
                /// every 'obstacles' string and storing all the DetectCollision functions within
                /// Player.cs. I propose a better solution would be to have a static class per 
                /// collidable (e.g. "WallCollision", "RampCollision", etc) with their own ways
                /// of handling collision, and calling their CollideLeft, CollideRight, etc. methods
                /// after we handle the direction check in Player.cs, and wish to now 'act' on 
                /// our information. So this solution would have:
                /// 1. Only one DecectCollision method in Player.cs
                /// 2. A static class per collidable prefab (but never attach this script to the 
                /// prefabs, as this would take too much processing power. That's why we have it 
                /// as a static class. 
                /// 3. Possibly have each Collision script inherent from a mother Collision script.
                /// 4. Hence, Player detects collision and furthermore detects the direction of collision, 
                /// then calls the respective Collision script based on the 'obstacles' parameter,
                /// which then handles how to act given its colliding with the player from the given direction.
                if (obstacles == "Wall")
                {
                    DetectCollisionWall(playerBody, box);
                }
                if (obstacles == "RampN")
                {
                    DetectCollisionRampN(playerBody, box);
                }
                if (obstacles == "RampNDown")
                {
                    DetectCollisionRampNDown(playerBody, box);
                }
                if (obstacles == "RampS")
                {
                    DetectCollisionRampS(playerBody, box);
                }
                if (obstacles == "RampSDown")
                {
                    DetectCollisionRampSDown(playerBody, box);
                }
                if (obstacles == "RampW")
                {
                    DetectCollisionRampW(playerBody, box);
                }
                if (obstacles == "RampWDown")
                {
                    DetectCollisionRampWDown(playerBody, box);
                }
                if (obstacles == "RampE")
                {
                    DetectCollisionRampE(playerBody, box);
                }
                if (obstacles == "RampEDown")
                {
                    DetectCollisionRampEDown(playerBody, box);
                }
            }
            if (obstacles == "null" && _currentGrid[x, y] == null && _currentLayer > 0 &&
                _gridLayers[_currentLayer - 1][x, y] == null)
            {
                DetectCollisionEmpty(playerBody, ConvertToWorldPosition(x, "x"), ConvertToWorldPosition(y, "y"), _gridSize);
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
    /// <c>GenerateLight</c> lights up the tiles next to the player, 
    /// to some 'depthSize' distance.
    /// </summary>
    private void GenerateLight()
    {
        for (int i = 0; i < _lightGrid.GetLength(0); i += 1)
        {
            for (int j = 0; j < _lightGrid.GetLength(1); j += 1)
            {
                _lightGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 1f);
                _brightGrid[i, j].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
            }
        }
        int depthSize = 9;

        GenerateLightHelper(_gridLocation[0] + 1, _gridLocation[1] + 1, depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0] - 1, _gridLocation[1] + 1, depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0] + 1, _gridLocation[1] - 1, depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0] - 1, _gridLocation[1] - 1, depthSize, depthSize);

        GenerateLightHelper(_gridLocation[0] - 1, _gridLocation[1], depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0] + 1, _gridLocation[1], depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0], _gridLocation[1] - 1, depthSize, depthSize);
        GenerateLightHelper(_gridLocation[0], _gridLocation[1] + 1, depthSize, depthSize);

        GenerateLightHelper(_gridLocation[0], _gridLocation[1], depthSize, depthSize);
    }

    /// <summary>
    /// <c>GenerateLightHelper</c> is a helper function for 'GenerateLight', 
    /// which recursively calls itself 'maxDepth' times. 
    /// </summary>
    private void GenerateLightHelper(int x, int y, int depth, int maxDepth)
    {
        if (depth == 0)
        {
            return;
        }
        if (x < 0 || x >= _gridWidth * _gridSize
            || y < 0 || y >= _gridHeight * _gridSize)
        {
            return;
        }
        if (_lightGrid[y, x].GetComponent<SpriteRenderer>().color.a
            > (1f - ((float)depth / (float)maxDepth)))
        {
            _lightGrid[y, x].GetComponent<SpriteRenderer>().color
                = new Color(0f, 0f, 0f, 1f - ((float)depth / (float)maxDepth));
            GenerateBrightness(x, y);
            if (_currentGrid[x, y] != null && _currentGrid[x, y].tag == "Wall")
            {
                return;
            }
            else
            {
                GenerateLightHelper(x - 1, y, depth - 1, maxDepth);
                GenerateLightHelper(x + 1, y, depth - 1, maxDepth);
                GenerateLightHelper(x, y - 1, depth - 1, maxDepth);
                GenerateLightHelper(x, y + 1, depth - 1, maxDepth);
            }
        }
        else
        {
            return;
        }
    }
}
