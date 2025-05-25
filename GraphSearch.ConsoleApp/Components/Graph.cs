using System.Text;

namespace GraphSearch.ConsoleApp.Components;

public class Graph
{
    public const char SpaceCh = ' ';
    public const char NewLineCh = '\n';
    public static readonly char[] ReservedChars = [SpaceCh, NewLineCh];

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
        bool isRaw = serializedString[0] == 'R';

        void ProcessLine()
        {
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

        if (isRaw)
        {
            var parts = serializedString.Split(SpaceCh, NewLineCh);
            for (var i = 1; i < parts.Length; i++)
            {
                var item = parts[i];
                line.Add(item);
                if (line.Count < 5)
                    continue;

                ProcessLine();
            }
        }
        else
        {
            var parts = serializedString.Split(SpaceCh);
            var dictItems = parts[0].Split(NewLineCh);
            var edges = parts[1];
            var dict = new Dictionary<char, string>();

            for (var i = 1; i < dictItems.Length; i++)
            {
                var item = dictItems[i];
                var key = item[0];
                var value = item[1..];
                dict.Add(key, value);
            }

            for (var i = 0; i < edges.Length; i++)
            {
                var item = edges[i];

                if (line.Count < 3)
                    line.Add(dict[item]);
                else line.Add(new string([item]));

                if (line.Count < 5)
                    continue;

                ProcessLine();
            }
        }
    }

    public override string ToString()
    {
        return string.Join(NewLineCh, _edges.Select(e => e.ToString()));
    }

    public virtual string ToSerializedString(bool isCompressed)
    {
        var sb = new StringBuilder();
        sb.Append(isCompressed ? "C" : "R"); // C - compressed, R - raw

        if (!isCompressed)
        {
            sb.AppendLine();
            sb.Append(string.Join(NewLineCh, _edges.Select(e => e.SerializedString)));
            return sb.ToString();
        }

        var dict = new Dictionary<char, string>();
        var dictReverse = new Dictionary<string, char>();
        var currentCh = char.MinValue;

        char GetNextChar()
        {
            if (currentCh > char.MaxValue)
                throw new Exception($"Too many vertices to compress, max: {char.MaxValue}");
            if (ReservedChars.Contains(currentCh))
                currentCh++;
            return currentCh++;
        }

        var eBd = new StringBuilder();
        foreach (var e in _edges)
        {
            var from = e.From.Id;
            var to = e.To.Id;

            if (!dictReverse.TryGetValue(from, out var fromChar))
            {
                fromChar = GetNextChar();
                dict.Add(fromChar, from);
                dictReverse.Add(from, fromChar);
            }

            if (!dictReverse.TryGetValue(to, out var toChar))
            {
                toChar = GetNextChar();
                dict.Add(toChar, to);
                dictReverse.Add(to, toChar);
            }

            if (!dictReverse.TryGetValue(e.ConnectionSymbol, out var connectionSymbolChar))
            {
                connectionSymbolChar = GetNextChar();
                dict.Add(connectionSymbolChar, e.ConnectionSymbol);
                dictReverse.Add(e.ConnectionSymbol, connectionSymbolChar);
            }

            eBd.Append(fromChar);
            eBd.Append(connectionSymbolChar);
            eBd.Append(toChar);
            eBd.Append(e.Directed ? "1" : "0");
            eBd.Append(e.IsTree ? "1" : "0");
        }

        foreach (var (key, value) in dict)
        {
            sb.Append(NewLineCh);
            sb.Append(key);
            sb.Append(value);
        }

        sb.Append(SpaceCh);
        sb.Append(eBd);
        return sb.ToString();
    }
}