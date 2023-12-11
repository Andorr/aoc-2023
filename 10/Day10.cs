using utils;

Dict<Pipe, List<Direction>> pipeMap = new()
{
    { Pipe.Vertical, new List<Direction> { Direction.North, Direction.South } },
    { Pipe.Horizontal, new List<Direction> { Direction.East, Direction.West } },
    { Pipe.WestNorth, new List<Direction> { Direction.West, Direction.North } },
    { Pipe.EastNorth, new List<Direction> { Direction.East, Direction.North } },
    { Pipe.WestSouth, new List<Direction> { Direction.West, Direction.South } },
    { Pipe.EastSouth, new List<Direction> { Direction.East, Direction.South } },
    { Pipe.Start, new List<Direction> { /* Direction.North, Direction.East, Direction.South, Direction.West */ Direction.South } },
};

Dict<Direction, List<Pipe>> directionMap = new()
{
    { Direction.North, new List<Pipe> { Pipe.Vertical, Pipe.WestSouth, Pipe.EastSouth } },
    { Direction.East, new List<Pipe> { Pipe.Horizontal, Pipe.WestSouth, Pipe.WestNorth } },
    { Direction.South, new List<Pipe> { Pipe.Vertical, Pipe.WestNorth, Pipe.EastNorth } },
    { Direction.West, new List<Pipe> { Pipe.Horizontal, Pipe.EastNorth, Pipe.EastSouth } },
};

var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");



long part01(Input input) => (loop(input).Item1.step + 1) / 2;

long part02(Input input)
{
    var (_, visitedPipes) = loop(input);

    return input.grid.Where((el) => !visitedPipes.Contains(el.Key))
    .Select((el) =>
    {
        var coord = el.Key;
        return Enumerable.Range(0, coord.x)
        .Select((x) => new Coord(x, coord.y))
        .Where((coord) => visitedPipes.Contains(coord))
        .Where((coord) =>
        {
            return new Pipe[]{
                Pipe.Vertical, Pipe.WestNorth, Pipe.EastNorth, Pipe.Start
            }.Contains(input.grid[coord]);
        });
    })
    .Count(el => el.Count() % 2 == 1);
}

(Node, HashSet<Coord>) loop(Input input)
{
    var start = input.grid.First((el) => el.Value == Pipe.Start).Key;

    // New stack
    var stack = new Stack<Node>();
    stack.Push(new Node(start, Pipe.Start, 0));
    var visited = new HashSet<Coord>();
    var curNode = stack.Peek();
    while (stack.Count() > 0)
    {
        curNode = stack.Pop();
        visited.Add(curNode.coord);


        // Calculate neighbours
        var neighbours = curNode.pipe.Let(pipe =>
        {
            return pipeMap[pipe];
        })
        .Where(dir =>
        {
            var coord = dirToCoord(dir).Let(coord => new Coord(curNode.coord.x + coord.x, curNode.coord.y + coord.y));
            var validPipes = directionMap[dir];
            var pipe = input.grid[coord];
            return validPipes.Contains(pipe) && !visited.Contains(coord);
        })
        .Select(dir =>
        {
            var coord = dirToCoord(dir).Let(coord => new Coord(curNode.coord.x + coord.x, curNode.coord.y + coord.y));
            var pipe = input.grid[coord];
            return new Node(coord, pipe, curNode.step + 1);
        });

        if (neighbours.Count() == 0)
        {
            break;
        }

        foreach (var neighbour in neighbours)
        {
            stack.Push(neighbour);
        }
    }

    return (curNode, visited);
}

Input parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select((line, y) =>
        {
            return line.Select((pipe, x) =>
            {

                return ((Pipe)pipe, x, y);
            });
        })
        .SelectMany(x => x)
        .Aggregate(new Dict<Coord, Pipe>(), (agg, value) =>
        {
            agg.Add(
                new Coord(value.Item2, value.Item3),
                value.Item1
            );
            return agg;
        })
        .Let(d => new Input(d));
}


Coord dirToCoord(Direction dir)
{
    return dir switch
    {
        Direction.North => new Coord(0, -1),
        Direction.East => new Coord(1, 0),
        Direction.South => new Coord(0, 1),
        Direction.West => new Coord(-1, 0),
        _ => throw new Exception("Unknown direction"),
    };
}

record Node(Coord coord, Pipe pipe, long step);

record Coord(int x, int y);

enum Direction { North, East, South, West }

enum Pipe
{
    Start = 'S',
    Empty = '.',
    Horizontal = '-',
    Vertical = '|',
    WestNorth = 'J',
    EastNorth = 'L',
    WestSouth = '7',
    EastSouth = 'F',
}

record Input(Dict<Coord, Pipe> grid);