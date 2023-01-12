using System;
using System.Collections.Generic;
using System.Linq;

class Valve
{
    public string Name { get; }
    public int Flow { get; }

    IEnumerable<string> _neighborValves;

    public List<Tunnel> Neighbors = new();
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
            Neighbors.Add(new Tunnel(this, allValves.First(n => n.Name == neighborName), 1));
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
        => valve.Neighbors.Any(t => t.To == this);

    public override string ToString()
    {
        var neighbors = string.Join(" / ", Neighbors.Select(t => $"{t.To.Name} (f={t.To.Flow}, s={t.Steps})"));
        return $"Valve {Name}, f={Flow} --> {neighbors}";
    }
}