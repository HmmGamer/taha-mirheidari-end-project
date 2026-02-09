using System;
using System.Collections.Generic;
using UnityEngine;

public static class GeneralTools
{
    public static string _GetCoinsFormat(int iCoins)
    {
        string text;
        if (iCoins >= 1000000)
        {
            text = (iCoins / 1000000f).ToString("0.#") + "M";
        }
        else if (iCoins >= 10000)
        {
            text = (iCoins / 1000f).ToString("0.#") + "K";
        }
        else
        {
            text = iCoins.ToString();
        }
        return text;
    }
    public static void _RemoveAllChildren(GameObject iParent)
    {
        for (int i = iParent.transform.childCount - 1; i >= 0; i--)
        {
            GameObject iChild = iParent.transform.GetChild(i).gameObject;
            GameObject.Destroy(iChild);
        }
    }
    public static Vector2 _GetDirection(Directions iDirection)
    {
        if (iDirection == Directions.Up)
            return Vector2.up;
        else if (iDirection == Directions.Down)
            return Vector2.down;
        else if (iDirection == Directions.Right)
            return Vector2.right;
        else if (iDirection == Directions.Left)
            return Vector2.left;
        else 
            return Vector2.zero;
    }
    public static Color _MakeColor(Vector4 iColor)
    {
        return new Color(iColor.x, iColor.y, iColor.z, iColor.w);
    }
    public static Color _MakeColor(Color iColor , float iAlpha)
    {
        return new Color(iColor.r, iColor.g, iColor.b, iAlpha);
    }
}
public enum Directions
{
    Up = 0, Right = 1, Left = 2, Down = 3
}