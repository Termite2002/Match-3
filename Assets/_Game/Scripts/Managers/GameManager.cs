
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;

    //the dimensions of the matchable grid, set in the inspector
    [SerializeField] private Vector2Int dimensions;

    // UI text display content of grid data for Testing, Debugging
    [SerializeField] private TextMeshProUGUI gridOutput;

    private void Start()
    {
        // get refs to other important game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;

        // Setup the scene
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // loading scene

        // pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        // create the grid
        grid.InitializeGrid(dimensions);

        yield return null;

        StartCoroutine(grid.PopulateGrid(false, true));

        // remove loading screen

        // hint
        grid.CheckPossibleMoves();
    }
    public void NoMoreMoves()
    {
        grid.MatchEverything();
    }
}
