using UnityEngine;

public class TestScript : MonoBehaviour
{
    [OnStringChanges_Mono("_Test")]
    public string main;
    [SerializeField] string _lateTranslated;

    public void _Test()
    {
        main = FontTools._ConvertToPersian(main);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

}
