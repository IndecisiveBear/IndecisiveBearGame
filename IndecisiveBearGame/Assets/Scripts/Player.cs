using UnityEngine;

public class Player : MonoBehaviour
{
    const float WalkSpeed = 0.01f;
    public float PlayerSpeed = WalkSpeed;
    public bool InCollision = false;
    public Vector2 position;
    GameObject[] walls;
    BoxCollider2D body;

    void Start()
    {
        walls = GameObject.FindGameObjectsWithTag("Wall");
        body = gameObject.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        position = transform.position;
        Move();
        transform.position = position;

        if (Input.GetKey(KeyCode.Q)) // Warp to origin, for debugging 
        {
            position = Vector2.zero;
            transform.position = position;
        }
    }

    /// <summary>
    /// <c>Move</c> handles all player movement using `wasd` keys. If `Undo` is `true`, movement will be reversed.
    /// </summary>
    public void Move()
    {
        int _upMoveCount = 0;
        int _rightMoveCount = 0;

        if (Input.GetKeyDown(KeyCode.LeftShift)) PlayerSpeed = (PlayerSpeed == WalkSpeed) ? 2 * WalkSpeed : WalkSpeed;
        if (Input.GetKey(KeyCode.W)) _upMoveCount += 1;
        if (Input.GetKey(KeyCode.S)) _upMoveCount -= 1;
        if (Input.GetKey(KeyCode.A)) _rightMoveCount -= 1;
        if (Input.GetKey(KeyCode.D)) _rightMoveCount += 1;
 
        if (_upMoveCount != 0 || _rightMoveCount != 0)
        {
            PlayerCollision(body, walls);
            var magnitude = Mathf.Sqrt(_rightMoveCount * _rightMoveCount + _upMoveCount * _upMoveCount);
            var resultant = _rightMoveCount * Vector2.right + _upMoveCount * Vector2.up;
            position += (resultant / magnitude) * PlayerSpeed;
        }
    }

    /// <summary>
    /// <c>DetectCollision</c> handles collision detection logic between two BoxColliders. It also warps
    /// player to nearest acceptable location if they are inside of a wall.
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
                position.x = body2.bounds.center.x + body2.bounds.extents.x + body1.bounds.extents.x;
                transform.position = position;
            }
            if (body1.bounds.center.x < body2.bounds.center.x && 
                Mathf.Abs(body1.bounds.center.y - body2.bounds.center.y) < 1.5f * body2.bounds.extents.y)
            {
                // Colliding with left edge of body2
                position.x = body2.bounds.center.x - body2.bounds.extents.x - body1.bounds.extents.x;
                transform.position = position;
            }
            if (body1.bounds.center.y > body2.bounds.center.y && 
                Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
            {
                // Colliding with top edge of body2
                position.y = body2.bounds.center.y + body2.bounds.extents.y + body1.bounds.extents.y;
                transform.position = position;
            }
            if (body1.bounds.center.y < body2.bounds.center.y && 
                Mathf.Abs(body1.bounds.center.x - body2.bounds.center.x) < 1.5f * body2.bounds.extents.x)
            {
                // Colliding with bottom edge of body2
                position.y = body2.bounds.center.y - body2.bounds.extents.y - body1.bounds.extents.y;
                transform.position = position;
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
}