using System.Text;
using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Input input) => solve(input, 0, 3);
long part02(Input input) => solve(input, 4, 10);

long solve(Input input, int minStep, int maxStep)
{
    var queue = new PriorityQueue<Node, long>();
    var visited = new HashSet<(Vector2, Vector2, long)>();
    var target = new Vector2(input.grid[0].Length - 1, input.grid.Length - 1);
    queue.Enqueue(new Node(new Vector2(1, 0), new Vector2(1, 0), input.grid[0][1], input.grid[0][1], 0), 0);
    queue.Enqueue(new Node(new Vector2(0, 1), new Vector2(0, 1), input.grid[1][0], input.grid[1][0], 0), 0);

    while (queue.Count > 0)
    {
        var node = queue.Dequeue();
        if (node.pos == target)
        {
            return node.cost;
        }

        if (visited.Contains((node.pos, node.dir, node.steps)))
        {
            continue;
        }
        visited.Add((node.pos, node.dir, node.steps));

        var dirs = new List<Vector2>();
        if (node.steps < minStep - 1)
        {
            dirs.Add(node.dir);
        }
        else if (node.steps == maxStep - 1)
        {
            dirs.Add(new Vector2(node.dir.y, -node.dir.x));
            dirs.Add(new Vector2(-node.dir.y, node.dir.x));
        }
        else
        {
            dirs.Add(node.dir);
            dirs.Add(new Vector2(node.dir.y, -node.dir.x));
            dirs.Add(new Vector2(-node.dir.y, node.dir.x));
        }

        foreach (var dir in dirs)
        {
            var newPos = node.pos + dir;
            if (newPos.x < 0 || newPos.y < 0 || newPos.x >= input.grid[0].Length || newPos.y >= input.grid.Length)
            {
                continue;
            }

            var newValue = input.grid[newPos.y][newPos.x];
            var newCost = node.cost + newValue;
            var newSteps = node.dir == dir ? (node.steps + 1) : 0;
            queue.Enqueue(new Node(newPos, dir, newValue, newCost, newSteps, node), newCost);
        }
    }
    return -1;
}

Input parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line => line.Select(c => int.Parse(c.ToString())).ToArray())
        .ToArray()
        .Let(grid => new Input(grid));
}

record Vector2(int x, int y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
}
record Node(Vector2 pos, Vector2 dir, int value, int cost, int steps, Node parent = null);
record Input(int[][] grid)
{
    public string AsString(List<Node>? nodes = null)
    {
        var sb = new StringBuilder();
        for (var y = 0; y < grid.Length; y++)
        {
            for (var x = 0; x < grid[0].Length; x++)
            {
                var node = nodes?.FirstOrDefault(n => n.pos == new Vector2(x, y));
                if (node != null)
                {
                    sb.Append(node.dir switch
                    {
                        var d when d == new Vector2(0, 1) => 'v',
                        var d when d == new Vector2(0, -1) => '^',
                        var d when d == new Vector2(1, 0) => '>',
                        var d when d == new Vector2(-1, 0) => '<',
                        _ => 'X'
                    });
                }
                else
                {
                    sb.Append(grid[y][x]);
                }

            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
