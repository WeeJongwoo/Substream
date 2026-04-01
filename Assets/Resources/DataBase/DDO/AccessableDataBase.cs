using UnityEngine;

public class AccessableDataBase : MonoBehaviour
{
    protected DontDestroyOnLoadManager DataBase;
    protected ResourceManager ResourcesManager;
    protected bool ConnectDataBase()
    {
        DataBase = DontDestroyOnLoadManager.Instance;
        if (DataBase == null) return false;

        ResourcesManager = DataBase.ResourceManager;
        if (ResourcesManager == null) return false;
        return true;
    }
}