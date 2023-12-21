using utils;

var input = parse(args.Length > 0 ? args[0] : "input.txt");
Console.WriteLine($"Part01: {part01(input)}");
Console.WriteLine($"Part02: {part02(input)}");

long part01(Input input)
{
    input.Init();

    var sumLow = 0L;
    var sumHigh = 0L;

    for (var i = 0; i < 1000; i++)
    {
        var (low, high) = input.Push();
        sumLow += low;
        sumHigh += high;
    }
    return sumLow * sumHigh;
}

long part02(Input input)
{
    input.Init();

    var targetModule = "rx";
    targetModule = input.modules.Where(m => m.destination.Contains(targetModule)).First().name;


    // Find the cycles of the modules that eventuelly send to the target module
    var i = 0L;
    var latest = new Dict<string, long>();
    var cycles = new Dict<string, long>();
    var senderToTargetModules = input.modules.Where(m => m.destination.Contains(targetModule)).Select(m => m.name).ToList();
    while (cycles.Count < senderToTargetModules.Count)
    {
        i++;
        input.Push((pulse) =>
        {
            if (senderToTargetModules.Contains(pulse.target) && pulse.low)
            {
                if (latest.ContainsKey(pulse.target) && !cycles.ContainsKey(pulse.target))
                {
                    cycles[pulse.target] = i - latest[pulse.target];
                }
                latest[pulse.target] = i;
            }
            return "";
        });
    }

    // We have the cycles. Since the cycles starts from iteration 1, then we can just simply calculate lcm.
    var values = cycles.Values.ToArray()!;

    long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
    long LCM(long a, long b) => (a * b) / GCD(a, b);

    var lcm = values[0];
    for (var j = 1; j < values.Length; j++)
    {
        lcm = LCM(lcm, values[j]);
    }

    return lcm;
}

Input parse(string fileName)
{
    var lines = File.ReadAllLines(fileName);

    var modules = lines.Where(line => !line.StartsWith("broadcaster"))
        .Select<string, Module>(line =>
        {
            var components = line.Split(" ");

            var first = components[0];
            var isFlipFlop = first[0] == '%';
            var name = first[1..];
            var destinations = components[2..].Select(dest => dest.Replace(",", ""));
            if (isFlipFlop)
            {
                return new FlipFlopModule(name, destinations.ToList());
            }
            else
            {
                return new ConjunctionModule(name, destinations.ToList());
            }
        });

    var broadcaster = lines.First(line => line.StartsWith("broadcaster"))
        .Let(line =>
        {
            var components = line.Split(" ");
            var destinations = components[2..].Select(dest => dest.Replace(",", ""));
            return new Broadcaster(destinations.ToList());
        });

    return new Input(broadcaster, modules.ToList());
}

record Pulse(string target, string sender, bool low)
{
    public override string ToString()
    {
        var lowString = low ? "low" : "high";
        return $"{sender} -{lowString}-> {target}";
    }
}
record Broadcaster(List<String> modules);
abstract record Module(String name, List<string> destination)
{
    private Queue<Pulse> queue = new();

    public Pulse Peek()
    {
        return queue.Peek();
    }

    public void Send(Pulse pulse)
    {
        queue.Enqueue(pulse);
    }

    public bool HasPulses()
    {
        return queue.Count > 0;
    }

    public abstract void Init(List<Module> modules);

    public List<Pulse>? Process()
    {
        if (queue.Count == 0)
        {
            return null;
        }

        var pulse = queue.Dequeue();
        return Process(pulse);
    }

    public abstract List<Pulse>? Process(Pulse pulse);
}

record FlipFlopModule(String name, List<string> destination) : Module(name, destination)
{
    bool on = false;

    public override void Init(List<Module> modules)
    {
        on = false;
    }

    public override List<Pulse>? Process(Pulse pulse)
    {
        if (!pulse.low)
        {
            return null;
        }

        on = !on;

        return destination.Select(dest => new Pulse(dest, name, !on)).ToList();
    }

    public override string ToString()
    {
        return $"FlipFlop({name}) {{On: {on}}}";
    }
}

record ConjunctionModule(String name, List<string> destination) : Module(name, destination)
{
    Dictionary<string, Pulse> recentPulses = new();

    public override void Init(List<Module> inputModules)
    {
        recentPulses.Clear();

        // Get all modules that send to this module
        foreach (var sender in inputModules)
        {
            recentPulses[sender.name] = new Pulse(sender.name, name, true);
        }
    }

    public bool IsAllPulsesHigh()
    {
        return recentPulses.Values.All(p => !p.low);
    }

    public override List<Pulse>? Process(Pulse pulse)
    {
        recentPulses[pulse.sender] = pulse;

        if (recentPulses.Values.All(p => !p.low))
        {
            recentPulses.Clear();
            recentPulses[pulse.sender] = pulse;
            return destination.Select(dest => new Pulse(dest, name, true)).ToList();
        }
        else
        {
            return destination.Select(dest => new Pulse(dest, name, false)).ToList();
        }
    }

    public override string ToString()
    {
        return $"Conjunction({name}) {{Recent: {string.Join(", ", recentPulses.Values)}}}";
    }
}

record Input(Broadcaster broadcaster, List<Module> modules)
{
    public void Init()
    {
        modules.Where(m => m.GetType() == typeof(ConjunctionModule)).ToList().ForEach(m =>
        {
            // Get all modules that send to this module
            var senders = modules.Where(m2 => m2.destination.Contains(m.name));
            m.Init(senders.ToList());
        });

        modules.Where(m => m.GetType() == typeof(FlipFlopModule)).ToList().ForEach(m =>
        {
            m.Init(modules);
        });
    }

    public (long, long) Push(Func<Pulse, string>? onRXPress = null)
    {
        var countLow = 1L;
        var countHigh = 0L;

        var d = modules.ToDictionary(m => m.name, m => m);

        var queue = new Queue<string>();
        foreach (var moduleName in broadcaster.modules)
        {
            var module = d[moduleName];
            var pulse = new Pulse(module.name, "broadcaster", true);
            module.Send(pulse);
            queue.Enqueue(moduleName);
        }
        countLow += modules.Count(m => m.HasPulses());

        while (queue.Count > 0)
        {
            var moduleName = queue.Dequeue();
            var module = d[moduleName];
            var inputPulse = module.Peek();
            var pulses = module.Process();
            onRXPress?.Invoke(inputPulse);
            if (pulses != null)
            {
                foreach (var pulse in pulses)
                {
                    if (d.ContainsKey(pulse.target))
                    {
                        d[pulse.target].Send(pulse);
                        queue.Enqueue(pulse.target);
                    }
                }
                countLow += pulses.Count(p => p.low);
                countHigh += pulses.Count(p => !p.low);
            }
        }

        return (countLow, countHigh);
    }
}
