using System;
using System.Collections.Generic;
using System.Linq;

record class Path(List<(string name, bool isOpen)> Steps, int Score, int MaxSteps)
{
    public static Path Empty(int maxSteps) => new Path(new List<(string, bool)>(), 0, maxSteps);
    
    public string Dest => Steps[^1].name;

    public Path AddValveWithoutOpening(Valve valve, Dictionary<string, Valve> valves)
    {
        var move = Move.Step(valve.Name);
        return AddMove(move, valves);
    }

    public Path AddValveWithOpening(Valve valve, Dictionary<string, Valve> valves)
    {
        var newPath = AddValveWithoutOpening(valve, valves);
        return newPath.AddMove(Move.Open, valves);
    }

    public Path AddMove(Move move, Dictionary<string, Valve> valves)
    {
        var newSteps = new List<(string, bool)>(Steps);

        if (move.IsOpen)
        {
            var (last, _) = newSteps[^1];
            newSteps.RemoveAt(newSteps.Count - 1);
            newSteps.Add((last, true));
            var newScore = Score + (MaxSteps - Size() - 1) * valves[last].Flow;
            return new Path(newSteps, newScore, MaxSteps);
        }

        var next = move.GetValve(valves);
        newSteps.Add((next.Name, next.IsOpen));
        return new Path(newSteps, Score, MaxSteps);
    }

    public bool CanOpen(string name) => !Steps.Any(s => s == (name, true));

    public IEnumerable<Move> NextMoves(Dictionary<string, Valve> valves)
    {
        var (last, isOpen) = Steps.Last();
        if (!isOpen && CanOpen(last))
            yield return Move.Open;

        foreach (var t in valves[last].Neighbors)
            yield return Move.Step(t.To.Name);
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
        var steps = string.Join(",", Steps.Select(t => $"{(t.isOpen ? "" : "!")}{t.name}"));
        return $"Path ({Score}, {Size()}): {steps}";
    }   
}