using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string start = "AA";
    static int maxSteps = 26;

    // "DD,!CC,BB,!AA,!II,JJ,!II,!AA,!DD,!EE,!FF,!GG,HH,!GG,!FF,EE,!DD,CC"

    public static void Main(string[] args)
    {
        var valves = ReadValves("Input.txt");

        foreach (var valve in valves)
            Console.WriteLine(valve);

        Console.WriteLine();

        var solver = new Solver(valves);

        
        var paths = solver.FindAllPaths(start, maxSteps);

        var bestScore = 0;
        var bestPath = ScorePath.Empty(maxSteps);

        foreach (var path in paths)
        {
            if (path.Score > bestScore)
            {
                bestScore = path.Score;
                bestPath = path;
            }
        }
        
        Console.WriteLine($"\nBest score: {bestScore}");
        Console.WriteLine($"\nBest path: {bestPath}");
        

        /*
        var path = ScorePath.Empty(maxSteps);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 1 ==");
        path = path.AddMove(Move.Step("II"), Move.Step("DD"), solver.Valves);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 2 ==");
        path = path.AddMove(Move.Step("JJ"), Move.Open, solver.Valves);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 3 ==");
        path = path.AddMove(Move.Open, Move.Step("EE"), solver.Valves);
        Console.WriteLine(path+"\n");  

        Console.WriteLine("== Minute 4 ==");
        path = path.AddMove(Move.Step("II"), Move.Step("FF"), solver.Valves);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 5 ==");
        path = path.AddMove(Move.Step("AA"), Move.Step("GG"), solver.Valves);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 6 ==");
        path = path.AddMove(Move.Step("BB"), Move.Step("HH"), solver.Valves);
        Console.WriteLine(path+"\n");

        Console.WriteLine("== Minute 7 ==");
        path = path.AddMove(Move.Open, Move.Open, solver.Valves);
        Console.WriteLine(path+"\n"); 

        Console.WriteLine("== Minute 8 ==");
        path = path.AddMove(Move.Step("CC"), Move.Step("GG"), solver.Valves);
        Console.WriteLine(path+"\n"); 

        Console.WriteLine("== Minute 9 ==");
        path = path.AddMove(Move.Open, Move.Step("FF"), solver.Valves);
        Console.WriteLine(path+"\n"); 

        Console.WriteLine("== Minute 10 ==");
        path = path.AddMove(Move.Step("BB"), Move.Step("EE"), solver.Valves);
        Console.WriteLine(path+"\n"); 

        Console.WriteLine("== Minute 11 ==");
        path = path.AddMove(Move.Step("CC"), Move.Open, solver.Valves);
        Console.WriteLine(path+"\n"); 
        */
        
    }

    static void ShowSteps(string from, string to, Solver solver)
    {
        var steps = solver.GetSteps(from, to);
        Console.WriteLine($"{from}->{to}: [{string.Join(",", steps)}]");
        Console.WriteLine();
    }

    static List<Valve> ReadValves(string fileName)
    {
        List<Valve> allValves = new();

        foreach (var line in File.ReadLines(fileName))
            allValves.Add(Valve.From(line));

        foreach (var valve in allValves)
            valve.ConnectNeighbors(allValves);

        return allValves
            .OrderBy(v => v.Name)
            .ToList();
    }

    /*
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
    */
}