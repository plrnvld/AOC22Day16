using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string start = "AA";
    static int maxSteps = 30;
    static IList<string> exampleSteps = new[] { "DD", "!CC", "BB", "AA", "II", "JJ", "II", "AA", "DD", "!EE", "FF", "GG", "HH", "GG", "FF", "EE", "DD", "CC" }.ToList();

    // "DD,!CC,BB,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH,!GG,!FF,EE,!DD,CC"

    static IList<string> someAnswer = new[] { "!GC", "!IR", "!JZ", "LY","!EF","WL","EJ","CP","!ZK","!IV","UC","!JD","!IZ","SS","!WI","IR","!WG","LL","!SK","DD","!YY" }.ToList();

    public static void Main(string[] args)
    {
        var valves = ReadValves("Example.txt", clearClosed: false);

        foreach (var valve in valves)
            Console.WriteLine(valve);

        Console.WriteLine();

        var solver = new Solver(valves);        
        var paths = solver.FindAllPaths(start, maxSteps);

        var bestScore = 0;
        
        foreach (var path in paths)
        {
            bestScore = Math.Max(bestScore, path.Score);
            //if (path.Score > 1600)
            //    Console.WriteLine($"Good: {path}");  
        }

        Console.WriteLine($"\nBest score: {bestScore}");

        // Good: ScorePath (1695, 18): BB,CC,DD,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH
        // "DD,!CC,BB,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH,!GG,!FF,EE,!DD,CC"

        /*
        var path = Path.FromAnswer("DD,!CC,BB,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH,!GG,!FF,EE,!DD,CC");
        var scorePath = ScorePath.From(path, maxSteps, solver.Valves);

        Console.WriteLine(scorePath);
        */
    }

    static void ShowSteps(string from, string to, Solver solver)
    {
        var steps = solver.GetSteps(from, to);
        Console.WriteLine($"{from}->{to}: [{string.Join(",", steps)}]");
        Console.WriteLine();
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


// 1832 is too low