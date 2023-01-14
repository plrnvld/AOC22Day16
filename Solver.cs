using System;
using System.Collections.Generic;
using System.Linq;

class Solver
{
    public Dictionary<string, Valve> Valves { get; }

    public Solver(IEnumerable<Valve> _valves)
    {
        Valves = _valves.ToDictionary(v => v.Name);
    }

    public Dictionary<string, IEnumerable<string>> DestDirections(string start)
    {
        Dictionary<string, IEnumerable<string>> destMap = new();

        void SaveValveToDict(Valve valve, Path path) =>
            destMap.Add(valve.Name, path.Steps.Select(tup => tup.Item1));
        
        VisitBFS(start, SaveValveToDict);

        return destMap;
    }
    

    public IEnumerable<(string, ScorePath)> Solve(string start, int numSteps, int maxSteps)
    {
        var n = 1;
        var firstLevel = Valves[start].Neighbors
            .Select(t => new ScorePath(new List<(string, bool)> { (t.To.Name, false) }, 0, maxSteps));

        var groups = firstLevel.GroupBy(p => p.Dest);

        var nextLevel = groups.Select(group => (group.Key, group.MinBy(p => p.Score)));

        while (n < numSteps)
        {
            nextLevel = SolveNext(nextLevel);
            n += 1;
        }

        return nextLevel;
    }

    IEnumerable<(string, ScorePath)> SolveNext(IEnumerable<(string name, ScorePath path)> currPaths)
    {
        var nextPaths = currPaths.SelectMany(t =>
            {
                var nextMoves = t.path.NextMoves(Valves);
                return nextMoves.Select(m => t.path.AddMove(m, Valves));
            });

        var groups = nextPaths.GroupBy(p => p.Dest);

        foreach (var group in groups)
            yield return (group.Key, group.MaxBy(p => p.Score));
    }

    public IEnumerable<Path> GetPathsFrom(string start, int numSteps, int maxPathLength)
    {
        var currLevel = new List<(Valve, Path)> { (Valves[start], Path.Empty()) };
        var nextLevel = new List<(Valve, Path)>();

        foreach (var (valve, path) in currLevel)
        {
            // ##################
            
        }

        return Enumerable.Empty<Path>();
    }

    public void VisitBFS(string start, Action<Valve, ScorePath> valveAction, int maxSteps)
    {
        var currLevel = new List<(Valve, ScorePath)> { (Valves[start], ScorePath.Empty(maxSteps)) };
        var nextLevel = new List<(Valve, ScorePath)>();
        var visited = new List<Valve>();

        var n = 0;

        while (visited.Count < Valves.Count)
        {
            Console.WriteLine($"> Visit level {n}, num visited is {visited.Count}");

            foreach (var (curr, path) in currLevel)
            {
                if (!visited.Contains(curr))
                {
                    valveAction(curr, path);
                    visited.Add(curr);
                }

                var nextValvesWithPath = curr.Neighbors.Select(t => (t.To, path.AddValveWithoutOpening(t.To, Valves)));
                nextLevel.AddRange(nextValvesWithPath);
            }

            n += 1;
            currLevel = new List<(Valve, ScorePath)>(nextLevel);
            nextLevel.Clear();
        }
    }

    public void VisitBFS(string start, Action<Valve, Path> valveAction)
    {
        var currLevel = new List<(Valve, Path)> { (Valves[start], Path.Empty()) };
        var nextLevel = new List<(Valve, Path)>();
        var visited = new List<Valve>();

        var n = 0;

        while (visited.Count < Valves.Count)
        {
            Console.WriteLine($"> Visit level {n}, num visited is {visited.Count}");

            foreach (var (curr, path) in currLevel)
            {
                if (!visited.Contains(curr))
                {
                    valveAction(curr, path);
                    visited.Add(curr);
                }

                var nextValvesWithPath = curr.Neighbors.Select(t => (t.To, path.AddValveWithoutOpening(t.To, Valves)));
                nextLevel.AddRange(nextValvesWithPath);
            }

            n += 1;
            currLevel = new List<(Valve, Path)>(nextLevel);
            nextLevel.Clear();
        }
    }
}