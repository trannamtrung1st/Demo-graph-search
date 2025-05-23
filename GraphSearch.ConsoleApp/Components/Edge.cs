namespace GraphSearch.ConsoleApp.Components;

public class Edge
{
    public Vertex From { get; set; }
    public Vertex To { get; set; }

    /// <summary>
    /// Symbols:
    /// -: normal
    /// ~: normal apply children
    /// x: excluded
    /// ~x: exclude apply children
    /// </summary>
    public string ConnectionSymbol { get; set; }

    public bool IsTree { get; set; }
    public bool Directed { get; set; }
    public bool Normal => ConnectionSymbol == "-";
    public bool NormalAc => ConnectionSymbol == "~";
    public bool Excluded => ConnectionSymbol == "x";
    public bool ExcludedAc => ConnectionSymbol == "~x";

    public Edge() { }

    public Edge(Vertex from, Vertex to, string connectionSymbol, bool directed = false, bool isTree = false)
    {
        From = from;
        To = to;
        ConnectionSymbol = connectionSymbol;
        Directed = directed;
        IsTree = isTree;
    }

    public Vertex Other(Vertex v) => v == From ? To : From;

    public override string ToString() => $"{From} {ConnectionSymbol} {To}";

    public string ToSerializedString() => $"{From.Id} {ConnectionSymbol} {To.Id}";
}