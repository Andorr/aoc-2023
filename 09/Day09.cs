using utils;
var input = parse("input.txt");
// Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Input input)
{
    var diffs = input.nums.Select(seq => Sequence.DiffUntilAllZeros(seq.nums)).ToList();
    var extrapolated = diffs.Select(Sequence.Extrapolate).ToList();
    return extrapolated.Select(seqs => seqs[0][^1]).Sum();
}

long part02(Input input)
{
    var diffs = input.nums.Select(seq => Sequence.DiffUntilAllZeros(seq.nums)).ToList();
    var extrapolated = diffs.Select(Sequence.ExtrapolateBackwards).ToList();
    return extrapolated.Select(seqs => seqs[0][0]).Sum();
}

Input parse(string fileName)
{
    return File.ReadAllLines(fileName)
        .Select(line => line.Split(" ").Select(long.Parse).ToList())
        .ToList()
        .Let(sequences => new Input(sequences.Select(nums => new Sequence(nums)).ToList()));
}

record Sequence(List<long> nums)
{
    public long this[int index] => nums[index];

    public static List<List<long>> ExtrapolateBackwards(List<List<long>> diffs)
    {
        // Add a zero to the last diff
        diffs[^1].Insert(0, 0);

        for (var j = diffs.Count - 2; j >= 0; j--)
        {
            // c = a - b => a = c + b
            diffs[j].Insert(0, diffs[j][0] - diffs[j + 1][0]);
        }
        return diffs;
    }

    public static List<List<long>> Extrapolate(List<List<long>> diffs)
    {
        // Add a zero to the last diff
        diffs[^1].Add(0);

        for (var j = diffs.Count - 2; j >= 0; j--)
        {
            // c = a - b => b = a - c
            diffs[j].Add(diffs[j][^1] + diffs[j + 1][^1]);
        }
        return diffs;
    }

    public static List<List<long>> DiffUntilAllZeros(List<long> nums)
    {
        var diffs = new List<List<long>>
        {
            nums
        };
        var diff = Diff(nums);
        diffs.Add(diff.ToList());
        while (diff.Any(d => d != 0))
        {
            diff = Diff(diff);
            diffs.Add(diff.ToList());
        }
        return diffs;
    }

    public static List<long> Diff(List<long> nums)
    {
        var diff = Enumerable.Repeat(0L, nums.Count() - 1).Select(e => 0L).ToList();
        for (var i = 0; i < nums.Count() - 1; i++)
        {
            diff[i] = nums[i + 1] - nums[i];
        }
        return diff;
    }
}

record Input(List<Sequence> nums)
{
}