using System.Collections.Generic;
using System.Linq; // for List<int>.Max()
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject Light;
    public GameObject Player;
    public GameObject Wall;
    public GameObject RampN;
    public GameObject RampS;
    public GameObject RampE;
    public GameObject RampW;
    public GameObject Tile;
    public float GridSize = 1f;
    public int MaxObjectsPerLocation;
    public string[,] GridString;
    public GameObject[,] LightGrid;
    public GameObject[,] BrightGrid;
    public GameObject[,] DarkGrid;
    public GameObject[,,] Grid;
    public GameObject[,,] GridInstance;
    GameObject _objects;

    /// <summary>
    /// <c>GenerateGrid</c> generates a Unity Scene using `gridString` as a template.
    /// </summary>
    public void GenerateGrid(string[,] gridString)
    {
        GridString = gridString;
        SetMaxObjectsPerLocation(GridString);
        Grid = new GameObject[
            GridString.GetLength(0),
            GridString.GetLength(1),
            MaxObjectsPerLocation
        ];
        GridInstance = new GameObject[
            GridString.GetLength(0),
            GridString.GetLength(1),
            MaxObjectsPerLocation
        ];
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
        GameObject rampN = null,
        GameObject rampS = null,
        GameObject rampE = null,
        GameObject rampW = null,
        GameObject tile = null
    )
    {
        Player = player;
        Wall = wall;
        RampN = rampN;
        RampS = rampS;
        RampE = rampE;
        RampW = rampW;
        Tile = tile;
    }

    /// <summary>
    /// <c>SetMaxObjectsPerLocation</c> loops through `gridString` and sets 
    /// `GridGenerator.MaxObjectsPerLocation` to be the largest number of 
    /// game objects placed in a single location in a scene.
    /// </summary>
    private void SetMaxObjectsPerLocation(string[,] gridString)
    {
        int maxObjects = 0;
        int proposedMax;
        for (int i = 0; i < gridString.GetLength(0); i++)
        {
            for (int j = 0; j < gridString.GetLength(1); j++)
            {
                proposedMax = ParseGridString(gridString[i, j]).Length;
                maxObjects = (proposedMax > maxObjects) ? proposedMax : maxObjects;
            }
        }
        MaxObjectsPerLocation = maxObjects;
    }

    /// <summary>
    /// <c>SetGrid</c> generates `grid` from `gridString`
    /// </summary>
    private void SetGrid(GameObject[,,] grid, string[,] gridString)
    {
        string[] objectList;
        string item;
        for (int i = 0; i < gridString.GetLength(0); i++)
        {
            for (int j = 0; j < gridString.GetLength(1); j++)
            {
                objectList = ParseGridString(gridString[i, j]);
                for (int k = 0; k < MaxObjectsPerLocation; k++)
                {
                    try
                    {
                        item = objectList[k];
                        grid[i, j, k] = DetermineGridItem(item.ToUpper());
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        grid[i, j, k] = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// <c>DetermineGridItem</c> converts a string to a game object using an agreed upon dictionary.
    /// </summary>
    private GameObject DetermineGridItem(string item)
    {
        GameObject toInitialize;
        switch (item)
        {
            case " ":
                toInitialize = null;
                break;
            case "P":
                toInitialize = Player;
                break;
            case "W":
                toInitialize = Wall;
                break;
            case "T":
                toInitialize = RampN;
                break;
            case "B":
                toInitialize = RampS;
                break;
            case "R":
                toInitialize = RampE;
                break;
            case "L":
                toInitialize = RampW;
                break;
            default:
                toInitialize = null;
                Debug.Log(
                    "Tried to instantiate invalid GameObject with string ID \"" + item
                        + "\" in GridGenerator.DetermineGridItem()."
                );
                break;
        }
        return toInitialize;
    }

    /// <summary>
    /// <c>InstantiateGrid</c> creates a Unity scene from `grid`.
    /// </summary>
    private void InstantiateGrid(GameObject[,,] grid)
    {
        LightGrid = new GameObject[grid.GetLength(0), grid.GetLength(1)];
        BrightGrid = new GameObject[grid.GetLength(0), grid.GetLength(1)];
        DarkGrid = new GameObject[grid.GetLength(0), grid.GetLength(1)];
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                // Generate the Light Grid
                _objects = Instantiate(
                            Light,
                            new Vector2(j * GridSize, (GetSceneHeight() - i) * GridSize),
                            Quaternion.identity
                );
                _objects.GetComponent<SpriteRenderer>().sortingOrder = MaxObjectsPerLocation + 1;
                LightGrid[i, j] = _objects;

                // Generate the Bright Grid
                _objects = Instantiate(
                    Light,
                    new Vector2(j * GridSize, (GetSceneHeight() - i) * GridSize),
                    Quaternion.identity
                );
                _objects.GetComponent<SpriteRenderer>().sortingOrder =
                    MaxObjectsPerLocation + 1;
                _objects.GetComponent<SpriteRenderer>().color
                = new Color(1f, 1f, 1f, 0f);
                BrightGrid[i, j] = _objects;

                // Generate the Dark Grid
                _objects = Instantiate(
                    Light,
                    new Vector2(j * GridSize, (GetSceneHeight() - i) * GridSize),
                    Quaternion.identity
                );
                _objects.GetComponent<SpriteRenderer>().sortingOrder =
                    MaxObjectsPerLocation + 1;
                _objects.GetComponent<SpriteRenderer>().color
                = new Color(0f, 0f, 0f, 0f);
                DarkGrid[i, j] = _objects;
            }
        }

        // Generate the World Grid
        GameObject player = null;
        int playerLayer = -1;
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                for (int k = 0; k < MaxObjectsPerLocation; k++)
                {
                    if (grid[i, j, k] is not null)
                    {
                        _objects = Instantiate(
                            grid[i, j, k],
                            new Vector2(j * GridSize, (GetSceneHeight() - i) * GridSize),
                            Quaternion.identity
                        );
                        _objects.GetComponent<SpriteRenderer>().sortingOrder = k;

                        if (grid[i, j, k] == RampN ||
                            grid[i, j, k] == RampS ||
                            grid[i, j, k] == RampE ||
                            grid[i, j, k] == RampW)
                        {
                            _objects.GetComponent<SpriteRenderer>().sortingOrder = k - 1;
                            if (k > 0)
                            {
                                if (grid[i, j, k - 1] == RampN ||
                                    grid[i, j, k - 1] == RampS ||
                                    grid[i, j, k - 1] == RampE ||
                                    grid[i, j, k - 1] == RampW)
                                {
                                    _objects.GetComponent<SpriteRenderer>().sortingOrder = k - 2;
                                    _objects.tag = _objects.tag + "Down";
                                }
                            }
                        }
                        if (grid[i, j, k] == Player)
                        {
                            player = _objects;
                            playerLayer = k;
                            GridInstance[i, j, k] = null;
                        }
                        else
                        {
                            GridInstance[i, j, k] = _objects;
                        }
                    }
                }
            }
        }
        if (player != null)
        {
            player.GetComponent<Player>().SetGridInformation(
                gridString: GridString,
                gridSize: GridSize,
                lightGrid: LightGrid,
                gridLayers: GridInstance,
                currentLayer: playerLayer,
                maxLayer: MaxObjectsPerLocation,
                brightGrid: BrightGrid,
                darkGrid: DarkGrid
            );
        }
    }

    /// <summary>
    /// <c>ParseGridString</c> parses string `text` into an array of strings
    /// where the elements of the array are strings from `text` separated by `delimiter`.
    /// </summary>
    private string[] ParseGridString(string text, char delimiter = ':')
    {
        string[] tempStringArray = new string[text.Length];
        string substring = "";
        int count = 0;
        foreach (char c in text)
        {
            if (c == delimiter)
            {
                if (substring != "")
                {
                    tempStringArray[count] = substring;
                    substring = "";
                    count += 1;
                }
            }
            else
            {
                substring += c;
            }
        }
        if (substring != "")
        {
            tempStringArray[count] = substring;
        }
        else
        {
            count -= 1;
        }

        if (count < 0)
        {
            return null;
        }
        string[] returnString = tempStringArray[0..(count + 1)];
        return returnString;
    }

    public int GetSceneHeight() { return Grid.GetLength(0); }

    public int GetSceneWidth() { return Grid.GetLength(1); }
}
