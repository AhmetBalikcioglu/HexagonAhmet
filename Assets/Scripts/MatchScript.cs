using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchScript : MonoBehaviour
{
    public List<GameObject> currentMatches = new List<GameObject>();
    private BoardScript board;
    private GameObject currentTile;
    private GameObject upTile;
    private GameObject rightTile;
    private GameObject rightUpTile;
    private GameObject rightDownTile;

    private void Start()
    {
        board = FindObjectOfType<BoardScript>(); ;
    }

    private void AddToListAndMatch(GameObject tile)
    {
        if (!currentMatches.Contains(tile))
        {
            currentMatches.Add(tile);
        }
        tile.GetComponent<TileBehavior>().isMatched = true;
    }

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
    
    public void MatchSearch()
    {
        for (int i = 0; i < board.width - 1; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.allTiles[i, j] != null)
                {
                    currentTile = board.allTiles[i, j];
                    //Çift sütun kontrolü
                    if (i % 2 == 0)
                    {
                        rightTile = board.allTiles[i + 1, j];
                        //Çift sütunlarda en üst sıra kontrolü
                        if (j != board.height - 1)
                        {
                            upTile = board.allTiles[i, j + 1];
                            GetNearbyTiles(currentTile, upTile, rightTile);
                        }
                        //Çift sütunlarda en alt sıra kontrolü
                        if (j != 0)
                        {
                            rightDownTile = board.allTiles[i + 1, j - 1];
                            GetNearbyTiles(currentTile, rightDownTile, rightTile);
                        }
                    }
                    else if (j != board.height - 1)
                    {
                        //Tek sütunlarda en üst sıra kontrolü
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
