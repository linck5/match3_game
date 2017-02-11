using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class TileSpriteData
{
    public int Width, Height, PixelsPerUnit;
    public Dictionary<string, Sprite> TileSprites;
    public Sprite HighLightSprite;

    public TileSpriteData(int width, int height, int pixelsPerUnit)
    {
        this.Width = width;
        this.Height = height;
        this.PixelsPerUnit = pixelsPerUnit;
    }
}

public class Board {

    public enum Axis { HORIZONTAL, VERTICAL, BOTH, NONE }


    public Vector2 Position;
    public int Width = 10;
    public int Height = 10;
    public List<TileType> PossibleTileIDs;
    public TileSpriteData SpriteData;

    public GameObject BoardGO;
    public GameObject TilesGO;

    public List<List<Tile>> Tiles = new List<List<Tile>>();

    public Board(Vector2 position, int gridWidth, int gridHeight, List<TileType> possibleTileValues, TileSpriteData spriteData)
    {
        this.Position = position;
        this.Width = gridWidth;
        this.Height = gridHeight;
        this.PossibleTileIDs = possibleTileValues;
        this.SpriteData = spriteData;

        PopulateTlesListWithNULL();
    }
    
    void PopulateTlesListWithNULL()
    {
        for (int i = 0; i < Width; i++) // i = X grid position
        {
            List<Tile> columns = new List<Tile>();
            for (int j = 0; j < Height; j++) // j = Y grid position
            {
                columns.Add(null);
            }
            Tiles.Add(columns);
        }
    }

    public bool GridPositionIsWithinBounds(GridPosition pos)
    {
        return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
    }

    public GridPosition WorldPosition2GridPosition(Vector2 worldPosition)
    {

        float pperunit = 32f;

        Vector2 tileDimensionsInPixels = new Vector2(32f, 32f);
        GridPosition gridPos;

        gridPos.x = (int)Mathf.Floor((worldPosition.x * pperunit / tileDimensionsInPixels.x) + pperunit / tileDimensionsInPixels.x / 2);
        gridPos.y = (int)Mathf.Floor((worldPosition.y * pperunit / tileDimensionsInPixels.y) + pperunit / tileDimensionsInPixels.x / 2) * -1;

        //if value is out of bounds, set it to -1
        if (gridPos.x < 0 || gridPos.x >= Width) gridPos.x = -1;
        if (gridPos.y < 0 || gridPos.y >= Height) gridPos.y = -1;

        return gridPos;
    }

    public Vector2 GridPos2WorldPos(GridPosition pos)
    {
        float pperunit = 32f;

        Vector2 tileDimensionsInPixels = new Vector2(32f, 32f);

        Vector2 worldPos = new Vector2(
            pos.x / (pperunit / tileDimensionsInPixels.x),
            (pos.y / (pperunit / tileDimensionsInPixels.y)) * -1
            );

        return worldPos;
    }

    
    
}
