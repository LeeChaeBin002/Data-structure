
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject tilePrefab;
    private GameObject[] tileObjs;
    private GameObject player;
    private List<Tile> lastPath = new();
    public int mapWidth = 20;
    public int mapHeight = 20;

    [Range(0f, 0.9f)]
    public float erodePercent = 0.5f;
    public int erodeIteration = 2;
    [Range(0f, 0.9f)]
    public float lakePercent = 0.1f;

    [Range(0f, 0.9f)]
    public float treePercent = 0.1f;
    [Range(0f, 0.9f)]
    public float hillPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float moutainPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float townPercent = 0.1f;
    [Range(0f, 0.9f)]
    public float monsterPercent = 0.1f;

    public Vector2 tileSize = new Vector2(16, 16);

    //public Texture2D islandTexture;
    public Sprite[] islandSprites;
    public Sprite[] fowSprites;
    private TileSearch tileSearch;   // 🔹 A*용 탐색기
    private Tile currentTile;        // 🔹 플레이어가 현재 서 있는 타일
    private Map map;

    public Map Map
    {
        get { return map; }
    }

    private Vector3 firstTilePos;

    public int ScreenPosToTileId(Vector3 screenPos)
    {
        screenPos.z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return WorldPosToTileId(worldPos);
    }

    public int WorldPosToTileId(Vector3 worldPos)
    {
        var pivot = firstTilePos;
        pivot.x -= tileSize.x * 0.5f;
        pivot.y += tileSize.y * 0.5f;

        var diff = worldPos - pivot;
        int x = Mathf.FloorToInt(diff.x / tileSize.x);
        int y = -Mathf.CeilToInt(diff.y / tileSize.y);

        x = Mathf.Clamp(x, 0, mapWidth - 1);
        y = Mathf.Clamp(y, 0, mapHeight - 1);

        return y * mapWidth + x;
    }

    public Vector3 GetTilePos(int y, int x)
    {
        var pos = firstTilePos;
        pos.x += tileSize.x * x;
        pos.y -= tileSize.y * y;
        return pos;
    }

    public Vector3 GetTilePos(int tileId)
    {
        return GetTilePos(tileId / mapWidth, tileId % mapWidth);
    }

    private void ResetStage()
    {
        bool succeed = false;
        while (!succeed)
        {
            map = new Map();
            map.Init(mapHeight, mapWidth);
            succeed = map.CreateIsland(erodePercent, erodeIteration, lakePercent,
                treePercent, hillPercent, moutainPercent, townPercent, monsterPercent);
        }
        CreateGrid();
        CreatePlayer();

        // 🔹 탐색기 초기화
        if (tileSearch == null)
            tileSearch = new TileSearch();
        tileSearch.Init(map);

        // 🔹 플레이어의 시작 타일 저장
        currentTile = map.startTile;

        // 마을→성 변환
        ChangeRandomTownToCastle();
    }
    private void CreatePlayer()
    {
        if (player != null)
        {
            Destroy(player);
        }
        player = Instantiate(playerPrefab, GetTilePos(map.startTile.id), Quaternion.identity);
    }

    private void CreateGrid()
    {
        if (tileObjs != null)
        {
            foreach (var tile in tileObjs)
            {
                Destroy(tile.gameObject);
            }
        }
        tileObjs = new GameObject[mapHeight * mapWidth];

        firstTilePos = Vector3.zero;
        firstTilePos.x -= mapWidth * tileSize.x * 0.5f;
        firstTilePos.y += mapHeight * tileSize.y * 0.5f;
        var pos = firstTilePos;
        for (int i = 0; i < mapHeight; ++i)
        {
            for (int j = 0; j < mapWidth; ++j)
            {
                var tileId = i * mapWidth + j;
                var tile = map.tiles[tileId];

                var newGo = Instantiate(tilePrefab, transform);
                newGo.transform.localPosition = pos;
                pos.x += tileSize.x;
                newGo.name = $"Tile ({i} , {j})";
                tileObjs[tileId] = newGo;
                DecorateTile(tileId);
            }
            pos.x = firstTilePos.x;
            pos.y -= tileSize.y;
        }
    }

    public void DecorateTile(int tileId)
    {
        var tile = map.tiles[tileId];
        var tileGo = tileObjs[tileId];
        var ren = tileGo.GetComponent<SpriteRenderer>();
        if (tile.autoTileId != (int)TileTypes.Empty)
        {
            ren.sprite = islandSprites[tile.autoTileId];
        }
        else
        {
            ren.sprite = null;
        }

        // if (tile.isVisited)
        // {
        //     if (tile.autoTileId != (int)TileTypes.Empty)
        //     {
        //         ren.sprite = islandSprites[tile.autoTileId];
        //     }
        //     else
        //     {
        //         ren.sprite = null;
        //     }
        // }
        // else
        // {
        //     ren.sprite = fowSprites[tile.autoFowId];
        // }
    }

    public int visiteRadius = 1;
    private void ClearPathColors()
    {
        if (lastPath == null || lastPath.Count == 0) return;

        foreach (var tile in lastPath)
        {
            if (tile == null) continue;
            var tileGo = tileObjs[tile.id];
            if (tileGo != null)
            {
                var ren = tileGo.GetComponent<SpriteRenderer>();
                if (ren != null)
                    ren.color = Color.white; // 원래 색으로 복귀
            }
        }

        lastPath.Clear();
    }
    //색상변경
    private void ShowPath(List<Tile> path)
    {
        if (path == null || path.Count == 0) return;

        ClearPathColors();

        // 🔹 새 경로 저장
        lastPath = new List<Tile>(path);


        Color mint = new Color(0.6f, 1.0f, 0.9f, 0.7f);
        Color pink = new Color(1.0f, 0.7f, 0.9f, 0.7f);




        for (int i = 0; i < path.Count; i++)
        {
            var tile = path[i];
            if (tile == null) continue;

          
            float t = (float)i / (path.Count - 1);
            var color = Color.Lerp(mint, pink, t);

            var tileGo = tileObjs[tile.id];
            if (tileGo != null)
            {
                var ren = tileGo.GetComponent<SpriteRenderer>();
                if (ren != null)
                    ren.color = color;
            }
        }
    }


    public void OnTileVisited(Tile tile)
    {
        int centerX = tile.id % mapWidth;
        int centerY = tile.id / mapWidth;

        int radius = visiteRadius;
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                int x = centerX + j;
                int y = centerY + i;
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                    continue;

                int id = y * mapWidth + x;
                map.tiles[id].isVisited = true;
                DecorateTile(id);
            }
        }
        radius += 1;
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
 
                if (i == radius || i == -radius || j == radius || j == -radius)
                {
                    int x = centerX + j;
                    int y = centerY + i;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        continue;

                    int id = y * mapWidth + x;
                    map.tiles[id].UpdateAuotoFowId();
                    DecorateTile(id);
                }
            }
        }
    }

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log(ScreenPosToTileId(Input.mousePosition));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetStage();
            //ChangeRandomTownToCastle();
        }
        // 🔹 마우스 왼쪽 클릭 시 이동
        if (Input.GetMouseButtonDown(0))
        {
            MovePlayerToMouse();
        }
    
    }


    private void MovePlayerToMouse()
    {
        if (player == null || map == null || tileSearch == null) return;

        // 1️⃣ 클릭한 타일
        int tileId = ScreenPosToTileId(Input.mousePosition);
        var clicked = map.tiles[tileId];

        // 2️⃣ 클릭 타일이 이동 불가하면, 주변에서 가장 가까운 '이동 가능' 타일 찾기
        var target = GetBestReachableTarget(clicked);
        if (target == null)
        {
            Debug.Log("도달 가능한 목표 타일이 없습니다.");
            return;
        }

        // 3️⃣ A* 경로 계산 시도
        if (tileSearch.AStar(currentTile, target) && tileSearch.path.Count > 0)
        {
            var partial = TrimToWalkablePrefix(tileSearch.path);
            if (partial.Count > 0)
            {
                StopAllCoroutines();
                ShowPath(partial);
                StartCoroutine(MoveAlongPath(partial));
            }
            else
            {
                Debug.Log("이동 가능한 경로 구간이 없습니다.");
            }
        }
        else
        {
            // 4️⃣ A* 실패 시 BFS fallback
            var reachable = CollectReachableTiles(currentTile);
            var best = ChooseClosestTo(clicked, reachable);
            if (best != null && tileSearch.AStar(currentTile, best))
            {
                var partial = TrimToWalkablePrefix(tileSearch.path);
                if (partial.Count > 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveAlongPath(partial));
                }
                else
                {
                    Debug.Log("이동 가능한 경로 구간이 없습니다.(BFS fallback)");
                }
            }
            else
            {
                Debug.Log("경로를 찾을 수 없습니다.");
            }
        }
    }
    //BFS로 현재 위치에서 도달 가능한 모든 타일 수집
    private List<Tile> CollectReachableTiles(Tile start)
    {
        var list = new List<Tile>();
        var q = new Queue<Tile>();
        var visited = new HashSet<Tile>();

        if (start == null || !start.CanMove) return list;

        q.Enqueue(start);
        visited.Add(start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            list.Add(cur);

            foreach (var n in cur.adjacents)
            {
                if (n != null && n.CanMove && !visited.Contains(n))
                {
                    visited.Add(n);
                    q.Enqueue(n);
                }
            }
        }
        return list;
    }
    //클릭에 가장 가까운 타일 고르기
    private Tile ChooseClosestTo(Tile target, List<Tile> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;

        Tile best = null;
        int bestH = int.MaxValue;

        foreach (var t in candidates)
        {
            int h = Manhattan(target, t);
            if (h < bestH)
            {
                bestH = h;
                best = t;
            }
        }
        return best;
    }

    private Tile GetBestReachableTarget(Tile clicked)
    {
        // 클릭 타일이 바로 이동 가능하면 그걸 목표로
        if (clicked.CanMove) return clicked;

        // 1) 클릭 주변의 '이동 가능' 타일을 작은 반경부터 검색
        //    반경은 필요하면 늘리세요 (2~5 정도)
        for (int r = 1; r <= 5; r++)
        {
            var candidates = RingTiles(clicked, r);
            Tile best = null;
            int bestH = int.MaxValue;

            foreach (var t in candidates)
            {
                if (t != null && t.CanMove)
                {
                    int h = Manhattan(clicked, t);
                    if (h < bestH)
                    {
                        bestH = h;
                        best = t;
                    }
                }
            }
            if (best != null) return best;
        }
        return null;
    }

    // clicked 기준 반지름 r의 '링' 좌표 수집
    private IEnumerable<Tile> RingTiles(Tile center, int r)
    {
        int cols = map.cols; // Map에 cols/rows 필드가 있다고 가정
        int rows = map.rows;

        int cx = center.id % cols;
        int cy = center.id / cols;

        // 사각형 테두리만 순회
        for (int dx = -r; dx <= r; dx++)
        {
            int x1 = cx + dx, y1 = cy - r;
            int x2 = cx + dx, y2 = cy + r;
            if (InRange(x1, y1, cols, rows)) yield return map.tiles[y1 * cols + x1];
            if (r > 0 && InRange(x2, y2, cols, rows)) yield return map.tiles[y2 * cols + x2];
        }
        for (int dy = -r + 1; dy <= r - 1; dy++)
        {
            int x1 = cx - r, y1 = cy + dy;
            int x2 = cx + r, y2 = cy + dy;
            if (InRange(x1, y1, cols, rows)) yield return map.tiles[y1 * cols + x1];
            if (r > 0 && InRange(x2, y2, cols, rows)) yield return map.tiles[y2 * cols + x2];
        }
    }

    private bool InRange(int x, int y, int cols, int rows)
        => (x >= 0 && x < cols && y >= 0 && y < rows);

    private int Manhattan(Tile a, Tile b)
    {
        int cols = map.cols;
        int ax = a.id % cols, ay = a.id / cols;
        int bx = b.id % cols, by = b.id / cols;
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }
    private List<Tile> TrimToWalkablePrefix(List<Tile> fullPath)
    {
        var result = new List<Tile>(fullPath.Count);
        foreach (var t in fullPath)
        {
            if (!t.CanMove) break;
            result.Add(t);
        }
        return result;
    }

    private System.Collections.IEnumerator MoveAlongPath(List<Tile> path)
    {
        float speed = 5f;
        float totalDistance = 0f; // 🔹 이동 거리 누적 변수


        for (int i = 0; i < path.Count; i++)
        {
            var tile = path[i];

            // 🔹 길 위에서만 이동 (CanMove = true)
            if (!tile.CanMove)
                continue;

            Vector3 targetPos = GetTilePos(tile.id);
            Vector3 startPos = player.transform.position;
            float t = 0f;

            // 🔹 거리 계산 (맨해튼 or Weight 기반)
            if (i > 0)
            {
                // 이전 타일과의 맨해튼 거리
                int prevId = path[i - 1].id;
                //totalDistance += Manhattan(path[i - 1], tile);

                // 또는 가중치 기반으로 하고 싶다면:
                 totalDistance += tile.Weight;
            }

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            player.transform.position = targetPos;

            // 🔹 현재 타일 업데이트
            currentTile = tile;
        }

        // 이동 완료 후 원래 색상 복원
        foreach (var tile in path)
        {
            var tileGo = tileObjs[tile.id];
            if (tileGo != null)
            {
                var ren = tileGo.GetComponent<SpriteRenderer>();
                if (ren != null)
                    ren.color = Color.white;
            }
        }
        Debug.Log($" 총 비용: {totalDistance}");
    }
    private System.Collections.IEnumerator MovePlayerSmooth(Vector3 targetPos)
    {
        float speed = 5f; // 이동 속도
        Vector3 startPos = player.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        player.transform.position = targetPos;
    }
    private void ChangeRandomTownToCastle()
    {
        // TileTypes enum에 Town, Castle이 있다고 가정
        var towns = new List<Tile>();

        // 모든 타일 중 마을만 찾기
        foreach (var tile in map.tiles)
        {
            if (tile.autoTileId == (int)TileTypes.Towns)
            {
                // 시작칸에서 A*로 도달 가능한 마을만 후보에 추가
                if (tileSearch.AStar(map.startTile, tile))
                    towns.Add(tile);
            }
        }

        if (towns.Count == 0)
        {
            Debug.Log("마을이 없습니다!");
            return;
        }

        // 랜덤하게 마을 하나 선택
        var randomTown = towns[Random.Range(0, towns.Count)];

        // 성으로 변경
        randomTown.autoTileId = (int)TileTypes.Castle;

        // 스프라이트 갱신
        DecorateTile(randomTown.id);

        Debug.Log($"타일 {randomTown.id} 마을 → 성으로 변경됨!");
    }
}
