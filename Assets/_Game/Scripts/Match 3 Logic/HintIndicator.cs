using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class HintIndicator : Singleton<HintIndicator>
{
    private SpriteRenderer spriteRenderer;

    private Transform hintLocation;

    private Coroutine autoHintCR;

    [SerializeField]
    private Button hintButton;

    [SerializeField]
    private float delayBeforeAutoHint;

    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;
        hintButton.interactable = false;
    }

    public void IndicateHint(Transform hintLocation)
    {
        CancelHint();
        transform.position = hintLocation.position;
        spriteRenderer.enabled = true;
    }
    public void CancelHint()
    {
        spriteRenderer.enabled = false;
        hintButton.interactable = false;
        if (autoHintCR != null)
            StopCoroutine(autoHintCR);
        autoHintCR = null;
    }

    public void EnableHintButton()
    {
        hintButton.interactable = true;
    }
    public void StartAutoHint(Transform hintLocation)
    {
        this.hintLocation = hintLocation;

        autoHintCR = StartCoroutine(WaitAndIndicatorHint());
    }
    private IEnumerator WaitAndIndicatorHint()
    {
        yield return new WaitForSeconds(delayBeforeAutoHint);

        EnableHintButton();

        //IndicateHint(hintLocation);
    }
}
