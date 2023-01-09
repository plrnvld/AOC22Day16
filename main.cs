using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    public static void Main(string[] args)
    {
        List<Valve> allValves = new();

        foreach (var line in File.ReadLines("Input.txt"))
            allValves.Add(Valve.From(line));        

        foreach (var valve in allValves)
            valve.ConnectNeighbors(allValves);

        foreach (var valve in allValves)
            Console.WriteLine(valve);
    }
}

class Valve
{

    public string Name { get; }
    public int Flow { get; }

    public List<Valve> Neighbors = new();

    IEnumerable<string> neighborValves;

    public Valve(string name, int flow, IEnumerable<string> neighborValves)
    {
        Name = name;
        Flow = flow;

        this.neighborValves = neighborValves;
    }

    public void ConnectNeighbors(List<Valve> allValves)
    {
        foreach (var neighborName in neighborValves)
        {
            var match = allValves.First(n => n.Name == neighborName);
            Neighbors.Add(match);
        }
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
        var neighbors = string.Join("/", Neighbors.Select(n => n.Name));
        return $"Valve {Name} with flow={Flow} connected to {neighbors}";
    }
}