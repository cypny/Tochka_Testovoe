using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static public List<string> Solve(List<(string, string)> edges)
    {
        var result = new List<string>();
        var graph = BuildGraph(edges);
        var virus = InitializeVirus(graph);
        var gateways = GetGateways(graph);

        while (true)
        {
            var edgeToRemove = SelectCriticalEdge(virus, graph, gateways);
            if (edgeToRemove.Gateway == null && edgeToRemove.Node == null)
            {
                break;
            }

            RemoveEdgeAndRecord(graph, edgeToRemove, result);
            virus.MakeMove(graph, gateways);
        }

        return result;
    }

    private static Graph BuildGraph(List<(string, string)> edges)
    {
        var graph = new Graph();
        foreach (var edge in edges)
        {
            graph.AddEdge(edge.Item1, edge.Item2);
        }

        return graph;
    }

    private static Virus InitializeVirus(Graph graph)
    {
        var startNode = graph.GetNode("a") ?? throw new InvalidOperationException("Start node 'a' not found");
        return new Virus(startNode, graph);
    }

    private static List<Node> GetGateways(Graph graph)
    {
        return graph.GetAllNodes().Where(n => n.IsGateway).ToList();
    }

    private static (Node? Gateway, Node? Node) SelectCriticalEdge(Virus virus, Graph graph, List<Node> gateways)
    {
        (var distances, var prev) = graph.CalculateDistances(virus.CurrentPosition);
        var availableCorridors = graph.GetAvailableCorridors(gateways);
        if (!availableCorridors.Any())
        {
            return (null, null);
        }

        var corridorsList = availableCorridors
            .Where(x => distances.ContainsKey(x.Gateway))
            .OrderBy(x => distances[x.Gateway])
            .ThenBy(x => x.Gateway.Name)
            .ThenBy(x => x.Node.Name)
            .ToList();
        if (!corridorsList.Any())
        {
            return availableCorridors.First();
        }

        var сriticalCorridor = GetCriticalCorridor(corridorsList, distances, prev);
        return сriticalCorridor.Node == null ? availableCorridors.First() : сriticalCorridor;
    }

    private static (Node? Gateway, Node? Node) GetCriticalCorridor(List<(Node Gateway, Node Node)> corridorsList,
        Dictionary<Node, int> distances, Dictionary<Node, Node> prev)
    {
        var nodeGatewayCounts = СorridorsOpirations.GetNodeGatewayCounts(corridorsList);
        var countLives = CalculateCoutLives(corridorsList, nodeGatewayCounts, distances, prev);
        if (countLives[corridorsList[0].Node] <= 0)
        {
            return corridorsList[0];
        }

        if (corridorsList.Any(x => countLives[x.Node] <= 0))
        {
            var criticalCorridors = corridorsList.FirstOrDefault(x => nodeGatewayCounts[x.Node] > 1);
            return criticalCorridors;
        }

        return (null, null);
    }

    private static Dictionary<Node, int> CalculateCoutLives(List<(Node Gateway, Node Node)> corridorsList,
        Dictionary<Node, int> nodeGatewayCounts, Dictionary<Node, int> distances, Dictionary<Node, Node> prev)
    {
        var countLives = new Dictionary<Node, int>();
        var corridorsDistant = СorridorsOpirations.CalculateGatewayNeighborDistances(corridorsList, distances, prev);

        countLives[corridorsList[0].Node] = corridorsDistant[corridorsList[0].Node] -
                                            nodeGatewayCounts[corridorsList[0].Node];
        var num = 1;
        if (corridorsList.Count > 1 &&
            nodeGatewayCounts.ContainsKey(corridorsList[0].Node) &&
            nodeGatewayCounts[corridorsList[0].Node] == 1)
        {
            countLives[corridorsList[1].Node] = distances[corridorsList[1].Gateway] -
                                                nodeGatewayCounts[corridorsList[1].Node];
            num = 2;
        }

        for (int i = num; i < corridorsList.Count; i++)
        {
            var previousItem = corridorsList[i - 1];
            var currentItem = corridorsList[i];
            countLives.TryAdd(currentItem.Node,
                countLives[previousItem.Node] +
                corridorsDistant[currentItem.Node] -
                nodeGatewayCounts[currentItem.Node]);
        }

        return countLives;
    }

    private static void RemoveEdgeAndRecord(Graph graph, (Node Gateway, Node Node) edge, List<string> result)
    {
        graph.RemoveEdge(edge.Gateway, edge.Node);
        result.Add($"{edge.Gateway.Name}-{edge.Node.Name}");
    }


    static public void Main()
    {
        var edges = ReadInputEdges();
        var result = Solve(edges);

        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }

    private static List<(string, string)> ReadInputEdges()
    {
        var edges = new List<(string, string)>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split('-');
                if (parts.Length == 2)
                {
                    edges.Add((parts[0], parts[1]));
                }
            }
        }

        return edges;
    }
}

public class Node
{
    public string Name { get; }
    public bool IsGateway => char.IsUpper(Name[0]);

    public Node(string name)
    {
        Name = name;
    }

    public override bool Equals(object obj)
    {
        return obj is Node other && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }
}

public static class СorridorsOpirations
{
    public static Node FindLCA(Node a, Node b, Dictionary<Node, Node> parents, Dictionary<Node, int> distances)
    {
        var nodeA = a;
        var nodeB = b;

        while (distances[nodeA] > distances[nodeB])
        {
            nodeA = parents[nodeA];
        }

        while (distances[nodeB] > distances[nodeA])
        {
            nodeB = parents[nodeB];
        }

        while (nodeA != nodeB)
        {
            nodeA = parents[nodeA];
            nodeB = parents[nodeB];
        }

        return nodeA;
    }

    public static Dictionary<Node, int> CalculateGatewayNeighborDistances(
        List<(Node Gateway, Node Node)> corridorsList, Dictionary<Node, int> distances, Dictionary<Node, Node> prev)
    {
        var result = new Dictionary<Node, int>();
        if (corridorsList.Count > 0)
        {
            result[corridorsList[0].Node] = distances[corridorsList[0].Gateway];
        }

        for (int i = 1; i < corridorsList.Count; i++)
        {
            var currentCorridor = corridorsList[i];
            var previousGateway = corridorsList[i - 1].Gateway;
            if (!distances.ContainsKey(currentCorridor.Node) || result.ContainsKey(currentCorridor.Node))
            {
                continue;
            }

            var lca = FindLCA(currentCorridor.Node, previousGateway, prev, distances);
            int distance = distances[currentCorridor.Node] + distances[previousGateway] - 2 * distances[lca] - 1;
            result[currentCorridor.Node] = distance;
        }

        return result;
    }



    public static Dictionary<Node, int> GetNodeGatewayCounts(List<(Node Gateway, Node Node)> corridorsList)
    {
        var nodeGatewayCounts = new Dictionary<Node, int>();
        foreach (var corridor in corridorsList)
        {
            var node = corridor.Node;
            nodeGatewayCounts[node] = nodeGatewayCounts.TryGetValue(node, out int count)
                ? count + 1
                : 1;
        }

        return nodeGatewayCounts;
    }
}

public class Graph
{
    private readonly Dictionary<Node, HashSet<Node>> _adjacencyList = new();
    private readonly Dictionary<string, Node> _nodes = new();

    public void AddEdge(string u, string v)
    {
        var nodeU = GetOrCreateNode(u);
        var nodeV = GetOrCreateNode(v);
        _adjacencyList[nodeU].Add(nodeV);
        _adjacencyList[nodeV].Add(nodeU);
    }

    private Node GetOrCreateNode(string name)
    {
        if (!_nodes.TryGetValue(name, out var node))
        {
            node = new Node(name);
            _nodes[name] = node;
            _adjacencyList[node] = new HashSet<Node>();
        }

        return node;
    }

    public void RemoveEdge(Node u, Node v)
    {
        if (_adjacencyList.TryGetValue(u, out var valueU))
        {
            valueU.Remove(v);
        }

        if (_adjacencyList.TryGetValue(v, out var valueV))
        {
            valueV.Remove(u);
        }
    }

    public HashSet<Node> GetNeighbors(Node node)
    {
        return _adjacencyList.TryGetValue(node, out var neighbors)
            ? neighbors
            : new HashSet<Node>();
    }

    public List<(Node Gateway, Node Node)> GetAvailableCorridors(
        List<Node> gateways)
    {
        return gateways
            .SelectMany(gateway => GetNeighbors(gateway)
                .Select(node => (Gateway: gateway, Node: node)))
            .OrderBy(x => x.Gateway.Name)
            .ThenBy(x => x.Node.Name)
            .ToList();
    }

    public Node GetNode(string name) => _nodes.TryGetValue(name, out var node) ? node : null;

    public IEnumerable<Node> GetAllNodes() => _nodes.Values;

    public (Dictionary<Node, int> distances, Dictionary<Node, Node> parents) CalculateDistances(Node start)
    {
        var distances = new Dictionary<Node, int>();
        var parents = new Dictionary<Node, Node>();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        visited.Add(start);
        distances[start] = 0;
        parents[start] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var neighbors = GetNeighbors(current).OrderBy(n => n.Name);

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    distances[neighbor] = distances[current] + 1;
                    parents[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return (distances, parents);
    }
}

public class Virus
{
    public Node CurrentPosition { get; private set; }
    private readonly Graph _graph;

    public Virus(Node startPosition, Graph graph)
    {
        CurrentPosition = startPosition;
        _graph = graph;
    }

    public void MakeMove(Graph graph, List<Node> gateways)
    {

        (var distances, var parents) = graph.CalculateDistances(CurrentPosition);
        var targetGateway = CalculateTargetGateway(gateways, distances);
        if (targetGateway == null)
        {
            return;
        }

        MoveTowardsTarget(targetGateway, distances);
    }

    public Node CalculateTargetGateway(List<Node> gateways, Dictionary<Node, int> distances)
    {
        return gateways
            .Where(x => distances.ContainsKey(x))
            .OrderBy(x => distances[x])
            .ThenBy(x => x.Name)
            .FirstOrDefault();
    }

    private void MoveTowardsTarget(Node targetGateway, Dictionary<Node, int> distances)
    {
        var path = FindShortestPath(CurrentPosition, targetGateway, distances);
        if (path != null && path.Count > 1)
        {
            CurrentPosition = path[1];
        }
    }

    private List<Node> FindShortestPath(Node start, Node target, Dictionary<Node, int> distances)
    {
        if (!distances.ContainsKey(target))
        {
            return null;
        }

        var path = new List<Node> { target };
        var current = target;

        while (current != start)
        {
            var neighbors = _graph.GetNeighbors(current)
                .Where(x => distances.ContainsKey(x) && distances[x] == distances[current] - 1)
                .OrderBy(x => x.Name)
                .ToList();
            if (!neighbors.Any())
            {
                return null;
            }

            current = neighbors.First();
            path.Insert(0, current);
        }
//просто коментарий
        return path;
    }
}