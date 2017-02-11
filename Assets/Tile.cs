using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileState { IDLE, SWAPPING, FALLING }

public enum TileType
{
    C_BLUE,
    C_GREEN,
    C_RED,
    C_YELLOW,

    D_BLACK,
    D_COPPER,
    D_DARK_BLUE,
    D_WHITE,

    T_CYAN,
    T_ORANGE,
    T_PURPLE,
    T_YELLOW
}

public struct GridPosition
{
    public int x, y;
    public static GridPosition zero
    {
        get { return new GridPosition(0, 0); }
    }

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public static GridPosition operator + (GridPosition posA, GridPosition posB)
    {
       return new GridPosition(posA.x + posB.x, posA.y + posB.y);
    }

    public static GridPosition operator - (GridPosition posA, GridPosition posB)
    {
        return new GridPosition(posA.x - posB.x, posA.y - posB.y);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}



public class Tile : MonoBehaviour {

    public static Dictionary<int, string> idToSpriteNameDic = new Dictionary<int, string>();
    
    public GridPosition gridPos;
    public TileType id;
    public Board board;
    public TileState state = TileState.IDLE;

    public int ManhattanDistanceFromTile(Tile t)
    {
        return Mathf.Abs(t.gridPos.x - this.gridPos.x) + Mathf.Abs(t.gridPos.y - this.gridPos.y);
    }

	void Start () {
	
	}
	
	void Update () {
	
	}
}
