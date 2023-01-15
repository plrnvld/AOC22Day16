using System;
using System.Collections.Generic;
using System.Linq;

record class ScorePath(List<(string humanName, bool humanOpen, string elephantName, bool elephantOpen)> Steps, int Score, int MaxSteps)
{
    public static ScorePath Empty(int maxSteps) => new ScorePath(new List<(string, bool, string, bool)>(), 0, maxSteps);

    public string HumanDest => Steps[^1].humanName;
    
    public string ElephantDest => Steps[^1].elephantName;
    
    public ScorePath AddMove(Move humanMove, Move elephantMove, Dictionary<string, Valve> valves)
    {
        var newSteps = new List<(string, bool, string, bool)>(Steps);

        if (humanMove.IsOpen && elephantMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            newSteps.Add((lastHuman, true, lastElephant, true));
            var rem = MaxSteps - Size() - 1;
            var added = rem * valves[lastHuman].Flow + rem * valves[lastElephant].Flow;
            var newScore = Score + added;
            // Console.WriteLine($"Human opens {lastHuman}");
            // Console.WriteLine($"Elephant opens {lastElephant}");
            // Console.WriteLine($"> Adding {added} ({remHuman} * {valves[lastHuman].Flow} + {remElephant} * {valves[lastElephant].Flow})");
            return new ScorePath(newSteps, newScore, MaxSteps);
        }
        else if (humanMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            var nextElephant = elephantMove.GetValve(valves);
            newSteps.Add((lastHuman, true, nextElephant.Name, nextElephant.IsOpen));
            
            var rem = MaxSteps - Size() - 1;
            var added = rem * valves[lastHuman].Flow;
            var newScore = Score + added;
            // Console.WriteLine($"Human opens {lastHuman}");
            // Console.WriteLine($"> Adding {added} ({remHuman} * {valves[lastHuman].Flow} + {remElephant} * 0)");
            
            return new ScorePath(newSteps, newScore, MaxSteps);
        }
        else if (elephantMove.IsOpen)
        {
            var (lastHuman, _, lastElephant, _) = newSteps[^1];
            var nextHuman = humanMove.GetValve(valves);
            newSteps.Add((nextHuman.Name, nextHuman.IsOpen, lastElephant, true));

            var rem = MaxSteps - Size() - 1;
            var added = rem * valves[lastElephant].Flow;
            var newScore = Score + added;
            // Console.WriteLine($"Elephant opens {lastElephant}");
            // Console.WriteLine($"> Adding {added} ({remHuman} * 0 + {remElephant} * {valves[lastElephant].Flow})");
            
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

    public int Size() => Steps.Select(t => (t.humanOpen && t.elephantOpen ? 2 : 1)).Sum();
    
    public override string ToString()
    {
        var steps = string.Join(",", Steps.Select(tup => $"{(tup.humanOpen ? "" : "!")}{tup.humanName}^{(tup.elephantOpen ? "" : "!")}{tup.elephantName}"));
        return $"ScorePath ({Score}): {steps}";
    }

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