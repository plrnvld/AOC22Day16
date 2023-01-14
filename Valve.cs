using System;
using System.Collections.Generic;
using System.Linq;

class Valve
{
    public string Name { get; }
    public int Flow { get; }

    IEnumerable<string> _neighborValves;

    public List<Valve> Neighbors = new();
    public bool IsOpen { get; set; }

    public Valve(string name, int flow, IEnumerable<string> neighborValves)
    {
        Name = name;
        Flow = flow;

        IsOpen = false;

        _neighborValves = neighborValves;
    }

    public void ConnectNeighbors(List<Valve> allValves)
    {
        foreach (var neighborName in _neighborValves)
            Neighbors.Add(allValves.First(n => n.Name == neighborName));
    }

    public static Valve From(string valveLine)
    {
        var name = valveLine.Substring(6, 2);
        var flow = int.Parse(string.Concat(valveLine.Substring(23).TakeWhile(c => c >= '0' && c <= '9')));
        var tokens = valveLine.Split(" ").Reverse();
        var neighborValveNames = tokens.TakeWhile(t => !t.StartsWith("valve")).Reverse().Select(t => t.Substring(0, 2));

        return new Valve(name, flow, neighborValveNames);
    }

    public bool ReachableFrom(Valve valve)
        => valve.Neighbors.Any(n => n == this);

    public override string ToString()
    {
        var neighbors = string.Join(" / ", Neighbors.Select(n => $"{n.Name} (f={n.Flow})"));
        return $"Valve {Name}, f={Flow} --> {neighbors}";
    }
}