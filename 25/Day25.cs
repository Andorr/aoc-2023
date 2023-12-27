using utils;
var input = parse(args.Length > 0 ? args[0] : "input.txt");

Console.WriteLine($"Part01: {part01(input)}");

long part01(Graph input)
{
    // Created a .dot file and used graphvis to reveal the 3 edges that should be cut.
    // Used the following command:
    // neato -Tpng input.dot -o input.png

    // Remove edges that are the bridge
    var edgesToRemove = new List<Edge>(){
        new Edge("ttj", "rpd"),
        new Edge("fqn", "dgc"),
        new Edge("htp", "vps"),
    };

    // Breadth first search to find the number of vertices in the graph
    var bfs = (List<Edge> edges, string start) =>
    {
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        var parents = new Dictionary<string, string>();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            if (visited.Contains(vertex))
            {
                continue;
            }
            visited.Add(vertex);

            var neighbors = edges
                .Where(e => e.Vertex1 == vertex || e.Vertex2 == vertex)
                .Select(e => e.Vertex1 == vertex ? e.Vertex2 : e.Vertex1)
                .ToList();

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    parents[neighbor] = vertex;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return visited.Count;
    };


    // Remove the edges that are the bridge and then count the number of vertices in the graph
    var edges = input.Edges.Where(e =>
        !edgesToRemove.Any(
            edge => (edge.Vertex1 == e.Vertex1 && edge.Vertex2 == e.Vertex2) ||
                    (edge.Vertex1 == e.Vertex2 && edge.Vertex2 == e.Vertex1)
        )
    ).ToList();

    var vertexCount = bfs(edges, edgesToRemove[0].Vertex1);
    var other = input.Vertices.Count - vertexCount;
    return vertexCount * other;
}

Graph parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);

    var wiring = lines.Select(line => (line.Split(": ").Let(parts => (parts[0], parts[1]))));

    var edges = new List<Edge>();
    foreach (var wire in wiring)
    {
        var vertex = wire.Item1;
        var vertexEdges = wire.Item2.Split(" ").ToList();
        foreach (var edge in vertexEdges)
        {
            edges.Add(new Edge(vertex, edge));
        }
    }

    var verticies = edges.SelectMany(e => new[] { e.Vertex1, e.Vertex2 }).Distinct().ToList();

    return new Graph(verticies, edges.DistinctBy(edge => edge.Vertex1 + edge.Vertex2).ToList());
}

public record Edge(string Vertex1, string Vertex2);

public record Graph(List<string> Vertices, List<Edge> Edges);