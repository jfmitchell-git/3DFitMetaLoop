using UnityEngine;
using System.Collections;
namespace MetaLoop.Common.PlatformCommon.Unity.Utils {
public class UpdateCaller : MonoBehaviour
{
    private static UpdateCaller instance = null;
    public static System.Action OnUpdate;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (this != instance)
            Destroy(this);
    }

    void Update()
    {
        if (OnUpdate != null)
            OnUpdate();
    }
}
}
