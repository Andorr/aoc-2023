
using System.Text.RegularExpressions;
using utils;
var input = parse("input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(List<Card> input)
{
    return input.Select(card =>
    {
        var cnt = card.winning.Intersect(card.numbers).Count();
        return (long)Math.Pow(2, cnt - 1);
    })
    .Sum();
}

long part02(List<Card> input)
{
    var d = input.ToDictionary(g => g.id, g => 1);
    for (int i = 1; i <= input.Count; i++)
    {
        var card = input[i - 1];
        var cnt = card.winning.Intersect(card.numbers).Count();
        for (int j = i + 1; j < i + 1 + cnt; j++)
        {
            d[j] = d.GetValueOrDefault(j, 0) + d[i];
        }
    }
    return d.Values.Sum();
}

List<Card> parse(string fileName)
{
    return File.ReadAllLines(fileName).Select((line, i) =>
    {
        var numberLines = line.Split(":").Last().Split("|")
            .Select(numLine => Regex.Matches(numLine, @"\d+").Select(m => long.Parse(m.Value)).ToLst());
        return new Card(i + 1, numberLines.First(), numberLines.Last());
    }).ToLst();
}


record Card(int id, List<long> winning, List<long> numbers)
{
    public override string ToString() => $"Card {id}: {winning.ToLst()} | {numbers.ToLst()}";
}