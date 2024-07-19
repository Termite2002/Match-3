
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private AudioMixer audioMixer;
    private ScoreManager scoreManager;

    [SerializeField]
    private Fader loadingScreen, 
                        darker;

    [SerializeField]
    private TextMeshProUGUI finalScoreText;
    [SerializeField]
    private Image[] star;

    [SerializeField]
    private Movable popupComplete;

    [SerializeField]
    private bool levelIsTimed;
    [SerializeField]
    private LevelTimer timer;
    [SerializeField]
    private float timeLimit;


    //the dimensions of the matchable grid, set in the inspector
    [SerializeField] private Vector2Int dimensions;

    // UI text display content of grid data for Testing, Debugging
    [SerializeField] private TextMeshProUGUI gridOutput;
    [SerializeField] private bool debugMode;

    private void Start()
    {
        // get refs to other important game objects
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        audioMixer = AudioMixer.Instance;
        cursor = Cursor.Instance;
        scoreManager = ScoreManager.Instance;

        // Setup the scene
        StartCoroutine(Setup());
    }

    //private void Update()
    //{
    //    if (debugMode && Input.GetButtonDown("Jump"))
    //        NoMoreMoves();
    //}

    private IEnumerator Setup()
    {
        // disable user input
        cursor.enabled = false;

        // unhide loading scene
        loadingScreen.Hide(false);

        // if level rush
        if (levelIsTimed)
            timer.SetTimer(timeLimit);

        // pool the matchables
        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        // create the grid
        grid.InitializeGrid(dimensions);

        // fade out loading screen
        StartCoroutine(loadingScreen.Fade(0));

        // start bg music
        audioMixer.PlayMusic();


        yield return null;

        yield return StartCoroutine(grid.PopulateGrid(false, true));

        // remove loading screen

        // hint
        grid.CheckPossibleMoves();

        // enable user input
        cursor.enabled = true;

        // if level rush
        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }
    public void NoMoreMoves()
    {
        if (levelIsTimed)
            grid.MatchEverything();
        else
            GameOver();
    }

    public void GameOver()
    {
        // get and update popup
        finalScoreText.text = scoreManager.Score.ToString();
        for (int i = 0; i < star.Length; i++)
            star[i].enabled = false;
        if (scoreManager.Score < 1000)
        {
            star[0].enabled = true;
        }
        else if (scoreManager.Score >= 1000 && scoreManager.Score < 5000)
        {
            star[0].enabled = true;
            star[1].enabled = true;
        }
        else if (scoreManager.Score >= 5000)
        {
            for (int i = 0; i < star.Length; i++)
                star[i].enabled = true;
        }

        // disable cursor
        cursor.enabled = false;

        // unhide
        darker.Hide(false);
        StartCoroutine(darker.Fade(0.75f));

        // move popup
        StartCoroutine(popupComplete.MoveToPostion(new Vector2(Screen.width / 2, Screen.height / 2)));
    }

    public void BackButton()
    {
        StartCoroutine(Quit());
    }
    private IEnumerator Quit()
    {
        StartCoroutine(popupComplete.MoveToPostion(new Vector2(Screen.width / 2, Screen.height / 2) + Vector2.down * 1080));
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Main Menu");
    }

    public void RetryButton()
    {
        StartCoroutine(Retry());
    }
    private IEnumerator Retry()
    {
        // Fade visual
        StartCoroutine(popupComplete.MoveToPostion(new Vector2(Screen.width / 2, Screen.height / 2) + Vector2.down * 1080));
        yield return StartCoroutine(darker.Fade(0));
        darker.Hide(true);

        // reset all
        if (levelIsTimed)
            timer.SetTimer(timeLimit);
        cursor.Reset();
        scoreManager.Reset();

        yield return StartCoroutine(grid.Reset());

        yield return null;

        cursor.enabled = true;

        // if level rush
        if (levelIsTimed)
            StartCoroutine(timer.Countdown());
    }

    public void AddTimer(float amount)
    {
        if (levelIsTimed)
        {
            timer.AddTime(amount);
        }
    }
}
