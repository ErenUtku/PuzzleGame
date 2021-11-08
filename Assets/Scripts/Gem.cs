using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    //[HideInInspector]
    public Vector2Int posIndex;
    //[HideInInspector]
    public Board board;

    private Vector2 firstTouchPos;
    private Vector2 finalTouchPos;

    private bool mousePressed;
    private float swipeAngle = 0;


    private Gem otherGem;

    public enum GEMTYPE{Blue,Green,Red,Orange,Purple,bomb}

    public GEMTYPE type;

    public bool isMatched;

    private Vector2Int previousPos;

    public GameObject destroyEffect;

    public int blastSize = 1;


    void Update()
    {
        
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, board.gemMoveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(posIndex.x, (posIndex.y), 0f);
            board.AllGems[posIndex.x, posIndex.y] = this;          
        }


        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            if (board.currentstate == Board.BOARDSTATE.move)
            {              
                finalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    public void SetupGems(Vector2Int pos, Board theBoard)
    {
        posIndex = pos;
        board = theBoard;
    }
    private void OnMouseDown()
    {
        if (board.currentstate == Board.BOARDSTATE.move)
        {
            firstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }

  

    }
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPos.y - firstTouchPos.y, finalTouchPos.x - firstTouchPos.x);//atan2 giving angle
        swipeAngle = swipeAngle * 180 / Mathf.PI;

        Debug.Log(swipeAngle);

        if (Vector3.Distance(firstTouchPos, finalTouchPos) > 0.5f)
        {
            MovePieces();
        }

    }
    private void MovePieces()
    {
        previousPos = posIndex;

        if (swipeAngle < 45 && swipeAngle > -45 && posIndex.x < board.width - 1)
        {
            otherGem = board.AllGems[posIndex.x + 1, posIndex.y];
            otherGem.posIndex.x--;
            posIndex.x++;
        }

        else if(swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < board.height - 1)
        {
            otherGem = board.AllGems[posIndex.x, posIndex.y+1];
            otherGem.posIndex.y--;
            posIndex.y++;
        }
        else if(swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
        {
            otherGem = board.AllGems[posIndex.x, posIndex.y -1];
            otherGem.posIndex.y++;
            posIndex.y--;
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && posIndex.x >0)
        {
            otherGem = board.AllGems[posIndex.x - 1, posIndex.y];
            otherGem.posIndex.x++;
            posIndex.x--;
        }
      
        
        board.AllGems[posIndex.x, posIndex.y] = this;
        board.AllGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;
        StartCoroutine(CheckMove());
    }
    
 public IEnumerator CheckMove()
    {
        board.currentstate = Board.BOARDSTATE.wait;

        yield return new WaitForSeconds(0.7f);
        board.matchfinder.FindAllMatches();
        if (otherGem != null)
        {
            if (!isMatched && !otherGem.isMatched)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                board.AllGems[posIndex.x, posIndex.y] = this;
                board.AllGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;

                yield return new WaitForSeconds(0.5f);

                board.currentstate = Board.BOARDSTATE.move;
            }
            else
            {
                board.DestroyMatches();
            }
        }
    }
}
