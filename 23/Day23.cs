using utils;
var input = parse(args.Length > 0 ? args[0] : "input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Input input)
{
    var start = new Vector2(1, 0);
    var target = new Vector2(input.map.GetLength(1) - 2, input.map.GetLength(0) - 1);
    var verticies = input.ToDAG(slopes: true);
    return dfs(start, target, verticies);
}

long part02(Input input)
{

    var start = new Vector2(1, 0);
    var target = new Vector2(input.map.GetLength(1) - 2, input.map.GetLength(0) - 1);
    var verticies = input.ToDAG(slopes: false);
    return dfs(start, target, verticies);
}

long dfs(Vector2 startPos, Vector2 targetPos, Dictionary<Vector2, Vertex> verticies)
{
    var queue = new Stack<(Vector2, long, HashSet<Vector2>)>();
    queue.Push((startPos, 0, new HashSet<Vector2>()));
    var maxDistance = 0L;

    while (queue.Count > 0)
    {
        var (curPos, distance, visited) = queue.Pop();
        var curNode = verticies[curPos];

        if (curPos == targetPos)
        {
            maxDistance = Math.Max(maxDistance, distance);
            continue;
        }

        foreach (var edge in curNode.to)
        {
            if (visited.Contains(edge.to.pos))
            {
                continue;
            }

            var newDistance = distance + edge.cost;
            var newVisited = visited.ToHashSet();
            newVisited.Add(edge.to.pos);
            queue.Push((edge.to.pos, newDistance, newVisited));
        }
    }
    return maxDistance;
}

Input parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);
    var map = new Tile[lines.Length, lines[0].Length];
    for (var y = 0; y < lines.Length; y++)
    {
        for (var x = 0; x < lines[y].Length; x++)
        {
            map[y, x] = (Tile)lines[y][x];
        }
    }
    return new Input(
        map
    );
}


public enum Tile
{
    Empty = '.',
    Forest = '#',
    SlopeRight = '>',
    SlopeLeft = '<',
    SlopeUp = '^',
    SlopeDown = 'v',
}



public record Node
{
    public Vector2 pos { get; init; }
    public long cost { get; set; }
    public Node? parent { get; set; }

    public HashSet<Vector2> visited { get; set; } = new HashSet<Vector2>();

    public Node(Vector2 pos, long cost, Node? parent = null, HashSet<Vector2>? visited = null)
    {
        this.pos = pos;
        this.cost = cost;
        this.parent = parent;

        this.visited = visited ?? new HashSet<Vector2>();
        if (parent != null)
        {
            visited?.Add(parent.pos);
        }
    }
}
public record Vector2(int x, int y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
    public static Vector2 operator *(Vector2 a, int b) => new Vector2(a.x * b, a.y * b);
}
public record Input(Tile[,] map)
{
    public void PrintMap(List<Vector2>? path = null)
    {
        for (var y = 0; y < map.GetLength(0); y++)
        {
            for (var x = 0; x < map.GetLength(1); x++)
            {
                if (path != null && path.Any(pos => pos == new Vector2(x, y)))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write(map[y, x]);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }



    public Dictionary<Vector2, Vertex> ToDAG(bool slopes = false)
    {
        var vertexPossibilities = new List<List<Vector2>>() {
            new List<Vector2>{ new Vector2(-1, 0), new Vector2(0, -1), new Vector2(0, 1) },
            new List<Vector2> { new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1) },
            new List<Vector2>{ new Vector2(-1, 0), new Vector2(0, 1), new Vector2(1, 0) },
            new List<Vector2> { new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0) },
        };
        var allowedChars = new[] { Tile.Empty, Tile.SlopeDown, Tile.SlopeLeft, Tile.SlopeRight, Tile.SlopeUp };

        var vertices = new Dictionary<Vector2, Vertex>();
        var start = new Vector2(1, 0);
        vertices.Add(start, new Vertex(start, new List<Edge>()));
        var target = new Vector2(map.GetLength(1) - 2, map.GetLength(0) - 1);
        vertices.Add(target, new Vertex(target, new List<Edge>()));
        for (var y = 0; y < map.GetLength(0); y++)
        {
            for (var x = 0; x < map.GetLength(1); x++)
            {
                if (map[y, x] == Tile.Forest)
                {
                    continue;
                }

                var pos = new Vector2(x, y);

                var isVertex = vertexPossibilities.Any(dirs => dirs.Select(dir => pos + dir).All(pos => allowedChars.Contains((Tile)map[pos.y, pos.x])));
                if (isVertex)
                {
                    var vertex = new Vertex(pos, new List<Edge>());
                    vertices.Add(pos, vertex);
                }
            }
        }

        foreach (var vertex in vertices.Values)
        {

            var queue = new Stack<Vector2>();
            queue.Push(vertex.pos);

            var visited = new HashSet<Vector2>();
            var cost = new Dictionary<Vector2, int>
            {
                { vertex.pos, 0 }
            };

            while (queue.Count > 0)
            {
                var curPos = queue.Pop();

                if (visited.Contains(curPos))
                {
                    continue;
                }

                visited.Add(curPos);

                var isVertex = vertices.ContainsKey(curPos);
                if (isVertex && curPos != vertex.pos)
                {
                    var neighbourVertex = vertices[curPos];
                    vertex.to.Add(new Edge(vertex, neighbourVertex, cost[curPos]));
                    continue;
                }

                Tile tile = (Tile)(map[curPos.y, curPos.x]);
                tile
                .TileDirections(slopes)
                .Select(dir => dir + curPos)
                .Where(pos => pos.x >= 0 && pos.x < map.GetLength(1) && pos.y >= 0 && pos.y < map.GetLength(0))
                .Where(pos => allowedChars.Contains((Tile)map[pos.y, pos.x]))
                .Where(pos => !visited.Contains(pos))
                .ToList()
                .ForEach(pos =>
                {
                    queue.Push(pos);
                    cost.Add(pos, cost[curPos] + 1);
                });
            }
        }

        return vertices;
    }

}
public record Edge(Vertex from, Vertex to, long cost);
public record Vertex(Vector2 pos, List<Edge> to);

public static class TileExtensions
{
    public static Vector2[] TileDirections(this Tile tile, bool slopes)
    {
        return tile switch
        {
            Tile.SlopeRight when slopes => new[] { new Vector2(1, 0) },
            Tile.SlopeLeft when slopes => new[] { new Vector2(-1, 0) },
            Tile.SlopeUp when slopes => new[] { new Vector2(0, -1) },
            Tile.SlopeDown when slopes => new[] { new Vector2(0, 1) },
            _ => new[]
                {
                    new Vector2(0, -1),
                    new Vector2(0, 1),
                    new Vector2(-1, 0),
                    new Vector2(1, 0),
                }
        };
    }
}