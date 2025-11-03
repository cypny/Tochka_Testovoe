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
        var distances = graph.CalculateDistances(virus.CurrentPosition);
        var availableCorridors = GetAvailableCorridors(gateways, graph);
        if (!availableCorridors.Any())
        {
            return (null, null);
        }

        var gatewayNodeCounts = GetGatewayNodeCounts(availableCorridors);
        var criticalCorridors = availableCorridors
            .Where(x => distances.ContainsKey(x.Gateway))
            .Where(x => distances[x.Gateway] - gatewayNodeCounts[x.Node] <= 0)
            .OrderBy(x => distances[x.Gateway])
            .ThenBy(x => x.Gateway.Name)
            .ThenBy(x => x.Node.Name)
            .ToList();
        return criticalCorridors.Any()
            ? criticalCorridors.First()
            : availableCorridors.First();
    }

    private static Dictionary<Node, int> GetGatewayNodeCounts(List<(Node Gateway, Node Node)> availableCorridors)
    {
        var gatewayNodeCounts = new Dictionary<Node, int>();
        foreach (var items in availableCorridors)
        {
            var node = items.Node;
            if (!gatewayNodeCounts.TryGetValue(node, out var countGateway))
            {
                gatewayNodeCounts[node] = 0;
            }

            gatewayNodeCounts[node]++;
        }

        return gatewayNodeCounts;
    }

    private static void RemoveEdgeAndRecord(Graph graph, (Node Gateway, Node Node) edge, List<string> result)
    {
        graph.RemoveEdge(edge.Gateway, edge.Node);
        result.Add($"{edge.Gateway.Name}-{edge.Node.Name}");
    }

    private static List<(Node Gateway, Node Node)> GetAvailableCorridors(
        List<Node> gateways,
        Graph graph)
    {
        return gateways
            .SelectMany(gateway => graph.GetNeighbors(gateway)
                .Select(node => (Gateway: gateway, Node: node)))
            .OrderBy(x => x.Gateway.Name)
            .ThenBy(x => x.Node.Name)
            .ToList();
    }

    static void Main()
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
        if (_adjacencyList.ContainsKey(u))
        {
            _adjacencyList[u].Remove(v);
        }

        if (_adjacencyList.ContainsKey(v))
        {
            _adjacencyList[v].Remove(u);
        }
    }

    public HashSet<Node> GetNeighbors(Node node)
    {
        return _adjacencyList.TryGetValue(node, out var neighbors)
            ? neighbors
            : new HashSet<Node>();
    }

    public Node GetNode(string name) => _nodes.TryGetValue(name, out var node) ? node : null;

    public IEnumerable<Node> GetAllNodes() => _nodes.Values;

    public Dictionary<Node, int> CalculateDistances(Node start)
    {
        var distances = new Dictionary<Node, int>();
        var visited = new HashSet<Node>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        visited.Add(start);
        distances[start] = 0;

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
                    queue.Enqueue(neighbor);
                }
            }
        }

        return distances;
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
        if (CurrentPosition == null)
        {
            return;
        }

        var distances = graph.CalculateDistances(CurrentPosition);
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

        return path;
    }
}