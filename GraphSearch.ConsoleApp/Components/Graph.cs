namespace GraphSearch.ConsoleApp.Components;

public class Graph
{
    protected Dictionary<int, Vertex> _vertices = [];
    protected readonly List<Edge> _edges = [];

    public virtual Dictionary<int, Vertex> GetVertices() => _vertices;

    public virtual void AddVertex(params Vertex[] vertices)
    {
        foreach (var vertex in vertices)
            _vertices.Add(vertex.Id, vertex);
    }

    public virtual void AddEdge(params Edge[] edges)
    {
        foreach (var edge in edges)
        {
            _edges.Add(edge);
            edge.From.AddEdge(edge);
            edge.To.AddEdge(edge);
        }
    }

    public virtual void RemoveEdge(int fromId, int toId, bool directed = false, string connectionSymbol = null)
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
        _edges.Remove(edge);
        edge.From.RemoveEdge(edge);
        edge.To.RemoveEdge(edge);
    }

    public virtual void ChangeParent(int movingAssetId, int parentAssetId)
    {
        var movingAsset = V(movingAssetId);
        var parentAsset = V(parentAssetId);
        if (movingAsset.Parent != null)
            RemoveEdge(movingAsset.ParentEdge);

        AddEdge(new Edge(parentAsset, movingAsset, "-", directed: true, isTree: true));
    }

    public virtual Vertex V(int id)
    {
        return _vertices[id];
    }

    public virtual void Load(string serializedString, Dictionary<int, Vertex> vertices)
    {
        _vertices = vertices;
        var edges = serializedString.Split("\n").Select(e => e.Split(" "))
            .Select(e => new Edge(
                from: V(int.Parse(e[0])),
                to: V(int.Parse(e[2])),
                connectionSymbol: e[1],
                directed: e[3] == "1",
                isTree: e[4] == "1"
            ));
        foreach (var edge in edges)
            _edges.Add(edge);
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