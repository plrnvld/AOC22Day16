using System;
using System.Collections.Generic;
using System.Linq;

struct Move
{
    const string OPEN = "open";
    static Move openMove = new Move(OPEN);
    string content;

    Move(string content) => this.content = content;

    public static Move Open => openMove;

    public static Move Step(string next) => new Move(next);

    public bool IsOpen => content == OPEN;

    public Valve GetValve(Dictionary<string, Valve> valves) => valves[content];
}