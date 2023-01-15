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

    public IEnumerable<ScorePath> FindAllPaths(string start, int maxSteps)
    {
        var moves = Valves[start].Neighbors.Select(n => Move.Step(n.Name));
        var empty = ScorePath.Empty(maxSteps);
        var nextPaths = new List<ScorePath>();
        foreach (var hMove in moves)
            foreach (var eMove in moves)
                nextPaths.Add(empty.AddMove(hMove, eMove, Valves));

        return ExpandPathsFilteredNoRecursion(1, maxSteps, nextPaths);
    }

    public List<ScorePath> ExpandPathsFilteredNoRecursion(int i, int maxSteps, List<ScorePath> paths)
    {
        var pathsWithLevel = new List<ScorePath>(paths);

        while (i + 1 < maxSteps)
        {
            Console.WriteLine($"> Expanding level {i + 1}, max steps = {maxSteps}");
            
            var filteredPaths = WhereSubPathIsOptimal(i, pathsWithLevel);

            var longerPaths = new List<ScorePath>();

            foreach (var path in filteredPaths)
            {
                var humanMoves = path.NextHumanMoves(Valves);
                var elephantMoves = path.NextElephantMoves(Valves);

                foreach (var hMove in humanMoves)
                {
                    foreach (var eMove in elephantMoves)
                    {
                        var openSame = hMove.IsOpen && eMove.IsOpen && path.HumanDest == path.ElephantDest;
                        if (!openSame)
                        {
                            var nextPath = path.AddMove(hMove, eMove, Valves);
                            longerPaths.Add(nextPath);
                        }
                    }
                }
            }

            Console.WriteLine($"  > Num paths: {longerPaths.Count}");
            
            pathsWithLevel = longerPaths;
            i++;
        }

        return pathsWithLevel;
    }

    public List<ScorePath> ExpandPathsFiltered(int i, int maxSteps, List<ScorePath> paths)
    {
        Console.WriteLine($"> Expanding level {i + 1}, max steps = {maxSteps}");

        if (i + 1 == maxSteps)
            return paths;

        var filteredPaths = WhereSubPathIsOptimal(i, paths);

        var longerPaths = new List<ScorePath>();

        foreach (var path in filteredPaths)
        {
            var humanMoves = path.NextHumanMoves(Valves);
            var elephantMoves = path.NextElephantMoves(Valves);

            foreach (var hMove in humanMoves)
            {
                foreach (var eMove in elephantMoves)
                {
                    var openSame = hMove.IsOpen && eMove.IsOpen && path.HumanDest == path.ElephantDest;
                    if (!openSame)
                    {
                        var nextPath = path.AddMove(hMove, eMove, Valves);
                        longerPaths.Add(nextPath);
                    }
                }
            }
        }

        Console.WriteLine($"  > Num paths: {longerPaths.Count}");
        return ExpandPathsFiltered(i + 1, maxSteps, longerPaths);
    }

    public IEnumerable<ScorePath> WhereSubPathIsOptimal(int i, List<ScorePath> paths)
    {
        var result = new List<ScorePath>();
        foreach (var group in paths.GroupBy(p => p.FilterKey))
            result.Add(group.MaxBy(p => p.Score));

        if (i == 10 || i > 10 && (i + 10) % 9 == 0)
            result = paths.OrderByDescending(p => p.Score).Take(300).ToList();

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