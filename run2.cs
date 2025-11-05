using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static public List<string> Solve(List<(string, string)> edges)
    {
        var graph = BuildGraph(edges);
        var virus = InitializeVirus(graph);
        var result = new List<string>();

        while (true)
        {
            var edgeToRemove = SelectEdgeToRemove(graph, virus);
            if (edgeToRemove.Gateway == null || edgeToRemove.Node == null)
            {
                break;
            }

            RemoveEdgeAndRecord(graph, edgeToRemove, result);
            
            var nextMove = virus.ComputeNextMove(graph);
            if (nextMove.gate == null)
            {
                break;
            }

            virus.MoveTo(nextMove.nextNode);
        }

        return result;
    }

    private static Graph BuildGraph(List<(string, string)> edges)
    {
        var graph = new Graph();
        foreach (var (u, v) in edges)
        {
            graph.AddEdge(u, v);
        }
        return graph;
    }

    private static Virus InitializeVirus(Graph graph)
    {
        var startNode = new Node("a");
        return new Virus(startNode);
    }

    public static List<Node> GetGateways(Graph graph)
    {
        return graph.nodes.Values.Where(n => n.IsGateway).ToList();
    }

    private static (Node Gateway, Node Node) SelectEdgeToRemove(Graph graph, Virus virus)
    {
        var result = GetEdgeToRemove(graph, virus);
        if (result?.EdgeToRemove == null)
        {
            return (null, null);
        }

        var (gatewayNode, node) = result.EdgeToRemove.Value;
        return (gatewayNode, node);
    }

    private static Dictionary<string, Dictionary<string, SearchResult>> _memo = new Dictionary<string, Dictionary<string, SearchResult>>();

    private static SearchResult? GetEdgeToRemove(Graph graph, Virus virus)
    {
        var edgesKey = graph.GetStateKey();
        var virusPosition = virus.currentPosition;
        
        if (!_memo.ContainsKey(edgesKey))
        {
            _memo[edgesKey] = new Dictionary<string, SearchResult>();
        }

        if (_memo[edgesKey].ContainsKey(virusPosition.name))
        {
            return _memo[edgesKey][virusPosition.name];
        }
        
        var move = virus.ComputeNextMove(graph);
        if (move.gate == null)
        {
            var result = new SearchResult(null, new List<(Node, Node)>());
            _memo[edgesKey][virusPosition.name] = result;
            return result;
        }

        var candidates = graph.GetAvailableCorridors(GetGateways(graph))
            .OrderBy(x => x.Gateway.name)
            .ThenBy(x => x.Node.name)
            .ToList();

        if (candidates.Count == 0)
        {
            _memo[edgesKey][virusPosition.name] = null;
            return null;
        }

        foreach (var corridor in candidates)
        {
            var edge = (corridor.Gateway, corridor.Node);
            var newGraph = graph.Clone();
            newGraph.RemoveEdge(edge.Item1, edge.Item2);
            
            var simulatedVirus = new Virus(virus.currentPosition);
            var newMove = simulatedVirus.ComputeNextMove(newGraph);
            if (newMove.gate == null)
            {
                var result = new SearchResult(edge, new List<(Node, Node)>());
                _memo[edgesKey][virusPosition.name] = result;
                return result;
            }

            var nextPos = newMove.nextNode;
            if (nextPos != null && nextPos.IsGateway)
            {
                continue;
            }
            
            simulatedVirus.MoveTo(nextPos);
            var deeper = GetEdgeToRemove(newGraph, simulatedVirus);
            if (deeper != null)
            {
                var sequence = new List<(Node, Node)> { edge };
                if (deeper.EdgeToRemove != null)
                {
                    sequence.Add(deeper.EdgeToRemove.Value);
                }
                sequence.AddRange(deeper.RemainingEdges);

                var firstEdge = sequence.Count > 0 ? sequence[0] : ((Node, Node)?)null;
                var remaining = sequence.Skip(1).ToList();
                
                var result = new SearchResult(firstEdge, remaining);
                _memo[edgesKey][virusPosition.name] = result;
                return result;
            }
        }

        _memo[edgesKey][virusPosition.name] = null;
        return null;
    }

    private static void RemoveEdgeAndRecord(Graph graph, (Node Gateway, Node Node) edge, List<string> result)
    {
        graph.RemoveEdge(edge.Gateway, edge.Node);
        result.Add($"{edge.Gateway.name}-{edge.Node.name}");
    }

    static void Main()
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
        var result = Solve(edges);
        foreach (var edge in result)
        {
            Console.WriteLine(edge);
        }
    }
}

public class Node
{
    public string name;
    public bool IsGateway => char.IsUpper(name[0]);

    public Node(string name)
    {
        this.name = name;
    }
    public override bool Equals(object obj)
    {
        return obj is Node other && name == other.name;
    } 

    public override int GetHashCode()
    {
        return name?.GetHashCode() ?? 0;
    } 
}

public class Graph
{
    public readonly Dictionary<Node, HashSet<Node>> adjacencyList = new();
    public readonly Dictionary<string, Node> nodes = new();

    public void AddEdge(string u, string v)
    {
        var nodeU = GetOrCreateNode(u);
        var nodeV = GetOrCreateNode(v);
        AddEdge(nodeU, nodeV);
    }
    
    private void AddEdge(Node u, Node v)
    {
        if (!adjacencyList.ContainsKey(u))
        {
            adjacencyList[u] = new HashSet<Node>();
        }

        if (!adjacencyList.ContainsKey(v))
        {
            adjacencyList[v] = new HashSet<Node>();
        }

        adjacencyList[u].Add(v);
        adjacencyList[v].Add(u);
    }

    public void RemoveEdge(Node u, Node v)
    {
        if (adjacencyList.TryGetValue(u, out var neighborsU))
        {
            neighborsU.Remove(v);
        }

        if (adjacencyList.TryGetValue(v, out var neighborsV))
        {
            neighborsV.Remove(u);
        }
    }

    public Graph Clone()
    {
        var clone = new Graph();
        foreach (var node in nodes.Values)
        {
            clone.nodes[node.name] = new Node(node.name);
        }

        foreach (var kvp in adjacencyList)
        {
            var source = clone.nodes[kvp.Key.name];
            clone.adjacencyList[source] = new HashSet<Node>();
            foreach (var neighbor in kvp.Value)
            {
                var target = clone.nodes[neighbor.name];
                clone.AddEdge(source, target);
            }
        }
        return clone;
    }
    private Node GetOrCreateNode(string name)
    {
        if (!nodes.TryGetValue(name, out var node))
        {
            node = new Node(name);
            nodes[name] = node;
        }
        return node;
    }
}
public static class GraphExtentions
{
    public static Dictionary<Node, int> CalculateDistances(this Graph graph,Node start)
    {
        var distances = new Dictionary<Node, int>();
        var queue = new Queue<Node>();

        queue.Enqueue(start);
        distances[start] = 0;
        //мда
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in  graph.adjacencyList[current].OrderBy(n => n.name))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return distances;
    }
    public static string GetStateKey(this Graph graph)
    {
        var edges = new List<(string, string)>();
        foreach (var kvp in graph.adjacencyList)
        {
            var u = kvp.Key.name;
            foreach (var v in kvp.Value)
            {
                if (string.Compare(u, v.name, StringComparison.Ordinal) <= 0)
                {
                    edges.Add((u, v.name));
                }
            }
        }
        edges.Sort();
        return string.Join(";", edges.Select(e => $"{e.Item1}-{e.Item2}"));
    }
    public static List<(Node Gateway, Node Node)> GetAvailableCorridors(this Graph graph,List<Node> gateways)
    {
        return gateways
            .SelectMany(gateway => graph.adjacencyList[gateway]
                .Where(node => !node.IsGateway)
                .Select(node => (Gateway: gateway, Node: node)))
            .ToList();
    }

}
public class Virus
{
    public Node currentPosition;

    public Virus(Node currentPosition)
    {
        this.currentPosition = currentPosition;
    }

    public void MoveTo(Node newPosition)
    {
        currentPosition = newPosition;
    }

    public (Node gate, Node nextNode) ComputeNextMove(Graph graph)
    {
        var gateways = graph.nodes.Values.Where(n => n.IsGateway).ToList();
        if (gateways.Count == 0)
        {
            return (null,null);
        }

        Node bestGate = null;
        int bestDist = int.MaxValue;
        Dictionary<Node, int> bestDistMap = null;

        foreach (var gate in gateways)
        {
            var distMap = graph.CalculateDistances(gate);
            if (!distMap.ContainsKey(currentPosition))
            {
                continue;
            }

            int d = distMap[currentPosition];
            if (d < bestDist || (d == bestDist && string.Compare(gate.name, bestGate?.name, StringComparison.Ordinal) < 0))
            {
                bestDist = d;
                bestGate = gate;
                bestDistMap = distMap;
            }
        }

        if (bestGate == null)
        {
            return (null,null);
        }

        if (bestDist == 0)
        {
            return (bestGate, bestGate);
        }

        var neighbors = graph.adjacencyList[currentPosition].OrderBy(n => n.name).ToList();
        var nextCandidates = neighbors
            .Where(n => bestDistMap.TryGetValue(n, out int d) && d == bestDist - 1)
            .ToList();

        if (nextCandidates.Count == 0)
        {
            return (null,null);
        }

        return (bestGate, nextCandidates[0]);
    }
}

public class SearchResult
{
    public (Node, Node)? EdgeToRemove { get; }
    public List<(Node, Node)> RemainingEdges { get; }

    public SearchResult((Node, Node)? edgeToRemove, List<(Node, Node)> remainingEdges)
    {
        EdgeToRemove = edgeToRemove;
        RemainingEdges = remainingEdges;
    }
}