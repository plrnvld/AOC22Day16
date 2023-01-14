
using System;
using System.Collections.Generic;
using System.Linq;

record class Path(List<(string name, bool isOpen)> Steps)
{
    public static Path Empty() => new Path(new List<(string, bool)>());

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
            return new Path(newSteps);
        }

        var next = move.GetValve(valves);
        newSteps.Add((next.Name, next.IsOpen));
        return new Path(newSteps);
    }

    public Path AddSteps(IList<string> newSteps, bool openLast)
    {
        var newStepsWithBool = newSteps.Select(s => (s, false)).ToList();

        if (openLast && newStepsWithBool.Any())
            newStepsWithBool[^1] = (newStepsWithBool[^1].Item1, true);

        return new Path(Steps.Concat(newStepsWithBool).ToList());
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

    public IEnumerable<Path> NextPaths(Dictionary<string, Valve> valves, Valve currValve)
    {
        // ###############################
        if (Steps.Count == 0)
        {
            return currValve.Neighbors.Select(n => new Path(new List<(string, bool)> { (n.Name, false) }));
        }

        var moves = NextMoves(valves);
        return moves.Select(m => AddMove(m, valves));
    }

    public int Size() => Steps.Select(t => (t.isOpen ? 2 : 1)).Sum();

    public int LastValveOpeningBenefit(Dictionary<string, Valve> valves, int maxSteps)
    {
        var (last, isOpen) = Steps.Last();
        if (isOpen || !CanOpen(last))
            throw new Exception($"Cannot open last valve {last}");

        return (maxSteps - Size() - 1) * valves[last].Flow;

    }

    public override string ToString()
    {
        var steps = string.Join(",", Steps.Select(tup => $"{(tup.isOpen ? "" : "!")}{tup.name}"));
        return $"Path (size={Size()}): {steps}";
    }

    public static Path FromAnswer(string answer)
    {
        var splitted = answer.Split(",");
        var steps = splitted.Select(step => step.StartsWith("!") ? (step.Substring(1), false) : (step, true)).ToList();
        return new Path(steps);
    }
}
