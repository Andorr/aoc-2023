using System.Numerics;
using Microsoft.Z3;
using utils;

var inputFile = args.Length > 0 ? args[0] : "input.txt";
var input = parse(inputFile);

var testArea = inputFile == "input.txt" ? (200000000000000d, 400000000000000d) : (7d, 27d);
Console.WriteLine($"Part 01: {part01(input, testArea)}");
Console.WriteLine($"Part 02: {part02(input)}");


long part01(List<Hailstone> input, (double min, double max) testArea)
{

    var checkedPairs = new HashSet<(int, int)>();

    var count = 0L;
    for (var i = 0; i < input.Count; i++)
    {
        for (var j = 0; j < input.Count; j++)
        {
            if (i == j)
                continue;

            if (checkedPairs.Contains((i, j)) || checkedPairs.Contains((j, i)))
                continue;

            checkedPairs.Add((i, j));
            checkedPairs.Add((j, i));

            var intersection = input[i].LineIntersectionXYPlane(input[j]);

            if (intersection is not null)
            {
                var isWithinTestArea =
                    intersection.Value.Item1 >= testArea.min && intersection.Value.Item1 <= testArea.max &&
                    intersection.Value.Item2 >= testArea.min && intersection.Value.Item2 <= testArea.max;

                var intersectionPosition = new Vector3(new BigInteger(intersection.Value.Item1), new BigInteger(intersection.Value.Item2), 0);
                var AB = intersectionPosition - input[i].position;
                var isOpposite = (AB.x * input[i].velocity.x) + (AB.y * input[i].velocity.y) < 0;
                if (isOpposite)
                    continue;

                var CB = intersectionPosition - input[j].position;
                isOpposite = (CB.x * input[j].velocity.x) + (CB.y * input[j].velocity.y) < 0;
                if (isOpposite)
                    continue;

                if (isWithinTestArea)
                {
                    count++;
                }

            }
        }
    }
    return count;
}

BigInteger part02(List<Hailstone> input)
{
    // Find the position and velocity where all the halistones will collide after time t
    // px = px + vx*t
    // py = py + vy*t
    // pz = pz + vz*t

    // p0x + v0x*t1 = p1x + v1x*t1
    // p0x + v0x*t2 = p2x + v2x*t2
    // p0x + v0x*t3 = p3x + v3x*t3
    // p0y + v0y*t1 = p1y + v1y*t1
    // p0y + v0y*t2 = p2y + v2y*t2
    // p0y + v0y*t3 = p3y + v3y*t3
    // p0z + v0z*t1 = p1z + v1z*t1
    // p0z + v0z*t2 = p2z + v2z*t2
    // p0z + v0z*t3 = p3z + v3z*t3

    // Example data:
    // 19, 13, 30 @ -2,  1, -2
    // 18, 19, 22 @ -1, -1, -2
    // 20, 25, 34 @ -2, -2, -4
    // 12, 31, 28 @ -1, -2, -1
    // 20, 19, 15 @  1, -5, -3

    // Example equations:
    // p0x + v0xt1 = 19 + -2t1
    // p0x + v0xt2 = 18 + -1t2
    // p0x + v0xt3 = 20 + -2t3

    // p0y + v0yt1 = 13 + 1t1
    // p0y + v0yt2 = 19 + -1t2
    // p0y + v0yt3 = 25 + -2t3

    // p0z + v0zt1 = 30 + -2t1
    // p0z + v0zt2 = 22 + -2t2
    // p0z + v0zt3 = 34 + -4t3


    var h1 = input[0];
    var h2 = input[1];
    var h3 = input[2];

    // Make Z3 solver solve this system of equations
    var ctx = new Context();

    var t1 = ctx.MkRealConst("t1");
    var t2 = ctx.MkRealConst("t2");
    var t3 = ctx.MkRealConst("t3");

    var p0x = ctx.MkRealConst("p0x");
    var p0y = ctx.MkRealConst("p0y");
    var p0z = ctx.MkRealConst("p0z");
    var v0x = ctx.MkRealConst("v0x");
    var v0y = ctx.MkRealConst("v0y");
    var v0z = ctx.MkRealConst("v0z");


    var p1x = ctx.MkReal((long)h1.position.x);
    var p1y = ctx.MkReal((long)h1.position.y);
    var p1z = ctx.MkReal((long)h1.position.z);
    var v1x = ctx.MkReal((long)h1.velocity.x);
    var v1y = ctx.MkReal((long)h1.velocity.y);
    var v1z = ctx.MkReal((long)h1.velocity.z);

    var p2x = ctx.MkReal((long)h2.position.x);
    var p2y = ctx.MkReal((long)h2.position.y);
    var p2z = ctx.MkReal((long)h2.position.z);
    var v2x = ctx.MkReal((long)h2.velocity.x);
    var v2y = ctx.MkReal((long)h2.velocity.y);
    var v2z = ctx.MkReal((long)h2.velocity.z);

    var p3x = ctx.MkReal((long)h3.position.x);
    var p3y = ctx.MkReal((long)h3.position.y);
    var p3z = ctx.MkReal((long)h3.position.z);
    var v3x = ctx.MkReal((long)h3.velocity.x);
    var v3y = ctx.MkReal((long)h3.velocity.y);
    var v3z = ctx.MkReal((long)h3.velocity.z);

    // Make sure the time is positive
    var t1Positive = ctx.MkGt(t1, ctx.MkReal(0));
    var t2Positive = ctx.MkGt(t2, ctx.MkReal(0));
    var t3Positive = ctx.MkGt(t3, ctx.MkReal(0));

    // Create the equations
    var p0xPlusV0xt1 = ctx.MkEq(ctx.MkAdd(p0x, ctx.MkMul(v0x, t1)), ctx.MkAdd(p1x, ctx.MkMul(v1x, t1)));
    var p0xPlusV0xt2 = ctx.MkEq(ctx.MkAdd(p0x, ctx.MkMul(v0x, t2)), ctx.MkAdd(p2x, ctx.MkMul(v2x, t2)));
    var p0xPlusV0xt3 = ctx.MkEq(ctx.MkAdd(p0x, ctx.MkMul(v0x, t3)), ctx.MkAdd(p3x, ctx.MkMul(v3x, t3)));

    var p0yPlusV0yt1 = ctx.MkEq(ctx.MkAdd(p0y, ctx.MkMul(v0y, t1)), ctx.MkAdd(p1y, ctx.MkMul(v1y, t1)));
    var p0yPlusV0yt2 = ctx.MkEq(ctx.MkAdd(p0y, ctx.MkMul(v0y, t2)), ctx.MkAdd(p2y, ctx.MkMul(v2y, t2)));
    var p0yPlusV0yt3 = ctx.MkEq(ctx.MkAdd(p0y, ctx.MkMul(v0y, t3)), ctx.MkAdd(p3y, ctx.MkMul(v3y, t3)));

    var p0zPlusV0zt1 = ctx.MkEq(ctx.MkAdd(p0z, ctx.MkMul(v0z, t1)), ctx.MkAdd(p1z, ctx.MkMul(v1z, t1)));
    var p0zPlusV0zt2 = ctx.MkEq(ctx.MkAdd(p0z, ctx.MkMul(v0z, t2)), ctx.MkAdd(p2z, ctx.MkMul(v2z, t2)));
    var p0zPlusV0zt3 = ctx.MkEq(ctx.MkAdd(p0z, ctx.MkMul(v0z, t3)), ctx.MkAdd(p3z, ctx.MkMul(v3z, t3)));

    // Create the solver
    var solver = ctx.MkSolver();

    // Add the equations to the solver
    solver.Assert(t1Positive);
    solver.Assert(t2Positive);
    solver.Assert(t3Positive);

    solver.Assert(p0xPlusV0xt1);
    solver.Assert(p0xPlusV0xt2);
    solver.Assert(p0xPlusV0xt3);

    solver.Assert(p0yPlusV0yt1);
    solver.Assert(p0yPlusV0yt2);
    solver.Assert(p0yPlusV0yt3);

    solver.Assert(p0zPlusV0zt1);
    solver.Assert(p0zPlusV0zt2);
    solver.Assert(p0zPlusV0zt3);

    // Check if the solver is satisfiable
    var result = solver.Check();

    // If the solver is satisfiable, get the model
    if (result != Status.SATISFIABLE)
    {
        Console.WriteLine("The solver is not satisfiable");
        return -1;
    }
    var model = solver.Model;

    // We already know p1x,p1y,p1z,p2x,p2y,p2z,p3x,p3y,p3z
    // We need to find t1,t2,t3,p0x,p0y,p0z,v0x,v0y,v0z

    // Get the values from the model
    var t1Value = model.Eval(t1).ToString();
    var t2Value = model.Eval(t2).ToString();
    var t3Value = model.Eval(t3).ToString();

    var p0xValue = model.Eval(p0x).ToString();
    var p0yValue = model.Eval(p0y).ToString();
    var p0zValue = model.Eval(p0z).ToString();

    var v0xValue = model.Eval(v0x).ToString();
    var v0yValue = model.Eval(v0y).ToString();
    var v0zValue = model.Eval(v0z).ToString();

    return BigInteger.Parse(p0xValue) + BigInteger.Parse(p0yValue) + BigInteger.Parse(p0zValue);

}

List<Hailstone> parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line => line
            .Split(" @ ")
            .Let(parts =>
                parts
                .Select(part =>
                    part
                    .Split(", ").Select(BigInteger.Parse)
                    .ToArray()
                    .Let(points => new Vector3(points[0], points[1], points[2]))
                )
            )
            .ToArray()
            .Let(vectors => new Hailstone(vectors[0], vectors[1]))
        ).ToList();
}




record Vector3(BigInteger x, BigInteger y, BigInteger z)
{
    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Vector3 operator *(Vector3 a, BigInteger b) => new Vector3(a.x * b, a.y * b, a.z * b);
}
record Hailstone(Vector3 position, Vector3 velocity)
{

    public (double, double)? LineIntersectionXYPlane(Hailstone other)
    {
        // a1x1 + b1y1 + c1 = 0
        // a2x2 + b2y2 + c2 = 0
        var a1 = (double)this.velocity.y;
        var b1 = (double)-this.velocity.x;
        var c1 = (double)(this.velocity.x * this.position.y - this.velocity.y * this.position.x);

        var a2 = (double)other.velocity.y;
        var b2 = (double)-other.velocity.x;
        var c2 = (double)(other.velocity.x * other.position.y - other.velocity.y * other.position.x);

        var div = (a1 * b2 - a2 * b1);
        if (div == 0)
        {
            return null;
        }

        var x = (c2 * b1 - c1 * b2) / (a1 * b2 - a2 * b1);
        var y = (a2 * c1 - a1 * c2) / (a1 * b2 - a2 * b1);

        return ((double)x, (double)y);
    }
}

