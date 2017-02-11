using UnityEngine;
using System.Collections;

public class UIEvents : MonoBehaviour {
    

    public void Button_Uncolorize_OnClick()
    {
        GameManager.inst.boardController.UncolorizeBoard();
    }

    public void Button_ColorizeMatches_OnClick()
    {
        GameManager.inst.boardController.ColorizeMatches();
    }

    public void Button_RemoveMatches_OnClick()
    {
        BoardProcessor.inst.MakeBoardHaveNoMatches(GameManager.inst.gameBoard);
    }

    public void Button_RandomizeBoard_OnClick()
    {
        GameManager.inst.boardController.RandomizeBoard();
    }

}
