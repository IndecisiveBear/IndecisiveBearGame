using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    float speed = 0.01f;
    GameObject[] walls;
    void Start()
    {
        walls = GameObject.FindGameObjectsWithTag("Wall");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            transform.position += transform.up * speed;
        }
        if (Input.GetKey("s"))
        {
            transform.position -= transform.up * speed;
        }
        if (Input.GetKey("a"))
        {
            transform.position -= transform.right * speed;
        }
        if (Input.GetKey("d"))
        {
            transform.position += transform.right * speed;
        }
    }
}
