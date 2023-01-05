using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Player;
    public GameObject Walls;
    public float GridSize = 2f;
    public string[,] GridString;
    GameObject[,] Grid;
    GameObject Objects;
    

    public GridGenerator(string[,] gridString)
    { 
        GridString = gridString;
    }

    void Main()
    {
        Grid = new GameObject[GridString.GetLength(0), GridString.GetLength(1)];
        SetGrid(Grid, GridString);
        InstantiateGrid(Grid);
    }

    void SetGrid(GameObject[,] grid, string[,] gridString)
    {
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                // Get the list of game objects to place on the scene at grid point (i,j).
                string[] objectList = GridStringParser(gridString[i, j]);
                foreach (string item in objectList)
                { 
                    if (item.ToUpper() == " ")
                    {
                        grid[i, j] = null;
                    }
                    if (item.ToUpper() == "P")
                    {
                        grid[i, j] = Player;
                    }
                    if (item.ToUpper() == "W")
                    {
                        // We need additonal structure. Should we make Grid a 3D array?
                        // Grid[i, j, k] can be such that k denotes the kth item from 
                        // the ground at location (i,j).
                        grid[i, j] = Walls;
                    }
                }
            }
        }
    }

    void InstantiateGrid(GameObject[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {

                if (grid[i, j] is not null)
                {
                     Objects = Instantiate(
                        grid[i, j], 
                        new Vector2(j * GridSize, (4 - i) * GridSize), 
                        Quaternion.identity);
                }
            }
        }
    }

    /// <summary>
    /// <c>GridStringParser</c> parses string `text` into an array of strings
    /// where the elements of the array are strings from `text` separated by `delimiter`.
    /// </summary>
    string[] GridStringParser(string text, string delimiter = ":")
    { 
        // Some expected outputs...

        // GridStringParser("A:BC:D");
        // >>> string[] { "A", "BC", "D" };

        // GridStringParser(":A:BC:D:");
        // >>> string[] { "A", "BC", "D" };

        // GridStringParser("ABCD");
        // >>> string[] { "ABCD" };

        string[] returnString = new string[2]; // Boilder plate code.
        return returnString;
    }

    public int GetSceneWidth() { return Grid.GetLength(1); }

    public int GetSceneHeight() { return Grid.GetLength(0); }

    public float GetGridSize() { return GridSize; }

    ~GridGenerator() {}
}
