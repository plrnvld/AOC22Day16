using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string start = "AA";
    static IList<string> exampleSteps = new[] { "DD", "!CC", "BB", "AA", "II", "JJ", "II", "AA", "DD", "!EE", "FF", "GG", "HH", "GG", "FF", "EE", "DD", "CC" }.ToList();

    static IList<string> someAnswer = new[] { "!GC", "!IR", "!JZ", "LY","!EF","WL","EJ","CP","!ZK","!IV","UC","!JD","!IZ","SS","!WI","IR","!WG","LL","!SK","DD","!YY" }.ToList();

    public static void Main(string[] args)
    {
        var valves = ReadValves("Example.txt", clearClosed: false);

        foreach (var valve in valves)
            Console.WriteLine(valve);

        Console.WriteLine();

        var solver = new Solver(valves);
        var result = solver.Solve(start, 30, 30);

        Console.WriteLine();
        
        var bestScore = 0;
        
        foreach (var (name, path) in result)
        {
            Console.WriteLine($"Valve {name} with {path}");
            if (path.Score > bestScore)
                bestScore = path.Score;            
        }

        Console.WriteLine();
        Console.WriteLine($"Best score is {bestScore}");
        
        // var score = CalculateScore(start, someAnswer, valves);
        // Console.WriteLine();
        // Console.WriteLine($"Score={score}");
    }

    static List<Valve> ReadValves(string fileName, bool clearClosed)
    {
        List<Valve> allValves = new();

        foreach (var line in File.ReadLines(fileName))
            allValves.Add(Valve.From(line));

        foreach (var valve in allValves)
            valve.ConnectNeighbors(allValves);

        var result = clearClosed
            ? ClearClosedValves(allValves)
            : allValves;

        return result
            .OrderBy(v => v.Name)
            .ToList();
    }

    static IEnumerable<Valve> ClearClosedValves(IEnumerable<Valve> valves)
    {
        var clearedValves = new List<Valve>();

        foreach (var valve in valves)
        {
            if (valve.Name == "AA" || valve.Flow > 0)
                clearedValves.Add(valve);
            else
                RewireAroundClosedValve(valve);
        }

        return clearedValves;
    }

    static void RewireAroundClosedValve(Valve valve)
    {
        var outgoing = valve.Neighbors;
        var incoming = outgoing.SelectMany(t => t.To.Neighbors).Where(t => t.To == valve).ToList();

        foreach (var oldIncoming in incoming)
        {
            var outgoingToDifferentPlace = outgoing.Where(t => t.To != oldIncoming.From);
            foreach (var oldOutgoing in outgoingToDifferentPlace)
            {
                var newTunnel = Combine(oldIncoming, oldOutgoing);
                oldIncoming.From.Neighbors.Remove(oldIncoming);
                oldIncoming.From.Neighbors.Add(newTunnel);
            }
        }
    }

    static Tunnel Combine(Tunnel t1, Tunnel t2)
    {
        if (t1.To != t2.From)
            throw new Exception($"Tunnel {t1} and {t2} don't connect");

        return new Tunnel(t1.From, t2.To, t1.Steps + t2.Steps);
    }

    static int CalculateScore(string start, IList<string> steps, IEnumerable<Valve> allValves)
    {
        const int maxSteps = 30;
        int score = 0;

        var curr = FindValve(start, allValves);

        var numSteps = 0;

        foreach (var step in steps)
        {
            numSteps += 1;

            var shouldOpen = !step.StartsWith("!");
            var next = FindValve(step, allValves);
            var isOpen = next.IsOpen;

            Console.WriteLine();
            Console.WriteLine($"== Minute {numSteps} ==");
            Console.WriteLine($"You move to {step}.");

            if (!isOpen && shouldOpen)
            {
                numSteps += 1;

                next.IsOpen = true;

                Console.WriteLine();
                Console.WriteLine($"== Minute {numSteps} ==");
                Console.WriteLine($"You open valve {step}.");

                var remainingSteps = maxSteps - numSteps;
                if (numSteps <= maxSteps)
                    score += next.Flow * remainingSteps;
                else
                    Console.WriteLine($"Stopped counting: num steps = {numSteps} > max steps = {maxSteps}");
            }

            curr = next;
        }

        return score;
    }

    static Valve FindValve(string step, IEnumerable<Valve> allValves)
    {
        var name = step.StartsWith("!") ? step.Substring(1) : step;
        return allValves.FirstOrDefault(v => v.Name == name) ?? throw new Exception($"Valve {name} not found");
    }
}

class Solver
{
    Dictionary<string, Valve> valves;

    public Solver(IEnumerable<Valve> _valves)
    {
        this.valves = _valves.ToDictionary(v => v.Name);
    }

    public IEnumerable<(string, Path)> Solve(string start, int numSteps, int maxSteps)
    {
        var n = 1;
        var firstLevel = valves[start].Neighbors.Select(t => new Path(new List<(string, bool)> { (t.To.Name, false) }, 0));

        var groups = firstLevel.GroupBy(p => p.Dest);

        var nextLevel = groups.Select(group => (group.Key, group.MinBy(p => p.Score)));

        while (n < numSteps)
        {
            nextLevel = SolveNext(nextLevel, maxSteps - n);
            n += 1;
        }

        return nextLevel;
    }

    IEnumerable<(string, Path)> SolveNext(IEnumerable<(string name, Path path)> currPaths, int remainingSteps)
    {

        var nextPaths = currPaths.SelectMany(t =>
            {
                var nextMoves = t.path.NextMoves(valves);
                return nextMoves.Select(m => t.path.AddMove(m, remainingSteps, valves));
            });

        var groups = nextPaths.GroupBy(p => p.Dest);

        foreach (var group in groups)
            yield return (group.Key, group.MaxBy(p => p.Score));
    }
}

class Valve
{
    public string Name { get; }
    public int Flow { get; }

    IEnumerable<string> _neighborValves;

    public List<Tunnel> Neighbors = new();
    public bool IsOpen { get; set; }

    public Valve(string name, int flow, IEnumerable<string> neighborValves)
    {
        Name = name;
        Flow = flow;

        IsOpen = false;

        _neighborValves = neighborValves;
    }

    public void ConnectNeighbors(List<Valve> allValves)
    {
        foreach (var neighborName in _neighborValves)
            Neighbors.Add(new Tunnel(this, allValves.First(n => n.Name == neighborName), 1));
    }

    public static Valve From(string valveLine)
    {
        var name = valveLine.Substring(6, 2);
        var flow = int.Parse(string.Concat(valveLine.Substring(23).TakeWhile(c => c >= '0' && c <= '9')));
        var tokens = valveLine.Split(" ").Reverse();
        var neighborValveNames = tokens.TakeWhile(t => !t.StartsWith("valve")).Reverse().Select(t => t.Substring(0, 2));

        return new Valve(name, flow, neighborValveNames);
    }

    public bool ReachableFrom(Valve valve)
        => valve.Neighbors.Any(t => t.To == this);

    public override string ToString()
    {
        var neighbors = string.Join(" / ", Neighbors.Select(t => $"{t.To.Name} (f={t.To.Flow}, s={t.Steps})"));
        return $"Valve {Name}, f={Flow} --> {neighbors}";
    }
}

record class Path(List<(string name, bool isOpen)> Steps, int Score)
{
    public string Dest => Steps[^1].name;

    public Path AddMove(Move move, int remainingSteps, Dictionary<string, Valve> valves)
    {
        var newSteps = new List<(string, bool)>(Steps);

        if (move.IsOpen)
        {
            var (last, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            newSteps.Add((last, true));
            var newScore = Score + (remainingSteps - 1) * valves[last].Flow;
            return new Path(newSteps, newScore);
        }

        var next = move.GetValve(valves);
        newSteps.Add((next.Name, next.IsOpen));
        return new Path(newSteps, Score);
    }

    bool CanOpen(string name) => !Steps.Any(s => s == (name, true));

    public IEnumerable<Move> NextMoves(Dictionary<string, Valve> valves)
    {
        var (last, isOpen) = Steps.Last();
        if (!isOpen && CanOpen(last))
            yield return Move.Open;

        foreach (var t in valves[last].Neighbors)
            yield return Move.Step(t.To.Name);
    }

    public int Size() => Steps.Select(t => (t.isOpen ? 2 : 1)).Sum();

    public override string ToString()
    {
        var steps = string.Join(",", Steps.Select(t => $"{(t.isOpen ? "" : "!")}{t.name}"));
        return $"Path ({Score}, {Size()}): {steps}";
    }
}

record class Tunnel(Valve From, Valve To, int Steps);

struct Move
{
    const string OPEN = "open";
    static Move openMove = new Move(OPEN);
    string content;

    Move(string content) => this.content = content;

    public static Move Open => openMove;

    public static Move Step(string next) => new Move(next);

    public bool IsOpen => content == OPEN;

    public Valve GetValve(Dictionary<string, Valve> valves) => valves[content];
}

// 1832 is too low