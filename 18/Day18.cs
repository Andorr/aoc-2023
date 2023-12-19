using System.Numerics;
using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(DigPlan input) => areaOfDigPlan(input);

long part02(DigPlan input)
{
    var instructions = input.instructions.Select(instr =>
    {
        var color = instr.color;
        var hexDigit = color.Substring(2, 5);
        var r = int.Parse(hexDigit, System.Globalization.NumberStyles.HexNumber);
        var dir = color[^2] switch
        {
            '0' => Direction.R,
            '1' => Direction.D,
            '2' => Direction.L,
            '3' => Direction.U,
            _ => throw new Exception("Unknown direction")
        };
        return new Instruction(dir, r, color);
    });
    return areaOfDigPlan(new DigPlan(instructions.ToList()));
}

long areaOfDigPlan(DigPlan input)
{
    var grid = new Dictionary<Vector2, string>();
    var pos = new Vector2(0, 0);
    var verticies = new List<Vector2>();
    var permimeterSize = 0L;

    foreach (var instruction in input.instructions)
    {
        var (x, y) = pos;
        var dir = instruction.d switch
        {
            Direction.R => new Vector2(1, 0),
            Direction.L => new Vector2(-1, 0),
            Direction.U => new Vector2(0, 1),
            Direction.D => new Vector2(0, -1),
            _ => throw new Exception("Unknown direction")
        };

        pos = pos + dir * instruction.steps;
        verticies.Add(pos + dir * instruction.steps);
        permimeterSize += instruction.steps;
    }

    // Shoelace formula
    var area = 0L;
    for (var i = 0; i < verticies.Count - 1; i++)
    {
        area += verticies[i].x * verticies[i + 1].y;
        area -= verticies[i + 1].x * verticies[i].y;
    }
    area += verticies[verticies.Count - 1].x * verticies[0].y;
    area -= verticies[0].x * verticies[verticies.Count - 1].y;
    area = Math.Abs(area) / 2L;

    return area + (permimeterSize / 2L) + 1L;
}

DigPlan parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line =>
            line.Split(" ")
            .Let(comps =>
            {
                return new Instruction(
                    Enum.Parse<Direction>(comps[0]),
                    int.Parse(comps[1]),
                    comps[2]
                );
            })
        )
        .Let(instructions => new DigPlan(instructions.ToList()));
}

enum Direction
{
    R = 'R',
    L = 'L',
    U = 'U',
    D = 'D'
}

record Vector2(long x, long y)
{
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
    public static Vector2 operator *(Vector2 a, int b) => new Vector2(a.x * b, a.y * b);

    public override string ToString() => $"({x}, {y})";
}
record Instruction(Direction d, int steps, string color)
{
    public override string ToString() => $"{d} {steps} {color}";
}
record DigPlan(List<Instruction> instructions);
