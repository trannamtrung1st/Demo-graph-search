using GraphSearch.ConsoleApp.Components;

var graph = new Graph();

var ou1 = Vertex.OU(1, "ou1");
var ou2 = Vertex.OU(2, "ou2");
var ou3 = Vertex.OU(3, "ou3");
var ou4 = Vertex.OU(4, "ou4");
var ou5 = Vertex.OU(5, "ou5");

var u1 = Vertex.User(6, "u1");
var u2 = Vertex.User(7, "u2");
var u3 = Vertex.User(8, "u3");
var u4 = Vertex.User(9, "u4");
var u5 = Vertex.User(10, "u5");
var u6 = Vertex.User(11, "u6");
var u7 = Vertex.User(12, "u7");
var u8 = Vertex.User(13, "u8");
var u9 = Vertex.User(14, "u9");
var u10 = Vertex.User(15, "u10");

var a1 = Vertex.Asset(16, RootAssetLabel);
var a2 = Vertex.Asset(17, "a2");
var a3 = Vertex.Asset(18, "a3");
var a4 = Vertex.Asset(19, "a4");
var a5 = Vertex.Asset(20, "a5");
var a6 = Vertex.Asset(21, "a6");
var a7 = Vertex.Asset(22, "a7");
var a8 = Vertex.Asset(23, "a8");
var a9 = Vertex.Asset(24, "a9");
var a10 = Vertex.Asset(25, "a10");
var a11 = Vertex.Asset(26, "a11");
var a12 = Vertex.Asset(27, "a12");

var labelVertexMap = new Dictionary<string, Vertex>()
{
    { ou1.Label, ou1 },
    { ou2.Label, ou2 },
    { ou3.Label, ou3 },
    { ou4.Label, ou4 },
    { ou5.Label, ou5 },
    { u1.Label, u1 },
    { u2.Label, u2 },
    { a1.Label, a1 },
    { a2.Label, a2 },
    { a3.Label, a3 },
    { a4.Label, a4 },
    { a5.Label, a5 },
    { a6.Label, a6 },
    { a7.Label, a7 },
    { a8.Label, a8 },
    { a9.Label, a9 },
    { a10.Label, a10 },
    { a11.Label, a11 },
    { a12.Label, a12 }
};

// Add Org Units
graph.AddVertex(ou1, ou2, ou3, ou4, ou5);

// Add Users
graph.AddVertex(u1, u2);

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

var graph2 = new VisibilityGraph();
graph2.Load(graph.ToSerializedString(), graph.GetVertices());

Start(graph2, labelVertexMap);

static void Start(VisibilityGraph graph, Dictionary<string, Vertex> labelVertexMap)
{
    while (true)
    {
        try
        {
            ExecuteOnce(graph, labelVertexMap);
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

static void ExecuteOnce(VisibilityGraph graph, Dictionary<string, Vertex> labelVertexMap)
{
    Console.WriteLine("1. Get visible assets");
    Console.WriteLine("2. Get first visible asset tree");
    Console.WriteLine("3. Get full asset tree");
    Console.WriteLine("4. Serialize graph");
    Console.WriteLine("5. Exit");
    Console.Write("Please choose an option: ");
    var option = Console.ReadLine();
    Console.Clear();
    switch (option)
    {
        case "1":
            {
                Console.Write("Please enter the user label: ");
                var userId = labelVertexMap[Console.ReadLine()].Id;
                var visibleAssets = graph.GetVisibleAssets(userId);
                PrintTree(visibleAssets.ToList());
            }
            break;
        case "2":
            {
                Console.Write("Please enter the user label: ");
                var userId = labelVertexMap[Console.ReadLine()].Id;
                Console.Write("Please enter the asset label: ");
                var assetId = labelVertexMap[Console.ReadLine()].Id;
                var rootAssetId = labelVertexMap[RootAssetLabel].Id;
                var (includedInTree, unauthorized) = graph.CheckVisibility(userId, rootAssetId);
                var firstVisibleTree = graph.GetFirstVisibleAssetTree(assetId, includedInTree, unauthorized);
                PrintTree(firstVisibleTree, unauthorized);
            }
            break;
        case "3":
            {
                Console.Write("Please enter the user label: ");
                var userId = labelVertexMap[Console.ReadLine()].Id;
                var rootAssetId = labelVertexMap[RootAssetLabel].Id;
                var (includedInTree, unauthorized) = graph.CheckVisibility(userId, rootAssetId);
                var fullTree = graph.GetFullAssetTree(includedInTree, rootAssetId);
                PrintTree(fullTree, unauthorized);
            }
            break;
        case "4":
            {
                Console.WriteLine(graph.ToSerializedString());
            }
            break;
        case "5":
            {
                Environment.Exit(0);
            }
            break;
    }
}

static void PrintTree(List<Vertex> tree, HashSet<int> unauthorized = null)
{
    Console.WriteLine(string.Join(", ", tree.Select(v => unauthorized?.Contains(v.Id) == true ? $"{v}?" : v.ToString())));
}

public partial class Program
{
    public const string RootAssetLabel = "a1";
}