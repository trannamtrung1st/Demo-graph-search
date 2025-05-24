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

    public string SerializedString => $"{From.Id} {ConnectionSymbol} {To.Id} {(Directed ? 1 : 0)} {(IsTree ? 1 : 0)}";

    public override string ToString() => $"{From} {ConnectionSymbol} {To}";

    public override bool Equals(object obj)
    {
        return obj is Edge edge &&
               From == edge.From &&
               To == edge.To &&
               ConnectionSymbol == edge.ConnectionSymbol &&
               Directed == edge.Directed &&
               IsTree == edge.IsTree;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To, ConnectionSymbol, Directed, IsTree);
    }
}