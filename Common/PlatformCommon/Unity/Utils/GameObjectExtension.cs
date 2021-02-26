#if !BACKOFFICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Utils
{
    public static class GameObjectExtension
    {

        public static List<KeyValuePair<GameObject, int>> SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {

            //keep original layer ?
            List<KeyValuePair<GameObject, int>> myList = new List<KeyValuePair<GameObject, int>>();

            KeyValuePair<GameObject, int> parentKeyValue = new KeyValuePair<GameObject, int>(parent,parent.layer);
            myList.Add(parentKeyValue);

            parent.layer = layer;
            if (includeChildren)
            {
                foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
                {
                    //fix a weird problem
                    if (trans.gameObject == parent.gameObject) continue;

                    KeyValuePair<GameObject, int> childKeyValue = new KeyValuePair<GameObject, int>(trans.gameObject, trans.gameObject.layer);
                    myList.Add(childKeyValue);
                    trans.gameObject.layer = layer;
                }
            }

            return myList;
        }
    }
}
#endif
