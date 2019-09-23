using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameCells : MonoBehaviour
{
    public static GameCells instance;

    public enum Directions
    {
        LEFT = 0,
        RIGHT,
        UP,
        DOWN
    }

    const int COLS = 14;
    const int ROWS = 10;

    int[] _gridSize = new int[2];

    Cell[,] _spawnGrid;

    Cell _selectedCell = null;

    [SerializeField]
    GameObject cellGameObject;


    void Awake()
    {
        instance = this;

        _gridSize[0] = COLS + 2;
        _gridSize[1] = ROWS + 2;

        _spawnGrid = new Cell[_gridSize[0], _gridSize[1]];
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateGameBoard();
    }

    public void GenerateGameBoard()
    {
        List<Vector2Int> availableCells = new List<Vector2Int>();

        //Generate Board
        for (int col = 0; col < _gridSize[0]; col++)
        {
            for (int row = 0; row < _gridSize[1]; row++)
            {
                Cell cell;
                if (_spawnGrid[col, row] == null)
                {
                    GameObject tile = Instantiate(cellGameObject, new Vector3(transform.position.x + col * cellGameObject.GetComponent<RectTransform>().sizeDelta.x,
                                                                   transform.position.y - row * cellGameObject.GetComponent<RectTransform>().sizeDelta.y,
                                                                   transform.position.z),
                                                                   Quaternion.identity, transform);
                    cell = tile.GetComponent<Cell>();
                }
                else
                {
                    cell = _spawnGrid[col, row];
                }

                if (col == 0 || col >= _gridSize[0] - 1 || row == 0 || row >= _gridSize[1] - 1)
                    cell.ClearType();
                else
                {
                    //cell.RandomizeType();
                    availableCells.Add(new Vector2Int(col, row));
                }

                cell.SetGridPosition(col, row);

                _spawnGrid[col, row] = cell;
            }
        }

        //Assign Cell Types
        while(availableCells.Count > 0)
        {
            int randomSrcCellIndex = Random.Range(0, availableCells.Count);

            Vector2Int emptySrcCell = availableCells[randomSrcCellIndex];
            _spawnGrid[emptySrcCell.x, emptySrcCell.y].RandomizeType();
            availableCells.Remove(emptySrcCell);

            int randomDestCellIndex = Random.Range(0, availableCells.Count);

            Vector2Int emptyDestCell = availableCells[randomDestCellIndex];
            _spawnGrid[emptyDestCell.x, emptyDestCell.y].SetCellType(_spawnGrid[emptySrcCell.x, emptySrcCell.y].GetCellType());
            availableCells.Remove(emptyDestCell);
        }

        //De-Select if a previous tile is selected
        if (_selectedCell != null)
            _selectedCell.DeSelect();
    }

    public void SelectCell(Cell cell)
    {

        if (_selectedCell == null)
            _selectedCell = cell;

        List<Cell> validPath = GetValidPath(_selectedCell, cell);
        if (validPath != null)
        {
            _selectedCell.ClearType();
            cell.ClearType();
            _selectedCell = null;

            HighlightPath(validPath);

            if (IsGameWon())
                GameManager.instance.WinGame();
            else
                GameManager.instance.AddTime();
        }
        else
        {
            _selectedCell.DeSelect();
            _selectedCell = cell;
            _selectedCell.Highlight(0f);
        }
    }

    void HighlightPath(List<Cell> path)
    {
        foreach(Cell cell in path)
        {
            cell.Highlight(0.5f);
        }
    }

    bool IsGameWon()
    {
        for (int col = 1; col < _gridSize[0] - 1; col++)
        {
            for (int row = 1; row < _gridSize[1] - 1; row++)
            {
                if (!_spawnGrid[col, row].IsClear())
                {
                    return false;
                }
            }
        }
        return true;
    }

    //Gets a valid shortest path from the source to the destination cell.
    //Returns null if no such path exists.
    List<Cell> GetValidPath(Cell srcCell, Cell destCell)
    {
        if (srcCell == destCell)
            return null;

        if (srcCell.GetCellType() != destCell.GetCellType())
            return null;

        return GetValidPathFromAdjacentCells(srcCell, destCell);
    }

    //Get a valid shortest path from source to the destination cell by checking each adjacent cell with a limited number of turns in the path.
    //Has no direction constrants. Needing to turn wouldn't cost a turnsLeft.
    //Returns null if no such path exists.
    List<Cell> GetValidPathFromAdjacentCells(Cell srcCell, Cell destCell, int turnsLeft = 2)
    {
        int[] cellLocation = srcCell.GetGridPosition();

        Dictionary<Directions, float> cellWeights = new Dictionary<Directions, float>();

        bool checkLeft = cellLocation[0] == 0 ? false : true;
        bool checkRight = cellLocation[0] == _gridSize[0] ? false : true;
        bool checkUp = cellLocation[1] == 0 ? false : true;
        bool checkDown = cellLocation[1] == _gridSize[1] ? false : true;

        cellWeights.Add(Directions.LEFT, Mathf.Abs(cellLocation[0] - 1 - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.RIGHT, Mathf.Abs(cellLocation[0] + 1 - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.UP, Mathf.Abs(cellLocation[0] - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - 1 - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.DOWN, Mathf.Abs(cellLocation[0] - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] + 1 - destCell.GetGridPosition()[1]));

        List<Cell> winPathCells = null;
        foreach (KeyValuePair<Directions, float> cellToCheck in cellWeights.OrderBy(key => key.Value))
        {
            if (checkLeft && cellToCheck.Key == Directions.LEFT)
            {
                if (cellLocation[0] - 1 >= 0)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0] - 1, cellLocation[1]], destCell, Directions.LEFT, turnsLeft);
            }
            else if (checkRight && cellToCheck.Key == Directions.RIGHT)
            {
                if (cellLocation[0] + 1 <= _gridSize[0] - 1)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0] + 1, cellLocation[1]], destCell, Directions.RIGHT, turnsLeft);
            }
            else if (checkUp && cellToCheck.Key == Directions.UP)
            {
                if (cellLocation[1] - 1 >= 0)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0], cellLocation[1] - 1], destCell, Directions.UP, turnsLeft);
            }
            else if (checkDown && cellToCheck.Key == Directions.DOWN)
            {
                if (cellLocation[1] + 1 <= _gridSize[1] - 1)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0], cellLocation[1] + 1], destCell, Directions.DOWN, turnsLeft);
            }

            if (winPathCells != null)
            {
                winPathCells.Add(srcCell);
                break;
            }
        }

        return winPathCells;
    }

    //Get a valid shortest path from source to the destination cell by checking each adjacent cell with a limited number of turns in the path.
    //Has direction constrants. Needing to turn cost a turnsLeft.
    //Returns null if no such path exists.
    List<Cell> GetValidPathFromAdjacentCells(Cell srcCell, Cell destCell, Directions movement, int turnsLeft)
    {

        int[] cellLocation = srcCell.GetGridPosition();

        bool checkLeft = movement == Directions.RIGHT || cellLocation[0] == 0 ? false : true;
        bool checkRight = movement == Directions.LEFT || cellLocation[0] == _gridSize[0] ? false : true;
        bool checkUp = movement == Directions.DOWN || cellLocation[1] == 0 ? false : true;
        bool checkDown = movement == Directions.UP || cellLocation[1] == _gridSize[1] ? false : true;

        //Important for the order we want to check the adjacent cells in, kind of like in A* algorithm where there are weighted cells.
        Dictionary<Directions, float> cellWeights = new Dictionary<Directions, float>();

        cellWeights.Add(Directions.LEFT, Mathf.Abs(cellLocation[0] - 1 - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.RIGHT, Mathf.Abs(cellLocation[0] + 1 - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.UP, Mathf.Abs(cellLocation[0] - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] - 1 - destCell.GetGridPosition()[1]));
        cellWeights.Add(Directions.DOWN, Mathf.Abs(cellLocation[0] - destCell.GetGridPosition()[0]) + Mathf.Abs(cellLocation[1] + 1 - destCell.GetGridPosition()[1]));

        List<Cell> winPathCells = null;
        foreach (KeyValuePair<Directions, float> cellToCheck in cellWeights.OrderBy(key => key.Value))
        {
            if (checkLeft && cellToCheck.Key == Directions.LEFT)
            {
                if (cellLocation[0] - 1 >= 0)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0] - 1, cellLocation[1]], destCell, Directions.LEFT, movement != Directions.LEFT ? turnsLeft - 1 : turnsLeft);
            }
            else if (checkRight && cellToCheck.Key == Directions.RIGHT)
            {
                if (cellLocation[0] + 1 <= _gridSize[0] - 1)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0] + 1, cellLocation[1]], destCell, Directions.RIGHT, movement != Directions.RIGHT ? turnsLeft - 1 : turnsLeft);
            }
            else if (checkUp && cellToCheck.Key == Directions.UP)
            {
                if (cellLocation[1] - 1 >= 0)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0], cellLocation[1] - 1], destCell, Directions.UP, movement != Directions.UP ? turnsLeft - 1 : turnsLeft);
            }
            else if (checkDown && cellToCheck.Key == Directions.DOWN)
            {
                if (cellLocation[1] + 1 <= _gridSize[1] - 1)
                    winPathCells = GetWinningPath(_spawnGrid[cellLocation[0], cellLocation[1] + 1], destCell, Directions.DOWN, movement != Directions.DOWN ? turnsLeft - 1 : turnsLeft);
            }

            if (winPathCells != null)
            {
                winPathCells.Add(srcCell);
                break;
            }
        }
        
        return winPathCells;
    }

    //Get a valid shortest path from source to the destination cell. 
    //Has direction constrants. Needing to turn cost a turnsLeft.
    //Returns null if no such path exists.
    List<Cell> GetWinningPath(Cell srcCell, Cell destCell, Directions movement, int turnsLeft)
    {
        List<Cell> winPathCells = null;
        if (turnsLeft >= 0)
        {
            if (srcCell == destCell)
            {
                winPathCells = new List<Cell>();
                winPathCells.Add(srcCell);
            }
            else if (srcCell.IsClear())
            {
                winPathCells = GetValidPathFromAdjacentCells(srcCell, destCell, movement, turnsLeft);
            }
        }

        return winPathCells;
    }
}
