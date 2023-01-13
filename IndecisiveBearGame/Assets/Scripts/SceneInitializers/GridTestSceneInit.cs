using UnityEngine;

public class GridTestSceneInit : MonoBehaviour
{
    public GameObject Player;
    public GameObject Wall;
    public GameObject RampN;
    public GameObject RampS;
    public GameObject RampE;
    public GameObject RampW;
    public GameObject Tile;

    GridGenerator Grid;
    static string[,] _gridString = new string[,] {
        {" : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : "," : : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : "," : : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : ","W: : : ","L:L: : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : ","R:R: : ","W: : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : ","W: : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : ","W: : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W: : : ","W:R:R: ","W:W: : ","W:W:R:R","W:W:W: ","W:W:W:W","W:W:W:W"},
        {" : : : ","W:W:W: ","W:W:L:L","W:W: : ","W: : : ","T:T: : "," : : : ","T:T: : "," : : : "," : : : ","W:W: : ","W:W: : ","W:W:W: ","W:W:W:W","W:W:W:W"},
        {" : : : ","W:W:W: ","W:W:W: ","W:W: : ","W: : : "," : : : "," : : : "," : : : "," : : : "," : : : ","W:W: : ","W:W: : ","W:W:W: ","W:W:W: ","W:W:W:W"},
        {" : : : ","W:W:W: ","W:W:W: ","W:W: : ","W: : : "," : : : "," : : : ","P: : : "," : : : "," : : : ","W: : : ","W:W: : ","W:W:W: ","W:W:W: ","W:W:W:W"},
        {" : : : "," : : : ","W:W:W: ","W:W: : ","W: : : "," : : : "," : : : "," : : : "," : : : ","R:R: : ","W: : : ","W:W: : ","W:W:W: ","W:W:W:W","W:W:W:W"},
        {" : : : "," : : : ","W:W:W: ","W:W: : ","W: : : "," : : : "," : : : "," : : : "," :W: : "," : : : ","W: : : ","W:W: : ","W:W:W: "," : : : "," : : : "},
        {" : : : "," : : : ","W:W:W: ","W:W: : ","W: : : ","B:B: : "," : : : "," : : : "," :W: : "," : : : ","W: : : ","W:W: : ","W:W:W: "," : : : "," : : : "},
        {" : : : "," : : : ","W:W:W: ","W:W: : ","W: : : ","W: : : ","W: : : ","W:R:R: ","W:W: : ","W:L:L: ","W: : : ","W:W: : ","W:W:W: "," : : : "," : : : "},
        {" : : : "," : : : ","W:W:W: ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W: : ","W:W:W: "," : : : "," : : : "},
        {" : : : "," : : : ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: ","W:W:W: "," : : : "," : : : "},
        {" : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "},
        {" : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "," : : : "}
    };

    void Awake()
    {
        GameObject gameObject = new GameObject("GridGenerator");
        Grid = gameObject.AddComponent<GridGenerator>();
        Grid.SetPrefabs(
            player: Player,
            wall: Wall,
            rampN: RampN,
            rampS: RampS,
            rampE: RampE,
            rampW: RampW,
            tile: Tile
        );
        Grid.GenerateGrid(_gridString);
    }
}