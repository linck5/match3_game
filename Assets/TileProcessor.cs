using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileProcessor {

    private static TileProcessor instance = null;
    public static TileProcessor inst
    {
        get { if (instance == null) { instance = new TileProcessor(); } return instance; }
    }

    public void ReactToInput(BoardController controller, GridPosition tileGridPos, Vector2 mouseWorldRelativePos, float maxMovementMagnitude, float movementReductionFactor)
    {
        Vector2 movement = mouseWorldRelativePos * movementReductionFactor;

        float progress = Mathf.Clamp01(movement.magnitude / maxMovementMagnitude);

        movement = new Vector2(
            PC2D.EasingFunctions.EaseOutSine(Vector2.zero.x, maxMovementMagnitude * movement.normalized.x, progress),
            PC2D.EasingFunctions.EaseOutSine(Vector2.zero.y, maxMovementMagnitude * movement.normalized.y, progress)
            );

        Tile targetTile = controller.board.Tiles[tileGridPos.x][tileGridPos.y];

        targetTile.transform.position = controller.board.GridPos2WorldPos(targetTile.gridPos) + movement;
    }

    public Tile CreateNewTile(Board board, GridPosition pos)
    {
        GameObject tileGO = new GameObject();
        tileGO.transform.SetParent(board.TilesGO.transform);


        SpriteRenderer tileRenderer = tileGO.AddComponent<SpriteRenderer>();

        Tile tile = tileGO.AddComponent<Tile>();
        tile.gridPos.x = pos.x;
        tile.gridPos.y = pos.y;
        tile.board = board;

        RandomizeTileID(board, ref tile);
        SetTileSpriteToMachItsValue(board.SpriteData.TileSprites, ref tile);

        tileGO.transform.position = new Vector2(
                        (pos.x / (float)tile.board.SpriteData.PixelsPerUnit) * tile.board.SpriteData.Width,
                        (-pos.y / (float)tile.board.SpriteData.PixelsPerUnit) * tile.board.SpriteData.Height
                        );

        SetUpTileName(ref tile);

        return tile;
    }

    public void SetUpTileName(ref Tile tile)
    {
        tile.gameObject.name =
            "Tile_P_" +
            tile.gridPos.x.ToString().PadLeft(2, '0') +
            "_" +
            tile.gridPos.y.ToString().PadLeft(2, '0');
    }

    public void SetTileSpriteToMachItsValue(Dictionary<string, Sprite> tileSprites, ref Tile tile)
    {
        string spriteName = "";

        switch (tile.id)
        {
            case TileType.C_BLUE : spriteName = "C_Blue"; break;
            case TileType.C_GREEN : spriteName = "C_Green"; break;
            case TileType.C_RED : spriteName = "C_Red"; break;
            case TileType.C_YELLOW : spriteName = "C_Yellow"; break;
            case TileType.D_BLACK : spriteName = "D_Black"; break;
            case TileType.D_COPPER : spriteName = "D_Copper"; break;
            case TileType.D_DARK_BLUE : spriteName = "D_DarkBlue"; break;
            case TileType.D_WHITE : spriteName = "D_White"; break;
            case TileType.T_CYAN : spriteName = "T_Cyan"; break;
            case TileType.T_ORANGE : spriteName = "T_Orange"; break;
            case TileType.T_PURPLE : spriteName = "T_Purple"; break;
            case TileType.T_YELLOW : spriteName = "T_Yellow"; break;
        }

        tile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteName];

    }

    public void AssignTileNewPosition(Board board, ref Tile tile, GridPosition pos, bool nullifyPreviousPosition = true)
    {
        if (nullifyPreviousPosition && board.GridPositionIsWithinBounds(tile.gridPos))
        {
            board.Tiles[tile.gridPos.x][tile.gridPos.y] = null;
        }
        board.Tiles[pos.x][pos.y] = tile;

        tile.gridPos = pos;
        SetUpTileName(ref tile);

    }

    public void RandomizeTileID(Board board, ref Tile tile)
    {


        tile.id = board.PossibleTileIDs[ UnityEngine.Random.Range(0, board.PossibleTileIDs.Count) ];


        
    }

    public void CycleTileID(Board board, ref Tile tile)
    {
        int index = board.PossibleTileIDs.IndexOf(tile.id) + 1;
        
        if (index >= board.PossibleTileIDs.Count)
        {
            index = 0;
        }
        tile.id = board.PossibleTileIDs[index];
    }

}
