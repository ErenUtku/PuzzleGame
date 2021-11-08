 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width = 9;
    public int height = 9;


    [SerializeField] private Vector2 posy;
    [SerializeField] private Vector2 Mainpos;

    public GameObject bgTilePrefab;

    public Gem[] gems;

    public Gem[,] AllGems;

    public float gemMoveSpeed = 1f;

    public MatchFinder matchfinder;

    public enum BOARDSTATE
    {
        wait,move
    }
    public BOARDSTATE currentstate=BOARDSTATE.move;

    public Gem bomb;
    public float bombChance = 2f;

    private void Awake()
    {
        matchfinder = FindObjectOfType<MatchFinder>();
    }
    private void Start()
    {
        AllGems = new Gem[width, height];

        Setup();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ShuffleBoard();
        }
    }

    private void Setup()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                    Vector2 pos = new Vector2(x, y);
                    GameObject bgTile = Instantiate(bgTilePrefab, pos, Quaternion.identity);
                    bgTile.transform.parent = transform;
                    bgTile.name = "Bg Tile = " + x + "," + y;

                    int gemsTouse = Random.Range(0, gems.Length);

                    int iteration = 0;

                    while (MatchesAt(new Vector2Int(x, y), gems[gemsTouse]) && iteration < 100)
                    {
                        gemsTouse = Random.Range(0, gems.Length);
                        iteration++;
                    }

                    SpawnGems(new Vector2Int(x, y), gems[gemsTouse]);
                
              
            }

        }
    }
    private void SpawnGems(Vector2Int position, Gem spawnGems)
    {

        if (Random.Range(0f, 100f) < bombChance)
        {
            spawnGems = bomb;
        }
       
            Gem _gem = Instantiate(spawnGems, new Vector3(position.x, position.y + 5, 0f), Quaternion.identity);
            _gem.transform.parent = this.transform;
            _gem.name = "Gem = " + position.x + "," + position.y;
            AllGems[position.x, position.y] = _gem;
            _gem.SetupGems(position, this);
        
     }
    bool MatchesAt(Vector2Int posToCheck, Gem gemToCheck)
    {

        if (posToCheck.x > 1)
        {
            if (AllGems[posToCheck.x - 1, posToCheck.y].type == gemToCheck.type && AllGems[posToCheck.x - 2, posToCheck.y].type == gemToCheck.type)
            {
                return true;
            }
        }

        if (posToCheck.y > 1)
        {
            if (AllGems[posToCheck.x, posToCheck.y - 1].type == gemToCheck.type && AllGems[posToCheck.x, posToCheck.y - 2].type == gemToCheck.type)
            {
                return true;
            }
        }

        return false;
    }
    private void DestroyMatchedGemAt(Vector2Int pos)
    {
        if (AllGems[pos.x, pos.y] != null)
        {
            if (AllGems[pos.x, pos.y].isMatched)
            {

                Instantiate(AllGems[pos.x, pos.y].destroyEffect, new Vector2(pos.x, pos.y), Quaternion.identity);
                Destroy(AllGems[pos.x, pos.y].gameObject);
                AllGems[pos.x, pos.y] = null;
            }
        }
    }
    public void DestroyMatches()
    {
        for (int i = 0; i < matchfinder.currentMatches.Count; i++)
        {
            if (matchfinder.currentMatches[i] != null)
            {
                DestroyMatchedGemAt(matchfinder.currentMatches[i].posIndex);
            }
        }
        StartCoroutine(DecreaseRow());
    }
    private IEnumerator DecreaseRow()
    {
        yield return new WaitForSeconds(0.2f);
        int nullCounter = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (AllGems[x, y] == null)
                {
                    nullCounter++;
                }
                else if (nullCounter > 0)
                {
                    
                    AllGems[x, y].posIndex.y -= nullCounter;
                    
                    AllGems[x, y - nullCounter] = AllGems[x, y];

                    AllGems[x, y] = null;

                }
            }
            nullCounter = 0;
        }

        StartCoroutine(FillBoard());
    }
    private IEnumerator FillBoard()
    {
        yield return new WaitForSeconds(0.5f);
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
        matchfinder.FindAllMatches();
        if (matchfinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            currentstate = BOARDSTATE.move;
        }
    }
    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (AllGems[x, y] == null)
                {
                    int gemtoUse = Random.Range(0, gems.Length);
                    SpawnGems(new Vector2Int(x, y), gems[gemtoUse]);
                }
            }
        }

        CheckMissPlacedGems();
    }

    private void CheckMissPlacedGems()
    {
        List<Gem> foundGems = new List<Gem>();
        foundGems.AddRange(FindObjectsOfType<Gem>());
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundGems.Contains(AllGems[x, y]))
                {
                    foundGems.Remove(AllGems[x, y]);
                }
            }
        }
        foreach(Gem g in foundGems)
        {
            Destroy(g.gameObject);
        }
    }
    public void ShuffleBoard()
    {
        if (currentstate != BOARDSTATE.wait)
        {
            currentstate = BOARDSTATE.wait;
            List<Gem> listAllGem = new List<Gem>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    listAllGem.Add(AllGems[x, y]);
                    AllGems[x, y] = null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int gemtoUse = Random.Range(0, listAllGem.Count);

                    int iteration = 0;
                    while(MatchesAt(new Vector2Int(x,y), listAllGem[gemtoUse])&&iteration>100&& listAllGem.Count>1)
                    {
                        iteration++;
                        gemtoUse = Random.Range(0, listAllGem.Count);
                    }
                    listAllGem[gemtoUse].SetupGems(new Vector2Int(x, y), this);
                    AllGems[x, y] = listAllGem[gemtoUse];
                    listAllGem.RemoveAt(gemtoUse);
                }
            }

        }
        StartCoroutine(FillBoard());
    }
}

