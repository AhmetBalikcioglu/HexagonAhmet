using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    [Header("Board Related")]
    public int column;
    public int row;
    public float targetY;
    public bool isMatched = false;
    public bool needsToMove;
    public GameObject[] selectedTiles;
    public float turningWay;
    public bool isTurning;

    private MatchScript matchManager;
    private BoardScript board;
    private InputScript inputManager;
    private float xOffSet = 0.914f;
    private float yOffSet = 1.06f;
    private int turn1;
    private int turn2;
    private float rotateSpeed = 240f;
    private Vector2 tempPosition;
    
    [Header("Bomb Related")]
    public bool isBomb;


    void Start()
    {
        board = FindObjectOfType<BoardScript>();
        matchManager = FindObjectOfType<MatchScript>();
        inputManager = FindObjectOfType<InputScript>();
        selectedTiles = new GameObject[3];
        isTurning = false;
        needsToMove = true;
        turn1 = 0;
        turn2 = 0;
        //column = Convert.ToInt32(transform.position.x / xOffSet);
        //row = column % 2 == 0 ? Convert.ToInt32(transform.position.y / yOffSet) : Convert.ToInt32((transform.position.y - 0.53f) / yOffSet);
    }
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
    // Update is called once per frame
    void FixedUpdate()
    {
        if (isTurning)
        {
            StartCoroutine(MovePiecesCo());
        }
    }
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
            if (turn1 == 25)
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
                else if (turn2 < 75)
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
                    Debug.Log("MovePiecesCo");
                }
            }
        }
    }
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

    void FindMatches()
    {
        matchManager.MatchSearch();
    }
}
