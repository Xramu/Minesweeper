using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement("Minesweeper Game")]
public partial class MinesweeperGame : VisualElement
{
    public static string USS_CLASS_FIELD_ROW = "fieldRow";

    #region UXML Attributes

    [UxmlAttribute]
    public float minefieldMaxHeightPixels = 500;

    #endregion

    #region Run Specific Variables

    private bool gameRunning = false;

    private int gameMineAmount = 0;

    private int gameRevealedFieldsCounter = 0;

    private GamePreset gamePreset;

    private VisualElement[] fieldRows;

    private List<MinefieldBlock> minefieldBlocks;

    #endregion

    #region Properties

    public bool GameRunning => gameRunning;

    #endregion

    #region Neighbour Detection

    /// <summary>
    /// Creates a list of NeighbourMinefieldBlocks that surround the field of the given index in the grid of blocks given.
    /// </summary>
    /// <param name="fieldIndex">Index of the block to find neighbours of</param>
    /// <param name="blocks">List of all the blocks.</param>
    /// <param name="gridSize">Size of the grid that the blocks are arranged to.</param>
    /// <returns>List of up to 8 neighbours.</returns>
    private List<NeighbourMinefieldBlock> FindNeighbouringFields(int fieldIndex, List<MinefieldBlock> blocks, Vector2Int gridSize)
    {
        List<NeighbourMinefieldBlock> neighbours = new List<NeighbourMinefieldBlock>();

        float modX = (fieldIndex + 1) % gridSize.x;
        float rowIndex = fieldIndex / gridSize.x;

        // Horizontal & Vertical
        bool hasLeft = modX != 1;
        bool hasRight = modX != 0;
        bool hasTop = rowIndex > 0;
        bool hasBottom = rowIndex + 1 < gridSize.y;

        // Diagonal
        bool hasTopRight = hasTop && hasRight;
        bool hasBottomRight = hasBottom && hasRight;
        bool hasBottomLeft = hasBottom && hasLeft;
        bool hasTopLeft = hasTop && hasLeft;

        if (hasTop) // Top
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex - gridSize.x], neighbourDirection = NeighbourDirection.Top });

        if (hasTopRight) // Top Right
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex - gridSize.x + 1], neighbourDirection = NeighbourDirection.TopRight });

        if (hasRight) // Right
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex + 1], neighbourDirection = NeighbourDirection.Right });

        if (hasBottomRight) // Bottom Right
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex + gridSize.x + 1], neighbourDirection = NeighbourDirection.BottomRight });

        if (hasBottom) // Bottom
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex + gridSize.x], neighbourDirection = NeighbourDirection.Bottom });

        if (hasBottomLeft) // Bottom Left
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex + gridSize.x - 1], neighbourDirection = NeighbourDirection.BottomLeft });

        if (hasLeft) // Left
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex - 1], neighbourDirection = NeighbourDirection.Left });

        if (hasTopLeft) // Top Left
            neighbours.Add(new NeighbourMinefieldBlock { minefieldBlock = blocks[fieldIndex - gridSize.x - 1], neighbourDirection = NeighbourDirection.TopLeft });

        return neighbours;
    }

    /// <summary>
    /// Assigns neigbours to all of the blocks in a given list and its grid size.
    /// </summary>
    private void AssignNeighbours(List<MinefieldBlock> blocks, Vector2Int gridSize)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            // Assign neigbours to each field block
            blocks[i].NeighbouringMinefieldBlocks = FindNeighbouringFields(i, blocks, gridSize);
        }
    }

    #endregion

    #region Creating New Field

    /// <summary>
    /// Creates a specified amount of row elements inside the given visual element.
    /// </summary>
    /// <returns>The created rows parented to the root element.</returns>
    private VisualElement[] CreateFieldRows(int rowAmount, VisualElement rootElement)
    {
        VisualElement[] fieldRows = new VisualElement[rowAmount];

        for (int i = 0; i < fieldRows.Length; i++)
        {
            // Create a basic visual element with the right USS class
            fieldRows[i] = new VisualElement();
            fieldRows[i].AddToClassList(USS_CLASS_FIELD_ROW);

            rootElement.Add(fieldRows[i]);
        }

        return fieldRows;
    }

    /// <summary>
    /// Creates new list of MinefieldBlocks and fieldrows from the given game preset.
    /// </summary>
    /// <param name="preset">Game preset to follow.</param>
    /// <returns>List of MinefieldBlocks that have neighbours assigned and their visual elements added to the field rows.</returns>
    private List<MinefieldBlock> CreateMinefieldBlocks(GamePreset preset)
    {
        List<MinefieldBlock> blocks = new List<MinefieldBlock>();

        // Set up rows and field pixel size
        fieldRows = CreateFieldRows(preset.GameSize.y, this);
        float fieldSize = minefieldMaxHeightPixels / preset.GameSize.y;

        // Create all field blocks
        for (int i = 0; i < preset.GetFieldCount(); i++)
        {
            // Divide into the right row and create a new minefield block
            VisualElement row = fieldRows[i / preset.GameSize.x];
            MinefieldBlock block = new MinefieldBlock(i, fieldSize, row);

            // Subscribe to on reveal event
            block.onRevealed += MinefieldBlock_OnReveal;

            blocks.Add(block);
        }

        // Assign neighbours
        AssignNeighbours(blocks, preset.GameSize);

        return blocks;
    }

    #endregion

    #region Arming Minefield

    /// <summary>
    /// Adds the given amount of mines into the given list of blocks by random.
    /// </summary>
    /// <returns>List of the blocks that were mined.</returns>
    private List<MinefieldBlock> AddMinesToCurrentMinefield(int mineAmount, List<MinefieldBlock> blocks)
    {
        List<MinefieldBlock> armedBlocks = new List<MinefieldBlock>();

        List<MinefieldBlock> possibleBlocks = new List<MinefieldBlock>(blocks);

        // Randomize mined fields
        for (int i = 0; i < mineAmount; i++)
        {
            // Get reference by random
            int randIndex = Random.Range(0, possibleBlocks.Count);
            MinefieldBlock field = possibleBlocks[randIndex];

            // Remove from safe list
            possibleBlocks.Remove(field);

            armedBlocks.Add(field);
            field.HasMine = true;
        }

        return armedBlocks;
    }

    #endregion

    #region Game Creation

    // TODO: Subscribe a function from here to all of the minefield blocks when they get clicked.
    // TODO: Make a start screen where you input the game values and game the game loop back to it after each round

    public void StartNewGame(GamePreset preset)
    {
        Debug.Log("Starting new game...");

        // End current game with removal of the field
        EndCurrentGame(true);

        // Assign got preset
        gamePreset = preset;

        // Create new field
        CreateNewMinefield();

        gameRunning = true;
    }

    public void CompleteCurrentGame()
    {
        EndCurrentGame();

        Debug.Log("Game completed!");
    }

    public void EndCurrentGame(bool removeField = false)
    {
        if (removeField)
        {
            DestroyCurrentField();
        }

        gameRunning = false;
    }

    private void DestroyCurrentField()
    {
        // Fields
        if (!minefieldBlocks.IsEmptyOrNull())
        {
            // Dispose all blocks
            for (int i = 0; i < minefieldBlocks.Count; i++)
            {
                if (minefieldBlocks[i] != null)
                    minefieldBlocks[i].Dispose();
            }

            // Clear all references from list
            minefieldBlocks.Clear();
        }

        // Rows
        if (!fieldRows.IsEmptyOrNull())
        {
            // Remove from hierarchy and set reference null
            for (int i = 0; i < fieldRows.Length; i++)
            {
                fieldRows[i].RemoveFromHierarchy();
                fieldRows[i] = null;
            }

            // Whole array to null
            fieldRows = null;
        }
    }

    private void CreateNewMinefield()
    {
        // Size sanity check
        if (gamePreset.GameSize.x < 0 || gamePreset.GameSize.y < 0)
        {
            Debug.LogError($"ERROR: Given size for the game is invalid! Given size: {gamePreset.GameSize.x}x{gamePreset.GameSize.y}");
            return;
        }

        // Mine amount sanity check
        if (gamePreset.MineAmount >= gamePreset.GetFieldCount())
        {
            Debug.LogError($"ERROR: Given mine amount exceeds the allowed amount for this game size! Mines: {gamePreset.MineAmount} Size: {gamePreset.GameSize.x}x{gamePreset.GameSize.y} = {gamePreset.GetFieldCount()}");
            gamePreset.MineAmount = gamePreset.GetFieldCount() - 1;
        }

        Debug.Log($"Generating new game with a size of {gamePreset.GameSize.x}x{gamePreset.GameSize.y}...");

        // Create all the Minefield blocks and add them to the list
        minefieldBlocks = CreateMinefieldBlocks(gamePreset);

        Debug.Log($"MinefieldBlocks count: {minefieldBlocks.Count}");

        AddMinesToCurrentMinefield(gamePreset.MineAmount, minefieldBlocks);

        // Assign final mine amount
        gameMineAmount = gamePreset.MineAmount;

        // Reset revealed fields count
        gameRevealedFieldsCounter = 0;
    }

    #endregion

    #region Callbacks

    public void MinefieldBlock_OnReveal(object sender, MinefieldBlockEventArgs eventArgs)
    {
        if (eventArgs == null || eventArgs.minefieldBlock == null)
        {
            Debug.LogError("ERROR: Passed a null MinefieldBlock to OnMinefieldBlockRevealed!");
            return;
        }

        MinefieldBlock block = eventArgs.minefieldBlock;

        if (block.HasMine)
        {
            // Hit a mine, end game!
            Debug.Log("Game Ended! Player hit a mine!");
            return;
        }

        // Add one to counter
        gameRevealedFieldsCounter++;

        if (gameRevealedFieldsCounter + gameMineAmount >= gamePreset.GetFieldCount())
        {
            // Game completed!
            CompleteCurrentGame();
        }
    }

    #endregion

    #region Debug

    public void ToggleMineDebug()
    {
        if (minefieldBlocks.IsEmptyOrNull())
        {
            return;
        }

        for (int i = 0; i < minefieldBlocks.Count; ++i)
        {
            minefieldBlocks[i].DebugToggleShowMine();
        }
    }

    #endregion
}
