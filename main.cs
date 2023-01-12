using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string start = "AA";
    static int maxSteps = 30;
    static IList<string> exampleSteps = new[] { "DD", "!CC", "BB", "AA", "II", "JJ", "II", "AA", "DD", "!EE", "FF", "GG", "HH", "GG", "FF", "EE", "DD", "CC" }.ToList();

    static IList<string> someAnswer = new[] { "!GC", "!IR", "!JZ", "LY","!EF","WL","EJ","CP","!ZK","!IV","UC","!JD","!IZ","SS","!WI","IR","!WG","LL","!SK","DD","!YY" }.ToList();

    public static void Main(string[] args)
    {
        var valves = ReadValves("Example.txt", clearClosed: false);

        foreach (var valve in valves)
            Console.WriteLine(valve);

        Console.WriteLine();

        var solver = new Solver(valves);
        var maxBenefit = 0;
        Valve bestValve = null;

        void CheckValve(Valve valve, Path path)
        {
            if (path.CanOpen(valve.Name))
            {
                var benefit = path.Size() == 0 
                    ? (valve.IsOpen ? 0 : valve.Flow * (path.MaxSteps - 1)) 
                    : path.LastValveOpeningBenefit(solver.Valves);

                Console.WriteLine($"Benefit of opening {valve.Name} is {benefit}");
                
                if (benefit > maxBenefit)
                {
                    maxBenefit = benefit;
                    bestValve = valve;
                }                
            }            
        }

        solver.VisitBFS(start, CheckValve, maxSteps);

        Console.WriteLine();
        Console.WriteLine($"Best next valve is {bestValve} with benefit {maxBenefit}");

        // var result = solver.Solve(start, 30, maxSteps);
        //
        // var bestScore = 0;
        
        // foreach (var (name, path) in result)
        // {
        //    Console.WriteLine($"Valve {name} with {path}");
        //    if (path.Score > bestScore)
        //        bestScore = path.Score;            
        //}

        //Console.WriteLine();
        //Console.WriteLine($"Best score is {bestScore}");

        /*
        var scoreToCheck = "DD,!CC,BB,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH,!GG,!FF,EE,!DD,CC,!DD,!EE,FF,!GG,!HH";
        Console.WriteLine(scoreToCheck);
        var steps = scoreToCheck.Split(",");
        var score = CalculateScore(start, steps, valves);
        Console.WriteLine();
        Console.WriteLine($"Score={score}");
        */
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