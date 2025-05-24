using GraphSearch.ConsoleApp.Constants;

namespace GraphSearch.ConsoleApp.Components;

public class Vertex
{
    private readonly List<Edge> _edges = [];
    public int Id { get; set; }
    public string Label { get; set; }
    public EVertexType Type { get; set; }
    public Vertex Parent { get; set; }
    public Edge ParentEdge { get; set; }

    public Vertex() { }

    public Vertex(int id, string label, EVertexType type)
    {
        Id = id;
        Label = label ?? $"{id}";
        Type = type;
    }

    public void AddEdge(Edge edge)
    {
        _edges.Add(edge);
        if (edge.IsTree && edge.To.Id == Id)
        {
            Parent = edge.From;
            ParentEdge = edge;
        }
    }

    public void RemoveEdge(Edge edge)
    {
        _edges.Remove(edge);
        if (edge.IsTree && edge.To.Id == Id)
        {
            Parent = null;
            ParentEdge = null;
        }
    }

    public IEnumerable<Edge> GetEdges() => _edges;

    public override string ToString() => Label;

    // Breadth-First Search
    public List<Vertex> BFS(
        Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter,
        object initialState)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<(Vertex v, Edge e, object s)>();
        var result = new List<Vertex>();
        queue.Enqueue((this, null, initialState));
        visited.Add(this.Id);
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
        Func<(Vertex v, Edge e, object s), (bool Accepted, bool ShouldExpand, object State)> process,
        Func<Vertex, bool> neighborFilter,
        Action<Vertex, Edge, object> beforePop,
        object initialState)
    {
        var visited = new HashSet<int>();
        var result = new List<Vertex>();
        process ??= (_ => (true, true, initialState));
        DFSUtil((this, null, initialState), visited, result, process, neighborFilter, beforePop);
        return result;
    }

    private static void DFSUtil(
        (Vertex v, Edge e, object s) args, HashSet<int> visited, List<Vertex> result,
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

    public static Vertex OU(int id, string label) => new(id, label, EVertexType.OrgUnit);
    public static Vertex User(int id, string label) => new(id, label, EVertexType.User);
    public static Vertex Asset(int id, string label) => new(id, label, EVertexType.Asset);
}
