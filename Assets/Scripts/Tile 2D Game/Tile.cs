using System;

public enum Sides
{
    TopLeft,     // 0
    Top,         // 1
    TopRight,    // 2
    Left,        // 3
    Right,       // 4
    BottomLeft,  // 5
    Bottom,      // 6
    BottomRight  // 7
}

public class Tile
{
    public static readonly int[] tableWeight =
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        2, 4, int.MaxValue, 1, 1, 1
    };

    public int id;
    public Tile[] adjacents = new Tile[8];

    public int autoTileId;
    public int autoFowId;

    public Tile previous = null;

    public bool isVisited = false;

    public bool CanMove
    {
        get
        {
            return (autoTileId != (int)TileTypes.Empty && Weight < int.MaxValue);
        }
    }

    public int Weight
    {
        get
        {
            if (autoTileId < 0 || autoTileId >= tableWeight.Length)
            {
                return int.MaxValue;
            }
            return tableWeight[autoTileId];
        }
    }

    public void Clear()
    {
        previous = null;
    }

    public void UpdateAuotoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] != null)
            {
                autoTileId |= (1 << (adjacents.Length - 1 - i));
            }
        }
    }

    public void UpdateAuotoFowId()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] == null)
            {
                sb.Append("1");
            }
            else
            {
                sb.Append(adjacents[i].isVisited ? "0" : "1");
            }
        }
        autoFowId = System.Convert.ToInt32(sb.ToString(), 2);
    }

    public void RemoveAdjacents(Tile tile)
    {
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] == null)
                continue;

            if (adjacents[i].id == tile.id)
            {
                adjacents[i] = null;
                break;
            }
        }
        UpdateAuotoTileId();
    }

    public void ClearAdjacents()
    {
        for (int i = 0; i < adjacents.Length; ++i)
        {
            if (adjacents[i] == null)
                continue;
            adjacents[i].RemoveAdjacents(this);
            adjacents[i] = null;
        }
        UpdateAuotoTileId();
    }
}
