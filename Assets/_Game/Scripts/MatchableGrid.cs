using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;

    [SerializeField] private Vector3 offScreenOffset;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
    }
    public IEnumerator PopulateGrid(bool allowMatches = false)
    {
        Matchable newMatchable;
        Vector3 onScreenPosition;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                // get a matchable from the pool
                newMatchable = pool.GetRandomMatchable();

                // position the matchable on screen
//                newMatchable.transform.position = transform.position + new Vector3(1.35f*x, 1.35f*y);
                onScreenPosition = transform.position + new Vector3(1.35f * x, 1.35f * y);
                newMatchable.transform.position = onScreenPosition + offScreenOffset;

                // activate the matchable
                newMatchable.gameObject.SetActive(true);

                // tell matchable where it is on the grid
                newMatchable.position = new Vector2Int(x, y);

                // place the matchable in the grid
                PutItemAt(newMatchable, x, y);

                int type = newMatchable.Type;

                while(!allowMatches && IsPartOfAMatch(newMatchable)) 
                {
                    // change the matchable's type
                    if (pool.NextType(newMatchable) == type)
                    {
                        Debug.LogWarning("Failed to find a matchable type at (" + x + ", " + y + ")");
                        Debug.Break();
                        break;
                    }

                }

                // move the matchable to its on screen position
                StartCoroutine(newMatchable.MoveToPostion(onScreenPosition));

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // Check if the matchable being populated is part of a match or not
    private bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0,
            verticalMatches = 0;

        // first look to the left 
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);

        // right
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
        {
            return true;
        }

        // up
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);

        // down
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
        {
            return true;
        }

        return false;
    }

    // Count the number of matches on the grid starting from the matchable to be match moving in the direction
    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;

        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position) && GetItemAt(position).Type == toMatch.Type)
        {
            ++matches;
            position += direction;
        }
        return matches;
    }
    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        // Make a local copy of what we are swapping so Cursor doesn't overwrite
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        // yield until matchables animate swapping
        yield return StartCoroutine(Swap(copies));

        // check for a valid match
        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);
        /*
        * TODO : complete match validation!
        */
        if (matches[0] != null)
        {
            Debug.Log(matches[0]);
        }
        if (matches[1] != null)
        {
            Debug.Log(matches[1]);
        }

        //  if there's no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            StartCoroutine(Swap(copies));
        }
    }
    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);

        Match horizontalMatch,
              verticalMatch;

        // horizontal match
        horizontalMatch = GetMatchesInDirection(toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(toMatch, Vector2Int.right));
        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
        }

        // vertical match
        verticalMatch = GetMatchesInDirection(toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(toMatch, Vector2Int.down));
        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
        }

        if (match.Count == 1)
            return null;

        return match;
    }
    // Add each matching matchable in the direction to a match and return it
    private Match GetMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();

        Vector2Int position = toMatch.position + direction;
        Matchable next;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);

            if (next.Type == toMatch.Type && next.Idle)
            {
                match.AddMatchable(GetItemAt(position));
                position += direction;
            }
            else
                break;
        }
        return match;
    }
    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        // swap them in the grid data structure
        SwapItemsAt(toBeSwapped[0].position, toBeSwapped[1].position);

        // tell the matchables their new positions
        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;


        // get the world positions of both
        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;

        // move them to their new positions on screen
                     StartCoroutine(toBeSwapped[0].MoveToPostion(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPostion(worldPosition[0]));
    }
}
