using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GraphSearch.ConsoleApp.Components;

var graph = new Graph();

var ou1 = new Vertex("ou:1");
var ou2 = new Vertex("ou:2");
var ou3 = new Vertex("ou:3");
var ou4 = new Vertex("ou:4");
var ou5 = new Vertex("ou:5");

var u1 = new Vertex("u:1");
var u2 = new Vertex("u:2");
var u3 = new Vertex("u:3");
var u4 = new Vertex("u:4");
var u5 = new Vertex("u:5");
var u6 = new Vertex("u:6");
var u7 = new Vertex("u:7");
var u8 = new Vertex("u:8");
var u9 = new Vertex("u:9");
var u10 = new Vertex("u:10");

var a1 = new Vertex(RootAssetId);
var a2 = new Vertex("a:2");
var a3 = new Vertex("a:3");
var a4 = new Vertex("a:4");
var a5 = new Vertex("a:5");
var a6 = new Vertex("a:6");
var a7 = new Vertex("a:7");
var a8 = new Vertex("a:8");
var a9 = new Vertex("a:9");
var a10 = new Vertex("a:10");
var a11 = new Vertex("a:11");
var a12 = new Vertex("a:12");

// Add Org Units
graph.AddVertex(ou1, ou2, ou3, ou4, ou5);

// Add Users
graph.AddVertex(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);

// Add Assets
graph.AddVertex(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);

// Add Edges
graph.AddEdge(
    new(u1, ou1, "-"),
    new(u1, ou2, "-"),
    new(u1, ou3, "-"),
    new(u1, ou5, "-"),
    new(u2, ou4, "-")
);

graph.AddEdge(
    new(ou2, a1, "-"),
    new(ou3, a1, "-"),
    new(ou4, a3, "-"),
    new(ou5, a7, "-")
// , new(ou5, a11, "-") // [NOTE] test
);

graph.AddEdge(
    new(ou5, a2, "~"),
    new(ou5, a9, "~"),
    new(ou4, a2, "~")
);

graph.AddEdge(
    new Edge(ou5, a4, "~x")
);

graph.AddEdge(
    new Edge(a1, a2, "-", true, true),
    new Edge(a1, a3, "-", true, true),
    new Edge(a2, a4, "-", true, true),
    new Edge(a2, a5, "-", true, true),
    new Edge(a5, a6, "-", true, true),
    new Edge(a3, a7, "-", true, true),
    new Edge(a4, a8, "-", true, true),
    new Edge(a4, a9, "-", true, true),
    new Edge(a8, a10, "-", true, true),
    new Edge(a8, a11, "-", true, true),
    new Edge(a9, a12, "-", true, true)
);

var u_ou_edges = graph.GetEdges().Where(e => e.From.Type == "u" && e.To.Type == "ou").ToList();
var ou_a_edges = graph.GetEdges().Where(e => e.From.Type == "ou" && e.To.Type == "a").ToList();
var a_a_edges = graph.GetEdges().Where(e => e.From.Type == "a" && e.To.Type == "a").ToList();
var u_ou_serialized = string.Join("\n", u_ou_edges.Select(e => e.SerializedString));
var ou_a_serialized = string.Join("\n", ou_a_edges.Select(e => e.SerializedString));
var a_a_serialized = string.Join("\n", a_a_edges.Select(e => e.SerializedString));

var graph2 = new Graph();
graph2.Load(u_ou_serialized);
graph2.Load(ou_a_serialized);
graph2.Load(a_a_serialized);

var graphManager = new VisibilityGraphManager(graph2);
await Start(graphManager);

static async Task Start(VisibilityGraphManager graphManager)
{
    while (true)
    {
        try
        {
            await ExecuteOnce(graphManager);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
        Console.Clear();
    }
}

static async Task ExecuteOnce(VisibilityGraphManager graphManager)
{
    Console.WriteLine("1. Get visible assets");
    Console.WriteLine("2. Get first visible asset tree");
    Console.WriteLine("3. Get full asset tree");
    Console.WriteLine("4. Serialize graph");
    Console.WriteLine("5. Change parent");
    Console.WriteLine("6. Add edge");
    Console.WriteLine("7. Remove edge");
    Console.WriteLine("8. Report random graph");
    Console.WriteLine("9. Generate random test data");
    Console.WriteLine("10. Exit");
    Console.Write("Please choose an option: ");
    var option = Console.ReadLine();
    Console.Clear();
    switch (option)
    {
        case "1":
            {
                Console.Write("Please enter the user id: ");
                var userId = Console.ReadLine();
                var visibleAssets = graphManager.GetVisibleAssets(userId);
                PrintTree(visibleAssets.ToList());
            }
            break;
        case "2":
            {
                Console.Write("Please enter the user id: ");
                var userId = Console.ReadLine();
                Console.Write("Please enter the asset id: ");
                var assetId = Console.ReadLine();
                var rootAssetId = RootAssetId;
                var (includedInTree, unauthorized) = graphManager.CheckVisibility(userId, rootAssetId);
                var firstVisibleTree = graphManager.GetFirstVisibleAssetTree(assetId, includedInTree, unauthorized);
                PrintTree(firstVisibleTree, unauthorized);
            }
            break;
        case "3":
            {
                Console.Write("Please enter the user id: ");
                var userId = Console.ReadLine();
                var rootAssetId = RootAssetId;
                var (includedInTree, unauthorized) = graphManager.CheckVisibility(userId, rootAssetId);
                var fullTree = graphManager.GetFullAssetTree(includedInTree, rootAssetId);
                PrintTree(fullTree, unauthorized);
            }
            break;
        case "4":
            {
                Console.WriteLine(graphManager.Graph.ToSerializedString());
            }
            break;
        case "5":
            {
                Console.Write("Please enter the moving asset id: ");
                var movingAssetId = Console.ReadLine();
                Console.Write("Please enter the parent asset id: ");
                var parentAssetId = Console.ReadLine();
                graphManager.Graph.ChangeParent(movingAssetId, parentAssetId);
            }
            break;
        case "6":
            {
                Console.Write("Please enter the from vertex id: ");
                var fromVertexId = Console.ReadLine();
                var fromVertex = graphManager.Graph.V(fromVertexId, upsert: true);
                Console.Write("Please enter the to vertex id: ");
                var toVertexId = Console.ReadLine();
                var toVertex = graphManager.Graph.V(toVertexId, upsert: true);
                Console.Write("Please enter the connection symbol: ");
                var connectionSymbol = Console.ReadLine();
                var directed = false; var tree = false;
                if (fromVertex.Type == "a" && toVertex.Type == "a")
                {
                    directed = true;
                    tree = true;
                }
                graphManager.Graph.AddEdge(new Edge(fromVertex, toVertex, connectionSymbol, directed, tree));
            }
            break;
        case "7":
            {
                Console.Write("Please enter the from vertex id: ");
                var fromVertex = Console.ReadLine();
                Console.Write("Please enter the to vertex id: ");
                var toVertex = Console.ReadLine();
                Console.Write("Please enter directed (1/0): ");
                var directed = Console.ReadLine() == "1";
                Console.Write("Please enter the connection symbol: ");
                var connectionSymbol = Console.ReadLine();
                graphManager.Graph.RemoveEdge(fromVertex, toVertex, directed, connectionSymbol);
            }
            break;
        case "8":
            {
                Console.Write("Please enter the number of users: ");
                var users = int.Parse(Console.ReadLine());
                Console.Write("Please enter the number of org units: ");
                var orgUnits = int.Parse(Console.ReadLine());
                Console.Write("Please enter the number of assets: ");
                var assets = int.Parse(Console.ReadLine());
                Console.Write("Please enter the number of random edges: ");
                var randomEdges = int.Parse(Console.ReadLine());
                Console.Write("Please enter the path to the serialized graph: ");
                var path = Console.ReadLine();

                bool shouldStop;
                do
                {
                    var newManager = new VisibilityGraphManager();
                    newManager.GenerateRandomGraph(users, orgUnits, assets, randomEdges);
                    await newManager.ExecuteTestsAndWriteReport(path, users, orgUnits, assets);
                    Console.Write("Do you want to stop? (1/0): ");
                    shouldStop = Console.ReadLine() == "1";
                } while (!shouldStop);
            }
            break;
        case "9":
            {
                Console.Write("Please enter the number of assets: ");
                var assets = int.Parse(Console.ReadLine());
                var visibleAssets = new List<TestAsset>();
                for (var i = 0; i < assets; i++)
                {
                    var data = new TestAsset
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Asset {i + 1}",
                        ResourcePath = $"{Guid.NewGuid()}/{Guid.NewGuid()}",
                        IsOwner = Random.Shared.Next() % 2 == 0
                    };
                    visibleAssets.Add(data);
                }
                var stopwatch = Stopwatch.StartNew();
                var serialized = JsonSerializer.Serialize(visibleAssets);
                var totalBytes = Encoding.UTF8.GetByteCount(serialized);
                Console.WriteLine($"Serialized total {totalBytes} bytes in {stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Restart();
                var deserialized = JsonSerializer.Deserialize<TestAsset[]>(serialized);
                Console.WriteLine($"Deserialized total {deserialized.Length} items in {stopwatch.ElapsedMilliseconds}ms");
            }
            break;
        case "10":
            {
                Environment.Exit(0);
            }
            break;
    }
}

static void PrintTree(List<Vertex> tree, HashSet<string> unauthorized = null)
{
    Console.WriteLine(string.Join(", ", tree.Select(v => unauthorized?.Contains(v.Id) == true ? $"{v}?" : v.ToString())));
}

public partial class Program
{
    public const string RootAssetId = "a:1";
}

public class TestAsset
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ResourcePath { get; set; }
    public bool IsOwner { get; set; }
}