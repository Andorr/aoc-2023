using System.Text.RegularExpressions;
using utils;
var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(Input input) => Enumerable.Range(0, input.time.Count)
    .Select(race => numWaysToBeatRecord(input.time[race], input.distance[race]))
    .Aggregate((a, b) => a * b);


long part02(Input input)
{
    var raceTime = long.Parse(input.time.Select(t => t.ToString()).Aggregate((a, b) => $"{a}{b}"));
    var bestDistance = long.Parse(input.distance.Select(d => d.ToString()).Aggregate((a, b) => $"{a}{b}"));

    return numWaysToBeatRecord(raceTime, bestDistance);
}

long numWaysToBeatRecord(long raceTime, long bestDistance)
{
    // x = time
    // speed = x
    // y = speed * time = speed * (raceTime - x) = x * (raceTime - x) = -x^2 + raceTime * x
    // -x^2 + raceTime * x = y => -x^2 + raceTime * x - y = 0
    // x = (-raceTime +- sqrt(raceTime^2 - 4 * -1 * -bestDistance)) / (2 * -1)
    var x = (long)(-raceTime + Math.Sqrt(raceTime * raceTime - 4 * -1 * -bestDistance)) / (2 * -1);
    return raceTime - 2 * x - 1; // Remove the possibilities that are not possible to beat the best distance, -1 because we start at 0
}

Input parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line => Regex.Matches(line, @"(\d+)"))
        .Select(matches => matches.Select(match => int.Parse(match.Value)).ToList())
        .ToList()
        .Let((a) => new Input(a[0], a[1]));
}

record Input(List<int> time, List<int> distance);


