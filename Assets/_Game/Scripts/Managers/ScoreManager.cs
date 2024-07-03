using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreManager : Singleton<ScoreManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;

    [SerializeField]
    private Transform collectionPoint;

    private TextMeshProUGUI scoreText;
    private int score;

    public int Score
    {
        get
        {
            return score;
        }
    }
    protected override void Init()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score : " + score;
    }
    public IEnumerator ResolveMatch(Match toResolve, MatchType powerupUsed = MatchType.invalid)
    {
        Matchable powerupFormed = null;
        Matchable matchable;

        Transform target = collectionPoint;

        // if larger match is made, create power up
        if (powerupUsed == MatchType.invalid && toResolve.Count > 3)
        {
            powerupFormed = pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.Type, toResolve.orientation);

            toResolve.RemoveMatchable(powerupFormed);

            target = powerupFormed.transform;

            powerupFormed.SortingOrder = 3;
        }




        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            // check if this is a match 5 powerup, if true -> dont remove or resolve it
            if (powerupUsed != MatchType.match5 && matchable.IsGem)
                continue;

            // remove the matchables from the grid
            grid.RemoveItemAt(matchable.position);

            // move them off to the side of the green
            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(target));
            else 
                StartCoroutine(matchable.Resolve(target));
        }


        // update player's score
        AddScore(toResolve.Count * toResolve.Count);

        if (powerupFormed != null)
        {
            powerupFormed.SortingOrder = 1;
        }
    }
}
