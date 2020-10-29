using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class BoardScript : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public float offSet = 10f;
    public GameObject[] tiles;
    public GameObject[] bombs;
    public GameObject[,] allTiles;
    public GameObject destroyEffect;
    public float refillDelay = 0.5f;

    private ScoreScript scoreManager;
    private MatchScript matchManager;
    private GameManager gameManager;
    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;
    private int bombCounter;
    private bool refillCounter = false;
    
    

    void Start()
    {
        allTiles = new GameObject[width, height];
        matchManager = FindObjectOfType<MatchScript>();
        scoreManager = FindObjectOfType<ScoreScript>();
        gameManager = FindObjectOfType<GameManager>();
        bombCounter = 0;
        BoardSetUp();
    }

    private void BoardSetUp()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i * xOffSet, j * yOffSet + offSet);
                if (i %2 == 1)
                {
                    tempPosition = new Vector2(i * xOffSet, j * yOffSet + yOffSet / 2f + offSet);
                }
                int tileToUse = Random.Range(0, tiles.Length);
                int maxIterations = 0;
                while (MatchesAt(i, j, tiles[tileToUse]) && maxIterations <100)
                {
                    tileToUse = Random.Range(0, tiles.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                GameObject tile = Instantiate(tiles[tileToUse], tempPosition, Quaternion.identity);
                tile.GetComponent<TileBehavior>().row = j;
                tile.GetComponent<TileBehavior>().column = i;
                tile.transform.SetParent(this.transform);
                tile.name = "Tile_" + i + "," + j;
                allTiles[i, j] = tile;
            }
        }
    }

    private bool MatchesAt(int column, int row, GameObject tile)
    {
        if (column > 0 && row > 0)
        {
            if (column % 2 == 1)
            {
                if (row < height - 1)
                {
                    //Tek sütunlarda sol taraf kontrolü
                    if (allTiles[column - 1, row].tag == tile.tag && allTiles[column - 1, row + 1].tag == tile.tag)
                    {
                        return true;
                    }
                }
                //Tek sütunlarda aşağı taraf kontrolü
                if (allTiles[column - 1, row].tag == tile.tag && allTiles[column, row - 1].tag == tile.tag)
                {
                    return true;
                }
            }
            else
            {
                //Çift sütunlarda sol taraf kontrolü
                if (allTiles[column - 1, row].tag == tile.tag && allTiles[column - 1, row - 1].tag == tile.tag)
                {
                    return true;
                }
                //Çift sütunlarda aşağı taraf kontrolü
                if (allTiles[column - 1, row - 1].tag == tile.tag && allTiles[column, row - 1].tag == tile.tag)
                {
                    return true;
                }
            }
        }
        else if (column == 1 && row == 0)
        {
            //En alt sol üclü kontrolü
            if (allTiles[column - 1, row + 1].tag == tile.tag && allTiles[column - 1, row].tag == tile.tag)
            {
                return true;
            }
        }
        return false;
    }

    private bool DestroyMatchesAt(int column, int row)
    {
        if (allTiles[column, row].GetComponent<TileBehavior>().isMatched)
        {
            matchManager.currentMatches.Remove(allTiles[column, row]);
            string colorString = allTiles[column, row].tag;
            switch (colorString)
            {
                case "PurpleTile":
                    destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.magenta;
                    break;
                case "RedTile":
                    destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.red;
                    break;
                case "BlueTile":
                    destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.blue;
                    break;
                case "YellowTile":
                    destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.yellow;
                    break;
                case "GreenTile":
                    destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.green;
                    break;
                default:
                    break;
            }
            GameObject particle = Instantiate(destroyEffect, allTiles[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allTiles[column, row]);
            allTiles[column, row] = null;
            scoreManager.ChangeScore();
            bombCounter++;
            return true;
        }
        return false;
    }
    public bool DestroyMatches()
    {
        bool tempBool = false;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] != null)
                {
                    if (DestroyMatchesAt(i, j))
                    {
                        tempBool = true;
                    }
                }
            }
        }
        if (tempBool)
        {
            StartCoroutine(DecreaseRowCo());
        }
        return tempBool;
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allTiles[i, j].GetComponent<TileBehavior>().row -= nullCount;
                    allTiles[i, j].GetComponent<TileBehavior>().needsToMove = true;
                    allTiles[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        if (!refillCounter)
        {
            StartCoroutine(FillBoardCo());
            refillCounter = true;
        }
        
    }

    private IEnumerator RefillBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i,j] == null)
                {
                    Vector2 tempPosition = new Vector2(i * xOffSet, j * yOffSet + offSet);
                    if (i % 2 == 1)
                    {
                        tempPosition = new Vector2(i * xOffSet, j * yOffSet + yOffSet / 2f + offSet);
                    }
                    if (bombCounter >= 20)
                    {
                        int bombToUse = Random.Range(0, bombs.Length);
                        GameObject bomb = Instantiate(bombs[bombToUse], tempPosition, Quaternion.identity);
                        allTiles[i, j] = bomb;
                        bomb.GetComponent<TileBehavior>().row = j;
                        bomb.GetComponent<TileBehavior>().column = i;
                        bomb.GetComponent<TileBehavior>().isBomb = true;
                        bombCounter = 0;
                    }
                    else
                    {
                        int tileToUse = Random.Range(0, tiles.Length);
                        GameObject tile = Instantiate(tiles[tileToUse], tempPosition, Quaternion.identity);
                        allTiles[i, j] = tile;
                        tile.GetComponent<TileBehavior>().row = j;
                        tile.GetComponent<TileBehavior>().column = i;
                    }
                }
            }
        }
        yield return new WaitForSeconds(0f);
    }

    private bool MatchesOnBoard()
    {
        matchManager.MatchSearch();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] != null)
                {
                    if (allTiles[i, j].GetComponent<TileBehavior>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        //currentState = GameState.wait;
        StartCoroutine(RefillBoard());
        yield return new WaitForSeconds(refillDelay);
        
        while(MatchesOnBoard())
        {
            DestroyMatches();
            yield return new WaitForSeconds(refillDelay);
            StartCoroutine(RefillBoard());
            yield return new WaitForSeconds(refillDelay);
        }
        yield return new WaitForSeconds(refillDelay);
        if (IsDeadLocked())
        {
            gameManager.GameOver();
        }
        currentState = GameState.move;
        refillCounter = false;
    }

    private void SwitchTiles(int column, int row, Vector2 direction)
    {
        //Take the first piece and save it in a holder
        GameObject holder = allTiles[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //Switching the first dot to be the second position
        allTiles[column + (int)direction.x, row + (int)direction.y] = allTiles[column, row];
        //Set the first tile to be the second tile
        allTiles[column, row] = holder;
    }
    private bool GetNearbyTilesForDeadlock(GameObject tile1, GameObject tile2, GameObject tile3)
    {
        if (tile1 != null && tile2 != null && tile3 != null)
        {
            if (tile1.tag == tile2.tag && tile1.tag == tile3.tag)
            {
                return true;
            }
        }
        return false;
    }

    private bool MatchSearchForDeadlock()
    {
        GameObject currentTile;
        GameObject upTile;
        GameObject rightTile;
        GameObject rightUpTile;
        GameObject rightDownTile;

        for (int i = 0; i < width - 1; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] != null)
                {
                    currentTile = allTiles[i, j];
                    //Çift sütun kontrolü
                    if (i % 2 == 0)
                    {
                        rightTile = allTiles[i + 1, j];
                        //Çift sütunlarda en üst sıra kontrolü
                        if (j != height - 1)
                        {
                            upTile = allTiles[i, j + 1];
                            if (GetNearbyTilesForDeadlock(currentTile, upTile, rightTile))
                            {
                                return true;
                            }

                        }
                        //Çift sütunlarda en alt sıra kontrolü
                        if (j != 0)
                        {
                            rightDownTile = allTiles[i + 1, j - 1];
                            if (GetNearbyTilesForDeadlock(currentTile, rightDownTile, rightTile))
                            {
                                return true;
                            }
                        }
                    }
                    else if (j != height - 1)
                    {
                        //Tek sütunlarda en üst sıra kontrolü
                        rightTile = allTiles[i + 1, j];
                        upTile = allTiles[i, j + 1];
                        rightUpTile = allTiles[i + 1, j + 1];
                        if (GetNearbyTilesForDeadlock(currentTile, upTile, rightUpTile))
                        {
                            return true;
                        }
                        if (GetNearbyTilesForDeadlock(currentTile, rightTile, rightUpTile))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        if (column + direction.x >= 0 && column + direction.x < width && row + direction.y >= 0 && row + direction.y < height)
        {
            SwitchTiles(column, row, direction);
            if (MatchSearchForDeadlock())
            {
                SwitchTiles(column, row, direction);
                return true;
            }
            SwitchTiles(column, row, direction);
            return false;
        }
        return false;
    }

    private bool IsDeadLocked()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] != null)
                {
                    if (i % 2 == 0)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            for (int l = -1; l < 1; l++)
                            {
                                if (!(l == 0 && k == 0))
                                {
                                    if (SwitchAndCheck(i, j, new Vector2(k, l)))
                                    {
                                        return false;
                                    }
                                }
                                else if (SwitchAndCheck(i, j, new Vector2(k, l + 1)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            for (int l = 0; l < 2; l++)
                            {
                                if (!(l == 0 && k == 0))
                                {
                                    if (SwitchAndCheck(i, j, new Vector2(k, l)))
                                    {
                                        return false;
                                    }
                                }
                                else if (SwitchAndCheck(i, j, new Vector2(k, l - 1)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }
        return true;
    }

    public IEnumerator DestroyAll()
    {
        currentState = GameState.wait;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allTiles[i, j] != null)
                {
                    string colorString = allTiles[i, j].tag;
                    switch (colorString)
                    {
                        case "PurpleTile":
                            destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.magenta;
                            break;
                        case "RedTile":
                            destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.red;
                            break;
                        case "BlueTile":
                            destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.blue;
                            break;
                        case "YellowTile":
                            destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.yellow;
                            break;
                        case "GreenTile":
                            destroyEffect.GetComponentInChildren<ParticleSystem>().startColor = Color.green;
                            break;
                        default:
                            break;
                    }
                    GameObject particle = Instantiate(destroyEffect, allTiles[i, j].transform.position, Quaternion.identity);
                    Destroy(allTiles[i, j]);
                    Destroy(particle, .5f);
                }
            }
        }
        yield return new WaitForSeconds(2f);
        Time.timeScale = 0;
    }
    
}
