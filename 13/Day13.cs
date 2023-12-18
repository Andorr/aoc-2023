var input = parse("input.txt");

Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(List<Ground> grounds) => solve(grounds);
long part02(List<Ground> grounds) => solve(grounds, true);

long solve(List<Ground> grounds, bool smugde = false) => grounds
    .Select((ground, i) =>
    {
        var y = ground.horizontal(smugde);
        var x = ground.vertical(smugde);
        return x == 0 ? y * 100L : x;
    })
    .Sum();


List<Ground> parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);
    var grounds = new List<Ground>();
    while (lines.Count() > 0)
    {
        var ground = lines.TakeWhile(l => !string.IsNullOrWhiteSpace(l))
            .Select(line => line.ToCharArray())
            .ToArray();
        lines = lines.Skip(ground.Length + 1).ToArray();

        grounds.Add(new Ground(ground));
    }
    return grounds;
}

record Ground(char[][] ground)
{
    public int Width => ground[0].Length;
    public int Height => ground.Length;

    public long horizontal(bool smugde = false)
    {
        for (int y = 1; y < Height; y++)
        {
            var dy = 0;
            var foundSmudge = false;
            while (
                true
            )
            {
                var rowA = this.ground[y + dy];
                var rowB = this.ground[y - dy - 1];

                // Count the number of differences
                var differences = rowA.Zip(rowB).Count(pair => pair.First != pair.Second);
                if (smugde)
                {
                    if (differences == 1 && !foundSmudge)
                    {
                        foundSmudge = true;
                    }
                    else if (differences > 0)
                    {
                        break;
                    }
                }
                else if (differences > 0)
                {
                    break;
                }

                dy++;

                if (!(y - dy - 1 >= 0 && y + dy < Height))
                {
                    if ((smugde && foundSmudge) || !smugde)
                    {
                        return y;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return 0;
    }

    public long vertical(bool smugde = false)
    {
        for (int x = 1; x < Width; x++)
        {
            var dx = 0;
            var foundSmudge = false;
            while (
                true
            )
            {
                var columnA = this.ground.Select(row => row[x + dx]).ToArray();
                var columnB = this.ground.Select(row => row[x - dx - 1]).ToArray();
                var differences = columnA.Zip(columnB).Count(pair => pair.First != pair.Second);
                if (smugde)
                {
                    if (differences == 1 && !foundSmudge)
                    {
                        foundSmudge = true;
                    }
                    else if (differences > 0)
                    {
                        break;
                    }
                }
                else if (differences > 0)
                {
                    break;
                }

                dx++;
                if (!(x - dx - 1 >= 0 && x + dx < Width))
                {
                    if ((smugde && foundSmudge) || !smugde)
                    {
                        return x;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return 0;
    }
}

