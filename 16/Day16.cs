using utils;
var input = parse("input.txt");

Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(Cave input) => solve(input, new Beam(new Vector2(0, 0), new Vector2(1, 0)));

long part02(Cave input)
{
    // Generate all the beams at the edges
    var beams = new List<Beam>();
    var maxX = input.tiles.Keys.Max(x => x.x);
    var maxY = input.tiles.Keys.Max(x => x.y);

    for (var x = 0; x <= maxX; x++)
    {
        beams.Add(new Beam(new Vector2(x, 0), new Vector2(0, 1)));
        beams.Add(new Beam(new Vector2(x, maxY), new Vector2(0, -1)));
    }

    for (var y = 0; y <= maxY; y++)
    {
        beams.Add(new Beam(new Vector2(0, y), new Vector2(1, 0)));
        beams.Add(new Beam(new Vector2(maxX, y), new Vector2(-1, 0)));
    }


    return beams.Select(beam => solve(input, beam)).Max();
}

long solve(Cave input, Beam start)
{
    var visited = new HashSet<(Vector2, Vector2)>();
    var beams = new List<Beam> { start };

    while (beams.Count > 0)
    {
        var beamsToRemove = new List<int>();
        var beamsToAdd = new List<Beam>();

        foreach (Beam b in beams)
        {
            visited.Add((b.pos, b.direction));
            var nextTile = input.tiles[b.pos];
            switch (nextTile)
            {
                case Tile.Vertical:
                    if (b.direction.x != 0)
                    {
                        beamsToAdd.Add(new Beam(b.pos, new Vector2(0, 1)));
                        b.direction = new Vector2(0, -1);
                    }
                    break;
                case Tile.Horizontal:
                    if (b.direction.y != 0)
                    {
                        // We split into two beams
                        beamsToAdd.Add(new Beam(b.pos, new Vector2(1, 0)));
                        b.direction = new Vector2(-1, 0);
                    }
                    break;
                case Tile.MirrorA:
                    b.direction = new Vector2(-b.direction.y, -b.direction.x);
                    break;
                case Tile.MirrorB:
                    b.direction = new Vector2(b.direction.y, b.direction.x);
                    break;
            }

            b.pos += b.direction;
            beamsToAdd.Add(b);
        }

        beams = beamsToAdd.Where(beam => input.tiles.ContainsKey(beam.pos) && !visited.Contains((beam.pos, beam.direction))).ToList();
    }

    return visited.GroupBy(x => x.Item1).Count();
}

Cave parse(string fileName) =>
    File.ReadAllLines(fileName)
        .SelectMany((line, y) => line.Select((c, x) => new { x, y, c }))
        .ToDictionary(x => new Vector2(x.x, x.y), x =>
        {
            switch (x.c)
            {
                case '.': return Tile.Empty;
                case '|': return Tile.Vertical;
                case '-': return Tile.Horizontal;
                case '/': return Tile.MirrorA;
                case '\\': return Tile.MirrorB;
                default: throw new Exception($"Unknown tile {x.c}");
            }
        })
        .Let(tiles => new Cave(tiles.ToDict()));

record Beam
{
    public Vector2 pos;
    public Vector2 direction;
    public Beam(Vector2 pos, Vector2 direction)
    {
        this.pos = pos;
        this.direction = direction;
    }
}
record Vector2(int x, int y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
}
enum Tile
{
    Empty = '.',
    Vertical = '|',
    Horizontal = '-',
    MirrorA = '/',
    MirrorB = '\\',
}

record Cave(Dict<Vector2, Tile> tiles);