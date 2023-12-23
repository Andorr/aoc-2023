using utils;

var input = parse(args.Length > 0 ? args[0] : "input.txt");

Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(Input input)
{
    var (bricks, space) = input.fall();

    // Check for which bricks are safe to disintegrate
    return bricks.Values.Count(brick =>
        disintegrateAndCountFallen(new List<Cuboid>() { brick }, (int[,,])space.Clone(), bricks) == 0
    );
}

long part02(Input input)
{
    var (bricks, space) = input.fall();
    return bricks.Values.Select(brick =>
        disintegrateAndCountFallen(new List<Cuboid>() { brick }, (int[,,])space.Clone(), bricks)
    )
    .Sum();
}

long disintegrateAndCountFallen(List<Cuboid> disintegrated, int[,,] space, Dictionary<int, Cuboid> bricks)
{
    if (disintegrated.Count == 0)
        return 0;

    // For all bricks in b, set their space to be 0
    foreach (var brick in disintegrated)
        for (var x = brick.start.x; x <= brick.end.x; x++)
            for (var y = brick.start.y; y <= brick.end.y; y++)
                for (var z = brick.start.z; z <= brick.end.z; z++)
                {
                    space[x, y, z] = 0;
                }

    var bricksAbove = disintegrated.Select(brick => brick.PositionsWithZ(brick.end.z + 1)
        .Where(pos => space[pos.x, pos.y, pos.z] != 0)
        .Select(pos => space[pos.x, pos.y, pos.z])
    )
    .SelectMany(x => x)
    .Distinct()
    .ToList();

    var bricksThatFall = bricksAbove.Where(brickAboveId =>
    {
        var brickAbove = bricks[brickAboveId];
        var positionsBelowBrick = brickAbove.PositionsWithZ(brickAbove.start.z - 1);
        return positionsBelowBrick.All(pos => space[pos.x, pos.y, pos.z] == 0);
    }).ToList();

    return bricksThatFall.Count + disintegrateAndCountFallen(bricksThatFall.Select(id => bricks[id]).ToList(), space, bricks);
}

Input parse(string fileName)
{
    return new Input(
        File.ReadAllLines(fileName)
        .Select((line, i) =>
            line.Split("~")
            .Select(part => part.Split(",")
                .Select(long.Parse)
                .ToList())
            .Select(nums => new Vector3(nums[0], nums[1], nums[2]))
            .Let(parts => new Cuboid(i + 1, parts.First(), parts.Last()))
        )
        .ToList()
    );
}

record Vector3(long x, long y, long z);
record Cuboid
{
    public int id { get; init; }
    public Vector3 start { get; set; }
    public Vector3 end { get; init; }
    public Cuboid(int id, Vector3 start, Vector3 end)
    {
        this.id = id;
        this.start = start;
        this.end = end;
    }

    public IEnumerable<Vector3> PositionsWithZ(long z) =>
        Enumerable.Range((int)this.start.x, (int)(this.end.x - this.start.x + 1))
                .SelectMany(x => Enumerable.Range((int)this.start.y, (int)(this.end.y - this.start.y + 1))
                    .Select(y => new Vector3(x, y, z)));
}
record Input(List<Cuboid> bricks)
{
    public (Dictionary<int, Cuboid>, int[,,]) fall()
    {
        var bricks = this.bricks.ToDictionary(b => b.id, b => b);

        var maxX = this.bricks.Max(b => b.end.x);
        var maxY = this.bricks.Max(b => b.end.y);
        var maxZ = this.bricks.Max(b => b.end.z);

        // Create a 3D space
        var space = new int[maxX + 1, maxY + 1, maxZ + 1];
        foreach (var brick in this.bricks)
        {
            for (var x = brick.start.x; x <= brick.end.x; x++)
                for (var y = brick.start.y; y <= brick.end.y; y++)
                    for (var z = brick.start.z; z <= brick.end.z; z++)
                    {
                        var pos = new Vector3(x, y, z);
                        space[x, y, z] = brick.id;
                    }
        };

        // Make the bricks fall
        var curZ = 2;
        while (curZ <= maxZ)
        {
            // Find distinct bricks that are on z
            var bricksToFall = Enumerable.Range(0, (int)maxX + 1)
                .SelectMany(x => Enumerable.Range(0, (int)maxY + 1)
                    .Select(y => new Vector3(x, y, curZ))
                    .Where(pos => space[pos.x, pos.y, pos.z - 1] == 0)
                    .Select(pos => space[pos.x, pos.y, pos.z])
                    .Where(id => id != 0)
                    .Distinct()
                )
                .Distinct()
                .ToList();

            // For each brick that is on z, make it fall as far down as possible
            foreach (var brickId in bricksToFall)
            {
                var brick = bricks[brickId];

                // Get all the positions at x of the brick
                var positions = Enumerable.Range((int)brick.start.x, (int)(brick.end.x - brick.start.x + 1))
                    .SelectMany(x => Enumerable.Range((int)brick.start.y, (int)(brick.end.y - brick.start.y + 1))
                        .Select(y => new Vector3(x, y, curZ))
                    )
                    .ToList();


                // Check if the tile below is empty

                var newZ = brick.start.z - 1;
                while (newZ > 0 && positions.All(pos => space[pos.x, pos.y, newZ] == 0))
                {
                    newZ--;
                }
                newZ++;

                if (newZ != brick.start.z)
                {
                    var newBrick = brick with
                    {
                        start = brick.start with { z = newZ },
                        end = brick.end with { z = newZ + (brick.end.z - brick.start.z) }
                    };

                    // Remove the old brick from the space
                    for (var x = brick.start.x; x <= brick.end.x; x++)
                        for (var y = brick.start.y; y <= brick.end.y; y++)
                            for (var z = brick.start.z; z <= brick.end.z; z++)
                            {
                                space[x, y, z] = 0;
                            }

                    // Add the new brick to the space
                    for (var x = newBrick.start.x; x <= newBrick.end.x; x++)
                        for (var y = newBrick.start.y; y <= newBrick.end.y; y++)
                            for (var z = newBrick.start.z; z <= newBrick.end.z; z++)
                            {
                                space[x, y, z] = newBrick.id;
                            }

                    bricks[brickId] = newBrick;
                }
            }
            curZ++;
        }
        return (bricks, space);
    }
}

