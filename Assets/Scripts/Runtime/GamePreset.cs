using System;
using UnityEngine;

[Serializable]
public struct GamePreset
{
    [SerializeField]
    private string name;

    [SerializeField]
    private  Vector2Int gameSize;

    [SerializeField]
    private int mineAmount;

    /// <summary>
    /// Name of the preset when it's shown.
    /// </summary>
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    /// <summary>
    /// Size of the mine field grid.
    /// </summary>
    public Vector2Int GameSize
    {
        get
        {
            return gameSize;
        }
        set
        {
            // Set game size to the gotten absolute size
            gameSize = value.AbsoluteVector();
        }
    }

    /// <summary>
    /// Amount of mines that will be planted on the field in this mode.
    /// </summary>
    public int MineAmount
    {
        get
        {
            return mineAmount;
        }
        set
        {
            mineAmount = value;
        }
    }

    /// <returns>Absolute amount of fields in this preset's mine field size.</returns>
    public int GetFieldCount()
    {
        return Mathf.Abs(gameSize.x * gameSize.y);
    }
}
