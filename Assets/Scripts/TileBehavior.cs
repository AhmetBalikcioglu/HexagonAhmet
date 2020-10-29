using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    [Header("Board Related")]
    private BoardScript board;
    public int column;
    public int row;
    public float targetY;
    public bool isMatched;
    public bool needsToMove;
    public GameObject[] selectedTiles;

    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;

    [Header("Turn Related")]
    public float turningWay;
    public bool isTurning;

    private int turn1;
    private int turn2;
    private int turnCounter = 25;
    private float rotateSpeed = 240f;

    private MatchScript matchManager;
    private InputScript inputManager;    
    private Vector2 tempPosition;
    
    [Header("Bomb Related")]
    public bool isBomb;


    void Start()
    {
        board = FindObjectOfType<BoardScript>();
        matchManager = FindObjectOfType<MatchScript>();
        inputManager = FindObjectOfType<InputScript>();
        selectedTiles = new GameObject[3];
        isMatched = false;
        isTurning = false;
        needsToMove = true;
        turn1 = 0;
        turn2 = 0;
    }

    //Update checks if needsToMove bool value is true. True means tile is not where it is supposed to be
    //in position. It checks for column and row and Lerps the tile to its right position.
    private void Update()
    {
        if (needsToMove)
        {
            targetY = column % 2 == 0 ? row * yOffSet : row * yOffSet + yOffSet / 2;
            if (board.allTiles[column, row] != this.gameObject)
            {
                board.allTiles[column, row] = this.gameObject;
            }
            if (Mathf.Abs(targetY - transform.position.y) > .1f)
            {
                //Move towards target
                tempPosition = new Vector2(transform.position.x, targetY);
                transform.position = Vector2.Lerp(transform.position, tempPosition, .1f);
            }
            else
            {
                //Directly set position
                tempPosition = new Vector2(transform.position.x, targetY);
                transform.position = tempPosition;
                needsToMove = false;
            }
        }
    }

    //FixedUpdate checks isTurning value and calls MovePiecesCo coroutine.
    void FixedUpdate()
    {
        if (isTurning)
        {
            StartCoroutine(MovePiecesCo());
        }
    }

    //OnMouseDown calls MouseDown function from InputScript and gives this tile.
    public void OnMouseDown()
    {
        inputManager.MouseDown(this);
    }

    //OnMouseUp calls MouseDown function from InputScript and gives this tile.
    public void OnMouseUp()
    {
        inputManager.MouseUp(this);
    }

    //ClosestTiles function calculates the distance from firstTouchPosition to all tiles.
    //It selects the tile that is touched and 2 tiles closest to the firstTouchPosition
    //Sometimes there is odd behavior. We check that odd behavior and not let it happen with dontSelect bool.
    public void ClosestTiles()
    {
        selectedTiles[0] = this.gameObject;
        selectedTiles[2] = selectedTiles[1] = null;

        float nearestDist1 = float.MaxValue;
        float nearestDist2 = float.MaxValue;

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                float distance = Vector2.Distance(inputManager.firstTouchPosition, board.allTiles[i, j].transform.position);
                if (distance < nearestDist1 && board.allTiles[i, j] != this.gameObject)
                {

                    nearestDist2 = nearestDist1;
                    selectedTiles[2] = selectedTiles[1];
                    nearestDist1 = distance;
                    selectedTiles[1] = board.allTiles[i, j];
                }
                else if (distance < nearestDist2 && distance != nearestDist1 && board.allTiles[i, j] != this.gameObject)
                {
                    nearestDist2 = distance;
                    selectedTiles[2] = board.allTiles[i, j];
                }
            }
        }
        if ((row == selectedTiles[1].GetComponent<TileBehavior>().row && row == selectedTiles[2].GetComponent<TileBehavior>().row) || (column == selectedTiles[1].GetComponent<TileBehavior>().column && column == selectedTiles[2].GetComponent<TileBehavior>().column))
        {
            //Üst üste veya yanyana seçim olursa seçimi iptal ediyor
            inputManager.dontSelect = true;
        }
    }


    //MovePiecesCo coroutine is called in FixedUpdate if isTurning bool is true for the tile.
    //It rotates the selected 3 tiles until one of them gets a match or it is a full 360.
    //If there is a match it means 1 player move happened and if there are bombs on the screen
    //it ticks bomb timer.
    private IEnumerator MovePiecesCo()
    {
        if (selectedTiles[0] != null && selectedTiles[1] != null && selectedTiles[2] != null)
        {
            Vector3 rotatePos = (selectedTiles[0].transform.position + selectedTiles[1].transform.position + selectedTiles[2].transform.position) / 3;
            foreach (GameObject tile in selectedTiles)
            {
                tile.transform.RotateAround(rotatePos, turningWay * Vector3.forward, rotateSpeed * Time.deltaTime);
            }
            turn1++;
            turn2++;
            if (turn1 == turnCounter)
            {
                SnapTiles();
                FindMatches();
                isTurning = false;
                turn1 = 0;
                if (board.DestroyMatches())
                {
                    turn1 = turn2 = 0;
                    for (int i = 0; i < board.width; i++)
                    {
                        for (int j = 0; j < board.height; j++)
                        {
                            if (board.allTiles[i, j] != null)
                            {
                                if (board.allTiles[i, j].GetComponent<TileBehavior>().isBomb)
                                {
                                    board.allTiles[i, j].GetComponent<BombBehavior>().TickBombTimer();
                                }
                            }
                            
                        }
                    }
                }
                else if (turn2 < turnCounter * 3)
                {
                    isTurning = true;
                }
                else
                {
                    turn2 = 0;
                    for (int i = 0; i < selectedTiles.Length; i++)
                    {
                        selectedTiles[i] = null;
                    }
                    yield return new WaitForSeconds(.5f);
                    board.currentState = GameState.move;
                }
            }
        }
    }

    //SnapTiles function is called by MovePiecesCo every 120 degree rotation.
    //It calculates the new column and row for the rotating tiles and snaps them in place.
    void SnapTiles()
    {
        foreach (GameObject tile in selectedTiles)
        {
            tile.GetComponent<TileBehavior>().column = Convert.ToInt32(tile.transform.position.x / xOffSet);
            tile.GetComponent<TileBehavior>().row = Convert.ToInt32(tile.GetComponent<TileBehavior>().column % 2 == 0 ? Convert.ToInt32((tile.transform.position.y + 0.1f) / yOffSet) : Convert.ToInt32((tile.transform.position.y - 0.43f) / yOffSet));
            tile.transform.position = new Vector2(tile.GetComponent<TileBehavior>().column * xOffSet, tile.GetComponent<TileBehavior>().column % 2 == 0 ? tile.GetComponent<TileBehavior>().row * yOffSet : tile.GetComponent<TileBehavior>().row * yOffSet + yOffSet / 2);
            tile.transform.rotation = Quaternion.Euler(Vector3.zero);
            board.allTiles[tile.GetComponent<TileBehavior>().column, tile.GetComponent<TileBehavior>().row] = tile;
        }
    }

    //FindMatches is a caller function for MatchSearch function from MatchScript
    //It is called by MovePiecesCo every 120 degree rotation.
    void FindMatches()
    {
        matchManager.MatchSearch();
    }
}
