using System;
using System.Collections.Generic;
using System.Linq;

class Solver
{
    public Dictionary<string, Valve> Valves { get; }
    public Dictionary<string, Lazy<Dictionary<string, IList<string>>>> routingDict;

    public Solver(IEnumerable<Valve> _valves)
    {
        Valves = _valves.ToDictionary(v => v.Name);

        routingDict = new();
        foreach (var name in Valves.Keys)
            routingDict.Add(name, new Lazy<Dictionary<string, IList<string>>>(() => DestDirections(name)));
    }

    /*
    public IEnumerable<ScorePath> FindAllPaths(string start, int maxSteps)
    {
        var valvesToOpen = Valves.Values.Where(v => v.Flow > 0).ToList();
        var paths = ExpandPaths(start, Path.Empty(), 0, valvesToOpen, maxSteps);
        return paths.Select(p => ScorePath.From(p, maxSteps, Valves));        
    }
    */

    public IEnumerable<ScorePath> FindAllPaths(string start, int maxSteps)
    {
        var moves = Valves[start].Neighbors.Select(n => Move.Step(n.Name));
        var nextPaths = moves.Select(m => ScorePath.Empty(maxSteps).AddMove(m, Valves)).ToList();
        return ExpandPathsFiltered(1, maxSteps, nextPaths);
    }

    public List<ScorePath> ExpandPathsFiltered(int i, int maxSteps, List<ScorePath> paths)
    {
        Console.WriteLine($"> Expanding level {i + 1}");

        if (i == maxSteps)
            return paths;

        var filteredPaths = WhereSubPathIsOptimal(i, paths);

        var longerPaths = new List<ScorePath>();

        foreach (var path in filteredPaths)
        {
            var moves = path.NextMoves(Valves);
            var nextPaths = moves.Select(m => path.AddMove(m, Valves));

            longerPaths.AddRange(nextPaths);
        }

        Console.WriteLine($"  > Num paths: {longerPaths.Count}");
        return ExpandPathsFiltered(i + 1, maxSteps, longerPaths);
    }

    public IEnumerable<ScorePath> WhereSubPathIsOptimal(int i, List<ScorePath> paths)
    {
        if (i == 10 || i == 15 || i == 20 || i == 25)
            return paths.OrderByDescending(p => p.Score).Take(1000);

        var result = new List<ScorePath>();
        foreach (var group in paths.GroupBy(p => p.FilterKey))        
            result.Add(group.MaxBy(p => p.Score));

        return result;
    }

    public List<Path> ExpandPaths(string currentLocation, Path currPath, int numSteps, IEnumerable<Valve> closedValves, int maxSteps)
    {
        var result = new List<Path>();

        foreach (var closedValve in closedValves)
        {
            var steps = GetSteps(currentLocation, closedValve.Name);
            var numStepsTotal = numSteps + steps.Count + 1; // Include opening the destination value

            if (numStepsTotal > maxSteps)
            {
                result.Add(currPath);
            }
            else
            {
                var nextPath = currPath.AddSteps(steps, openLast: true);

                var remainingValves = closedValves.Where(v => v != closedValve);
                if (remainingValves.Any())
                    result.AddRange(ExpandPaths(nextPath.Dest, nextPath, numStepsTotal, remainingValves, maxSteps));
                else
                    result.Add(nextPath);
            }
        }

        return result;
    }




    public Dictionary<string, IList<string>> DestDirections(string start)
    {
        Console.WriteLine($"> calculcating destinations from {start}.");

        Dictionary<string, IList<string>> destMap = new();

        void SaveValveToDict(Valve valve, Path path) =>
            destMap.Add(valve.Name, path.Steps.Select(tup => tup.Item1).ToList());

        VisitBFS(start, SaveValveToDict);

        return destMap;
    }

    public IList<string> GetSteps(string from, string to) => routingDict[from].Value[to];

    public IEnumerable<(string, ScorePath)> Solve(string start, int numSteps, int maxSteps)
    {
        var n = 1;
        var firstLevel = Valves[start].Neighbors
            .Select(n => new ScorePath(new List<(string, bool)> { (n.Name, false) }, 0, maxSteps));

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

                var nextValvesWithPath = curr.Neighbors.Select(n => (n, path.AddValveWithoutOpening(n, Valves)));
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
            foreach (var (curr, path) in currLevel)
            {
                if (!visited.Contains(curr))
                {
                    valveAction(curr, path);
                    visited.Add(curr);
                }

                var nextValvesWithPath = curr.Neighbors.Select(n => (n, path.AddValveWithoutOpening(n, Valves)));
                nextLevel.AddRange(nextValvesWithPath);
            }

            n += 1;
            currLevel = new List<(Valve, Path)>(nextLevel);
            nextLevel.Clear();
        }
    }
}