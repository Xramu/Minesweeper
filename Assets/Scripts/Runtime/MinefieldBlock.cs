
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class MinefieldBlock : IDisposable
{
    #region Static variables

    public static float LABEL_FONT_SIZE_MULTIPLIER = 0.8f;

    public static string USS_CLASS_UNSEARCHED = "defaultField";

    public static string USS_CLASS_SEARCHED = "searchedField";

    public static string USS_CLASS_ARMED = "minedField";

    public static string USS_CLASS_FIELD_LABEL = "fieldLabel";
    public static string USS_CLASS_MINE_AMOUNT_PREFIX = "mineAmount";

    public static string USS_CLASS_FLAG = "flaggedField";

    #endregion

    #region Fields & Properties

    private bool hasMine;
    private bool searched;
    private bool isFlagged;

    private bool debugShowingMine = false;

    private Label fieldLabel;

    public event EventHandler<MinefieldBlockEventArgs> onRevealed;

    public List<NeighbourMinefieldBlock> NeighbouringMinefieldBlocks { get; set; } = new List<NeighbourMinefieldBlock>();

    /// <summary>
    /// Reference to the visual element that is representing this field block
    /// </summary>
    public VisualElement FieldVisualElement { get; set; } = null;

    /// <summary>
    /// Index this field has been assigned to
    /// </summary>
    public int FieldIndex { get; set; } = 0;

    /// <summary>
    /// Whether or not this field has a mine.
    /// </summary>
    public bool HasMine
    {
        get
        {
            return hasMine;
        }
        set
        {
            if (hasMine == value) return;

            hasMine = value;
        }
    }

    /// <summary>
    /// Whether or not this block has been already searched
    /// </summary>
    public bool Searched
    {
        get
        {
            return searched;
        }
        set
        {
            if (searched == value) return;

            searched = value;

            // Reset text and reassign class
            FieldVisualElement.ClearClassList();
            fieldLabel.text = string.Empty;
            FieldVisualElement.AddToClassList(searched ? USS_CLASS_SEARCHED : USS_CLASS_UNSEARCHED);

            // Don't calculate mines if this was hidden
            if (!searched) return;

            if (hasMine)
            {
                // Hit a mine!

                fieldLabel.text = "X";

                return;
            }

            // Check for showing a number or revealing neighbours
            int minesAround = GetMinesAroundAmount();

            if (minesAround > 0)
            {
                fieldLabel.AddToClassList(USS_CLASS_MINE_AMOUNT_PREFIX + minesAround);
                fieldLabel.text = minesAround.ToString();
            }
            else
            {
                // Reveal neighbours when this is a clear node
                RevealNeighbouringFields();
            }
        }
    }

    /// <summary>
    /// Whether or not this block has a flag placed on it by the player.
    /// </summary>
    public bool IsFlagged
    {
        get
        {
            return isFlagged;
        }
        set
        {
            if (isFlagged == value) return;

            isFlagged = value;

            // Toggle label's background image flag to show/hide
            fieldLabel.style.unityBackgroundImageTintColor = new StyleColor(new Color(1, 1, 1, isFlagged ? 1 : 0));
        }
    }

    #endregion

    /// <param name="fieldIndex">Index to assign to this field.</param>
    /// <param name="fieldSize">Pixel size of the visual field element.</param>
    /// <param name="rootVisualElement">Visual element that the field will be the child of.</param>
    public MinefieldBlock(int fieldIndex, float fieldSize, VisualElement rootVisualElement)
    {
        FieldIndex = fieldIndex;

        // Create field Visual element
        FieldVisualElement = new VisualElement();
        FieldVisualElement.AddToClassList(USS_CLASS_UNSEARCHED);

        StyleLength fieldStyleLength = new StyleLength(new Length(fieldSize, LengthUnit.Pixel));

        FieldVisualElement.style.width = fieldStyleLength;
        FieldVisualElement.style.height = fieldStyleLength;

        // Create label for mine count and flag
        StyleLength labelFontSizeStyleLength = new StyleLength(new Length(fieldSize * LABEL_FONT_SIZE_MULTIPLIER, LengthUnit.Pixel));

        fieldLabel = new Label(string.Empty);
        fieldLabel.style.fontSize = labelFontSizeStyleLength;
        FieldVisualElement.Add(fieldLabel);
        fieldLabel.AddToClassList(USS_CLASS_FIELD_LABEL);

        // Assign callback events to call back to this object
        FieldVisualElement.RegisterCallback<MouseDownEvent>(OnFieldMouseDown);

        // Add to the given root element
        rootVisualElement.Add(FieldVisualElement);
    }

    #region Neighbour Interaction

    /// <returns>The amount of mines in the neighbouring fields.</returns>
    public int GetMinesAroundAmount()
    {
        int minesAround = 0;

        // Sanity check on the neighbours
        if (NeighbouringMinefieldBlocks.Count <= 0)
        {
            Debug.LogError("ERROR: Tried to calculate mines from a minefield block with no neighbours assigned!");
            return minesAround;
        }

        // Add one to the counter from each neighbour block that has mine in it
        for (int i = 0; i < NeighbouringMinefieldBlocks.Count; i++)
        {
            if (NeighbouringMinefieldBlocks[i].minefieldBlock.HasMine)
                minesAround++;
        }

        return minesAround;
    }

    /// <returns>The amount of flags placed in this field's neighbouring fields.</returns>
    public int GetFlagsAroundAmount()
    {
        int flagsAround = 0;

        // Sanity check on the neighbours
        if (NeighbouringMinefieldBlocks.Count <= 0)
        {
            Debug.LogError("ERROR: Tried to calculate mines from a minefield block with no neighbours assigned!");
            return flagsAround;
        }

        // Add one to the counter from each neighbour block that has a flag in it
        for (int i = 0; i < NeighbouringMinefieldBlocks.Count; i++)
        {
            if (NeighbouringMinefieldBlocks[i].minefieldBlock.IsFlagged)
                flagsAround++;
        }

        return flagsAround;
    }

    /// <summary>
    /// Calls the RevealField method on all the neighbouring fields.
    /// </summary>
    private void RevealNeighbouringFields()
    {
        // Sanity check on the neighbours
        if (NeighbouringMinefieldBlocks.Count <= 0)
        {
            Debug.LogError("ERROR: Tried to reveal neighbours on a field with no neighbours!");
        }

        // Call each neighbour's reveal field method
        for (int i = 0; i < NeighbouringMinefieldBlocks.Count; i++)
        {
            if (!NeighbouringMinefieldBlocks[i].minefieldBlock.Searched)
            {
                NeighbouringMinefieldBlocks[i].minefieldBlock.RevealField();
            }
        }
    }

    /// <summary>
    /// Calls RevealField on all the neighbouring fields if the surrounding flag amount matches with the surrounding mine amount.
    /// </summary>
    /// <returns>True if amounts matched and reveal neighbours was called.</returns>
    private bool TryDefuseAround()
    {
        // Only defuse if player has matching amount of flags and mines around this block
        if (GetMinesAroundAmount() == GetFlagsAroundAmount())
        {
            RevealNeighbouringFields();
            return true;
        }

        return false;
    }

    #endregion

    /// <summary>
    /// Searches this field if it's not flagged. If already searched, will try to defuse around it.
    /// </summary>
    public void RevealField()
    {
        // Never reveal if this is flagged
        if (IsFlagged)
        {
            return;
        }

        if (Searched)
        {
            TryDefuseAround();
        }
        else
        {
            Searched = true;

            // Send event to anything subscribed
            onRevealed?.Invoke(this, new MinefieldBlockEventArgs() { minefieldBlock = this });
        }
    }

    /// <summary>
    /// Toggles IsFlagged state if not searched yet.
    /// </summary>
    public void ToggleFlag()
    {
        // Cannot flag a searched field
        if (Searched)
        {
            return;
        }

        IsFlagged = !IsFlagged;
    }

    private void OnFieldMouseDown(MouseDownEvent mouseDownEvent)
    {
        if (mouseDownEvent.button == 1)
        {
            ToggleFlag();
        }
        else
        {
            RevealField();
        }
    }

    /// <summary>
    /// Toggles between the debug class for showing if this field has a mine or not
    /// </summary>
    public void DebugToggleShowMine()
    {
        if (!hasMine)
            return;

        debugShowingMine = !debugShowingMine;

        if (debugShowingMine)
        {
            FieldVisualElement.AddToClassList(USS_CLASS_ARMED);
        }
        else
        {
            FieldVisualElement.RemoveFromClassList(USS_CLASS_ARMED);
        }
    }

    /// <summary>
    /// Handles disposing this minefield block's resources
    /// </summary>
    public void Dispose()
    {
        // Dispose
        if (FieldVisualElement != null)
        {
            FieldVisualElement.RemoveFromHierarchy();
            FieldVisualElement = null;
        }

        // Dispose neighbour data
        if (!NeighbouringMinefieldBlocks.IsEmptyOrNull())
        {
            for (int i = 0; i < NeighbouringMinefieldBlocks.Count; i++)
            {
                NeighbouringMinefieldBlocks[i].Dispose();
            }
        }

        GC.SuppressFinalize(this);
    }
}
