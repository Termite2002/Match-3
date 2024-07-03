using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    private ScoreManager score;

    [SerializeField] private Vector3 offScreenOffset;

    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        score = (ScoreManager)ScoreManager.Instance;
    }
    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        // List of new matchables added during population
        List<Matchable> newMatchables = new List<Matchable>();

        Matchable newMatchable;
        Vector3 onScreenPosition;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (IsEmpty(x, y))
                {
                    // get a matchable from the pool
                    newMatchable = pool.GetRandomMatchable();


                    newMatchable.transform.position = (transform.position + new Vector3(1.35f * x, 1.35f * y))
                        + offScreenOffset;

                    // activate the matchable
                    newMatchable.gameObject.SetActive(true);

                    // tell matchable where it is on the grid
                    newMatchable.position = new Vector2Int(x, y);

                    // place the matchable in the grid
                    PutItemAt(newMatchable, x, y);

                    // add the new matchable to the list 
                    newMatchables.Add(newMatchable);

                    int type = newMatchable.Type;

                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {
                        // change the matchable's type
                        if (pool.NextType(newMatchable) == type)
                        {
                            Debug.LogWarning("Failed to find a matchable type at (" + x + ", " + y + ")");
                            Debug.Break();
                            break;
                        }
                    }
                }
            }
        }

        // move each matchable to its on screen position, yielding until the last has finished 
        for (int i = 0; i < newMatchables.Count; i++)
        {
            // position the matchable on screen
            onScreenPosition = transform.position +
                new Vector3(1.35f * newMatchables[i].position.x, 1.35f * newMatchables[i].position.y);

            // move the matchable to its on screen position
            if (i == newMatchables.Count - 1)
                yield return StartCoroutine(newMatchables[i].MoveToPostion(onScreenPosition));
            else
                StartCoroutine(newMatchables[i].MoveToPostion(onScreenPosition));

            if (initialPopulation)
                yield return new WaitForSeconds(0.1f);
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

        // special cases for gems
        // if both are gems, then match everything
        if (copies[0].IsGem && copies[1].IsGem)
        {
            MatchEverything();
            yield break;
        }
        // if one is a gem, then match all matching the color of the other
        else if (copies[0].IsGem)
        {
            MatchEverythingByType(copies[0], copies[1], copies[1].Type);
            yield break;
        }
        else if (copies[1].IsGem)
        {
            MatchEverythingByType(copies[1], copies[0], copies[0].Type);
            yield break;
        }

        // check for a valid match
        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);
        /*
        * TODO : complete match validation!
        */
        if (matches[0] != null)
        {
            StartCoroutine(score.ResolveMatch(matches[0]));
        }
        if (matches[1] != null)
        {
            StartCoroutine(score.ResolveMatch(matches[1]));
        }

        //  if there's no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            if (ScanForMatches())
                StartCoroutine(FillAndScanGrid());
        }
        else
        {
            StartCoroutine(FillAndScanGrid());
        }
    }
    private IEnumerator FillAndScanGrid()
    {
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));

        // scan grid for chain reactions
        if (ScanForMatches())
        {
            // collapse, repopulate and scan again
            StartCoroutine(FillAndScanGrid());
        }
    }
    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);

        Match horizontalMatch,
              verticalMatch;

        // horizontal match
        horizontalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            // scan for the vertical branches
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }

        // vertical match
        verticalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            // scan for the horizotal branches
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }

        if (match.Count == 1)
            return null;

        return match;
    }
    // Add each matching matchable in the direction to a match and return it
    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();

        Vector2Int position = toMatch.position + direction;
        Matchable next;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);

            if (next.Type == toMatch.Type && next.Idle)
            {
                if (!tree.Contains(next))
                    match.AddMatchable(next);
                else
                    match.AddUnlisted();

                position += direction;
            }
            else
                break;
        }
        return match;
    }
    private void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach(Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, perpendicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }
        }
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
    private void CollapseGrid()
    {
        /*
         * Go through each column left to right
         * bot to up find empty space
         * then look above the empty space, rest of column
         * until find non empty space
         * Move the matchable at non emtpy space into empty space
         * then continue looking for empty spaces
         */


        for(int x = 0; x < Dimensions.x; x++)
        {
            for(int yEmpty = 0; yEmpty < Dimensions.y; yEmpty++)
            {
                if(IsEmpty(x, yEmpty))
                {
                    for(int yNotEmpty = yEmpty + 1; yNotEmpty < Dimensions.y; yNotEmpty++)
                    {
                        if(!IsEmpty(x, yNotEmpty) && GetItemAt(x, yNotEmpty).Idle)
                        {
                            // Move the matchable from NotEmpty to Empty
                            MoveMatchableToPosition(GetItemAt(x, yNotEmpty), x, yEmpty);
                            break;
                        }
                    }
                }
            }
        }
    }
    private void MoveMatchableToPosition(Matchable toMove, int x, int y) 
    {
        // move the matchable to its new position in the grid
        MoveItemTo(toMove.position, new Vector2Int(x, y));

        // update the matchable's internal grid position
        toMove.position = new Vector2Int(x, y);

        // start anim move
        StartCoroutine(toMove.MoveToPostion(transform.position + new Vector3(1.35f * x, 1.35f * y)));
    }

    
    // Scan the grid for any matches and resolve them
    private bool ScanForMatches()
    {
        bool madeAMatch = false;
        Matchable toMatch;
        Match match;

        // itrrate through the grid, looking for non-empty and idle matchables
        for(int y = 0; y < Dimensions.y; y++)
        {
            for(int x = 0; x < Dimensions.x; x++)
            {
                if (!IsEmpty(x, y))
                {
                    toMatch = GetItemAt(x, y);

                    if (!toMatch.Idle)
                        continue;

                    // try to match and resolve
                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        madeAMatch = true;
                        StartCoroutine(score.ResolveMatch(match));
                    }
                }
            } 
        }

        return madeAMatch;
    }


    // powerup match cross, match all matchables adjacent
    public void MatchAllAdjacent(Matchable powerup)
    {
        Match allAdjacent = new Match();

        for (int y = powerup.position.y - 1; y != powerup.position.y + 2; y++)
        {
            for (int x = powerup.position.x - 1; x != powerup.position.x + 2; x++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                {
                    allAdjacent.AddMatchable(GetItemAt(x, y));
                }
            }
        }

        StartCoroutine(score.ResolveMatch(allAdjacent, MatchType.cross));
    }

    // make a match of everything in row and collum
    public void MatchRowAndColumn(Matchable powerup)
    {
        Match rowAndColumn = new Match();
        
        if (powerup.OriMatch4 == Orientation.vertical)
        {
            for (int y = 0; y != Dimensions.y; y++)
            {
                if (CheckBounds(powerup.position.x, y) && !IsEmpty(powerup.position.x, y) && GetItemAt(powerup.position.x, y).Idle)
                {
                    rowAndColumn.AddMatchable(GetItemAt(powerup.position.x, y));
                }
            }
        }

        if (powerup.OriMatch4 == Orientation.horizontal)
        {
            for (int x = 0; x != Dimensions.x; x++)
            {
                if (CheckBounds(x, powerup.position.y) && !IsEmpty(x, powerup.position.y) && GetItemAt(x, powerup.position.y).Idle)
                {
                    rowAndColumn.AddMatchable(GetItemAt(x, powerup.position.y));
                }
            }
        }

        StartCoroutine(score.ResolveMatch(rowAndColumn, MatchType.match4));
    }

    // match everything on the grid with specific type, resolve
    public void MatchEverythingByType(Matchable gem, Matchable other, int type)
    {
        // TODO can resolve power up cross/4
        other.ResolveBefore();

        Match everythingByType = new Match(gem);

        for (int y = 0; y != Dimensions.y; y++)
            for (int x = 0; x != Dimensions.x; x++)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle && GetItemAt(x, y).Type == type)
                    everythingByType.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everythingByType, MatchType.match5));
        StartCoroutine(FillAndScanGrid());
    }

    // match every thing from the grid, resolve
    public void MatchEverything()
    {
        Match everything = new Match();

        for (int y = 0; y != Dimensions.y; y++)
            for (int x = 0; x != Dimensions.x; x++)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    everything.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everything, MatchType.match5));
        StartCoroutine(FillAndScanGrid());
    }
}
