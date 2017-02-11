using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardProcessor {

    private static BoardProcessor instance = null;
    public static BoardProcessor inst {
        get { if (instance == null) { instance = new BoardProcessor(); } return instance; }
    }
    
    public List<Tile> VerifyMatchesOnAxis(Board board, Tile tile, Board.Axis orientation)
    {
        if (orientation == Board.Axis.NONE)
            return null;
        if (orientation == Board.Axis.BOTH) //I see no reason to use this, but just in case
            return VerifyMatchesOnAxis(board, tile, Board.Axis.HORIZONTAL).Concat(VerifyMatchesOnAxis(board, tile, Board.Axis.VERTICAL)).ToList();
        
        List<Tile> axisMatches = new List<Tile>();
        int index;

        int sourceTileIndex = 0;
        int axisLenght = 0;

        switch (orientation)
        {
            case Board.Axis.HORIZONTAL:
                sourceTileIndex = tile.gridPos.x;
                axisLenght = board.Width;
                break;
            case Board.Axis.VERTICAL:
                sourceTileIndex = tile.gridPos.y;
                axisLenght = board.Height;
                break;
        }

        index = sourceTileIndex;
        while (index - 1 >= 0)
        {
            Tile tileToCheck = null;
            switch (orientation)
            {
                case Board.Axis.HORIZONTAL: tileToCheck = board.Tiles[index - 1][tile.gridPos.y]; break;
                case Board.Axis.VERTICAL: tileToCheck = board.Tiles[tile.gridPos.x][index - 1]; break;
            }

            if (tile.id != tileToCheck.id) break;

            axisMatches.Add(tileToCheck);
            index--;
        }

        index = sourceTileIndex;
        while (index + 1 < axisLenght)
        {
            Tile tileToCheck = null;
            switch (orientation)
            {
                case Board.Axis.HORIZONTAL: tileToCheck = board.Tiles[index + 1][tile.gridPos.y]; break;
                case Board.Axis.VERTICAL: tileToCheck = board.Tiles[tile.gridPos.x][index + 1]; break;
            }

            if (tile.id != tileToCheck.id) break;

            axisMatches.Add(tileToCheck);
            index++;
        }

        return axisMatches;
    }

    public List<Match> FindMatches(Board board, Tile sourceTile = null)
    {
        List<Match> matches = new List<Match>();

        List<Tile> visited = new List<Tile>();

        for (int i = 0; i < board.Width; i++) // i = X grid position
        {
            for (int j = 0; j < board.Height; j++) // j = Y grid position
            {
                Tile curr = board.Tiles[i][j];

                List<Tile> matchingTiles = new List<Tile>();

                if (!visited.Contains(curr))
                    matchingTiles = FloodFillMatches(board, curr, ref visited);


                if (matchingTiles.Count > 2)
                {
                    matches.Add(new Match(sourceTile, matchingTiles));
                }
            }
        }

        return matches;
    }

    List<Tile> FloodFillMatches(Board board, Tile tile, ref List<Tile> visited)
    {
        List<Tile> matches = FloodFillMatchesInternal(board, tile, ref visited, Board.Axis.BOTH);
        matches.Add(tile);
        return matches;
    }


    List<Tile> FloodFillMatchesInternal(Board board, Tile tile, ref List<Tile> visited, Board.Axis axis)
    {
        List<Tile> matches = new List<Tile>();

        visited.Add(tile);

        if (axis == Board.Axis.VERTICAL || axis == Board.Axis.BOTH)
        {
            List<Tile> verticalMatches = VerifyMatchesOnAxis(board, tile, Board.Axis.VERTICAL);
            if (verticalMatches.Count >= 2)
            {
                foreach (Tile t in verticalMatches)
                {

                    if (!visited.Contains(t))
                        matches.AddRange(FloodFillMatchesInternal(board, t, ref visited, Board.Axis.HORIZONTAL));

                    if (!matches.Contains(t))
                        matches.Add(t);

                }
            }
        }

        if (axis == Board.Axis.HORIZONTAL || axis == Board.Axis.BOTH)
        {
            List<Tile> horizontalMatches = VerifyMatchesOnAxis(board, tile, Board.Axis.HORIZONTAL);
            if (horizontalMatches.Count >= 2)
            {
                foreach (Tile t in horizontalMatches)
                {
                    if (!visited.Contains(t))
                        matches.AddRange(FloodFillMatchesInternal(board, t, ref visited, Board.Axis.VERTICAL));

                    if (!matches.Contains(t))
                        matches.Add(t);
                }
            }
        }
        return matches;
    }


    bool FloodFillRemoveMatchesFromTile(Board board, ref Tile t, Board.Axis axis)
    {
        bool foundAnyMatches = false;
        List<Tile> matchingAxis = VerifyMatchesOnAxis(board, t, axis);
        matchingAxis.Add(t);

        while (matchingAxis.Count >= 3)
        {
            foundAnyMatches = true;
            for (int i = 0; i < matchingAxis.Count; i++)
            {
                Tile currT = matchingAxis[i];
                TileProcessor.inst.CycleTileID(board, ref currT);
                matchingAxis = VerifyMatchesOnAxis(board, t, axis);
                if (matchingAxis.Count < 3)
                {
                    matchingAxis.Remove(currT);


                    Board.Axis otherAxis = axis == Board.Axis.HORIZONTAL ?
                            Board.Axis.VERTICAL :
                            Board.Axis.HORIZONTAL;

                    for (int i2 = 0; i2 < matchingAxis.Count; i2++)
                    {
                        Tile currT2 = matchingAxis[i2];

                        FloodFillRemoveMatchesFromTile(board, ref currT2, otherAxis);
                    }
                    break;
                }
            }
        }
        return foundAnyMatches;
    }

    public void MakeBoardHaveNoMatches(Board board)
    {
        bool foundAnyMatches;

        do
        {
            foundAnyMatches = false;
            for (int i = 0; i < board.Width; i++) // i = X grid position
            {
                for (int j = 0; j < board.Height; j++) // j = Y grid position
                {
                    Tile t = board.Tiles[i][j];

                    if (FloodFillRemoveMatchesFromTile(board, ref t, Board.Axis.HORIZONTAL) ||
                       FloodFillRemoveMatchesFromTile(board, ref t, Board.Axis.VERTICAL))
                    {
                        foundAnyMatches = true;
                    }
                }
            }
        } while (foundAnyMatches);


        for (int i = 0; i < board.Width; i++) // i = X grid position
        {
            for (int j = 0; j < board.Height; j++) // j = Y grid position
            {
                Tile t = board.Tiles[i][j];
                TileProcessor.inst.SetTileSpriteToMachItsValue(board.SpriteData.TileSprites, ref t);
            }
        }
    }

}
