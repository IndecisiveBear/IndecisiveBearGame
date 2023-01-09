using UnityEngine;

public class GridTestSceneInit : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject Player;
    public GameObject Wall;
    public GameObject RampN;
    
    GridGenerator Grid;
    static string[,] _gridString = new string[,] {
        {"W"," "," "," ","W","W"},
        {"W"," ","P"," ","W"," "},
        {"W"," "," "," ","W"," "},
        {"W","W"," ","W","W"," "}
    };

    void Awake()
    {
        GameObject gameObject = new GameObject("GridGenerator");
        Grid = gameObject.AddComponent<GridGenerator>();
        Grid.SetPrefabs(
            player: Player, 
            wall: Wall, 
            rampN: RampN
        );
        Grid.GenerateGrid(_gridString);
        Instantiate(MainCamera, new Vector2(0, 0), Quaternion.identity);
    }
}
