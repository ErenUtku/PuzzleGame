using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchFinder : MonoBehaviour
{
    private Board board;
    public List<Gem> currentMatches= new List<Gem>();

    private Gem currentGem;

    private Gem leftGem;
    private Gem rightGem;
    private Gem aboveGem;
    private Gem bellowGem;


    private void Awake()//awake because set-up before board's Start function 
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        currentMatches.Clear();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                currentGem = board.AllGems[x, y];

                if (currentGem != null)
                {
                    //SAME GEM TYPE ON X AXIS
                    if(x>0 && x < board.width - 1)
                    {
                        leftGem = board.AllGems[x - 1, y]; 
                        rightGem = board.AllGems[x + 1, y];

                        if (leftGem != null && rightGem != null)
                        {
                            if(leftGem.type==currentGem.type&& rightGem.type == currentGem.type)
                            {
                               currentGem.isMatched = true;
                               rightGem.isMatched = true;
                               leftGem.isMatched = true;

                                currentMatches.Add(currentGem);
                                currentMatches.Add(leftGem);
                                currentMatches.Add(rightGem);
                            }
                        }
                    }

                    //SAME GEM TYPE ON Y AXIS
                    if (y > 0 && y < board.height - 1)
                    {
                        aboveGem = board.AllGems[x, y+1];
                        bellowGem = board.AllGems[x, y-1];

                        if (aboveGem != null && bellowGem != null)
                        {
                            if (aboveGem.type == currentGem.type && bellowGem.type == currentGem.type)
                            {
                                currentGem.isMatched = true;
                                aboveGem.isMatched = true;
                                bellowGem.isMatched = true;

                                currentMatches.Add(currentGem);
                                currentMatches.Add(aboveGem);
                                currentMatches.Add(bellowGem);
                            }
                        }
                    }
                }
            }

        }
        if (currentMatches.Count>0)
        {
            currentMatches = currentMatches.Distinct().ToList();
        }
        CheckForBombs();
    }
    public void CheckForBombs()
    {
        for (int i = 0; i < currentMatches.Count; i++)
        {
            Gem gem = currentMatches[i];
            int x = gem.posIndex.x;
            int y = gem.posIndex.y;

            if (gem.posIndex.x > 0)
            {
                if (board.AllGems[x - 1, y] != null)
                {
                    if (board.AllGems[x - 1, y].type == Gem.GEMTYPE.bomb)
                    {
                       
                        MarkBombArea(new Vector2Int(x - 1, y),board.AllGems[x-1,y]);
                    }
                }
            }

            if (gem.posIndex.x < board.width-1)
            {
                if (board.AllGems[x + 1, y] != null)
                {
                    if (board.AllGems[x + 1, y].type == Gem.GEMTYPE.bomb)
                    {

                        MarkBombArea(new Vector2Int(x + 1, y), board.AllGems[x + 1, y]);
                    }
                }
            }
            if (gem.posIndex.y > 0)
            {
                if (board.AllGems[x, y - 1] != null)
                {
                    if (board.AllGems[x, y - 1].type == Gem.GEMTYPE.bomb)
                    {

                        MarkBombArea(new Vector2Int(x, y - 1), board.AllGems[x, y - 1]);
                    }
                }
            }

            if (gem.posIndex.y < board.height - 1)
            {
                if (board.AllGems[x, y + 1] != null)
                {
                    if (board.AllGems[x, y + 1].type == Gem.GEMTYPE.bomb)
                    {

                        MarkBombArea(new Vector2Int(x, y + 1), board.AllGems[x, y + 1]);
                    }
                }
            }
        }
    }

    public void MarkBombArea(Vector2Int bombpos,Gem thebomb)
    {
        for (int x= bombpos.x-thebomb.blastSize; x <= bombpos.x + thebomb.blastSize; x++)
        {
            for (int y = bombpos.y - thebomb.blastSize; y <= bombpos.y + thebomb.blastSize; y++)
            {
                if(x>=0 && x < board.width && y >= 0 && y < board.height)
                {
                    if (board.AllGems[x, y] != null)
                    {
                        board.AllGems[x, y].isMatched = true;
                        currentMatches.Add(board.AllGems[x,y]);
                    }
                }
            }
        }
        currentMatches = currentMatches.Distinct().ToList();
    }
}
