using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private AudioMixer audioMixer;

    [SerializeField]
    private Transform collectionPoint;

    [SerializeField]
    private Slider comboSlider;

    [SerializeField]
    private TextMeshProUGUI scoreText,
                            comboText;
    private int score;
    private int comboMultiplier;

    public int Score
    {
        get
        {
            return score;
        }
    }
    // how much time has passed since the player last scored ?
    private float timeSinceLastScore;

    // how much time should we allow before resetting the combo multiplier
    [SerializeField] private float maxComboTime;
    private float currentComboTime;

    private bool timerIsActive;
    
    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        audioMixer = AudioMixer.Instance;

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
    }
    public void AddScore(int amount)
    {
        score += amount * IncreaseCombo();
        scoreText.text = "Score : " + score;

        timeSinceLastScore = 0;

        if (!timerIsActive)
        {
            StartCoroutine(ComboTimer());
        }

        // add score sound
        audioMixer.PlaySound(SoundEffects.score);
    }
    private IEnumerator ComboTimer()
    {
        timerIsActive = true;

        comboText.enabled = true;
        comboSlider.gameObject.SetActive(true);

        do
        {
            timeSinceLastScore += Time.deltaTime;
            comboSlider.value = 1 - timeSinceLastScore / currentComboTime;
            yield return null;
        }
        while (timeSinceLastScore < currentComboTime);

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);

        comboMultiplier = 0;
        timerIsActive = false;
    }
    private int IncreaseCombo()
    {
        comboText.text = "Combo x" + ++comboMultiplier;

        currentComboTime = maxComboTime - Mathf.Log(comboMultiplier) / 2;

        return comboMultiplier;
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

            // upgrade sound
            audioMixer.PlaySound(SoundEffects.upgrade);
        }
        else
        {
            // resolve sound
            audioMixer.PlaySound(SoundEffects.resolve);
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
