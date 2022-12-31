using UnityEngine;

public class Player : MonoBehaviour
{
    const float PlayerSpeed = 0.01f;
    float speed = PlayerSpeed;
    Vector2 position;
    GameObject[] walls;

    // Start is called before the first frame update
    void Start()
    {
        walls = GameObject.FindGameObjectsWithTag("Wall");
    }

    // Update is called once per frame
    void Update()
    {
        position = transform.position;
        Move();
        transform.position = position;
    }

    public void Move()
    {
        int upMoveCount = 0;
        int rightMoveCount = 0;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed = (speed == PlayerSpeed) ? 2 * PlayerSpeed : PlayerSpeed;
        }
        if (Input.GetKey(KeyCode.W))
        {
            upMoveCount += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            upMoveCount -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            rightMoveCount -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rightMoveCount += 1;
        }
        if (upMoveCount != 0 || rightMoveCount != 0)
        {
            var magnitude =
                Mathf.Sqrt(rightMoveCount * rightMoveCount + upMoveCount * upMoveCount);
            var resultant =
                rightMoveCount * Vector2.right + upMoveCount * Vector2.up;
            position += (resultant / magnitude) * speed;
        }
    }
}
