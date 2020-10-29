using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchScript : MonoBehaviour
{
    [Header("Match Related")]
    public List<GameObject> currentMatches = new List<GameObject>();
    
    private GameObject currentTile;
    private GameObject upTile;
    private GameObject rightTile;
    private GameObject rightUpTile;
    private GameObject rightDownTile;

    private BoardScript board;

    private void Start()
    {
        board = FindObjectOfType<BoardScript>(); ;
    }

    //AddToListAndMatch is a helper function. It adds the given tile to currentMatches list
    //and turns that tiles isMatched bool to true. We use currentMatches list to see current matches in editor.
    private void AddToListAndMatch(GameObject tile)
    {
        if (!currentMatches.Contains(tile))
        {
            currentMatches.Add(tile);
        }
        tile.GetComponent<TileBehavior>().isMatched = true;
    }

    //GetNearbyTiles function checks tags for given 3 tiles. If tags are the same it gives the tiles
    //to AddToListAndMatch function. It also checks if the tiles isBomb value is true. If it is it gives
    //the tile to GetBombMatch function. And adds every tile given back to currentMatches list.
    private void GetNearbyTiles(GameObject tile1, GameObject tile2, GameObject tile3)
    {
        if (tile2 != null && tile3 != null)
        {
            if (tile1.tag == tile2.tag && tile1.tag == tile3.tag)
            {
                if (tile1.GetComponent<TileBehavior>().isBomb
                    || tile2.GetComponent<TileBehavior>().isBomb
                    || tile3.GetComponent<TileBehavior>().isBomb)
                {
                    currentMatches.Union(GetBombMatch(tile1.tag));
                }
                AddToListAndMatch(tile1);
                AddToListAndMatch(tile2);
                AddToListAndMatch(tile3);
            }
        }
    }
    
    //MatchSearh function goes through all of the tiles on the board and checks their tags for the right conditions.
    //It uses GetNearbyTiles function.
    public void MatchSearch()
    {
        for (int i = 0; i < board.width - 1; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allTiles[i, j] != null)
                {
                    currentTile = board.allTiles[i, j];
                    //Even column control
                    if (i % 2 == 0)
                    {
                        rightTile = board.allTiles[i + 1, j];
                        //Even column, highest row control
                        if (j != board.height - 1)
                        {
                            upTile = board.allTiles[i, j + 1];
                            GetNearbyTiles(currentTile, upTile, rightTile);
                        }
                        //Even column, lowest row control
                        if (j != 0)
                        {
                            rightDownTile = board.allTiles[i + 1, j - 1];
                            GetNearbyTiles(currentTile, rightDownTile, rightTile);
                        }
                    }
                    else if (j != board.height - 1)
                    {
                        //Odd column control without highest row
                        rightTile = board.allTiles[i + 1, j];
                        upTile = board.allTiles[i, j + 1];
                        rightUpTile = board.allTiles[i + 1, j + 1];
                        GetNearbyTiles(currentTile, upTile, rightUpTile);
                        GetNearbyTiles(currentTile, rightTile, rightUpTile);
                    }
                }
            }
        }
    }

    
    //GetBombMatch function checks the whole board with the given tag and matches them.
    //Matched tiles are added to the currentMatches and their isMatched bool is set to true.
    //When a bomb is matched this function is called and it matches every tile with the same color in the board.
    List<GameObject> GetBombMatch(string tag)
    {
        List<GameObject> tiles = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allTiles[i, j] != null)
                {
                    if (board.allTiles[i, j].tag == tag)
                    {
                        tiles.Add(board.allTiles[i, j]);
                        board.allTiles[i, j].GetComponent<TileBehavior>().isMatched = true;
                    }
                }
            }
        }
        return tiles;
    }
}
