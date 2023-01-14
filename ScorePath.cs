using System;
using System.Collections.Generic;
using System.Linq;

record class ScorePath(List<(string name, bool isOpen)> Steps, int Score, int MaxSteps)
{
    public static ScorePath Empty(int maxSteps) => new ScorePath(new List<(string, bool)>(), 0, maxSteps);

    public string Dest => Steps[^1].name;

    public ScorePath AddValveWithoutOpening(Valve valve, Dictionary<string, Valve> valves)
    {
        var move = Move.Step(valve.Name);
        return AddMove(move, valves);
    }

    public ScorePath AddValveWithOpening(Valve valve, Dictionary<string, Valve> valves)
    {
        var newPath = AddValveWithoutOpening(valve, valves);
        return newPath.AddMove(Move.Open, valves);
    }

    public ScorePath AddMove(Move move, Dictionary<string, Valve> valves)
    {
        var newSteps = new List<(string, bool)>(Steps);

        if (move.IsOpen)
        {
            var (last, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            newSteps.Add((last, true));
            var newScore = Score + (MaxSteps - Size() - 1) * valves[last].Flow;
            return new ScorePath(newSteps, newScore, MaxSteps);
        }

        var next = move.GetValve(valves);
        newSteps.Add((next.Name, next.IsOpen));
        return new ScorePath(newSteps, Score, MaxSteps);
    }

    public bool CanOpen(string name) => !Steps.Any(s => s == (name, true));

    public IEnumerable<Move> NextMoves(Dictionary<string, Valve> valves)
    {
        var (last, isOpen) = Steps.Last();
        if (!isOpen && CanOpen(last))
            yield return Move.Open;

        foreach (var n in valves[last].Neighbors)
            yield return Move.Step(n.Name);
    }

    public int Size() => Steps.Select(t => (t.isOpen ? 2 : 1)).Sum();

    public int LastValveOpeningBenefit(Dictionary<string, Valve> valves)
    {
        var (last, isOpen) = Steps.Last();
        if (isOpen || !CanOpen(last))
            throw new Exception($"Cannot open last valve {last}");

        return (MaxSteps - Size() - 1) * valves[last].Flow;

    }

    public override string ToString()
    {
        var steps = string.Join(",", Steps.Select(tup => $"{(tup.isOpen ? "" : "!")}{tup.name}"));
        return $"ScorePath ({Score}, {Size()}): {steps}";
    }

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

    public string FilterKey
    {
        get
        {
            if (Steps.Count == 0)
                return string.Empty;

            var initNames = Steps.Where(s => s.Item2).Select(s => s.Item1).Take(Steps.Count - 1);
            var lastName = Steps[^1];

            return string.Concat(initNames.OrderBy(n => n)) + lastName;
        }
    }
}