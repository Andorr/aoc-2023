using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(List<string> input) =>
    input.Select(
        x => x.Aggregate(0L, (agg, c) => hash(agg, c))
    ).Sum();

long part02(List<string> input)
{
    var d = new Dict<long, Box>();
    input.ForEach(x =>
    {
        var id = string.Join("", x.TakeWhile(c => char.IsAsciiLetter(c)));
        var idHash = id.Aggregate(0L, (agg, c) => hash(agg, c));

        d.TryAdd(idHash, new Box(idHash, new List<Lens>()));

        var operation = x.Substring(id.Count(), 1);
        var index = d[idHash].lenses.FindIndex(l => l.id == id);
        switch (operation)
        {
            case "=":
                var value = int.Parse(x.Substring(id.Count() + 1));
                if (index == -1)
                {
                    d[idHash].lenses.Add(new Lens(id, value));
                }
                else
                {
                    d[idHash].lenses[index] = new Lens(id, value);
                }
                break;
            case "-":
                // Remove the lens from the d[idHash]
                if (index != -1)
                {
                    d[idHash].lenses.RemoveAt(index);
                }
                break;
        }

        // Remove the d[idHash] from d if it is empty
        if (d[idHash].lenses.Count == 0)
        {
            d.Remove(idHash);
        }

    });

    return d.Values.Select(b => b.Power()).Sum();
}

long hash(long current, char c) => ((current + c) * 17) % 256;

List<string> parse(string fileName) => File.ReadAllLines(fileName)[0].Split(",").ToList();
record Lens(string id, int value);
record Box(long id, List<Lens> lenses)
{
    public long PowerAt(int index) => (id + 1) * (index + 1) * lenses[index].value;
    public long Power() => lenses.Select((l, i) => PowerAt(i)).Sum();
}


