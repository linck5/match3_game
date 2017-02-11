using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour {

    private static GameManager instance = null;
    public static GameManager inst
    {
        get { if (!instance) instance = GameObject.FindObjectOfType<GameManager>(); return instance; }
    }

    public Board gameBoard;
    public BoardController boardController;

    void Awake()
    {

        List<TileType> possibleTileTypes = new List<TileType>();
        possibleTileTypes.Add(TileType.C_BLUE);
        possibleTileTypes.Add(TileType.C_GREEN);
        possibleTileTypes.Add(TileType.D_BLACK);
        possibleTileTypes.Add(TileType.D_COPPER);
        possibleTileTypes.Add(TileType.T_CYAN);
        possibleTileTypes.Add(TileType.T_YELLOW);
        possibleTileTypes.Add(TileType.C_RED);

        TileSpriteData gameBoardSpriteData = new TileSpriteData(32, 32, 32);

        gameBoard = new Board(Vector2.zero, 8, 8, possibleTileTypes, gameBoardSpriteData);

        gameBoard.BoardGO = new GameObject("Board");
        gameBoard.TilesGO = new GameObject("Tiles");
        gameBoard.TilesGO.transform.SetParent(gameBoard.BoardGO.transform);

        boardController = new BoardController(gameBoard);

        gameBoard.BoardGO.AddComponent<BoardInput>().controller = boardController;

        boardController.CenterCameraOnBoard(Camera.main);

        gameBoard.SpriteData.TileSprites = boardController.LoadTileset("tile_set");
        gameBoard.SpriteData.HighLightSprite = Resources.Load<Sprite>("highlightTest");

        boardController.SpawnBoard();

    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}
