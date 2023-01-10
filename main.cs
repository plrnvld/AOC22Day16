using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    public static void Main(string[] args)
    {
        var valves = ReadValves("Example.txt");

        foreach (var valve in valves)
            Console.WriteLine(valve);
    }

    static List<Valve> ReadValves(string fileName)
    {
        List<Valve> allValves = new();

        foreach (var line in File.ReadLines(fileName))
            allValves.Add(Valve.From(line));

        foreach (var valve in allValves)
            valve.ConnectNeighbors(allValves);        

        return ClearClosedValves(allValves);
    }

    static List<Valve> ClearClosedValves(IEnumerable<Valve> valves)
    {    
        var clearedValves = new List<Valve>();

        foreach (var valve in valves)
        {
            if (valve.Name == "AA" || valve.Flow > 0)
                clearedValves.Add(valve);
            else             
                RewireAroundClosedValve(valve);
        }
        

        return clearedValves;
    }

    static void RewireAroundClosedValve(Valve valve)
    {
        var outgoing = valve.Neighbors;
        var incoming = outgoing.SelectMany(t => t.To.Neighbors).Where(t => t.To == valve).ToList();
        
        foreach (var oldIncoming in incoming)
        {
            var outgoingToDifferentPlace = outgoing.Where(t => t.To != oldIncoming.From);
            foreach (var oldOutgoing in outgoingToDifferentPlace)
            {
                var newTunnel = Combine(oldIncoming, oldOutgoing);
                oldIncoming.From.Neighbors.Remove(oldIncoming);
                oldIncoming.From.Neighbors.Add(newTunnel);               
            }
        }        
    }

    static Tunnel Combine(Tunnel t1, Tunnel t2)
    {
        if (t1.To != t2.From)
            throw new Exception($"Tunnel {t1} and {t2} don't connect");

        return new Tunnel(t1.From, t2.To, t1.Steps + t2.Steps);
    }
}

class Valve
{
    public string Name { get; }
    public int Flow { get; }
    public List<Tunnel> Neighbors = new();

    IEnumerable<string> _neighborValves;

    public Valve(string name, int flow, IEnumerable<string> neighborValves)
    {
        Name = name;
        Flow = flow;

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

    public override string ToString()
    {
        var neighbors = string.Join(" / ", Neighbors.Select(n => $"{n.To.Name} ({n.Steps} step{(n.Steps > 1 ? "s": "")})"));
        return $"Valve {Name} with flow={Flow} connected to {neighbors}";
    }
}

record class Tunnel(Valve From, Valve To, int Steps);