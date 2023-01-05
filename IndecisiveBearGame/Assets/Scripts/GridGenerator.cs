using System.Collections.Generic;
using System.Linq; // for List<int>.Max()
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Player;
    public GameObject Wall;
    public GameObject Ramp;
    public float GridSize = 2f;
    public int MaxObjectCount = 10;
    public string[,] GridString;
    GameObject[,,] Grid;
    GameObject Objects;
    

    public GridGenerator(string[,] gridString)
    { 
        GridString = gridString;
    }

    void Main()
    {
        SetMaxObjectCount(GridString);
        Grid = new GameObject[GridString.GetLength(0), GridString.GetLength(1), MaxObjectCount];
        SetGrid(Grid, GridString);
        InstantiateGrid(Grid);
    }

    void SetMaxObjectCount(string[,] gridString)
    {
        List<int> objectLengths = new List<int>();
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                objectLengths.Add(ParseGridString(gridString[i, j]).Length);
            }
        }
        MaxObjectCount = objectLengths.Max();
    }

    void SetGrid(GameObject[,,] grid, string[,] gridString)
    {
        string[] objectList;
        string item;
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                // Get the list of game objects to place on the scene at grid point (i,j).
                objectList = ParseGridString(gridString[i, j]);
                for (int k = 0; k < MaxObjectCount; k++)
                {
                    try 
                    {
                        item = objectList[k];
                        if (item.ToUpper() == " ") grid[i, j, k] = null;
                        if (item.ToUpper() == "P") grid[i, j, k] = Player;
                        if (item.ToUpper() == "W") grid[i, j, k] = Wall;
                        if (item.ToUpper() == "R") grid[i, j, k] = Ramp;
                    }
                    catch(System.IndexOutOfRangeException)
                    {
                        // If objectList.Length < MaxObjectCount, fill the rest with null
                        grid[i, j, k] = null;
                    }
                }
            }
        }
    }

    void InstantiateGrid(GameObject[,,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                for (int k = 0; k < MaxObjectCount; k++)
                {
                    if (grid[i, j, k] is not null)
                    {
                        Objects = Instantiate(
                            grid[i, j, k], 
                            new Vector2(j * GridSize, (4 - i) * GridSize), 
                            Quaternion.identity
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// <c>ParseGridString</c> parses string `text` into an array of strings
    /// where the elements of the array are strings from `text` separated by `delimiter`.
    /// </summary>
    string[] ParseGridString(string text, string delimiter = ":")
    { 
        // Some expected outputs...

        // ParseGridString("A:BC:D");
        // >>> string[] { "A", "BC", "D" };

        // ParseGridString(":A:BC:D:");
        // >>> string[] { "A", "BC", "D" };

        // ParseGridString("ABCD");
        // >>> string[] { "ABCD" };

        string[] returnString = new string[2]; // Boilder plate code.
        return returnString;
    }

    public int GetSceneWidth() { return Grid.GetLength(1); }

    public int GetSceneHeight() { return Grid.GetLength(0); }

    public float GetGridSize() { return GridSize; }

    ~GridGenerator() {}
}
