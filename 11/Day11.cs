using utils;
var input = parse("input.txt");
Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");


long part01(Universe universe) => solve(universe, 2);

long part02(Universe universe) => solve(universe, 1_000_000);

long solve(Universe universe, int n = 2)
{
    var (xs, ys) = universe.Expand();
    var galaxies = universe.Galaxies();

    // Create new matrix
    var matrix = new long[galaxies.Count, galaxies.Count];
    for (var i = 0; i < galaxies.Count; i++)
    {
        for (var j = 0; j < galaxies.Count; j++)
        {
            var galaxyA = galaxies[i];
            var galaxyB = galaxies[j];
            matrix[i, j] = galaxyA.pos.Dist(galaxyB.pos, xs, ys, n);
        }
    }

    // Take the same of the entire matrix
    var sum = 0L;
    for (var i = 0; i < galaxies.Count; i++)
    {
        for (var j = 0; j < galaxies.Count; j++)
        {
            sum += matrix[i, j];
        }
    }
    return sum / 2;
}

Universe parse(string fileName) => new Universe(
            File.ReadAllLines(fileName)
            .Select(line => line.ToCharArray().ToList())
            .ToList()
);


record Galaxy(Coord pos, int id);
record Coord(int x, int y)
{
    public long Dist(Coord other, List<int> xs, List<int> ys, long n = 1000000)
    {
        // Check how many times the range of x up to other.x is within xs
        long minX = Math.Min(x, other.x);
        long maxX = Math.Max(x, other.x);
        long minY = Math.Min(y, other.y);
        long maxY = Math.Max(y, other.y);

        long xCount = xs.Count(x => x >= minX && x <= maxX);
        long yCount = ys.Count(y => y >= minY && y <= maxY);

        long xLength = maxX - minX - xCount + xCount * n;
        long yLength = maxY - minY - yCount + yCount * n;
        return xLength + yLength;
    }
}
record Universe(List<List<char>> universe)
{

    public List<Galaxy> Galaxies()
    {
        var galaxies = new List<Galaxy>();
        for (var y = 0; y < universe.Count; y++)
        {
            for (var x = 0; x < universe[y].Count; x++)
            {
                if (universe[y][x] == '#')
                {
                    galaxies.Add(new Galaxy(new Coord(x, y), galaxies.Count + 1));
                }
            }
        }
        return galaxies;
    }

    public (List<int>, List<int>) Expand()
    {
        // Find all rows that are empty
        var xList = new List<int>();
        var yList = new List<int>();
        var y = 0;
        while (y < universe.Count)
        {
            var row = universe[y];
            if (row.All(c => c == '.'))
            {
                yList.Add(y);
            }
            y++;
        }

        // Find all columns that are empty
        var x = 0;
        while (x < universe[0].Count)
        {
            var column = universe.Select(row => row[x]).ToList();
            if (column.All(c => c == '.'))
            {
                xList.Add(x);
            }
            x++;
        }

        return (xList, yList);
    }

}

