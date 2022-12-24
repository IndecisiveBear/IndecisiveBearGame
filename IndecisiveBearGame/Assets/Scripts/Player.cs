using UnityEngine;

public class Player : MonoBehaviour
{
    float speed = 0.01f;
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
        MovementHandler();
        transform.position = position;
    }

    public void MovementHandler()
    {
        int upMoveCount = 0;
        int rightMoveCount = 0;

        if (Input.GetKey("w"))
        {
            upMoveCount += 1;
        }
        if (Input.GetKey("s"))
        {
            upMoveCount -= 1;
        }
        if (Input.GetKey("a"))
        {
            rightMoveCount -= 1;
        }
        if (Input.GetKey("d"))
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
