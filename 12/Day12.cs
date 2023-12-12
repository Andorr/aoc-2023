using utils;
var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

const char DAMAGED = '#';
const char UNKNOWN = '?';

long part01(List<SpringGroup> input) => input
    .Select((springGroup, i) =>
        arragements(springGroup.s, springGroup.groupSizes)
    ).Sum();

long part02(List<SpringGroup> input) => input
    .Select((springGroup) => new SpringGroup(
            string.Join("?", Enumerable.Repeat(springGroup.s, 5).ToList()),
            Enumerable.Repeat(springGroup.groupSizes, 5).SelectMany(x => x).ToList()
        )
    )
    .Select((springGroup, i) =>
         arragements(springGroup.s, springGroup.groupSizes)
    ).Sum();

long arragements(string springs, List<int> info, int groupIndex = 0, int i = 0, Dict<(int, int), long>? cache = null)
{
    cache = cache ?? new Dict<(int, int), long>();

    if (groupIndex >= info.Count)
    {
        return i >= springs.Length || springs.Substring(i).All(c => c != DAMAGED) ? 1 : 0;
    }

    long numArragements = 0;
    var groupSize = info[groupIndex];
    while (i + groupSize <= springs.Length)
    {
        if (cache.ContainsKey((groupIndex, i)))
        {
            return cache[(groupIndex, i)];
        }

        // If i is a possible solution
        if (springs.Substring(i, groupSize).All(c => c == DAMAGED || c == UNKNOWN) &&
            (i + groupSize >= springs.Length || springs[i + groupSize] != DAMAGED) &&
            (i - 1 < 0 || springs[i - 1] != DAMAGED)
        )
        {
            var num = arragements(springs, info, groupIndex + 1, i + groupSize + 1, cache);
            cache[(groupIndex + 1, i + groupSize + 1)] = num;
            numArragements += num;
        }

        if (springs[i] == DAMAGED)
        {
            break;
        }

        i++;
    }
    return numArragements;
}

List<SpringGroup> parse(string fileName) => File
    .ReadAllLines(fileName)
    .Select(line => line
        .Split(" ")
        .Let(comp => new SpringGroup(
                comp[0],
                comp[1].Split(",").Select(int.Parse).ToList()
            )
        )
    )
    .ToList();

record SpringGroup(String s, List<int> groupSizes);