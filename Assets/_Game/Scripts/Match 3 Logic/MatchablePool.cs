using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] colors;

    public void RandomizeType(Matchable toRandomize)
    {
        int random = Random.Range(0, howManyTypes);

        toRandomize.SetType(random, sprites[random]);
    }
    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObjects();

        RandomizeType(randomMatchable);

        return randomMatchable;
    }
    public int NextType(Matchable matchable)
    {
        int nextType = (matchable.Type + 1) % howManyTypes;

        matchable.SetType(nextType, sprites[nextType]);

        return nextType;
    }
    public void ChangeType(Matchable toChange, int type)
    {
        toChange.SetType(type, sprites[type]);
    }
}
