using System.Collections.Generic;
using System.Linq; // for List<int>.Max()
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Player;
    public GameObject Wall;
    public GameObject RampN;
    public float GridSize = 1f;
    public int MaxObjectsPerLocation;
    public string[,] GridString;
    public GameObject[,,] Grid;
    GameObject Objects;

    /// <summary>
    /// <c>GenerateGrid</c> generates a Unity Scene using `gridString` as a template.
    /// </summary>
    public void GenerateGrid(string[,] gridString)
    {
        Debug.Log("GridGenerator.GenerateGrid() running...");
        Debug.Log(Player);
        GridString = gridString; // Set global parameter
        SetMaxObjectsPerLocation(GridString);
        Grid = new GameObject[
            GridString.GetLength(0), 
            GridString.GetLength(1), 
            MaxObjectsPerLocation];
        SetGrid(Grid, GridString);
        InstantiateGrid(Grid);
    }

    /// <summary>
    /// <c>SetPrefabs</c> acts as a constructor. Run this immediately after instantiation 
    /// to attach prefabs to these GameObject references.
    /// </summary>
    public void SetPrefabs(
        GameObject player = null, 
        GameObject wall = null, 
        GameObject rampN = null
    )
    { 
        Player = player;
        Wall = wall;
        RampN = rampN;
    }

    /// <summary>
    /// <c>SetMaxObjectsPerLocation</c> loops through `gridString` and sets 
    /// `GridGenerator.MaxObjectsPerLocation` to be the largest number of 
    /// game objects placed in a single location in a scene.
    /// </summary>
    public void SetMaxObjectsPerLocation(string[,] gridString)
    {
        List<int> objectLengths = new List<int>();
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                objectLengths.Add(ParseGridString(gridString[i, j]).Length);
            }
        }
        MaxObjectsPerLocation = objectLengths.Max();
    }

    /// <summary>
    /// <c>SetGrid</c> generates `grid` from `gridString`
    /// </summary>
    public void SetGrid(GameObject[,,] grid, string[,] gridString)
    {
        string[] objectList;
        string item;
        for(int i = 0; i < gridString.GetLength(0); i++)
        {
            for(int j = 0; j < gridString.GetLength(1); j++)
            {
                objectList = ParseGridString(gridString[i, j]);
                for (int k = 0; k < MaxObjectsPerLocation; k++)
                {
                    try 
                    {
                        item = objectList[k];
                        if (item.ToUpper() == " ") grid[i, j, k] = null;
                        if (item.ToUpper() == "P") grid[i, j, k] = Player;
                        if (item.ToUpper() == "W") grid[i, j, k] = Wall;
                        if (item.ToUpper() == "R") grid[i, j, k] = RampN;
                    }
                    catch(System.IndexOutOfRangeException)
                    {
                        grid[i, j, k] = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// <c>InstantiateGrid</c> creates a Unity scene from `grid`.
    /// </summary>
    public void InstantiateGrid(GameObject[,,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                for (int k = 0; k < MaxObjectsPerLocation; k++)
                {
                    if (grid[i, j, k] is not null)
                    {
                        Objects = Instantiate(
                            grid[i, j, k], 
                            new Vector2(j * GridSize, (GetSceneHeight() - i) * GridSize),
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
    private string[] ParseGridString(string text, string delimiter = ":")
    { 
        // Some expected outputs...

        // ParseGridString("A:BC:D");
        // >>> string[] { "A", "BC", "D" };

        // ParseGridString(":A:BC:D:");
        // >>> string[] { "A", "BC", "D" };

        // ParseGridString("ABCD");
        // >>> string[] { "ABCD" };

        string[] returnString = new string[] {text}; // Boiler plate code.
        return returnString;
    }

    public int GetSceneHeight() { return Grid.GetLength(0); }
    public int GetSceneWidth() { return Grid.GetLength(1); }
}
