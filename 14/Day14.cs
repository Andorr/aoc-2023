using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");


long part01(Platform input) => input.tiltHorizontal(1).Points();

long part02(Platform input, int N = 1000000000)
{
    var p = input;
    var visisted = new Dict<string, int>();
    var cycle = 0;
    var i = 1;
    var key = string.Join("", p.grid.Select(line => string.Join("", line)));

    // Find the cycle
    while (i < N)
    {
        p = p.Cycle();
        key = string.Join("", p.grid.Select(line => string.Join("", line)));
        if (visisted.ContainsKey(key))
        {
            cycle = i - (visisted[key] + 1) + 1;
            break;
        }
        visisted.Add(key, i);
        i++;
    }


    // Calculate the remaining cycles to reach N
    var remaining = (N - i) % cycle;
    for (var j = 0; j < remaining; j++)
    {
        p = p.Cycle();
    }
    return p.Points();
}

Platform parse(string fileName)
{

    return new Platform(
        File.ReadAllLines(fileName)
        .Select(line => line.ToCharArray())
        .ToArray()
    );
}

record Platform(char[][] grid)
{

    public Platform tiltHorizontal(int step)
    {
        var newGrid = grid.Select(line => line.ToArray()).ToArray();
        for (int x = 0; x < newGrid[0].Length; x++)
        {
            var openSpaces = new List<int>();
            for (int y = step > 0 ? 0 : newGrid.Length - 1; step > 0 ? y < newGrid.Length : y >= 0; y += step)
            {
                if (newGrid[y][x] == '.')
                {
                    openSpaces.Add(y);
                }
                else if (newGrid[y][x] == 'O' && openSpaces.Count > 0)
                {
                    newGrid[openSpaces[0]][x] = 'O';
                    newGrid[y][x] = '.';
                    openSpaces.RemoveAt(0);
                    openSpaces.Add(y);
                }
                else if (newGrid[y][x] == '#')
                {
                    openSpaces.Clear();
                }
            }
        }

        return new Platform(newGrid);
    }

    public Platform tiltVertical(int step)
    {
        var newGrid = grid.Select(line => line.ToArray()).ToArray();

        for (int y = 0; y < newGrid.Length; y++)
        {
            var openSpaces = new List<int>();
            for (int x = step > 0 ? 0 : newGrid[0].Length - 1; step > 0 ? x < newGrid[0].Length : x >= 0; x += step)
            {
                if (newGrid[y][x] == '.')
                {
                    openSpaces.Add(x);
                }
                else if (newGrid[y][x] == 'O' && openSpaces.Count > 0)
                {
                    newGrid[y][openSpaces[0]] = 'O';
                    newGrid[y][x] = '.';
                    openSpaces.RemoveAt(0);
                    openSpaces.Add(x);
                }
                else if (newGrid[y][x] == '#')
                {
                    openSpaces.Clear();
                }
            }
        }
        return new Platform(newGrid);
    }


    public override string ToString() => string.Join("\n", grid.Select(line => string.Join("", line)));

    public long Points() => this.grid.Select((row, i) =>
            row.Count(c => c == 'O') * (row.Count() - i)
        ).Sum();

    public Platform Cycle() =>
        this.tiltHorizontal(1)
            .tiltVertical(1)
            .tiltHorizontal(-1)
            .tiltVertical(-1);
}

