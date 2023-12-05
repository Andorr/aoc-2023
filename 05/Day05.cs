using System.Diagnostics;
using System.Text.RegularExpressions;
using utils;
var input = parse("input.txt");
Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");


long part01(Almanac input) => input.seeds
    .Select(seed => input.mappers.Aggregate(seed, (agg, mapper) => mapper.Map(agg)))
    .Min();

long part02(Almanac input)
{
    var seedRanges = input.seeds.Chunk(2).Select(chunk => new Range(chunk[0], chunk[1]));
    input.mappers.Reverse();

    return Enumerable.Range(0, int.MaxValue)
        .First((location) =>
        {
            var seed = input.mappers.Aggregate((long)location, (agg, mapper) => mapper.MapReverse(agg));
            return seedRanges.Any(r => r.min <= seed && seed < r.min + r.cnt);
        });
}

Almanac parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);
    var seeds = Regex.Matches(lines[0], @"(\d+)").Select(m => long.Parse(m.Value)).ToLst();
    var maps = new List<List<Map>>();

    var i = 1;
    while (i < lines.Length)
    {
        var mappers = lines.Skip(i + 2).TakeWhile(line => line.Length > 0 && char.IsDigit(line[0])).ToLst();
        var map = Regex.Matches(string.Join(Environment.NewLine, mappers), @"(\d+)", RegexOptions.Multiline)
            .Select(m => long.Parse(m.Value))
            .Chunk(3)
            .Select(chunk => new Map(new Range(chunk[0], chunk[2]), new Range(chunk[1], chunk[2])))
            .ToLst();
        maps.Add(map);

        i += mappers.Count + 2;
    }

    return new Almanac(seeds, maps.Select((m) => new Mapper(m)).ToList());
}


record Almanac(List<long> seeds, List<Mapper> mappers);
record Range(long min, long cnt);
record Map(Range dest, Range src);
record Mapper(List<Map> maps)
{
    public long Map(long value)
    {
        var map = maps.FirstOrDefault(m => m.src.min <= value && value < m.src.min + m.src.cnt);
        return (map?.dest.min ?? 0) + (value - (map?.src.min ?? 0));
    }

    public long MapReverse(long value)
    {
        var map = maps.FirstOrDefault(m => m.dest.min <= value && value < m.dest.min + m.dest.cnt);
        return (map?.src.min ?? 0) + (value - (map?.dest.min ?? 0));
    }
}
