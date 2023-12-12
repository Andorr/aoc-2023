using utils;
var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(List<SpringGroup> input)
{
    return input.Select((springGroup, i) =>
        arragements(springGroup.s, springGroup.info)
    ).Sum();
}

long part02(List<SpringGroup> input)
{
    return input
    .Select((springGroup) =>
    {
        return new SpringGroup(
            string.Join("?", Enumerable.Range(0, 5).Select(i => springGroup.s)),
            Enumerable.Range(0, 5).Select(i => springGroup.info).SelectMany(x => x).ToList()
        );
    })
    .Select((springGroup, i) =>
         arragements(springGroup.s, springGroup.info)
    ).Sum();
}

long arragements(String springs, List<int> info, int groupIndex = 0, int i = 0, Dict<(int, int), long>? dp = null)
{
    if (dp == null)
    {
        dp = new Dict<(int, int), long>();
    }

    if (groupIndex >= info.Count)
    {
        return i >= springs.Length || springs.Substring(i).All(c => c != '#') ? 1 : 0;
    }

    long numArragements = 0;

    var groupSize = info[groupIndex];
    while (i + groupSize <= springs.Length && groupIndex < info.Count)
    {
        if (dp.ContainsKey((groupIndex, i)))
        {
            return dp[(groupIndex, i)];
        }

        var sub = springs.Substring(i, groupSize);

        // If i is a possible solution
        if (sub.All(c => c == '#' || c == '?') &&
            (i + groupSize >= springs.Length || springs[i + groupSize] != '#') &&
            (i - 1 < 0 || springs[i - 1] != '#')
        )
        {
            var num = arragements(springs, info, groupIndex + 1, i + groupSize + 1, dp);
            dp[(groupIndex + 1, i + groupSize + 1)] = num;
            numArragements += num;
        }

        if (springs[i] == '#')
        {
            // We can't skip a damaged
            break;
        }

        i++;
    }

    return numArragements;
}

List<SpringGroup> parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line =>
        {
            return line
                .Split(" ")
                .Let(comp =>
                {
                    var s = comp[0];
                    var info = comp[1].Split(",").Select(int.Parse).ToList();
                    return new SpringGroup(s, info);
                });
        })
        .ToList();
}


record SpringGroup(String s, List<int> info);