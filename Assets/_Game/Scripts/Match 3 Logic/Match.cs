using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Orientation
{
    none,
    horizontal,
    vertical,
    both
}
/*
 * This is a collection of Matchables that have been matched
 */
public class Match 
{
    private int unlisted = 0;

    // is this match horizontal or vertical
    public Orientation orientation = Orientation.none;

    // the internal list of matched matchables
    private List<Matchable> matchables;
    public List<Matchable> Matchables
    {
        get
        {
            return matchables;
        }
    }
    public int Count
    {
        get
        {
            return matchables.Count + unlisted;
        }
    }
    public bool Contains(Matchable toCompare)
    {
        return matchables.Contains(toCompare);
    }

    public Match()
    {
        matchables = new List<Matchable>(5);
    }
    public Match(Matchable original) : this()
    {
        AddMatchable(original);
    }
    public void AddMatchable(Matchable toAdd)
    {
        matchables.Add(toAdd);
    }
    public void AddUnlisted()
    {
        ++unlisted;
    }
    public void Merge(Match toMerge)
    {
        matchables.AddRange(toMerge.Matchables);
    }
    public override string ToString()
    {
        string s = "Match of type " + matchables[0].Type + " : ";

        foreach(Matchable m in matchables)
        {
            s += "(" + m.position.x + ", " + m.position.y + ") ";
        }

        return s;
    }
}
