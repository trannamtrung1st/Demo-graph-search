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

    public virtual Vertex V(int id)
    {
        return _vertices[id];
    }

    public virtual void Load(string serializedString, Dictionary<int, Vertex> vertices)
    {
        _vertices = vertices;
        var edges = serializedString.Split("\n").Select(e => e.Split(" ")).Select(e => new Edge(V(int.Parse(e[0])), V(int.Parse(e[2])), e[1]));
        foreach (var edge in edges)
            _edges.Add(edge);
    }

    public override string ToString()
    {
        return string.Join("\n", _edges.Select(e => e.ToString()));
    }

    public virtual string ToSerializedString()
    {
        return string.Join("\n", _edges.Select(e => e.ToSerializedString()));
    }
}