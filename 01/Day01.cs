var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(List<string> input)
{
    return input.Select(s =>
    {
        return int.Parse($"{s.First(char.IsNumber)}{s.Last(char.IsNumber)}");
    })
    .Sum();
}

long part02(List<string> input)
{
    var namedDigits = new List<string>{
        "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"
    };
    return input.Select(s =>
    {
        var digits = new List<int>();
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (char.IsDigit(c))
            {
                digits.Add(c - '0');
                continue;
            }

            for (var j = 0; j < namedDigits.Count; j++)
            {
                if (i + namedDigits[j].Length <= s.Length && s.Substring(i, namedDigits[j].Length) == namedDigits[j])
                {
                    digits.Add(j + 1);
                    break;
                }
            }
        }
        return digits.First() * 10 + digits.Last();
    })
    .Sum();
}

List<string> parse(string fileName)
{
    return File.ReadAllLines(fileName).ToList();
}

