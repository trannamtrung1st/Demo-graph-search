using System.Diagnostics;
using System.Text;

namespace GraphSearch.ConsoleApp.Components;

public class VisibilityGraphManager(Graph graph = null)
{
    public Graph Graph { get; } = graph ?? new Graph();

    public HashSet<Vertex> GetVisibleAssets(string vUserId)
    {
        var vAssets = new HashSet<Vertex>();
        var vUser = Graph.V(vUserId);
        var vOus = vUser.GetEdges().Select(e => e.Other(vUser)).ToArray();

        foreach (var vOu in vOus)
        {
            var vOuAssets = vOu.DFS(
                process: args =>
                {
                    var (v, e, s) = args;
                    if (v.Id == vOu.Id)
                        return (false, true, s);

                    var firstExclude = v.GetEdges().FirstOrDefault(e => e.Other(v).Id == vOu.Id && (e.Excluded || e.ExcludedAc));
                    var accepted = firstExclude == null;
                    var isExpanding = s.Equals(true);
                    var isApplyChildren = e.NormalAc;
                    var isNotExcludedAc = firstExclude?.ExcludedAc != true;
                    var shouldExpand = (isExpanding || isApplyChildren) && isNotExcludedAc;
                    return (accepted, shouldExpand, shouldExpand);
                },
                neighborFilter: v => v.Type == "a",
                beforePop: null,
                initialState: false
            );

            foreach (var vAsset in vOuAssets)
                vAssets.Add(vAsset);
        }

        return vAssets;
    }

    public List<Vertex> GetFullAssetTree(HashSet<string> includedInTree, string vAssetId)
    {
        var vAsset = Graph.V(vAssetId);
        var tree = vAsset.BFS(
            process: args =>
            {
                var (v, _, _) = args;
                var isIncluded = includedInTree.Contains(v.Id);
                return (isIncluded, isIncluded, null);
            },
            neighborFilter: v => v.Type == "a",
            initialState: null
        );
        return tree;
    }

    public (HashSet<string> IncludedInTree, HashSet<string> Unauthorized) CheckVisibility(string vUserId, string vAssetId)
    {
        var visibleAssets = GetVisibleAssets(vUserId);
        var vAsset = Graph.V(vAssetId);
        var unauthorized = new HashSet<string>();
        var includedInTree = new HashSet<string>();
        _ = vAsset.DFS(
            process: args =>
            {
                var (v, _, _) = args;
                var visible = visibleAssets.Contains(v);
                if (!visible)
                    unauthorized.Add(v.Id);

                return (false, true, visible);
            },
            neighborFilter: v => v.Type == "a",
            beforePop: (v, _, s) =>
            {
                if (s.Equals(true) || includedInTree.Contains(v.Id))
                {
                    includedInTree.Add(v.Id);
                    if (v.Parent is not null)
                        includedInTree.Add(v.Parent.Id);
                }
            },
            initialState: true
        );

        return (includedInTree, unauthorized);
    }

    public List<Vertex> GetFirstVisibleAssetTree(string vAssetId, HashSet<string> includedInTree, HashSet<string> unauthorized)
    {
        var vAsset = Graph.V(vAssetId);
        var found = false;
        var firstVisible = vAsset.BFS(
            process: args =>
            {
                if (found)
                    return (false, false, null);

                var (v, e, _) = args;
                if (v.Id == vAsset.Id)
                    return (false, true, null);

                var isIncluded = includedInTree.Contains(v.Id);
                found = isIncluded && !unauthorized.Contains(v.Id);

                return (found, !found, null);
            },
            neighborFilter: v => v.Type == "a",
            initialState: null
        ).FirstOrDefault();

        if (firstVisible is not null)
        {
            var tree = new List<Vertex>() { vAsset };
            var currentPathAsset = firstVisible;
            while (currentPathAsset is not null && currentPathAsset.Parent?.Id != vAsset.Id)
            {
                tree.Add(currentPathAsset);
                currentPathAsset = currentPathAsset.Parent;
            }

            var directChildren = vAsset.GetEdges().Where(e => e.From.Id == vAsset.Id && e.IsTree && includedInTree.Contains(e.To.Id)).Select(e => e.To);
            tree.AddRange(directChildren);
            return tree;
        }

        return [vAsset];
    }

    public void GenerateRandomGraph(int users, int orgUnits, int assets, int randomEdges)
    {
        var vUsers = new List<Vertex>();
        var vOrgUnits = new List<Vertex>();
        var vAssets = new List<Vertex>();

        for (var i = 0; i < users; i++)
        {
            var user = new Vertex($"u:{i}");
            vUsers.Add(user);
            Graph.AddVertex(user);
        }
        for (var i = 0; i < orgUnits; i++)
        {
            var orgUnit = new Vertex($"ou:{i}");
            vOrgUnits.Add(orgUnit);
            Graph.AddVertex(orgUnit);
        }
        for (var i = 0; i < assets; i++)
        {
            var asset = new Vertex($"a:{i}");
            vAssets.Add(asset);
            Graph.AddVertex(asset);
        }

        var random = new Random();
        for (var i = 0; i < users; i++)
        {
            var user = Graph.V($"u:{i}");
            var tempVOrgUnits = vOrgUnits.ToList();
            for (var j = 0; j < random.Next(1, randomEdges); j++)
            {
                var orgUnit = tempVOrgUnits[random.Next(tempVOrgUnits.Count)];
                tempVOrgUnits.Remove(orgUnit);
                Graph.AddEdge(new Edge(user, orgUnit, "-"));
            }
        }

        for (var i = 0; i < orgUnits; i++)
        {
            var orgUnit = Graph.V($"ou:{i}");
            var tempVAssets = vAssets.ToList();
            for (var j = 0; j < random.Next(1, randomEdges); j++)
            {
                var asset = tempVAssets[random.Next(tempVAssets.Count)];
                tempVAssets.Remove(asset);
                var randomConn = random.Next(3);
                var randomSymbol = randomConn switch
                {
                    0 => "x",
                    1 => "~x",
                    2 => "~",
                    _ => "-",
                };
                Graph.AddEdge(new Edge(orgUnit, asset, randomSymbol));
            }
        }

        var queue = new Queue<Vertex>();
        queue.Enqueue(vAssets[0]);
        vAssets.Remove(vAssets[0]);

        while (vAssets.Count > 0 && queue.Count > 0)
        {
            var currentParent = queue.Dequeue();
            for (var i = 0; i < random.Next(1, randomEdges) && vAssets.Count > 0; i++)
            {
                var asset = vAssets[random.Next(vAssets.Count)];
                vAssets.Remove(asset);
                queue.Enqueue(asset);
                Graph.AddEdge(new Edge(currentParent, asset, "-", directed: true, isTree: true));
            }
        }
    }

    public async Task ExecuteTestsAndWriteReport(string path, int users, int orgUnits, int assets, bool isCompressed)
    {
        path = string.IsNullOrEmpty(path) ? "./serialized.txt" : path;
        var report = new StringBuilder();
        var random = new Random();
        var stopwatch = new Stopwatch();
        var vertices = Graph.GetVertices().Values;
        var numberOfVertices = vertices.Count;
        var numberOfEdges = Graph.GetEdges().Count;
        var rootAsset = vertices.FirstOrDefault(v => v.Type == "a" && v.Parent is null);
        var randomUserId = $"u:{random.Next(users)}";
        var randomAssetId = $"a:{random.Next(assets)}";
        var randomUser = vertices.FirstOrDefault(v => v.Id == randomUserId);
        var randomAsset = vertices.FirstOrDefault(v => v.Id == randomAssetId);

        // Test 1: serialize graph
        stopwatch.Restart();
        var serializedGraph = Graph.ToSerializedString(isCompressed);
        report.AppendLine($"Test 1: serialize graph: {stopwatch.ElapsedMilliseconds}ms");

        // Test 2: load graph
        stopwatch.Restart();
        var newGraph = new VisibilityGraphManager();
        newGraph.Graph.Load(serializedGraph);
        report.AppendLine($"Test 2: load graph: {stopwatch.ElapsedMilliseconds}ms");

        // Test 3: check visibility
        stopwatch.Restart();
        var (includedInTree, unauthorized) = newGraph.CheckVisibility(randomUser.Id, rootAsset.Id);
        report.AppendLine($"Test 3: check visibility - included: {includedInTree.Count} - unauthorized: {unauthorized.Count} - {stopwatch.ElapsedMilliseconds}ms");

        // Test 4: get first visible asset tree
        stopwatch.Restart();
        var firstVisibleAssetTree = newGraph.GetFirstVisibleAssetTree(randomAsset.Id, includedInTree, unauthorized);
        report.AppendLine($"Test 4: get first visible asset tree - count: {firstVisibleAssetTree.Count} - {stopwatch.ElapsedMilliseconds}ms");

        // Test 5: get full asset tree
        stopwatch.Restart();
        var fullAssetTree = newGraph.GetFullAssetTree(includedInTree, rootAsset.Id);
        report.AppendLine($"Test 5: get full asset tree: {stopwatch.ElapsedMilliseconds}ms");

        // Test 6: add random edge
        stopwatch.Restart();
        var randomOuId = $"ou:{random.Next(orgUnits)}";
        var randomOu = newGraph.Graph.V(randomOuId);
        newGraph.Graph.AddEdge(new Edge(randomOu, randomAsset, "-"));
        report.AppendLine($"Test 6: add random edge: {stopwatch.ElapsedMilliseconds}ms");

        // Test 7: remove random edge
        stopwatch.Restart();
        newGraph.Graph.RemoveEdge(randomOu.Id, randomAsset.Id);
        report.AppendLine($"Test 7: remove random edge: {stopwatch.ElapsedMilliseconds}ms");

        report.AppendLine($"Serialized graph: {Encoding.UTF8.GetByteCount(serializedGraph)} bytes");
        Console.WriteLine();
        Console.WriteLine(report.ToString());

        await File.WriteAllTextAsync(path, serializedGraph);
    }
}
