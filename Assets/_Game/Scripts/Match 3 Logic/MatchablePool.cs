using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] colors;

    [SerializeField] private Sprite[] match4HorizontalPowerUp;
    [SerializeField] private Sprite[] match4VerticalPowerUp;
    [SerializeField] private Sprite[] matchCrossPowerUp;
    [SerializeField] private Sprite match5PowerUp;

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
    public Matchable UpgradeMatchable(Matchable toBeUpgraded, MatchType type, Orientation orientationType)
    {
        if (type == MatchType.cross)
        {
            return toBeUpgraded.Upgrade(MatchType.cross, matchCrossPowerUp[toBeUpgraded.Type]);
        }
        if (type == MatchType.match4)
        {
            if (orientationType == Orientation.horizontal)
            {
                return toBeUpgraded.Upgrade(MatchType.match4, match4HorizontalPowerUp[toBeUpgraded.Type], orientationType);
            }
            if (orientationType == Orientation.vertical)
            {
                return toBeUpgraded.Upgrade(MatchType.match4, match4VerticalPowerUp[toBeUpgraded.Type], orientationType);
            }

            Debug.LogWarning("No ori type 4 upgrade");
        }

        if (type == MatchType.match5)
        {
            return toBeUpgraded.Upgrade(MatchType.match5, match5PowerUp);
        }

        Debug.LogWarning("Tried to upgrade a matchable with an invalid match type");

        return toBeUpgraded;
    }
    public void ChangeType(Matchable toChange, int type)
    {
        toChange.SetType(type, sprites[type]);
    }
}
