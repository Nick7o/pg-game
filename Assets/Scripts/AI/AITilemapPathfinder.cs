using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AITilemapPathfinder : MonoBehaviour
{
    [Header("Tilemap")]
    [Tooltip("Used as the navigation grid reference. Additional walkable tilemaps should use the same Grid/cell layout.")]
    [SerializeField] private Tilemap _walkableTilemap;
    [Tooltip("Extra walkable tilemaps, for example separate island tilemaps on the same Grid.")]
    [SerializeField] private Tilemap[] _additionalWalkableTilemaps;
    [SerializeField] private bool _requireWalkableTile = true;

    [Header("Obstacles")]
    [SerializeField] private LayerMask _obstacleMask;
    [Min(0f)]
    [SerializeField] private float _obstacleProbeRadius = 0.15f;

    [Header("Search")]
    [SerializeField] private bool _allowDiagonalMovement = false;
    [Min(1)]
    [SerializeField] private int _maxVisitedCells = 700;
    [Min(0)]
    [SerializeField] private int _nearestWalkableSearchRadius = 3;

    private Collider2D[] _ownColliders;

    public bool HasWalkableTilemap => GetNavigationTilemap() != null;
    public Tilemap WalkableTilemap => GetNavigationTilemap();

    private void Awake()
    {
        _ownColliders = GetComponentsInChildren<Collider2D>();
    }

    public bool TryFindPath(Vector2 startWorld, Vector2 targetWorld, List<Vector2> path)
    {
        path.Clear();

        Tilemap navigationTilemap = GetNavigationTilemap();
        if (navigationTilemap == null)
            return false;

        Vector3Int startCell = navigationTilemap.WorldToCell(startWorld);
        Vector3Int targetCell = navigationTilemap.WorldToCell(targetWorld);

        if (!IsCellWalkable(startCell) && !TryFindNearestWalkableCell(startCell, out startCell))
            return false;

        if (!IsCellWalkable(targetCell) && !TryFindNearestWalkableCell(targetCell, out targetCell))
            return false;

        if (startCell == targetCell)
        {
            path.Add(GetCellCenterWorld(targetCell));
            return true;
        }

        List<PathNode> open = new();
        HashSet<Vector3Int> closed = new();
        Dictionary<Vector3Int, PathNode> nodes = new();

        PathNode startNode = GetOrCreateNode(startCell, targetCell, nodes);
        startNode.GCost = 0;
        open.Add(startNode);

        while (open.Count > 0 && closed.Count < _maxVisitedCells)
        {
            PathNode currentNode = GetLowestCostNode(open);
            if (currentNode.Cell == targetCell)
            {
                BuildWorldPath(currentNode, path);
                return path.Count > 0;
            }

            open.Remove(currentNode);
            closed.Add(currentNode.Cell);

            foreach (Vector3Int neighbourCell in GetNeighbours(currentNode.Cell))
            {
                if (closed.Contains(neighbourCell) || !IsCellWalkable(neighbourCell))
                    continue;

                if (IsDiagonalMove(currentNode.Cell, neighbourCell) && IsCornerCutBlocked(currentNode.Cell, neighbourCell))
                    continue;

                if (IsMovementBlocked(GetCellCenterWorld(currentNode.Cell), GetCellCenterWorld(neighbourCell)))
                    continue;

                PathNode neighbourNode = GetOrCreateNode(neighbourCell, targetCell, nodes);
                int tentativeGCost = currentNode.GCost + GetMovementCost(currentNode.Cell, neighbourCell);

                if (tentativeGCost >= neighbourNode.GCost && open.Contains(neighbourNode))
                    continue;

                neighbourNode.Parent = currentNode;
                neighbourNode.GCost = tentativeGCost;

                if (!open.Contains(neighbourNode))
                    open.Add(neighbourNode);
            }
        }

        return false;
    }

    public bool TryGetRandomReachablePoint(
        Vector2 pathStartWorld,
        Vector2 searchCenterWorld,
        float radius,
        int attempts,
        out Vector2 point,
        List<Vector2> path = null)
    {
        point = searchCenterWorld;

        Tilemap navigationTilemap = GetNavigationTilemap();
        if (navigationTilemap == null)
            return false;

        List<Vector2> workingPath = path ?? new List<Vector2>();
        Vector3Int centerCell = navigationTilemap.WorldToCell(searchCenterWorld);
        int cellRadius = Mathf.Max(0, Mathf.CeilToInt(radius / GetCellSize()));
        int safeAttempts = Mathf.Max(1, attempts);

        for (int i = 0; i < safeAttempts; i++)
        {
            Vector3Int candidateCell = centerCell + new Vector3Int(
                Random.Range(-cellRadius, cellRadius + 1),
                Random.Range(-cellRadius, cellRadius + 1),
                0);

            Vector2 candidateWorld = GetCellCenterWorld(candidateCell);
            if (Vector2.Distance(candidateWorld, searchCenterWorld) > radius)
                continue;

            if (!IsCellWalkable(candidateCell))
                continue;

            if (!TryFindPath(pathStartWorld, candidateWorld, workingPath))
                continue;

            point = candidateWorld;
            return true;
        }

        return false;
    }

    public bool IsWorldPositionWalkable(Vector2 worldPosition)
    {
        Tilemap navigationTilemap = GetNavigationTilemap();
        if (navigationTilemap == null)
            return !_requireWalkableTile && !IsObstacleAt(worldPosition);

        return IsCellWalkable(navigationTilemap.WorldToCell(worldPosition));
    }

    public bool IsMovementBlocked(Vector2 fromWorldPosition, Vector2 toWorldPosition)
    {
        if (_obstacleMask.value == 0)
            return false;

        Vector2 direction = toWorldPosition - fromWorldPosition;
        float distance = direction.magnitude;

        if (distance <= Mathf.Epsilon)
            return IsObstacleAt(toWorldPosition);

        Collider2D[] hits = Physics2D.OverlapCircleAll(toWorldPosition, _obstacleProbeRadius, _obstacleMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!IsOwnCollider(hits[i]))
                return true;
        }

        RaycastHit2D[] castHits = Physics2D.CircleCastAll(
            fromWorldPosition,
            _obstacleProbeRadius,
            direction.normalized,
            distance,
            _obstacleMask);

        for (int i = 0; i < castHits.Length; i++)
        {
            if (!IsOwnCollider(castHits[i].collider))
                return true;
        }

        return false;
    }

    public Vector2 GetCellCenterWorld(Vector3Int cell)
    {
        Tilemap navigationTilemap = GetNavigationTilemap();
        return navigationTilemap != null ? navigationTilemap.GetCellCenterWorld(cell) : new Vector2(cell.x, cell.y);
    }

    private bool TryFindNearestWalkableCell(Vector3Int originCell, out Vector3Int nearestCell)
    {
        nearestCell = originCell;

        for (int radius = 1; radius <= _nearestWalkableSearchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    Vector3Int candidateCell = originCell + new Vector3Int(x, y, 0);
                    if (!IsCellWalkable(candidateCell))
                        continue;

                    nearestCell = candidateCell;
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsCellWalkable(Vector3Int cell)
    {
        if (_requireWalkableTile && !HasWalkableTile(cell))
            return false;

        Vector2 worldPosition = GetCellCenterWorld(cell);
        return !IsObstacleAt(worldPosition);
    }

    private bool HasWalkableTile(Vector3Int cell)
    {
        if (_walkableTilemap != null && _walkableTilemap.HasTile(cell))
            return true;

        if (_additionalWalkableTilemaps == null)
            return false;

        for (int i = 0; i < _additionalWalkableTilemaps.Length; i++)
        {
            Tilemap tilemap = _additionalWalkableTilemaps[i];
            if (tilemap != null && tilemap.HasTile(cell))
                return true;
        }

        return false;
    }

    private Tilemap GetNavigationTilemap()
    {
        if (_walkableTilemap != null)
            return _walkableTilemap;

        if (_additionalWalkableTilemaps == null)
            return null;

        for (int i = 0; i < _additionalWalkableTilemaps.Length; i++)
        {
            if (_additionalWalkableTilemaps[i] != null)
                return _additionalWalkableTilemaps[i];
        }

        return null;
    }

    private bool IsObstacleAt(Vector2 worldPosition)
    {
        if (_obstacleMask.value == 0)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, _obstacleProbeRadius, _obstacleMask);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!IsOwnCollider(hits[i]))
                return true;
        }

        return false;
    }

    private bool IsOwnCollider(Collider2D hit)
    {
        if (hit == null || _ownColliders == null)
            return false;

        for (int i = 0; i < _ownColliders.Length; i++)
        {
            if (hit == _ownColliders[i])
                return true;
        }

        return false;
    }

    private IEnumerable<Vector3Int> GetNeighbours(Vector3Int cell)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if (!_allowDiagonalMovement && x != 0 && y != 0)
                    continue;

                yield return cell + new Vector3Int(x, y, 0);
            }
        }
    }

    private bool IsCornerCutBlocked(Vector3Int fromCell, Vector3Int toCell)
    {
        int xDirection = toCell.x - fromCell.x;
        int yDirection = toCell.y - fromCell.y;

        Vector3Int horizontalCell = fromCell + new Vector3Int(xDirection, 0, 0);
        Vector3Int verticalCell = fromCell + new Vector3Int(0, yDirection, 0);

        return !IsCellWalkable(horizontalCell) || !IsCellWalkable(verticalCell);
    }

    private bool IsDiagonalMove(Vector3Int fromCell, Vector3Int toCell)
    {
        return fromCell.x != toCell.x && fromCell.y != toCell.y;
    }

    private PathNode GetOrCreateNode(
        Vector3Int cell,
        Vector3Int targetCell,
        Dictionary<Vector3Int, PathNode> nodes)
    {
        if (nodes.TryGetValue(cell, out PathNode node))
            return node;

        node = new PathNode(cell, GetHeuristicCost(cell, targetCell));
        nodes.Add(cell, node);
        return node;
    }

    private PathNode GetLowestCostNode(List<PathNode> nodes)
    {
        PathNode bestNode = nodes[0];

        for (int i = 1; i < nodes.Count; i++)
        {
            PathNode candidateNode = nodes[i];
            if (candidateNode.FCost > bestNode.FCost)
                continue;

            if (candidateNode.FCost == bestNode.FCost && candidateNode.HCost >= bestNode.HCost)
                continue;

            bestNode = candidateNode;
        }

        return bestNode;
    }

    private int GetMovementCost(Vector3Int fromCell, Vector3Int toCell)
    {
        return IsDiagonalMove(fromCell, toCell) ? 14 : 10;
    }

    private int GetHeuristicCost(Vector3Int fromCell, Vector3Int toCell)
    {
        int xDistance = Mathf.Abs(fromCell.x - toCell.x);
        int yDistance = Mathf.Abs(fromCell.y - toCell.y);

        if (!_allowDiagonalMovement)
            return (xDistance + yDistance) * 10;

        int diagonalSteps = Mathf.Min(xDistance, yDistance);
        int straightSteps = Mathf.Abs(xDistance - yDistance);
        return diagonalSteps * 14 + straightSteps * 10;
    }

    private void BuildWorldPath(PathNode targetNode, List<Vector2> path)
    {
        path.Clear();

        PathNode currentNode = targetNode;
        while (currentNode != null)
        {
            path.Add(GetCellCenterWorld(currentNode.Cell));
            currentNode = currentNode.Parent;
        }

        path.Reverse();

        if (path.Count > 1)
            path.RemoveAt(0);
    }

    private float GetCellSize()
    {
        Tilemap navigationTilemap = GetNavigationTilemap();
        if (navigationTilemap == null || navigationTilemap.layoutGrid == null)
            return 1f;

        Vector3 cellSize = navigationTilemap.layoutGrid.cellSize;
        return Mathf.Max(Mathf.Max(cellSize.x, cellSize.y), 0.01f);
    }

    public void SetWalkableTilemaps(Tilemap mainTilemap, Tilemap[] additionalTilemaps)
    {
        _walkableTilemap = mainTilemap;
        _additionalWalkableTilemaps = additionalTilemaps;
    }
    private sealed class PathNode
    {
        public PathNode(Vector3Int cell, int hCost)
        {
            Cell = cell;
            HCost = hCost;
            GCost = int.MaxValue;
        }

        public Vector3Int Cell { get; }
        public PathNode Parent { get; set; }
        public int GCost { get; set; }
        public int HCost { get; }
        public int FCost => GCost + HCost;
    }
}
