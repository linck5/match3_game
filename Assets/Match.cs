using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MatchShape
{
    H_LINE, V_LINE, L_OR_T
}

public class Match {


    public List<Tile> Tiles;
    public Tile SourceTile;
    public MatchShape MatchShape;
    public Tile IntersectingTile, TopTile, BotTile, LeftTile, RightTile;

    public Match(Tile sourceTile, List<Tile> tiles)
    {
        this.Tiles = tiles;
        this.SourceTile = sourceTile;
        FigureOtherMatchData();
    }

    //figure the match shape, top, bot, left, right and intersecting tiles
    void FigureOtherMatchData()
    {
        int? repeatingX = null, repeatingY = null;
        Tile topTile = null, botTile = null, leftTile = null, rightTile = null;

        GridPosition lastGridPos = GridPosition.zero;

        for (int i = 0; i < Tiles.Count; i++)
        {
            Tile t = Tiles[i];

            if(topTile == null && botTile == null && leftTile == null && rightTile == null) {
                topTile=botTile=leftTile=rightTile = t;
            }
            if (t.gridPos.x > rightTile.gridPos.x)  rightTile = t;
            if (t.gridPos.x < leftTile.gridPos.x)   leftTile = t;
            if (t.gridPos.y < topTile.gridPos.y)    topTile = t;
            if (t.gridPos.y > botTile.gridPos.y)    botTile = t;

            if (i > 0)
            {
                if (t.gridPos.x == lastGridPos.x) repeatingX = t.gridPos.x;
                if (t.gridPos.y == lastGridPos.y) repeatingY = t.gridPos.y;
            }

            lastGridPos = t.gridPos;
        }

        if (repeatingX != null && repeatingY != null)
        {
            IntersectingTile = Tiles[0].board.Tiles[repeatingX ?? int.MinValue][repeatingY ?? int.MinValue];
            MatchShape = MatchShape.L_OR_T;
        }
        else if (repeatingX != null)
        {
            leftTile = rightTile = null;
            MatchShape = MatchShape.V_LINE;
        }
        else
        {
            topTile = botTile = null;
            MatchShape = MatchShape.H_LINE;
        }
        this.TopTile = topTile;
        this.BotTile = botTile;
        this.LeftTile = leftTile;
        this.RightTile = rightTile;
    }

    public Vector2 CalculateMatchCenter()
    {
        Board board = Tiles[0].board;
        
        switch (MatchShape)
        {
            case MatchShape.L_OR_T: return 
                board.GridPos2WorldPos(IntersectingTile.gridPos);
            case MatchShape.H_LINE:
                return 
                new Vector2(
                    (RightTile.transform.position.x - LeftTile.transform.position.x) / 2 + LeftTile.transform.position.x,
                    Tiles[0].transform.position.y
                    );
            case MatchShape.V_LINE: return 
                new Vector2(
                    Tiles[0].transform.position.x,
                    (TopTile.transform.position.y - BotTile.transform.position.y) / 2 + BotTile.transform.position.y
                    );
            default: throw new System.Exception("wrong match shape");
        }
    }
}

