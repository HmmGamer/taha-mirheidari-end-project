using System.Collections.Generic;
using UnityEngine;

public static class UniqueIdTools
{
    /// <summary>
    /// reminder : if you have the same script in one gameObject you will have a duplicated Id
    /// 
    /// to avoid this change one of these : position ,name ,parent name
    /// 
    /// reminder2 : if the gameObject is disabled at the start the Position is not set correctly 
    /// so use iIdForDisabledObjects = true to activate and deactivate it in the runTime but 
    /// avoid using this too many times as it uses a lot of cpu power
    /// </summary>
    public static string _MakeUniqueId(Transform iObject,bool iIdForDisabledObjects = false)
    {
        if (iIdForDisabledObjects)
            _ActivateToRoot(iObject.gameObject, true);

        string xPart = iObject.position.x.ToString("F2").Replace(".", "");
        string yPart = iObject.position.y.ToString("F2").Replace(".", "");
        string zPart = iObject.position.z.ToString("F2").Replace(".", "");
        string nPart = iObject.name;

        string pPart = iObject.parent != null ? iObject.parent.name : "Null";

        if (xPart.Length > 5)
            xPart = xPart.Substring(0, 4);
        if (yPart.Length > 5)
            yPart = yPart.Substring(0, 4);
        if (zPart.Length > 5)
            zPart = zPart.Substring(0, 4);

        if (nPart.Length > 6)
            nPart = nPart.Substring(0, 2) + nPart.Substring(nPart.Length - 2, 2);

        if (pPart.Length > 6)
            pPart = pPart.Substring(0, 2) + pPart.Substring(pPart.Length - 2, 2);

        int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;


        if (iIdForDisabledObjects)
            _ActivateToRoot(iObject.gameObject, false);

        return xPart + yPart + zPart + nPart + pPart + "_" + currentScene;
    }
    public static int _GetUniqueIdScene(string iUniqueId)
    {
        return int.Parse(iUniqueId.Split('_')[1]);
    }
    public static bool _IsUniqueIdInScene(string iUniqueId)
    {
        int uniqueIdScene = _GetUniqueIdScene(iUniqueId);
        return uniqueIdScene ==
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
    }
    private static void _ActivateToRoot(GameObject iObject, bool iActivation)
    {
        List<(GameObject obj, bool originalState)> _stateList = new List<(GameObject, bool)>();

        GameObject current = iObject;
        while (current != null)
        {
            _stateList.Add((current, current.activeSelf));
            current.SetActive(iActivation);

            if (current.transform.parent != null)
                current = current.transform.parent.gameObject;
            else
                break;
        }

        // Restore original states (reverse order to ensure proper hierarchy handling)
        for (int i = _stateList.Count - 1; i >= 0; i--)
        {
            _stateList[i].obj.SetActive(_stateList[i].originalState);
        }
    }

}
