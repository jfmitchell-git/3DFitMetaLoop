#if !BACKOFFICE

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MetaLoop.Common.PlatformCommon.Unity.Pooler
{

    public class DGPoolingSystem : MonoBehaviour
    {

        [System.Serializable]
        public class PoolingItems
        {
            public string name = "";
            public GameObject prefab;
            public int amount;

            
            public List<GameObject> pooledItems;

            //public for now
            public List<GameObject> allPooledItems;
        }

        public static DGPoolingSystem Instance;

        /// <summary>
        /// These fields will hold all the different types of assets you wish to pool.
        /// </summary>
        public List<PoolingItems> poolingItems;

        /// <summary>
        /// The default pooling amount for each object type, in case the pooling amount is not mentioned or is 0.
        /// </summary>
        public int defaultPoolAmount = 10;

        /// <summary>
        /// Do you want the pool to expand in case more instances of pooled objects are required.
        /// </summary>
        public bool poolExpand = true;

        void Awake()
        {

            //fix some problme with beta
            Canvas.ForceUpdateCanvases();

            Instance = this;


          
        }

        void Start()
        {


            for (int i = 0; i < poolingItems.Count; i++)
            {

                AddItem(poolingItems[i]);

            }



        }


        public void AddItem(PoolingItems poolingItem)
        {



            //if it s not in the list, we add it 
            if (poolingItems.Where(p => (p.name == "" && p.prefab.name == poolingItem.prefab.name) || (p.name != "" && p.name == poolingItem.name)).Count() == 0)
            {
                poolingItems.Add(poolingItem);
            }

            var newPooledItems = new List<GameObject>();

            int poolingAmount;
            if (poolingItem.amount > 0) poolingAmount = poolingItem.amount;
            else poolingAmount = defaultPoolAmount;

            for (int j = 0; j < poolingAmount; j++)
            {

                if (poolingItem.prefab == null) continue;

                GameObject newItem = (GameObject)Instantiate(poolingItem.prefab, transform, true);
                // newItem.SetActive(true);
                newItem.SetActive(false);
                newPooledItems.Add(newItem);
                //newItem.transform.parent = transform;
            }


            poolingItem.pooledItems = newPooledItems;


            if(poolingItem.allPooledItems != null && poolingItem.allPooledItems.Count > 0)
            {

                // poolingItems.ForEach(p => { p.pooledItems.ForEach(g => Destroy(g)); p.pooledItems.Clear(); });
                poolingItem.allPooledItems.ForEach(g => Destroy(g));
                poolingItem.allPooledItems.Clear();
            }
            //new security
            poolingItem.allPooledItems = new List<GameObject>();
            poolingItem.pooledItems.ForEach(p => poolingItem.allPooledItems.Add(p));

        }

        

      

        public void DestroyAPS(GameObject myObject,bool resetScaleAndPosition = false)
        {
            if (myObject != null)
            {
                myObject.SetActive(false);

                if (this != null && this.gameObject != null)
                {

                    Vector3 myscale = new Vector3();
                    Vector3 position = new Vector3();

                    Vector2 anchoredPosition = new Vector2();
                    Vector2 anchorMin = new Vector2();
                    Vector2 anchorMax = new Vector2();
                    Vector2 offsetMin = new Vector2();
                    Vector2 offsetMax = new Vector2();

                    RectTransform rectTransform = null;


                    if (resetScaleAndPosition)
                    {
                        //test
                        myscale = myObject.transform.localScale;
                        position = myObject.transform.position;

                        if(myObject.GetComponent<RectTransform>() != null)
                        {
                            rectTransform = myObject.GetComponent<RectTransform>();
                            anchoredPosition = rectTransform.anchoredPosition;
                            anchorMin = rectTransform.anchorMin;
                            anchorMax = rectTransform.anchorMax;
                            offsetMin = rectTransform.offsetMin;
                            offsetMax = rectTransform.offsetMax;
                        }

                    }
                    if(this.gameObject.transform != null)
                        myObject.transform.SetParent(this.gameObject.transform, true);

                    if (resetScaleAndPosition)
                    {
                        myObject.transform.localScale = myscale;
                        myObject.transform.position = new Vector3(position.x, position.y, 0);
                        
                        if(rectTransform != null)
                        {
                            rectTransform.anchoredPosition = anchoredPosition;
                            rectTransform.anchorMin = anchorMin;
                            rectTransform.anchorMax = anchorMax;
                            rectTransform.offsetMin = offsetMin;
                            rectTransform.offsetMax = offsetMax;

                        }

                    }

                }

               
            }

            //add it back
            for (int i = 0; i < poolingItems.Count; i++)
            {
                if (poolingItems[i].prefab.name + "(Clone)" == myObject.name)
                {
                    poolingItems[i].pooledItems.Add(myObject);
                    return;
                }
            }

           
        }

        public GameObject InstantiateAPS(string itemType,bool setActive = true)
        {
            GameObject newObject = GetPooledItem(itemType);

            if (setActive)
            {
                if(newObject == null)
                {
                    DGLogger.Log("Wtf " + itemType);
                }
                newObject.SetActive(true);

            }
            return newObject;
        }

        public GameObject InstantiateAPS(string itemType, Vector3 itemPosition, Quaternion itemRotation)
        {
            GameObject newObject = GetPooledItem(itemType);
            newObject.transform.position = itemPosition;
            newObject.transform.rotation = itemRotation;
            newObject.SetActive(true);
            return newObject;
        }

        public GameObject InstantiateAPS(string itemType, Vector3 itemPosition, Quaternion itemRotation, GameObject myParent)
        {
            if (GetPooledItem(itemType) != null)
            {
                GameObject newObject = GetPooledItem(itemType);
                newObject.transform.position = itemPosition;
                newObject.transform.rotation = itemRotation;
                newObject.transform.parent = myParent.transform;
                newObject.SetActive(true);
                return newObject;
            }
            return null;
        }

        private GameObject GetPooledItem(string itemType)
        {




            for (int i = 0; i < poolingItems.Count; i++)
            {
                if ((poolingItems[i].name == "" && poolingItems[i].prefab.name == itemType) || (poolingItems[i].name != "" && poolingItems[i].name == itemType))
                {

                    if (poolingItems[i].pooledItems.Count > 0)
                    {
                        GameObject returnGameObject = poolingItems[i].pooledItems[0];
                        poolingItems[i].pooledItems.Remove(returnGameObject);
                        return returnGameObject;
                    }



                    /*for (int j = 0; j < pooledItems[i].Count; j++)
                    {

                        GameObject returnGameObject = pooledItems[i][j];
                        pooledItems[i].RemoveAt(j);
                        return returnGameObject;
                       
                    }*/

                    if (poolExpand)
                    {
                        DGLogger.Log("EXPAND POOLING" + poolingItems[i].prefab.name);

                        GameObject newItem = (GameObject)Instantiate(poolingItems[i].prefab, transform, true);
                        newItem.SetActive(false);

                        poolingItems[i].allPooledItems.Add(newItem);

                        //pooledItems[i].Add(newItem);
                        newItem.transform.SetParent(transform);
                        return newItem;
                    }

                    break;
                }
            }

            return null;
        }


    }
}
#endif