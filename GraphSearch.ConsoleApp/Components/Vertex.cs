namespace GraphSearch.ConsoleApp.Components;

public class Vertex
{
    private readonly List<Edge> _edges = [];

    private string _id;
    public string Id
    {
        get => _id;
        set
        {
            _id = value;
            Type = _id?.Split(':')[0];
        }
    }

    public string Type { get; private set; }

    public Vertex Parent { get; set; }
    public Edge ParentEdge { get; set; }

    public Vertex() { }

    public Vertex(string id)
    {
        Id = id;
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

    public override string ToString() => Id;
}
