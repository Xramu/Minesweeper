
using System;
using System.Collections.Generic;
using UnityEngine;

public static class RamuExtensions
{
    #region List<T>
    /// <summary>
    /// Checks if the given index is valid and in range of this list.
    /// </summary>
    /// <typeparam name="T">List type.</typeparam>
    /// <param name="list">List to use as a reference.</param>
    /// <param name="index">Index to check.</param>
    /// <returns>True if is in range</returns>
    public static bool HasValidIndex<T>(this List<T> list, int index)
    {
        if (index < 0 || list == null) return false;

        return index < list.Count;
    }

    /// <summary>
    /// Checks if the list is empty or has 0 elements inside it.
    /// </summary>
    /// <typeparam name="T">List type.</typeparam>
    /// <param name="list">List to check.</param>
    /// <returns>True if list is either null or has no elements inside it</returns>
    public static bool IsEmptyOrNull<T>(this List<T> list)
    {
        return list == null || list.Count <= 0;
    }

    #endregion

    #region Array<T>

    /// <summary>
    /// Checks if this array is null or has no elements inside it.
    /// </summary>
    /// <param name="array">Array to check.</param>
    /// <returns>True if this array is null or has 0 elements inside it.</returns>
    public static bool IsEmptyOrNull(this Array array)
    {
        return array == null || array.Length <= 0;
    }

    #endregion

    #region Vector2Int

    /// <summary>
    /// Returns a new Vector2Int that is the absolute vector of this.
    /// </summary>
    /// <param name="vector2Int">Vector to use for absolute calculation.</param>
    /// <returns>New Vector2Int with absolute component values.</returns>
    public static Vector2Int AbsoluteVector (this  Vector2Int vector2Int)
    {
        return new Vector2Int(Mathf.Abs(vector2Int.x), Mathf.Abs(vector2Int.y));
    }

    #endregion
}