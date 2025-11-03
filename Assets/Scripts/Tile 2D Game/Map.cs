using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileTypes
{
    Empty = -1,
    // 0, 14
    Grass = 15,//평지
    Tree = 16,//나무
    Hills = 17,//언덕
    Mountains = 18,//산
    Towns = 19,//마을
    Castle = 20,//성
    Monster = 21//몬스터 지역
}

public class Map
{
    public int rows = 0;
    public int cols = 0;

    public Tile[] tiles;

    public Tile castleTile;
    public Tile startTile;

    public Tile[] CoastTiles
    {

        get
        {
            return tiles.Where(t => t.autoTileId < (int)TileTypes.Grass).ToArray();
        }
    }

    public Tile[] LandTiles
    {
        get
        {
            return tiles.Where(t => t.autoTileId >= (int)TileTypes.Grass).ToArray();
        }
    }

    public void Init(int rows, int cols)   // 0: O 1: X
    {
        this.rows = rows;//행 
        this.cols = cols;

        tiles = new Tile[rows * cols];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }

        // 8방향 연결용 오프셋 (y, x)
        int[,] dirs = new int[,]
        {
        { -1, -1 }, // ↖ TopLeft
        { -1,  0 }, // ↑  Top
        { -1,  1 }, // ↗ TopRight
        {  0, -1 }, // ← Left
        {  0,  1 }, // → Right
        {  1, -1 }, // ↙ BottomLeft
        {  1,  0 }, // ↓ Bottom
        {  1,  1 }  // ↘ BottomRight
        };

        // 각 타일의 인접 타일 설정
        for (int r = 0; r < rows; ++r) //행을 검사
        {
            for (int c = 0; c < cols; ++c)//열을 검사
            {
                int index = r * cols + c;
                var tile = tiles[index];//인덱스에 접근

                tile.adjacents = new Tile[8]; // ⬅️ 반드시 8로 지정

                for (int d = 0; d < 8; ++d)
                {
                    int ny = r + dirs[d, 0];
                    int nx = c + dirs[d, 1];

                    if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                        continue;

                    tile.adjacents[d] = tiles[ny * cols + nx];
                }
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAuotoTileId();
            tiles[i].UpdateAuotoFowId();
        }
    }

    public bool CreateIsland(
        float erodePercent,
        int erodeIterations,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float mountainPercent,
        float townPercent,
        float monsterPercent)
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; ++i)
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hills);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountains);
        DecorateTiles(LandTiles, townPercent, TileTypes.Towns);
        DecorateTiles(LandTiles, monsterPercent, TileTypes.Monster);

        var towns = tiles.Where(x => x.autoTileId == (int)TileTypes.Towns).ToArray();
        ShuffleTiles(towns);
        startTile = towns[0];

        var catsleTargets = tiles.Where(x => x.autoTileId <= (int)TileTypes.Grass &&
            x.autoTileId != (int)TileTypes.Empty).ToArray();
        castleTile = catsleTargets[Random.Range(0, catsleTargets.Length)];

        return true;
    }

    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        int total = Mathf.FloorToInt(tiles.Length * percent);

        ShuffleTiles(tiles);

        for (int i = 0; i < total; ++i)
        {
            if (tileType == TileTypes.Empty)
                tiles[i].ClearAdjacents();

            tiles[i].autoTileId = (int)tileType;
        }
    }

    public void ShuffleTiles(Tile[] tiles)
    {
        // Fisher-Yates 셔플 알고리즘 구현
        for (int i = tiles.Length - 1; i > 0; i--)
        {
            // 0과 i 사이의 무작위 인덱스 선택
            int randomIndex = Random.Range(0, i + 1);

            // i번째 요소와 무작위로 선택된 요소 교환
            Tile temp = tiles[i];
            tiles[i] = tiles[randomIndex];
            tiles[randomIndex] = temp;
        }
    }
}
