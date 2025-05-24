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

    public override string ToString()
    {
        return string.Join("\n", _edges.Select(e => e.ToString()));
    }

    public virtual string ToSerializedString()
    {
        return string.Join("\n", _edges.Select(e => e.SerializedString));
    }
}