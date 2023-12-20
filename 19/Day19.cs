using utils;
var input = parse("input.txt");

Console.WriteLine($"Part 01: {part01(input)}");
Console.WriteLine($"Part 02: {part02(input)}");

long part01(Input input)
{
    var acceptedParts = new List<Part>();
    foreach (Part p in input.parts)
    {
        var currentWorkflow = "in";
        while (currentWorkflow != "A" && currentWorkflow != "R")
        {
            var workflow = input.workflows[currentWorkflow];
            for (int i = 0; i < workflow.rules.Count; i++)
            {
                var rule = workflow.rules[i];
                if (rule.condition != null)
                {
                    if (rule.condition.Evaluate(p))
                    {
                        currentWorkflow = rule.exitWorkflow;
                        break;
                    }
                }
                else
                {
                    currentWorkflow = rule.exitWorkflow;
                    break;
                }
            }
        }

        if (currentWorkflow == "A")
        {
            acceptedParts.Add(p);
        }
    }

    return acceptedParts.Sum(p => p.x + p.a + p.m + p.s);
}

long part02(Input input)
{
    const int maxRange = 4000;

    var currentWorkFlow = "in";
    var ranges = new Dictionary<string, PartRanges>();
    ranges.Add(currentWorkFlow, new PartRanges(
        new Range(1, maxRange),
        new Range(1, maxRange),
        new Range(1, maxRange),
        new Range(1, maxRange)
    ));

    var workflows = new Queue<Workflow>();
    workflows.Enqueue(input.workflows[currentWorkFlow]);
    var approvedRanges = new List<PartRanges>();

    while (workflows.Count > 0)
    {
        var workflow = workflows.Dequeue();

        var range = ranges[workflow.name];
        var newRange = null as PartRanges;
        foreach (var rule in workflow.rules)
        {
            newRange = null;
            if (rule.condition != null)
            {
                var condition = rule.condition;
                newRange = range.apply(condition);

                var newCondition = condition.flip();
                range = range.apply(newCondition);
            }

            newRange ??= range;

            if (rule.exitWorkflow == "A")
            {
                approvedRanges.Add(newRange);
            }
            else if (rule.exitWorkflow != "R")
            {
                ranges.Add(rule.exitWorkflow, newRange);
                workflows.Enqueue(input.workflows[rule.exitWorkflow]);
            }
        }
    }

    return approvedRanges
        .Select(parRanges => parRanges.x.count() * parRanges.m.count() * parRanges.a.count() * parRanges.s.count())
        .Sum();
}

Input parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);
    var workflows = lines.TakeWhile(line => !line.StartsWith("{") && !string.IsNullOrWhiteSpace(line))
        .Select(line =>
        {
            var workflowName = string.Join("", line.TakeWhile(c => c != '{'));

            // Substring from workflowName.Length+1 up to the second last character
            var ruleComponents = line[(workflowName.Length + 1)..^1].Split(",");
            var rules = ruleComponents.Select((rule, i) =>
                {
                    if (rule.Contains(":"))
                    {
                        var condition = new Condition(
                            Enum.Parse<Category>(rule[0].ToString()),
                            rule[1] == '<',
                            long.Parse(string.Join("", rule[2..].TakeWhile(c => char.IsDigit(c))))
                        );
                        var exitWorkflow = rule.Split(":").Last();
                        return new Rule(condition, exitWorkflow);
                    }
                    return new Rule(null, exitWorkflow: rule);
                })
                .ToLst();

            return new Workflow(workflowName, rules);
        });

    var parts = lines[(workflows.Count() + 1)..]
    .Select(line =>
    {
        // {x=1167,m=654,a=205,s=508}
        // Parse this to a Part record
        var part = line[1..^1].Split(",")
            .Select(part =>
            {
                var key = part.Split("=")[0];
                var value = long.Parse(part.Split("=")[1]);
                return new KeyValuePair<string, long>(key, value);
            })
            .ToDictionary(v => v.Key, v => v.Value);

        return new Part(part["x"], part["m"], part["a"], part["s"]);
    });

    return new Input(workflows.ToDictionary(
        w => w.name,
        w => w
    ).ToDict(), parts.ToLst());
}

public enum Category
{
    x = 'x',
    m = 'm',
    a = 'a',
    s = 's',
}

public record Range(long min, long max)
{
    public Range restrict(Condition condition)
    {
        var newMin = min;
        var newMax = max;

        if (condition.lessThan)
        {
            newMax = Math.Min(max, condition.value - 1);
            newMin = Math.Min(min, newMax);
        }
        else
        {
            newMin = Math.Max(min, condition.value + 1);
            newMax = Math.Max(max, newMin);
        }

        return new Range(newMin, newMax);
    }

    public long count() => max - min + 1;
}

public record Condition(Category identifier, bool lessThan, long value)
{
    public bool Evaluate(Part p)
    {
        var x = identifier switch
        {
            Category.x => p.x,
            Category.m => p.m,
            Category.a => p.a,
            Category.s => p.s,
            _ => throw new Exception("Invalid identifier")
        };
        return Evaluate(x);
    }
    public bool Evaluate(long x) => lessThan ? x < value : x > value;

    public Condition flip() => new Condition(this.identifier, !this.lessThan, this.value + (this.lessThan ? -1 : 1));
}
public record Rule(Condition? condition, string exitWorkflow);
public record Workflow(string name, Lst<Rule> rules);
public record Part(long x, long m, long a, long s);
public record PartRanges(Range x, Range m, Range a, Range s)
{
    public PartRanges merge(PartRanges o)
    {
        return new PartRanges(
            new Range(
                Math.Max(this.x.min, o.x.min),
                Math.Min(this.x.max, o.x.max)
            ),
            new Range(
                Math.Max(this.m.min, o.m.min),
                Math.Min(this.m.max, o.m.max)
            ),
            new Range(
                Math.Max(this.a.min, o.a.min),
                Math.Min(this.a.max, o.a.max)
            ),
            new Range(
                Math.Max(this.s.min, o.s.min),
                Math.Min(this.s.max, o.s.max)
            )
        );
    }

    public PartRanges apply(Condition condition)
    {
        return condition.identifier switch
        {
            Category.x => new PartRanges(
                this.x.restrict(condition),
                this.m,
                this.a,
                this.s
            ),
            Category.m => new PartRanges(
                this.x,
                this.m.restrict(condition),
                this.a,
                this.s
            ),
            Category.a => new PartRanges(
                this.x,
                this.m,
                this.a.restrict(condition),
                this.s
            ),
            Category.s => new PartRanges(
                this.x,
                this.m,
                this.a,
                this.s.restrict(condition)
            ),
            _ => throw new Exception("Invalid identifier")
        };
    }
}
record Input(Dict<string, Workflow> workflows, Lst<Part> parts);


