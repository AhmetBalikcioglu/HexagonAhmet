using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

//GameState is used for when to get an input from the user and when to not.
public enum GameState
{
    wait,
    move
}

public class BoardScript : MonoBehaviour
{
    [Header("Board Related")]
    public GameState currentState = GameState.move;
    public int width;
    public int height;
    public float offSet = 10f;
    public float refillDelay = 0.5f;
    public bool fillBoardWorking = false;
    public GameObject[] tiles;
    public GameObject[] bombs;
    public GameObject[,] allTiles;

    private bool refillCounter = false;
    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;

    [Header("Match Related")]
    public GameObject destroyEffect;

    [Header("Managers")]
    private ScoreScript scoreManager;
    private MatchScript matchManager;
    private GameManager gameManager;

    [Header("Bomb Related")]
    private int bombCounter;
    private int bombFrequency = 200;
    
    
    void Start()
    {
        allTiles = new GameObject[width, height];
        matchManager = FindObjectOfType<MatchScript>();
        scoreManager = FindObjectOfType<ScoreScript>();
        gameManager = FindObjectOfType<GameManager>();
        bombCounter = 0;
        BoardSetUp();
    }

    //BoardSetUp function is called when the game is starting. It sets up the board with the random given tiles.
    //It gives the rows and columns to the tiles. 
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

    //MatchesAt is a helper function. It is called in BoardSetUp. It checks if there are matches for the given column, row and tile.
    //If there is a match it returns true else false.
    //It is used for not making any matches when we set up the board in the start.
    private bool MatchesAt(int column, int row, GameObject tile)
    {
        if (column > 0 && row > 0)
        {
            if (column % 2 == 1)
            {
                if (row < height - 1)
                {
                    //Odd column, left side control
                    if (allTiles[column - 1, row].tag == tile.tag && allTiles[column - 1, row + 1].tag == tile.tag)
                    {
                        return true;
                    }
                }
                //Odd column, bottom side control
                if (allTiles[column - 1, row].tag == tile.tag && allTiles[column, row - 1].tag == tile.tag)
                {
                    return true;
                }
            }
            else
            {
                //Even column, left side control
                if (allTiles[column - 1, row].tag == tile.tag && allTiles[column - 1, row - 1].tag == tile.tag)
                {
                    return true;
                }
                //Even column, bottom side control
                if (allTiles[column - 1, row - 1].tag == tile.tag && allTiles[column, row - 1].tag == tile.tag)
                {
                    return true;
                }
            }
        }
        else if (column == 1 && row == 0)
        {
            //Left bottom 3 control
            if (allTiles[column - 1, row + 1].tag == tile.tag && allTiles[column - 1, row].tag == tile.tag)
            {
                return true;
            }
        }
        return false;
    }

    //DestroyMatchesAt is a helper function. It is used to destroy the tile in the given column and tile.
    //It instantiates particle effects with the color of the destroyed tile.
    //It calls ChangeScore from ScoreManager everytime a tile is destroyed.
    //If the given column and rows tile has isMatched bool true it does all that and returns true.
    //If it is false it returns false and does nothing.
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

    //DestroyMatches function goes through all the board and gives every tiles column and row to DestroyMatchesAt function.
    //If there is a match it calls DecreaseRowCo coroutine.
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

    //DecreaseRowCo goes through the board and fills up the empty spaces with the tiles above it.
    //When it is finished it calls FillBoardCo. refillCounter is there for calling FillBoardCo once every cycle.
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

    //RefillBoard coroutine is a helper coroutine for FillBoardCo.
    //It goes through the board and Instantiates new tiles for the empty positions.
    //It checks if bombCounter passed bombFrequency and Instantiates a bomb instead of a tile.
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
                    if (bombCounter >= bombFrequency)
                    {
                        int bombToUse = Random.Range(0, bombs.Length);
                        GameObject bomb = Instantiate(bombs[bombToUse], tempPosition, Quaternion.identity);
                        allTiles[i, j] = bomb;
                        bomb.GetComponent<TileBehavior>().row = j;
                        bomb.GetComponent<TileBehavior>().column = i;
                        bomb.GetComponent<TileBehavior>().isBomb = true;
                        bomb.transform.SetParent(this.transform);
                        bombCounter = 0;
                    }
                    else
                    {
                        int tileToUse = Random.Range(0, tiles.Length);
                        GameObject tile = Instantiate(tiles[tileToUse], tempPosition, Quaternion.identity);
                        allTiles[i, j] = tile;
                        tile.GetComponent<TileBehavior>().row = j;
                        tile.GetComponent<TileBehavior>().column = i;
                        tile.transform.SetParent(this.transform);
                    }
                }
            }
        }
        yield return new WaitForSeconds(0f);
    }

    //MatchesOnBoard function is a helper function used in FillBoardCo.
    //It calls MatchSearch function from Match Manager.
    //Then it goes through the board and checks if there is a tile with isMatched value true.
    //If there is it returns true else false.
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

    //FillBoardCo coroutine is used for filling up the board using RefillBoard, checking if there is a match
    //on the board with MatchesOnBoard, destroys thoes matches with DestroyMatches, calls RefillBoard again and then
    //checks the matches with MatchesOnBoard. This loop continues till there is no matches on board.
    //When this loop finishes it checks for a deadlock using IsDeadLocked function. If there is a deadlock it calls
    //GameOver from GameManager and ends the game.
    private IEnumerator FillBoardCo()
    {
        fillBoardWorking = true;
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
        fillBoardWorking = false;
    }

    //SwitchTiles is a helper function.
    //It is used for switching tile positions on the grid. It uses given column, row and the direction to do it.
    private void SwitchTiles(int column, int row, Vector2 direction)
    {
        //Take the first piece and save it in a holder
        GameObject holder = allTiles[column + (int)direction.x, row + (int)direction.y] as GameObject;
        //Switching the first dot to be the second position
        allTiles[column + (int)direction.x, row + (int)direction.y] = allTiles[column, row];
        //Set the first tile to be the second tile
        allTiles[column, row] = holder;
    }

    //GetNearbyTilesForDeadlock is a helper function.
    //It checks the tags for the given tiles and returns true if they are the same, else not.
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

    //MatchSearchForDeadlock is almost the same function with MatchSearch in MatchScript.
    //Difference is it only checks for a potential matches and returns true if there is one.
    //It doesn't puts the tiles isMatched bool to true.
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
                    //Even column control
                    if (i % 2 == 0)
                    {
                        rightTile = allTiles[i + 1, j];
                        //Even column, highest row control
                        if (j != height - 1)
                        {
                            upTile = allTiles[i, j + 1];
                            if (GetNearbyTilesForDeadlock(currentTile, upTile, rightTile))
                            {
                                return true;
                            }

                        }
                        //Even column, lowest row control
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
                        //Odd column control without highest row
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

    //SwitchAndCheck is a helper function.
    //It switches the tile with the direction given using SwitchTiles, searches for a match using MatchSearchForDeadlock,
    //if there is a match it switches the tile back and returns true, else switches the tile back and returns false.
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

    //IsDeadLocked goes through the whole board and checks for a potential deadlock using SwitchAndCheck
    //function for every tile on the board.
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
                        //Even column
                        for (int k = -1; k < 2; k++)
                        {
                            //Column switch
                            for (int l = -1; l < 1; l++)
                            {
                                //Row switch
                                if (!(l == 0 && k == 0))
                                {
                                    if (SwitchAndCheck(i, j, new Vector2(k, l)))
                                    {
                                        return false;
                                    }
                                }
                                else if (SwitchAndCheck(i, j, new Vector2(k, l + 1)))
                                {
                                    //Above switch
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Odd column
                        for (int k = -1; k < 2; k++)
                        {
                            //Column switch
                            for (int l = 0; l < 2; l++)
                            {
                                //Row switch
                                if (!(l == 0 && k == 0))
                                {
                                    if (SwitchAndCheck(i, j, new Vector2(k, l)))
                                    {
                                        return false;
                                    }
                                }
                                else if (SwitchAndCheck(i, j, new Vector2(k, l - 1)))
                                {
                                    //Bottom switch
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

    //DestroyAll coroutine is called when the game is over.
    //It destroys all the tiles, bombs and instantiates particles effects according to the colors.
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
