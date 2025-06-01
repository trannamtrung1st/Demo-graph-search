using System.Text;

namespace GraphSearch.ConsoleApp.Components;

public class Graph
{
    protected readonly Dictionary<string, Vertex> _vertices = [];
    protected readonly HashSet<Edge> _edges = [];

    public virtual HashSet<Edge> GetEdges() => _edges;

    public virtual Dictionary<string, Vertex> GetVertices() => _vertices;

    public virtual void AddVertex(params Vertex[] vertices)
    {
        foreach (var vertex in vertices)
            _vertices.Add(vertex.Id, vertex);
    }

    public virtual void AddEdge(params Edge[] edges)
    {
        foreach (var edge in edges)
        {
            if (_edges.Add(edge))
            {
                edge.From.AddEdge(edge);
                edge.To.AddEdge(edge);
            }
        }
    }

    public virtual void RemoveEdge(string fromId, string toId, bool directed = false, string connectionSymbol = null)
    {
        var shouldCheckSymbol = !string.IsNullOrEmpty(connectionSymbol);
        var edge = _edges.FirstOrDefault(e =>
        {
            var validSymbol = !shouldCheckSymbol || e.ConnectionSymbol == connectionSymbol;
            if (directed)
                return e.From.Id == fromId && e.To.Id == toId && validSymbol;
            else
                return (e.From.Id == fromId && e.To.Id == toId || e.From.Id == toId && e.To.Id == fromId) && validSymbol;
        });

        if (edge != null)
            RemoveEdge(edge);
    }

    public virtual void RemoveEdge(Edge edge)
    {
        if (_edges.Remove(edge))
        {
            edge.From.RemoveEdge(edge);
            edge.To.RemoveEdge(edge);
        }
    }

    public virtual void ChangeParent(string movingAssetId, string parentAssetId)
    {
        var movingAsset = V(movingAssetId);
        var parentAsset = V(parentAssetId);
        if (movingAsset.Parent != null)
            RemoveEdge(movingAsset.ParentEdge);

        AddEdge(new Edge(parentAsset, movingAsset, "-", directed: true, isTree: true));
    }

    public virtual Vertex V(string id, bool upsert = false)
    {
        if (!_vertices.TryGetValue(id, out var v) && upsert)
        {
            v = new(id);
            _vertices.Add(id, v);
        }

        return v ?? throw new KeyNotFoundException($"Vertex {id} not found");
    }

    public virtual void Load(byte[] serializedBytes, bool reset = false)
    {
        Load(Encoding.UTF8.GetString(serializedBytes), reset);
    }

    public virtual void Load(string serializedString, bool reset = false)
    {
        if (reset)
        {
            _vertices.Clear();
            _edges.Clear();
        }

        var line = new List<string>();
        foreach (var item in serializedString.Split(' ', '\n'))
        {
            line.Add(item);
            if (line.Count < 5)
                continue;

            var from = line[0];
            var connectionSymbol = line[1];
            var to = line[2];
            var directed = line[3] == "1";
            var istree = line[4] == "1";

            if (!_vertices.TryGetValue(from, out var vFrom))
            {
                vFrom = new Vertex(from);
                _vertices.Add(from, vFrom);
            }

            if (!_vertices.TryGetValue(to, out var vTo))
            {
                vTo = new Vertex(to);
                _vertices.Add(to, vTo);
            }

            var edge = new Edge(vFrom, vTo, connectionSymbol, directed, istree);
            AddEdge(edge);
            line.Clear();
        }
    }

    // Breadth-First Search
    public List<Vertex> BFS(
        string startId, Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter, object initialState = null)
    {
        var start = V(startId);
        return BFS(start, process, neighborFilter, initialState);
    }

    public static List<Vertex> BFS(
        Vertex start, Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter, object initialState = null)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<(Vertex v, Edge e, object s)>();
        var result = new List<Vertex>();
        queue.Enqueue((start, null, initialState));
        visited.Add(start.Id);
        process ??= (_ => (true, true, initialState));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var (accepted, shouldExpand, state) = process(current);
            if (accepted)
                result.Add(current.v);

            if (!shouldExpand)
                continue;

            foreach (var edge in current.v.GetEdges())
            {
                var neighbor = edge.Other(current.v);
                if (edge.Directed && edge.From != current.v || !neighborFilter(neighbor))
                    continue;

                if (!visited.Contains(neighbor.Id))
                {
                    queue.Enqueue((neighbor, edge, state));
                    visited.Add(neighbor.Id);
                }
            }
        }

        return result;
    }

    // Depth-First Search
    public List<Vertex> DFS(
        string startId, Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter, Action<Vertex, Edge, object> beforePop, object initialState = null)
    {
        var start = V(startId);
        return DFS(start, process, neighborFilter, beforePop, initialState);
    }

    public static List<Vertex> DFS(
        Vertex start, Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter, Action<Vertex, Edge, object> beforePop, object initialState = null)
    {
        var visited = new HashSet<string>();
        var result = new List<Vertex>();
        process ??= (_ => (true, true, initialState));
        DFSUtil((start, null, initialState), visited, result, process, neighborFilter, beforePop);
        return result;
    }

    private static void DFSUtil(
        (Vertex v, Edge e, object s) args, HashSet<string> visited, List<Vertex> result,
        Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter,
        Action<Vertex, Edge, object> beforePop)
    {
        var (vertex, edge, _) = args;
        visited.Add(vertex.Id);
        var (accepted, shouldExpand, state) = process(args);
        if (accepted)
            result.Add(vertex);

        if (!shouldExpand)
            return;

        foreach (var e in vertex.GetEdges())
        {
            var neighbor = e.Other(vertex);
            if (e.Directed && e.From != vertex || !neighborFilter(neighbor))
                continue;

            if (!visited.Contains(neighbor.Id))
                DFSUtil((neighbor, e, state), visited, result, process, neighborFilter, beforePop);
        }

        beforePop?.Invoke(vertex, edge, state);
    }

    public override string ToString()
    {
        return string.Join("\n", _edges.Select(e => e.ToString()));
    }

    public virtual string SerializeToString()
    {
        return string.Join("\n", _edges.Select(e => e.SerializedString));
    }

    public virtual byte[] SerializeToBytes()
    {
        return Encoding.UTF8.GetBytes(SerializeToString());
    }
}