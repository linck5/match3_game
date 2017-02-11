using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BoardInput : MonoBehaviour {

    public enum SwipeDirection { UP, DOWN, LEFT, RIGHT, NONE };

    public BoardController controller;

    Vector2 mouseDownPos;
    float mouseDownTime;

	
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
            mouseDownTime = Time.realtimeSinceStartup;
        }

        if (Input.GetMouseButton(0))
        {
            GridPosition targetGridPosition = controller.board.WorldPosition2GridPosition(Camera.main.ScreenToWorldPoint(mouseDownPos));
            if (targetGridPosition.x == -1 || targetGridPosition.y == -1) return;

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            Vector2 mouseRelPos = mouseWorldPos - (Vector2)Camera.main.ScreenToWorldPoint(mouseDownPos);

            TileProcessor.inst.ReactToInput(controller, targetGridPosition, mouseRelPos, 0.5f, 0.033f);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mouseUpPos = Input.mousePosition;
            float mouseUpTime = Time.realtimeSinceStartup;

            GridPosition targetGridPosition = controller.board.WorldPosition2GridPosition(Camera.main.ScreenToWorldPoint(mouseDownPos));
            Tile targetTile = null;
            if (controller.board.GridPositionIsWithinBounds(targetGridPosition))
            {
                targetTile = controller.board.Tiles[targetGridPosition.x][targetGridPosition.y];
            }

            if (CheckSwipeOccoured(mouseDownPos, mouseUpPos, mouseDownTime, mouseUpTime, 40f, float.MaxValue))
            {
                Vector2 dir = mouseUpPos - mouseDownPos;
                SwipeDirection swipeDir = VectorToSwipeDirection(dir);
                
                if (swipeDir != SwipeDirection.NONE && targetTile != null)
                {
                    controller.PerformTileMove(targetTile, swipeDir);
                }
            }
            else if(targetTile != null)
            {
                controller.MoveTileToItsGridPosition(targetTile);
            }

        }
    }

    public static GridPosition SwapDirectionToGridOffset(SwipeDirection dir)
    {
        switch (dir)
        {
            case BoardInput.SwipeDirection.UP:      return new GridPosition(0, -1);
            case BoardInput.SwipeDirection.DOWN:    return new GridPosition(0, +1);
            case BoardInput.SwipeDirection.LEFT:    return new GridPosition(-1, 0);
            case BoardInput.SwipeDirection.RIGHT:   return new GridPosition(+1, 0);
            default:                                return GridPosition.zero;
        }
    }

    SwipeDirection VectorToSwipeDirection(Vector2 vector)
    {
        if (vector == Vector2.zero) return SwipeDirection.NONE;

        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
        {
            return vector.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
        }
        else
        {
            return vector.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
        }
    }

    bool CheckSwipeOccoured(Vector2 downPos, Vector2 upPos, float downTime, float upTime, float minDistance, float maxTime)
    {
        float distance = Vector2.Distance(downPos, upPos);
        float timeElapsed = upTime - downTime;

        return distance > minDistance && timeElapsed < maxTime;
    }


}
