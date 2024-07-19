using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Fader loadingScreen;

    private void Start()
    {
        loadingScreen.Hide(false);
        StartCoroutine(loadingScreen.Fade(0));
    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        Application.Quit();
    }
    public void QuitButton()
    {
        StartCoroutine(Quit());
    }

    private IEnumerator StartSurvivalGame()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Survival");
    }
    public void SurvivalButton()
    {
        StartCoroutine(StartSurvivalGame());
    }


    private IEnumerator StartRushGame()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Time Rush");
    }
    public void TimeRushButton()
    {
        StartCoroutine(StartRushGame());
    }
}
