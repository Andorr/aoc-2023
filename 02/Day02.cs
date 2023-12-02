using utils;
var input = parse("input.txt");
Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Lst<Game> input)
{
    var maxSet = new Set { red = 12, green = 13, blue = 14 };
    return input
        .Where(game => !game.sets.Any(set => set.red > maxSet.red || set.green > maxSet.green || set.blue > maxSet.blue))
        .Select(g => g.id)
        .Sum();
}

long part02(Lst<Game> input)
{
    return input.Select(g =>
        g.sets.Aggregate(new Set(), (acc, set) =>
        {
            acc.red = int.Max(set.red, acc.red);
            acc.green = int.Max(set.green, acc.green);
            acc.blue = int.Max(set.blue, acc.blue);
            return acc;
        })
    )
    .Select(s => s.red * s.green * s.blue)
    .Sum();
}

Lst<Game> parse(string fileName)
{
    return File.ReadAllLines(fileName).Select(line =>
    {
        line = line.Substring(5);
        var sections = line.Split(":");
        var id = int.Parse(sections[0]);
        var sets = sections[1].Split(";").Select(setLine =>
        {
            var cubes = setLine.Split(",").Select(cubes =>
            {
                var comps = cubes.Trim().Split(" ");
                return (comps[0], comps[1]);
            });
            var set = new Set();
            foreach (var cube in cubes)
            {
                switch (cube.Item2)
                {
                    case "red":
                        set.red = int.Parse(cube.Item1);
                        break;
                    case "green":
                        set.green = int.Parse(cube.Item1);
                        break;
                    case "blue":
                        set.blue = int.Parse(cube.Item1);
                        break;
                }
            }
            return set;
        }).ToLst();
        return new Game(id, sets);
    }).ToLst();
}


struct Set
{
    public int red;
    public int green;
    public int blue;

    public override string ToString() => $"({red}, {green}, {blue})";
}

struct Game
{
    public int id;
    public List<Set> sets;

    public Game(int id, List<Set> sets)
    {
        this.id = id;
        this.sets = sets;
    }

    public override string ToString() => $"Game {id}: {sets}";
}


