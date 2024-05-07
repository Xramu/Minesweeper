
using System;

public enum NeighbourDirection
{
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    TopLeft
}

public struct NeighbourMinefieldBlock : IDisposable
{
    public NeighbourDirection neighbourDirection;
    public MinefieldBlock minefieldBlock;

    public void Dispose()
    {
        minefieldBlock = null;

        GC.SuppressFinalize(this);
    }
}
