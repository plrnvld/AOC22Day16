using System;
using System.Collections.Generic;
using System.Linq;

record class ScorePath(List<(string humanName, bool humanOpen, string elephantName, bool elephantOpen)> Steps, int Score, int MaxSteps)
{
    public static ScorePath Empty(int maxSteps) => new ScorePath(new List<(string, bool, string, bool)>(), 0, maxSteps);

    public string DestHuman => Steps[^1].humanName;
    public string DestElephant => Steps[^1].humanName;

    public ScorePath AddValvesWithoutOpening(Valve humanValve, Valve elephantValve, bool elephantOpen, Dictionary<string, Valve> valves)
    {
        var humanMove = Move.Step(humanValve.Name);
        var elephantMove = Move.Step(elephantValve.Name);
        
        return AddMove(humanMove, elephantMove, valves);
    }

    /*
    public ScorePath AddValveWithOpening(Valve valve, Dictionary<string, Valve> valves)
    {
        var newPath = AddValveWithoutOpening(valve, valves);
        return newPath.AddMove(Move.Open, valves);
    }
    */

    public ScorePath AddMove(Move humanMove, Move elephantMove, Dictionary<string, Valve> valves)
    {
        var newSteps = new List<(string, bool, string, bool)>(Steps);

        if (humanMove.IsOpen && elephantMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            newSteps.Add((lastHuman, true, lastElephant, true));
            var newScore = Score + (MaxSteps - Size() - 1) * (valves[lastHuman].Flow + valves[lastElephant].Flow);
            return new ScorePath(newSteps, newScore, MaxSteps);
        }
        else if (humanMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            var nextElephant = elephantMove.GetValve(valves);
            newSteps.Add((lastHuman, true, nextElephant.Name, nextElephant.IsOpen));
            var newScore = Score + (MaxSteps - Size() - 1) * valves[lastHuman].Flow;
            return new ScorePath(newSteps, newScore, MaxSteps);
        }
        else if (elephantMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            var nextHuman = humanMove.GetValve(valves);
            newSteps.Add((nextHuman.Name, nextHuman.IsOpen, lastElephant, true));
            var newScore = Score + (MaxSteps - Size() - 1) * valves[lastElephant].Flow;
            return new ScorePath(newSteps, newScore, MaxSteps);
        }
        else
        {
            var nextHuman = humanMove.GetValve(valves);
            var nextElephant = elephantMove.GetValve(valves);
            newSteps.Add((nextHuman.Name, nextHuman.IsOpen, nextElephant.Name, nextElephant.IsOpen));
            return new ScorePath(newSteps, Score, MaxSteps);
        }
    }

    public bool CanOpen(string name) => !Steps.Any(step => step.humanOpen && name == step.humanName || step.elephantOpen && name == step.elephantName);

    public IEnumerable<Move> NextHumanMoves(Dictionary<string, Valve> valves)
    {
        var (lastHuman, humanOpen, _, _) = Steps.Last();
        if (!humanOpen && CanOpen(lastHuman))
            yield return Move.Open;

        foreach (var n in valves[lastHuman].Neighbors)
            yield return Move.Step(n.Name);
    }

    public IEnumerable<Move> NextElephantMoves(Dictionary<string, Valve> valves)
    {
        var (_, _, lastElephant, elephantOpen) = Steps.Last();
        if (!elephantOpen && CanOpen(lastElephant))
            yield return Move.Open;

        foreach (var n in valves[lastElephant].Neighbors)
            yield return Move.Step(n.Name);
    }

    public int Size() => Steps.Select(t => (t.humanOpen ? 2 : 1) + (t.elephantOpen ? 2 : 1)).Sum();

    public override string ToString()
    {
        var steps = string.Join(",", Steps.Select(tup => $"human->{(tup.humanOpen ? "" : "!")}{tup.humanName},elephant->{(tup.elephantOpen ? "" : "!")}{tup.elephantName}"));
        return $"ScorePath ({Score}, {Size()}): {steps}";
    }

    /*
    public static ScorePath From(Path path, int maxSteps, Dictionary<string, Valve> valves)
    {
        var score = 0;
        var turn = 0;
        for (var i = 1; i <= path.Steps.Count; i++)
        {
            turn++;
            var (name, isOpen) = path.Steps[i - 1];
            // Console.WriteLine($"[{turn}] Moving to {name}");
            if (isOpen)
            {
                turn++;
                var n = maxSteps - turn;
                var addedScore = n * valves[name].Flow;

                // Console.WriteLine($"[{turn}] Opening valve {name}, adding {addedScore} ({valves[name].Flow} * {n})");    
                score += addedScore;
            }
        }

        return new ScorePath(path.Steps, score, maxSteps);
    }
    */

    public string FilterKey
    {
        get
        {
            if (Steps.Count == 0)
                return string.Empty;

            var initNames = Steps.Where(s => s.humanOpen).Select(s => s.humanName)
                .Concat(Steps.Where(s => s.elephantOpen).Select(s => s.elephantName))
                .Take(Steps.Count - 1);

            var lastNames = new[] { Steps[^1].humanName, Steps[^1].elephantName }.OrderBy(n => n);

            return string.Concat(initNames.OrderBy(n => n)) + $"({lastNames.First()},{lastNames.Last()})";
        }
    }
}