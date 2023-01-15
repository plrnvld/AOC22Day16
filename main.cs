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
}

// 2478 too low
// 2513!