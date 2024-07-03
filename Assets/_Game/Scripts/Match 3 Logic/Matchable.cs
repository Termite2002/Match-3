using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private int type;

    private MatchType powerup = MatchType.invalid;
    private Orientation orientationMatch4 = Orientation.none;

    public int Type
    {
        get
        {
            return type;
        }
    }
    public Orientation OriMatch4
    {
        get
        {
            return orientationMatch4;
        }
    }
    public bool IsGem
    {
        get
        {
            return powerup == MatchType.match5;
        }
    }

    private SpriteRenderer spriteRenderer;

    // where is this matchable in the grid
    public Vector2Int position;
    private void Awake()
    {
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(int type, Sprite sprite, Color color)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
    }
    public void SetType(int type, Sprite sprite)
    {
        this.type = type;
        spriteRenderer.sprite = sprite;
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        // if matchable is a powerup, resolve it as such
        if (powerup != MatchType.invalid)
        {
            // resolve a match4 powerup
            if (powerup == MatchType.match4)
            {
                grid.MatchRowAndColumn(this);
            }


            // resolve a cross powerup
            if (powerup == MatchType.cross)
            {
                grid.MatchAllAdjacent(this);
            }

            powerup = MatchType.invalid;
        }
        if (collectionPoint == null)
            yield break;

        // draw above
        spriteRenderer.sortingOrder = 2;

        // move off the grid to a collection point
        yield return StartCoroutine(MoveToTransform(collectionPoint));

        // reset
        spriteRenderer.sortingOrder = 1;

        // return back to the pool
        pool.ReturnObjectToPool(this);
    }
    public Matchable Upgrade(MatchType powerupType, Sprite powerUpSprite, Orientation ori = Orientation.none)
    {
        // if this is already a powerup, resolve it before upgrading
        if (powerup != MatchType.invalid)
        {
            idle = false;
            StartCoroutine(Resolve(null));
            idle = true;
        }
        if (powerupType == MatchType.match5)
        {
            type = -1;
        }

        powerup = powerupType;
        spriteRenderer.sprite = powerUpSprite;
        orientationMatch4 = ori;

        return this;
    }
    public void ResolveBefore()
    {
        if (powerup != MatchType.invalid)
        {
            idle = false;
            StartCoroutine(Resolve(null));
            idle = true;
        }
    }
    public int SortingOrder
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }
    }
    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }
    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }
    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }
    public override string ToString()
    {
        return gameObject.name;
    }
}
