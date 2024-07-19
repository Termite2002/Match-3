using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    private GameManager gm;

    [SerializeField]
    private TextMeshProUGUI timerText;

    private float timerRemaining;
    private string timeAsString;

    private bool counting;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    public void SetTimer(float t)
    {
        StopAllCoroutines();
        timerRemaining = t;
        UpdateText();
    }

    private void UpdateText()
    {
        timeAsString = (int)timerRemaining / 60 + " : ";
        timeAsString += timerRemaining % 60 < 10 ? "0" : "";
        timerText.text = timeAsString + (int)timerRemaining % 60;
    }

    public IEnumerator Countdown()
    {
        counting = true;
        do
        {
            timerRemaining -= Time.deltaTime;
            UpdateText();
            yield return null;
        }
        while (timerRemaining > 0);

        counting = false;

        gm.GameOver();
    }

    public void AddTime(float amount)
    {
        timerRemaining += amount;
    }
}
