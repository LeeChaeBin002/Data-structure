using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class TileSearch
    {
    public readonly List<Tile> path = new(); // 탐색 순서 또는 결과 경로
    private Map map;

    public void Init(Map map)
    {
        this.map = map;
    }

    // -------------------------------
    // DFS (반복)
    // -------------------------------
    public void DFS(Tile start)
    {
        path.Clear();
        if (start == null) return;

        var visited = new HashSet<Tile>();
        var stack = new Stack<Tile>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null || visited.Contains(current) || !current.CanMove) continue;

            visited.Add(current);
            path.Add(current);

            foreach (var next in current.adjacents)
            {
                if (next != null && !visited.Contains(next) && next.CanMove)
                    stack.Push(next);
            }
        }
    }

    // -------------------------------
    // DFS (재귀)
    // -------------------------------
    public void DFSRecursive(Tile start)
    {
        path.Clear();
        var visited = new HashSet<Tile>();
        DFSRecursiveInternal(start, visited);
    }

    private void DFSRecursiveInternal(Tile tile, HashSet<Tile> visited)
    {
        if (tile == null || visited.Contains(tile) || !tile.CanMove) return;

        visited.Add(tile);
        path.Add(tile);

        foreach (var next in tile.adjacents)
            DFSRecursiveInternal(next, visited);
    }

    // -------------------------------
    // BFS (순회 순서)
    // -------------------------------
    public void BFS(Tile start)
    {
        path.Clear();
        if (start == null) return;

        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            path.Add(current);

            foreach (var next in current.adjacents)
            {
                if (next == null || !next.CanMove || visited.Contains(next)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }
    }

    // -------------------------------
    // BFS (길찾기: 최단경로 복원)
    // -------------------------------
    public bool PathFindingBFS(Tile start, Tile goal)
    {
        path.Clear();
        if (start == null || goal == null) return false;

        // 각 타일에 이전 노드 저장용 필드가 필요
        foreach (var t in map.tiles)
            t.previous = null;

        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();

        queue.Enqueue(start);
        visited.Add(start);

        bool found = false;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == goal)
            {
                found = true;
                break;
            }

            foreach (var next in current.adjacents)
            {
                if (next == null || !next.CanMove || visited.Contains(next)) continue;

                visited.Add(next);
                next.previous = current;
                queue.Enqueue(next);
            }
        }

        if (!found) return false;

        // 경로 복원
        var step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return true;
    }

    // -------------------------------
    // Dijkstra (가중치 기반 최소비용 경로)
    // -------------------------------
    public bool Dijkstra(Tile start, Tile goal)
    {
        path.Clear();
        if (start == null || goal == null) return false;

        foreach (var t in map.tiles)
            t.previous = null;

        var visited = new HashSet<Tile>();
        var pq = new PriorityQueue<Tile, int>();
        var dist = new Dictionary<Tile, int>();

        foreach (var t in map.tiles)
            dist[t] = int.MaxValue;

        dist[start] = 0;
        pq.Enqueue(start, 0);

        bool found = false;

        while (pq.Count > 0)
        {
            var current = pq.Dequeue();
            if (visited.Contains(current)) continue;

            if (current == goal)
            {
                found = true;
                break;
            }

            visited.Add(current);

            foreach (var next in current.adjacents)
            {
                if (next == null || !next.CanMove || visited.Contains(next)) continue;

                int newDist = dist[current] + next.Weight;
                if (newDist < dist[next])
                {
                    dist[next] = newDist;
                    next.previous = current;
                    pq.Enqueue(next, newDist);
                }
            }
        }

        if (!found) return false;

        var step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return true;
    }

    // -------------------------------
    // A* 알고리즘 (휴리스틱 포함)
    // -------------------------------
    private int Heuristic(Tile a, Tile b)
    {
        int cols = map.cols;

        int ax = a.id % cols;
        int ay = a.id / cols;

        int bx = b.id % cols;
        int by = b.id / cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    public bool AStar(Tile start, Tile goal)
    {
        path.Clear();
        if (start == null || goal == null) return false;

        foreach (var t in map.tiles)
            t.previous = null;

        var visited = new HashSet<Tile>();
        var pq = new PriorityQueue<Tile, int>();
        var gScore = new Dictionary<Tile, int>();
        var fScore = new Dictionary<Tile, int>();

        foreach (var t in map.tiles)
        {
            gScore[t] = int.MaxValue;
            fScore[t] = int.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        pq.Enqueue(start, fScore[start]);

        bool found = false;

        while (pq.Count > 0)
        {
            var current = pq.Dequeue();
            if (visited.Contains(current)) continue;

            if (current == goal)
            {
                found = true;
                break;
            }

            visited.Add(current);

            foreach (var next in current.adjacents)
            {
                if (next == null || !next.CanMove || visited.Contains(next)) continue;

                int tentativeG = gScore[current] + next.Weight;
                if (tentativeG < gScore[next])
                {
                    gScore[next] = tentativeG;
                    fScore[next] = tentativeG + Heuristic(next, goal);
                    next.previous = current;
                    pq.Enqueue(next, fScore[next]);
                }
            }
        }

        if (!found) return false;

        var step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return true;
    }
}

