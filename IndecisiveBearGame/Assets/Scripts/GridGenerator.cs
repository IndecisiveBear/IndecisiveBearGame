using System.Collections.Generic;
using System.Linq; // for List<int>.Max()
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Player;
    public GameObject Wall;
    public GameObject RampN;
    public float GridSize = 1f;
    public int MaxObjectsPerGridLocation;
    public string[,] GridString;
    public GameObject[,,] Grid;
    GameObject Objects;
    
    public GridGenerator(string[,] gridString)
    { 
        Debug.Log("GridGenerator Constructor running...");
        GridString = gridString;
        GenerateGrid();
    }

    public void GenerateGrid() //GenerateGrid(string[,] gridString)
    {
        Debug.Log("GridGenerator.GenerateGrid() running...");
        SetMaxObjectsPerGridLocation(GridString);
        Grid = new GameObject[GridString.GetLength(0), GridString.GetLength(1), MaxObjectsPerGridLocation];
        SetGrid(Grid, GridString);
        InstantiateGrid(Grid);
    }

    /// <summary>
    /// <c>SetMaxObjectsPerGridLocation</c> loops through `gridString` and sets 
    /// `GridGenerator.MaxObjectsPerGridLocation` to be the largest number of 
    /// game objects placed in a single location in a scene.
    /// </summary>
    void SetMaxObjectsPerGridLocation(string[,] gridString)
    {
        Debug.Log("GridGenerator.SetMaxObjectsPerGridLocation() running...");
        List<int> objectLengths = new List<int>();
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                objectLengths.Add(ParseGridString(gridString[i, j]).Length);
            }
        }
        MaxObjectsPerGridLocation = objectLengths.Max();
    }

    void SetGrid(GameObject[,,] grid, string[,] gridString)
    {
        Debug.Log("GridGenerator.SetGrid() running...");
        string[] objectList;
        string item;
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                objectList = ParseGridString(gridString[i, j]);
                for (int k = 0; k < MaxObjectsPerGridLocation; k++)
                {
                    try 
                    {
                        item = objectList[k];
                        Debug.Log(
                            "Item : " + item + " on (i, j, k) = (" + i + ", " + j + ", " + k + ")"
                        );
                        if (item.ToUpper() == " ") grid[i, j, k] = null;
                        if (item.ToUpper() == "P") grid[i, j, k] = Player;
                        if (item.ToUpper() == "W") grid[i, j, k] = Wall;
                        if (item.ToUpper() == "R") grid[i, j, k] = RampN;
                        Debug.Log(
                            "Grid(" + i + ", " + j + ", " + k + ") null? " + (grid[i, j, k] is null)
                        );
                    }
                    catch(System.IndexOutOfRangeException)
                    {
                        Debug.Log(
                            "Grid(" + i + ", " + j + ", " + k + ") set to null"
                        );
                        grid[i, j, k] = null;
                    }
                }
            }
        }
    }

    void InstantiateGrid(GameObject[,,] grid)
    {
        Debug.Log("GridGenerator.InstantiateGrid() running...");
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                for (int k = 0; k < MaxObjectsPerGridLocation; k++)
                {
                    Debug.Log("grid(" + i + ", " + j + ", " + k + ") = " + grid[i, j, k]);
                    if (grid[i, j, k] is not null)
                    {
                        Debug.Log("Instantiating Object " + grid[i, j, k]);
                        Objects = Instantiate(
                            grid[i, j, k], 
                            new Vector2((i - grid.GetLength(0)/2) * GridSize, 
                                        (j - grid.GetLength(1)/2) * GridSize), 
                            Quaternion.identity
                        );
                    }
                }
            }
        }
        Debug.Log("Done Instantiating.");
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

        string[] returnString = new string[] {text}; // Boilder plate code.
        return returnString;
    }

    public int GetSceneWidth() { return Grid.GetLength(0); }

    public int GetSceneHeight() { return Grid.GetLength(1); }
}
