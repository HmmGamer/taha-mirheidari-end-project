using System;
using System.Collections.Generic;
using UnityEngine;

public class Search_Sample
{
    public List<_DataMata> _dataBase;
    public string _searchName;

    [CreateMonoButton("Search")]
    public void _SearchList()
    {
        SearchTools._SearchAndSortList_First(_dataBase, _searchName, i => i._name);
    }
    [System.Serializable]
    public class _DataMata
    {
        public string _name;

        // other properties
    }
}

/// <summary>
/// this class is for searching a list/array
/// after searching them, it moves the founded ones to the top of the list/array
/// </summary>
public static class SearchTools
{
    //public static string _SearchList_First<T>(List<T> iList, string iSearchName)
    //{
    //    string searchLower = iSearchName.ToLower();
    //    for (int i = 0; i < iList.Count; i++)
    //    {
    //        if (iNameSelector(iList[i]).ToLower().Contains(searchLower))
    //        {
    //            T foundItem = iList[i];

    //            Debug.Log("the first similar result was moved to the top");
    //            return;
    //        }
    //    }
    //}

    #region Search & Sort

    /// <summary>
    /// finds the first matched item and moves it to the top - ignore's the rest
    /// </summary>
    public static void _SearchAndSortList_First<T>(List<T> iList, string iSearchName, Func<T, string> iNameSelector)
    {
        string searchLower = iSearchName.ToLower();
        for (int i = 0; i < iList.Count; i++)
        {
            if (iNameSelector(iList[i]).ToLower().Contains(searchLower))
            {
                T foundItem = iList[i];
                iList.RemoveAt(i);
                iList.Insert(0, foundItem);

                Debug.Log("the first similar result was moved to the top");
                return;
            }
        }
        Debug.Log("there is no item with the name <" + iSearchName + ">");
    }

    /// <summary>
    /// finds the first matched item to the first - ignore the rest
    /// </summary>
    public static void _SearchAndSortArray_First<T>(T[] iArray, string iSearchName, Func<T, string> iNameSelector)
    {
        string searchLower = iSearchName.ToLower();
        for (int i = 0; i < iArray.Length; i++)
        {
            if (iNameSelector(iArray[i]).ToLower().Contains(searchLower))
            {
                T foundItem = iArray[i];
                for (int j = i; j > 0; j--)
                {
                    iArray[j] = iArray[j - 1];
                }
                iArray[0] = foundItem;

                Debug.Log("the first similar result was moved to the top");
                return;
            }
        }
        Debug.Log("there is no item with the name <" + iSearchName + ">");
    }

    /// <summary>
    /// finds all of matched items and moves them to the top
    /// warning: may be a little heavy for big dataBases with many search results
    /// </summary>
    public static void _SearchAndSortList_Full<T>(List<T> iList, string iSearchName, Func<T, string> iNameSelector)
    {
        string searchLower = iSearchName.ToLower();
        List<T> matchedItems = new List<T>();

        for (int i = iList.Count - 1; i >= 0; i--)
        {
            if (iNameSelector(iList[i]).ToLower().Contains(searchLower))
            {
                matchedItems.Insert(0, iList[i]);
                iList.RemoveAt(i);
            }
        }

        if (matchedItems.Count > 0)
        {
            iList.InsertRange(0, matchedItems);
            Debug.Log(matchedItems.Count + " similar results were moved to the top");
        }
        else
        {
            Debug.Log("there is no item with the name <" + iSearchName + ">");
        }
    }

    /// <summary>
    /// finds all of matched items and moves them to the top
    /// warning: may be a little heavy for big dataBases with many search results
    /// </summary>
    public static void _SearchAndSortArray_Full<T>(T[] iArray, string iSearchName, Func<T, string> iNameSelector)
    {
        string searchLower = iSearchName.ToLower();
        List<T> matchedItems = new List<T>();
        List<T> otherItems = new List<T>();

        for (int i = 0; i < iArray.Length; i++)
        {
            if (iNameSelector(iArray[i]).ToLower().Contains(searchLower))
            {
                matchedItems.Add(iArray[i]);
            }
            else
            {
                otherItems.Add(iArray[i]);
            }
        }

        if (matchedItems.Count == 0)
        {
            Debug.Log(matchedItems.Count + " similar results were moved to the top");
            return;
        }

        int index = 0;
        foreach (var item in matchedItems)
        {
            iArray[index++] = item;
        }
        foreach (var item in otherItems)
        {
            iArray[index++] = item;
        }
    }
    #endregion
}
