using MathNet.Numerics;
var input = parse(args.Length > 0 ? args[0] : "input.txt");
var n = args.Length > 1 ? long.Parse(args[1]) : 26501365;

Console.WriteLine($"Part 01: {solve(input, 64)}");
Console.WriteLine($"Part 02: {solve(input, n, true)}");

long solve(Input input, long n, bool part2 = false)
{
    var maxX = input.map[0].Length;
    var maxY = input.map.Length;

    var positions = new HashSet<Vector2> { input.start };

    var mapCycles = new List<long>();

    for (var i = 0; i < n; i++)
    {
        var newPositions = new HashSet<Vector2>();

        foreach (var pos in positions)
        {
            var neighbours = new List<Vector2>
            {
                new Vector2(pos.X - 1, pos.Y),
                new Vector2(pos.X + 1, pos.Y),
                new Vector2(pos.X, pos.Y - 1),
                new Vector2(pos.X, pos.Y + 1)
            };

            foreach (var neighbour in neighbours)
            {
                // The map is infinite, so we need to calculate the position on the map
                // Calculate x and y such that they represent the same position on the input.map
                var x = neighbour.X % maxX;
                var y = neighbour.Y % maxY;
                x = x < 0 ? maxX + x : x;
                y = y < 0 ? maxY + y : y;

                // Is valid
                if (x < 0 || x >= maxX ||
                   y < 0 || y >= maxY || input.map[y][x] == '#')
                {
                    continue;
                }

                newPositions.Add(neighbour);
            }
        }

        if (part2)
        {
            // Save the number of positions for each map cycle
            var start = input.map.Length / 2;
            var next = start + input.map.Length;
            var next2 = start + input.map.Length * 2;
            if (i + 1 == start || i + 1 == next || i + 1 == next2)
            {
                mapCycles.Add(newPositions.Count);
                if (mapCycles.Count == 3)
                {
                    break;
                }
            }
        }

        positions = newPositions;
    }

    if (part2)
    {
        // Calculate the polynomial cofficients of a quadratic equation for the number of positions by map cycle
        var cofficients = Fit.Polynomial(
            new double[] { 0, 1, 2 },
            mapCycles.Take(3).Select(x => (double)x).ToArray(),
            2
        );

        // targetMap => the number of map cycles to reach n
        var targetMap = (n - (input.map.Length / 2)) / input.map.Length;
        var res = Polynomial.Evaluate(targetMap, cofficients);
        return (long)res;

    }
    return positions.Count;
}

Input parse(string fileName)
{
    var map = File.ReadAllLines(fileName)
        .Select(line => line.ToCharArray())
        .ToArray();

    return new Input(map, new Vector2(map[0].Length / 2, map.Length / 2));
}

record Vector2(int X, int Y);
record Input(char[][] map, Vector2 start);

