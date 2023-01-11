using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static string start = "AA";
    static IList<string> exampleSteps = new [] { "DD", "!CC", "BB", "AA", "II", "JJ", "II", "AA", "DD", "!EE", "FF", "GG", "HH", "GG", "FF", "EE", "DD", "CC" }.ToList();
    
    public static void Main(string[] args)
    {
        var valves = ReadValves("Example.txt", clearClosed: false);

        foreach (var valve in valves)
            Console.WriteLine(valve);

        var score = CalculateScore(start, exampleSteps, valves);
        Console.WriteLine();
        Console.WriteLine($"Score={score}");
    }

    static List<Valve> ReadValves(string fileName, bool clearClosed)
    {
        List<Valve> allValves = new();

        foreach (var line in File.ReadLines(fileName))
            allValves.Add(Valve.From(line));

        foreach (var valve in allValves)
            valve.ConnectNeighbors(allValves);

        var result = clearClosed
            ? ClearClosedValves(allValves)
            : allValves;

        return result
            .OrderBy(v => v.Name)
            .ToList();
    }

    static IEnumerable<Valve> ClearClosedValves(IEnumerable<Valve> valves)
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

    static int CalculateScore(string start, IList<string> steps, IEnumerable<Valve> allValves)
    {
        const int maxSteps = 30;
        int score = 0;

        var curr = FindValve(start, allValves);
        
        var numSteps = 0;
        
        foreach (var step in steps)
        {
            numSteps += 1;

            var shouldOpen = !step.StartsWith("!");            
            var next = FindValve(step, allValves);            
            var isOpen = next.IsOpen;

            Console.WriteLine();
            Console.WriteLine($"== Minute {numSteps} ==");
            Console.WriteLine($"You move to {step}.");
            
            if (!isOpen && shouldOpen)
            {
                numSteps += 1;
                
                next.IsOpen = true;
                
                Console.WriteLine();
                Console.WriteLine($"== Minute {numSteps} ==");
                Console.WriteLine($"You open valve {step}.");
                

                var remainingSteps = maxSteps - numSteps;                
                if (numSteps <= maxSteps)
                    score += next.Flow * remainingSteps;
                else
                    Console.WriteLine($"Stopped counting: num steps = {numSteps} > max steps = {maxSteps}");
            }
            
            curr = next;            
        }

        return score;
    }

    static Valve FindValve(string step, IEnumerable<Valve> allValves)
    {
        var name = step.StartsWith("!") ? step.Substring(1) : step;
        return allValves.FirstOrDefault(v => v.Name == name) ?? throw new Exception($"Valve {name} not found");
    }
}

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
        
        IsOpen = Flow == 0; // When the flow is 0 it makes no sense to open the valve, so set to Open from the start

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

record class Tunnel(Valve From, Valve To, int Steps);