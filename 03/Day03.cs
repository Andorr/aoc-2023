using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(EngineSchematic input) => input.numbers.Where(number =>
    Enumerable.Range(number.y - 1, 3)
            .Select(y => Enumerable.Range(number.x.Start.Value - 1, number.x.End.Value - number.x.Start.Value + 3).Select(x => (x, y)))
            .SelectMany(coords => coords)
            // Remove (x, y) values of the number itself
            .Where((row) => !(row.x >= number.x.Start.Value && row.x <= number.x.End.Value && row.y == number.y))
            // Remove (x, y) values outside the board
            .Where((row) => !(row.x < 0 || row.x >= input.maxX || row.y < 0 || row.y >= input.maxY))
            // Check if there is a symbol in any of the coords
            .Any(row => input.schematic[row.y][row.x] != '.')
    )
    .Select(num => num.value)
    .Sum();

long part02(EngineSchematic input)
{
    return Enumerable
        .Range(0, input.maxY).Select(y => Enumerable.Range(0, input.maxX).Select(x => (x, y)).ToList())
        .SelectMany(xy => xy)
        // Find all the gear coords 
        .Where((row) => input.schematic[row.y][row.x] == '*')
        .Select(coord =>
        {
            var (xx, yy) = coord;
            var numbers = input.numbers;
            var results = new List<Number>();
            return ArrayUtils.frame.Select((row) =>
            {
                var (dx, dy) = row;
                var x = xx + dx;
                var y = yy + dy;

                if (x < 0 || x >= input.maxX || y < 0 || y >= input.maxY)
                {
                    return null;
                }

                // Check if there is a number at the coord
                return numbers
                    .Where(number => number.x.Start.Value <= x && number.x.End.Value >= x && number.y == y)
                    .FirstOrDefault();
            })
            .Where(number => number is not null)
            .DistinctBy(number => number)
            .Select(number => number!);
        })
        .Where(numbers => numbers.Count() == 2)
        // To remove duplicates of the same pair of numbers (e.g. 1, 2 and 2, 1)
        // we use the sum of the hashcodes of the numbers
        .DistinctBy(g => g.First()!.GetHashCode() + g.Last()!.GetHashCode())
        .Select(numbers => numbers.First()!.value * numbers.Last()!.value)
        .Sum();
}

EngineSchematic parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);
    var board = new char[lines.Length][];
    var numbers = new Dict<(int, int), Number>();
    var maxX = lines[0].Length;
    var maxY = lines.Length;

    for (int y = 0; y < lines.Length; y++)
    {
        var line = lines[y];
        board[y] = new char[line.Length];
        for (int x = 0; x < line.Length; x++)
        {
            var c = line[x];
            board[y][x] = c;

            if (char.IsDigit(c))
            {
                // Check if there exist a number to the left
                if (x > 0 && char.IsDigit(line[x - 1]))
                {
                    var number = numbers[(x - 1, y)];
                    numbers[(x, y)] = new Number { value = number.value * 10 + (c - '0'), y = y, x = new Range(number.x.Start, x) };

                    // Remove the number to the left
                    numbers.Remove((x - 1, y));
                }
                else
                {
                    numbers[(x, y)] = new Number { value = c - '0', y = y, x = new Range(x, x) };
                }

            }
        }
    }

    return new EngineSchematic(board, numbers.Values.ToList());
}

record Number
{
    public long value;
    public int y;
    public Range x;
}

class EngineSchematic
{
    public char[][] schematic;
    public List<Number> numbers;

    public EngineSchematic(char[][] schematic, List<Number> numbers)
    {
        this.schematic = schematic;
        this.numbers = numbers;
    }

    public int maxX => schematic[0].Length;
    public int maxY => schematic.Length;
}

