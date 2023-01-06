using UnityEngine;

public class GridTestSceneInit : MonoBehaviour
{
    GridGenerator Grid;
    static string[,] _gridString = new string[,] {
        {"W"," "," "," ","W"},
        {"W"," ","P"," ","W"},
        {"W"," "," "," ","W"},
        {"W","W"," ","W","W"}
    };

    void Start()
    {
        Debug.Log("Grid Initializing...");
        GridGenerator Grid = new GridGenerator(_gridString);
    }
}
