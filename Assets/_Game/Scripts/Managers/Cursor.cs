using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    private MatchableGrid grid;

    private SpriteRenderer spriteRenderer;

    private Matchable[] selected;

    private Vector2 originSize = new Vector2(1.3f, 1.3f);
    private Vector2 verticalStretch = new Vector2(1.3f, 2.6f);
    private Vector2 horizontalStretch = new Vector2(2.6f, 1.3f);

    protected override void Init()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = false;

        selected = new Matchable[2];
    }
    private void Start()
    {
        grid = (MatchableGrid)MatchableGrid.Instance;
    }

    public void SelectFirst(Matchable toSelect)
    {
        selected[0] = toSelect;

        if (!enabled || selected[0] == null)
            return;

        transform.position = toSelect.transform.position;

        spriteRenderer.size = originSize;
        spriteRenderer.enabled = true;
    }
    public void SelectSecond(Matchable toSelect)
    {
        selected[1] = toSelect;

        if (!enabled || selected[0] == null || selected[1] == null || !selected[0].Idle || !selected[1].Idle || selected[0] == selected[1])
            return;

        if (SelectedAreAdjacent())
            StartCoroutine(grid.TrySwap(selected));

        SelectFirst(null);
    }

    private bool SelectedAreAdjacent()
    {
        if (selected[0].position.x == selected[1].position.x)
        {
            if (selected[0].position.y == selected[1].position.y + 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position = BetweenTwoMatchable(selected[0].transform.position, selected[1].transform.position);
                return true;
            }
            else if (selected[0].position.y == selected[1].position.y - 1)
            {
                spriteRenderer.size = verticalStretch;
                transform.position = BetweenTwoMatchable(selected[0].transform.position, selected[1].transform.position);
                return true;
            }
        }
        else if (selected[0].position.y == selected[1].position.y)
        {
            if (selected[0].position.x == selected[1].position.x + 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position = BetweenTwoMatchable(selected[0].transform.position, selected[1].transform.position);
                return true;
            }
            else if (selected[0].position.x == selected[1].position.x - 1)
            {
                spriteRenderer.size = horizontalStretch;
                transform.position = BetweenTwoMatchable(selected[0].transform.position, selected[1].transform.position);
                return true;
            }
        }
        return false;
    }
    private Vector3 BetweenTwoMatchable(Vector3 a, Vector3 b)
    {
        return (a + b) / 2;
    }
}
