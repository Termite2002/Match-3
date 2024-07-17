
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private AudioMixer audioMixer;

    [SerializeField]
    private Fader loadingScreen;

    //the dimensions of the matchable grid, set in the inspector
    [SerializeField] private Vector2Int dimensions;

    // UI text display content of grid data for Testing, Debugging
    [SerializeField] private TextMeshProUGUI gridOutput;

    private void Start()
    {
        // get refs to other important game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        audioMixer = AudioMixer.Instance;
        cursor = Cursor.Instance;

        // Setup the scene
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // disable user input
        cursor.enabled = false;

        // unhide loading scene
        loadingScreen.Hide(false);

        // pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        // create the grid
        grid.InitializeGrid(dimensions);

        // fade out loading screen
        StartCoroutine(loadingScreen.Fade(0));

        // start bg music
        audioMixer.PlayMusic();


        yield return null;

        StartCoroutine(grid.PopulateGrid(false, true));

        // remove loading screen

        // hint
        grid.CheckPossibleMoves();

        // enable user input
        cursor.enabled = true;
    }
    public void NoMoreMoves()
    {
        grid.MatchEverything();
    }
}
