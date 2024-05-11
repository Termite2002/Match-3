using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreManager : Singleton<ScoreManager>
{
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
        grid = (MatchableGrid)MatchableGrid.Instance;
    }
    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score : " + score;
    }
    public IEnumerator ResolveMatch(Match toResolve)
    {
        Matchable matchable;

        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];

            // remove the matchables from the grid
            grid.RemoveItemAt(matchable.position);

            // move them off to the side of the green
            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(collectionPoint));
            else 
                StartCoroutine(matchable.Resolve(collectionPoint));
        }


        // update player's score
        AddScore(toResolve.Count * toResolve.Count);

        yield return null;
    }
}
