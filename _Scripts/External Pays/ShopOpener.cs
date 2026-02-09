using UnityEngine;

public class ShopOpener : MonoBehaviour
{
    public void _OpenShop()
    {
        ShopManager._instance._ShopCanvasActivation(true);
    }
    public void _CloseShop()
    {
        ShopManager._instance._ShopCanvasActivation(false);
    }
}
