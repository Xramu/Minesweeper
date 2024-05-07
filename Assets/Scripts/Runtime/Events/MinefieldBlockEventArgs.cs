
using System;

/// <summary>
/// Args for any kind of Minefield Block event sent out by a single block.
/// </summary>
public class MinefieldBlockEventArgs : EventArgs
{
    public MinefieldBlock minefieldBlock {  get; set; }
}
