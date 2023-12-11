using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Network input)
{
    var i = 0;
    var curNode = input.nodes["AAA"];
    while (true)
    {
        var direction = input.directions[i % input.directions.Length];
        if (direction == 'L')
        {
            if (curNode.left == "ZZZ")
            {
                return i + 1;
            }
            curNode = input.nodes[curNode.left];
        }
        else
        {
            if (curNode.right == "ZZZ")
            {
                return i + 1;
            }
            curNode = input.nodes[curNode.right];
        }

        i += 1;
    }

}

long part02(Network input)
{
    var curNodes = input.nodes.Keys.Where(key => key.EndsWith("A")).ToList()!;
    var steps = new List<int>();

    for (var j = 0; j < curNodes.Count(); j++)
    {
        var i = 0;
        var curNode = curNodes[j];
        var visited = new Dict<String, int>();
        while (true)
        {

            if (curNode.EndsWith("Z"))
            {
                if (visited.ContainsKey(curNode))
                {
                    break;
                }
                visited.Add(curNode, i);
            }


            var direction = input.directions[i % input.directions.Length];
            if (direction == 'L')
            {
                curNode = input.nodes[curNode].left;
            }
            else
            {
                curNode = input.nodes[curNode].right;
            }
            i += 1;
        }
        steps.Add(visited[curNode]);
        Console.WriteLine($"CurNode: {curNodes[j]} - {visited[curNode]}");
    }

    // var s = new long[] { 2, 3, 4 };
    var s = steps.Select(step => (long)step).ToArray();
    s.Order();
    // long jump = gcd(s.Take(2).ToArray());
    // Console.WriteLine($"GCD: {jump}");
    Console.WriteLine(LCMAll(s));
    // var jump = s[1];
    // for (var k = 1; k < s.Length; k++)
    // {
    //     // var visisted = new HashSet<long>();

    //     var n1 = s[k];
    //     // var n2 = s[k + 1];
    //     while (n1 % s[k - 1] != 0)
    //     {
    //         n1 += jump;
    //     }
    //     jump = n1 / s[k];

    //     Console.WriteLine($"n1: {n1}");
    // }

    // while (!s.All(step => step == s[0]))
    // {
    //     k++;
    //     for (var i = 0; i < s.Length; i++)
    //     {
    //         s[i] += s[i];
    //     }
    // }

    // Console.WriteLine(jump);

    return -1;
}

long gcd(long[] nums)
{
    var smallest = nums.Min();
    while (!nums.All(n => n % smallest == 0))
    {
        smallest--;
    }
    return smallest;
}

long GCD(long a, long b)
{
    if (a == 0)
        return b;
    return GCD(b % a, a);
}

long LCM(long a, long b, long gcd)
{
    return Math.Abs(a * b) / gcd;
}

long LCMAll(long[] numbers)
{
    long lcm = numbers[0];
    for (int i = 1; i < numbers.Length; i++)
    {
        long gcd = GCD(lcm, numbers[i]);
        lcm = LCM(lcm, numbers[i], gcd);
    }

    return lcm;
}

Network parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);

    var nodes = lines.Skip(2).Select(line =>
    {
        return line.Split(" = ").Let(comps => (comps[0], comps[1].Substring(1, comps[1].Length - 2).Split(", ")));
    })
    .ToDictionary(g => g.Item1, g => new Node(g.Item2[0], g.Item2[1]))
    .ToDict();
    return new Network(lines[0], nodes);
}

record Node(String left, String right);


record Network(String directions, Dict<String, Node> nodes);