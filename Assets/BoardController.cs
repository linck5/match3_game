using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class BoardController {
    
    public Board board;
    
    public BoardController(Board board)
    {
        this.board = board;
    }

    public Dictionary<string, Sprite> LoadTileset(string path)
    {
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        Sprite[] tilesetSpriteList = Resources.LoadAll<Sprite>(path);

        for (int i = 0; i < tilesetSpriteList.Length; i++)
        {
            string spriteName = tilesetSpriteList[i].name;
            Tile.idToSpriteNameDic.Add(i, spriteName);
            sprites.Add(spriteName, tilesetSpriteList[i]);
        }
        return sprites;
    }

    public void CenterCameraOnBoard(Camera camera)
    {
        float tileWidthInUnits = 1f;
        float tileHeightInUnits = 1f;

        camera.transform.position = new Vector3(
            (board.Position.x + board.Width) / 2f - tileWidthInUnits / 2,
            (board.Position.y - board.Height) / 2f + tileHeightInUnits / 2,
            camera.transform.position.z
            );
    }

    public void DestroyMatches(Match match)
    {
        List<Match> singleMatchList = new List<Match>();
        singleMatchList.Add(match);
        DestroyMatches(singleMatchList);
    }

    public void DestroyMatches(List<Match> matches = null)
    {
        if (matches == null)
        {
            matches = BoardProcessor.inst.FindMatches(board);
        }

        if (matches.Count == 0) return;


        List<int> columnsWithDestroyedTilesIndexes = new List<int>();

        foreach (Match match in matches)
        {
            foreach (Tile tile in match.Tiles)
            {
                GameManager.Destroy(tile.gameObject);
                if (!columnsWithDestroyedTilesIndexes.Contains(tile.gridPos.x))
                {
                    columnsWithDestroyedTilesIndexes.Add(tile.gridPos.x);
                }

            }
        }


        //issue here
        Action onTilesFallComplete = () =>
        {


            GameManager.inst.StartCoroutine(test());
            
        };

        GameManager.inst.StartCoroutine(MakeTilesFall(onTilesFallComplete, columnsWithDestroyedTilesIndexes));

    }

    //not because something wasnt destroyed yet it seems
    IEnumerator test()
    {
        yield return null;
        yield return null;

        HighlightMatches(BoardProcessor.inst.FindMatches(this.board), () => { DestroyMatches(); });
    }

    void MakeTilleFallToPos(Tile tile, GridPosition pos, Action onFallComplete = null)
    {
        TileProcessor.inst.AssignTileNewPosition(board, ref tile, pos);

        Vector2 initialPosition = tile.transform.position;
        Vector2 targetPosition = board.GridPos2WorldPos(pos);

        float distance = Vector2.Distance(initialPosition, targetPosition);

        Action onMoveComplete = () =>
        {
            if (onFallComplete != null)
            {
                onFallComplete();
            }
        };

        MoveTile(tile, initialPosition, targetPosition, 0.5f * distance, PC2D.EasingFunctions.EaseOutBounce, TileState.FALLING, onMoveComplete);
    }

    void MoveTile(Tile tile, Vector2 start, Vector2 end, float duration, PC2D.EasingFunctions.EasingFunc easing, TileState state, Action onComplete = null)
    {
        tile.state = state;
        Timer timer = new Timer(0.5f, TimerType.ONE_TIME);
        timer.OnUpdate = (() => {
            tile.gameObject.transform.position = new Vector2(
                easing(start.x, end.x, timer.Progress),
                easing(start.y, end.y, timer.Progress)
                );
        });
        timer.OnComplete = (() =>
        {
            tile.transform.position = end;
            tile.state = TileState.IDLE;
            if (onComplete != null)
            {
                onComplete();
            }
        });
    }

    public void SwapTiles(Tile a, Tile b, Action onComplete = null)
    {
        if (a.state != TileState.IDLE || b.state != TileState.IDLE) return;

        Vector2 aInitialPosition = a.gameObject.transform.position;
        Vector2 aTargetPosition = board.GridPos2WorldPos(b.gridPos);

        Vector2 bInitialPosition = b.gameObject.transform.position;
        Vector2 bTargetPosition = board.GridPos2WorldPos(a.gridPos);

        GridPosition aNewGridPos = b.gridPos;
        GridPosition bNewGridPos = a.gridPos;

        TileProcessor.inst.AssignTileNewPosition(board, ref a, aNewGridPos);
        TileProcessor.inst.AssignTileNewPosition(board, ref b, bNewGridPos, false);
        
        int onTileMovementCompleteCalls = 0;
        Action onTileMovementComplete = () =>
        {
            onTileMovementCompleteCalls++;
            if (onTileMovementCompleteCalls >= 2)
            {
                if(onComplete != null)
                {
                    onComplete();
                }
            }
        };

        MoveTile(a, aInitialPosition, aTargetPosition, 0.5f, PC2D.EasingFunctions.EaseOutBack, TileState.SWAPPING, onTileMovementComplete);
        MoveTile(b, bInitialPosition, bTargetPosition, 0.5f, PC2D.EasingFunctions.EaseOutBack, TileState.SWAPPING, onTileMovementComplete);
    }

    public void MoveTileToItsGridPosition(Tile t)
    {
        MoveTile(t, t.transform.position, board.GridPos2WorldPos(t.gridPos), 0.5f, PC2D.EasingFunctions.EaseInOutSine, TileState.SWAPPING);
    }

    public void HighlightMatch(Match match, Action onComplete)
    {
        Debug.Log("highmatch");

        if(match.Tiles.Count == 0)
        {
            onComplete();
            return;
        }

        List<Tile> highlightOrder = match.Tiles;
        //highlightOrder.OrderBy(t => t.ManhattanDistanceFromTile(m.sourceTile));

        highlightOrder = (  from t in highlightOrder
                            orderby t.ManhattanDistanceFromTile(match.SourceTile)
                            select t).ToList<Tile>();
        
        GameObject matchHighlightGO = new GameObject("matchGroup");
        matchHighlightGO.transform.position = match.CalculateMatchCenter();

        foreach(Tile t in match.Tiles)
        {
            t.transform.parent = matchHighlightGO.transform;
            t.GetComponent<SpriteRenderer>().sortingOrder = 2;
        }
        matchHighlightGO.transform.parent = board.TilesGO.transform;


        int iterator = 0;
        int completedHighlights = 0;

        PC2D.EasingFunctions.EasingFunc scaleEasing = PC2D.EasingFunctions.EaseInOutExpo;
        float maxScale = 1.1f;

        Timer matchScaleEffectTimer = new Timer(0.5f, TimerType.ONE_TIME);
        matchScaleEffectTimer.OnUpdate = () =>
        {
            matchHighlightGO.transform.localScale = new Vector2(
                scaleEasing(1, maxScale, matchScaleEffectTimer.Progress),
                scaleEasing(1, maxScale, matchScaleEffectTimer.Progress)
                );
            
        };
        matchScaleEffectTimer.OnComplete = () =>
        {
            if(completedHighlights >= highlightOrder.Count) onComplete();

            matchHighlightGO.transform.localScale = new Vector2(
                scaleEasing(1, maxScale, matchScaleEffectTimer.Progress),
                scaleEasing(1, maxScale, matchScaleEffectTimer.Progress)
                );
        };

        Timer hlIterationTimer = new Timer(0.1f, TimerType.PERSISTENT);
        hlIterationTimer.OnComplete = () =>
        {
            if(iterator >= highlightOrder.Count)
            {
                hlIterationTimer.Destroy();
            }
            else
            {
                Timer hlTimer = new Timer(0.33f, TimerType.ONE_TIME);
                Tile t = highlightOrder[iterator];

                GameObject highlightGO = new GameObject("TileHighlight");
                highlightGO.transform.position = t.transform.position;
                SpriteRenderer highlightGORenderer = highlightGO.AddComponent<SpriteRenderer>();
                highlightGORenderer.sprite = board.SpriteData.HighLightSprite;
                highlightGORenderer.sortingLayerName = "TileHighlight"; //FIXME make a layer system
                highlightGORenderer.color = new Color(1, 1, 1, 0);
                highlightGO.transform.parent = t.transform;
                highlightGO.transform.localScale = Vector2.one;

                PC2D.EasingFunctions.EasingFunc alphaEasing = PC2D.EasingFunctions.EaseInOutExpo;
                float maxAlpha = 0.5f;

                hlTimer.OnUpdate = () =>
                {
                    highlightGORenderer.color = new Color(1, 1, 1, alphaEasing(0, maxAlpha, hlTimer.Progress));
                };
                hlTimer.OnComplete = () =>
                {
                    highlightGORenderer.color = new Color(1, 1, 1, alphaEasing(0, maxAlpha, hlTimer.Progress));

                    completedHighlights++;
                    if(completedHighlights >= highlightOrder.Count && matchScaleEffectTimer.IsComplete)
                    {
                        onComplete();
                    }
                };
            }

            iterator++;
        };
    }

    public void HighlightMatches(List<Match> matches, Action onComplete)
    {
        int completedHighlights = 0;
        foreach (Match match in matches)
        {
            HighlightMatch(match, () =>
            {
                //TODO make highlilght matcheS
                completedHighlights++;
                if (completedHighlights >= matches.Count)
                {
                    onComplete();
                }
            });
        }
    }

    public void PerformTileMove(Tile t, BoardInput.SwipeDirection dir)
    {
        Tile otherTile = null;

        Action onSwapComplete = () =>
        {
            List<Match> matches = BoardProcessor.inst.FindMatches(board, t);
            if(matches.Count > 0)
            {
                HighlightMatches(matches, () => { DestroyMatches(matches); });
                
            }
            else
            {
                SwapTiles(t, otherTile);
            }
        };
        
        GridPosition otherTilePos = t.gridPos + BoardInput.SwapDirectionToGridOffset(dir);
        if (board.GridPositionIsWithinBounds(otherTilePos))
        {
            otherTile = board.Tiles[otherTilePos.x][otherTilePos.y];

            SwapTiles(t, otherTile, onSwapComplete);
        }
        else
        {
            MoveTileToItsGridPosition(t);
        }
    }

    public void SpawnBoard()
    {
        for (int i = 0; i < board.Width; i++) // i = X grid position
        {
            for (int j = 0; j < board.Height; j++) // j = Y grid position
            {
                board.Tiles[i][j] = TileProcessor.inst.CreateNewTile(board, new GridPosition(i, j));

            }
        }

    }

    public void RandomizeBoard()
    {
        for (int i = 0; i < board.Width; i++) // i = X grid position
        {
            for (int j = 0; j < board.Height; j++) // j = Y grid position
            {
                Tile t = board.Tiles[i][j];
                TileProcessor.inst.RandomizeTileID(board, ref t);
                TileProcessor.inst.SetTileSpriteToMachItsValue(board.SpriteData.TileSprites, ref t);
            }
        }
    }

   

    IEnumerator MakeTilesFall(Action onComplete, List<int> specificColumns = null)
    {
        if(specificColumns == null)
        {
            specificColumns = new List<int>();
            for (int i = 0; i < board.Width; i++) specificColumns.Add(i);
        }

        //jump a frame to avoid dealing with tiles that will be destoyed next frame
        yield return null;

        int callBacksCalled = 0;
        Action columnFallCallback = () =>
        {
            callBacksCalled++;
            if(callBacksCalled == specificColumns.Count)
            {
                onComplete();
            }
        };

        foreach(int column in specificColumns)
        {
            MakeTilesFallOnColumn(column, columnFallCallback);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void MakeTilesFallOnColumn(int column, Action onFallsComplete = null)
    {
        int tilesToFall = 0;
        int i = board.Height - 1;

        int onTileFallCompleteCalls = 0;
        int requiredOnTileFallCompleteCalls = 0;

        Action onTileFallComplete = () =>
        {
            onTileFallCompleteCalls++;
            if (onTileFallCompleteCalls >= requiredOnTileFallCompleteCalls)
            {
                if (onFallsComplete != null)
                {
                    onFallsComplete();
                }
            }
        };

        while (i >= 0)
        {
            Tile t = board.Tiles[column][i];

            if (t == null)
            {
                tilesToFall++;
            }
            else
            {
                GridPosition targetPos;
                targetPos.x = column;
                targetPos.y = i + tilesToFall;

                MakeTilleFallToPos(t, targetPos, onTileFallComplete);
                requiredOnTileFallCompleteCalls++;
            }

            i--;
        }

        for (int i2 = 0; i2 < tilesToFall; i2++)
        {
            Tile t = TileProcessor.inst.CreateNewTile(board, new GridPosition(column, i2));
            board.Tiles[column][i2] = t;

            t.transform.position = board.GridPos2WorldPos(new GridPosition(column, i2 - tilesToFall));

            MakeTilleFallToPos(t, t.gridPos, onTileFallComplete);
            requiredOnTileFallCompleteCalls++;
        }
    }

    
    public void UncolorizeBoard()
    {
        foreach (List<Tile> columns in board.Tiles)
        {
            foreach (Tile t in columns)
            {
                t.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    public void ColorizeMatches()
    {
        UncolorizeBoard();
        List<Match> matches = BoardProcessor.inst.FindMatches(board);
        
        foreach(Match m in matches)
        {
            Color randomColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);

            foreach (Tile t in m.Tiles)
            {
                t.gameObject.GetComponent<SpriteRenderer>().color = randomColor;
            }
        }


    }

}
