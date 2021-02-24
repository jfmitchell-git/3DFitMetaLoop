using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MetaLoop.Common.PlatformCommon.Unity.Utils
{

    /*public enum RectOverflowType
    {
        None = 0,
        BottomLeft = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomRight = 4

    }*/

    public class GameObjectWithParent
    {
        public GameObject AffiliatedGameObject;
        public bool moveObject;
        public Transform parent;
        public int Order;
        public GameObject Placeholder;
        public RectTransform PlaceholderRT;
        public RectTransform GameObjectRT;
        public System.Action OnUpdate;
        public Canvas GameObjectCanvas;
        public Canvas PlaceHolderCanvas;

    }

    public enum FitType
    {
        Center = 0,
        Top = 1,
        Bottom = 2
    }


    public class Utils
    {
        public static IEnumerator SyncHttpRequest(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest();
            while (!www.isDone)
                yield return null;
            if (!www.isNetworkError && !www.isHttpError)
                yield return www.downloadHandler.text;
            else
                yield return string.Empty;
        }

        public static Tween DelayedCallWaitFrame(float delay, TweenCallback callback, MonoBehaviour mono, bool ignoreTimeScale = true, int numOfFrame = 2)
        {

            Tween myTween = DOVirtual.DelayedCall(delay, () => { if (mono != null && mono.gameObject != null && mono.gameObject.activeInHierarchy) { mono.StartCoroutine(WaitEndFrameCoRoutine(callback, numOfFrame)); }; }, ignoreTimeScale);

            return myTween;
        }

        private static IEnumerator WaitEndFrameCoRoutine(TweenCallback callback, int numOfFrame)
        {

            int i = 0;
            while (i < numOfFrame)
            {
                yield return new WaitForEndOfFrame();
                i++;
            }

            callback.Invoke();
        }




        //hideously slow as it iterates all objects, so don't overuse!
        public static GameObject FindInChildrenIncludingInactive(GameObject go, string name)
        {

            for (int i = 0; i < go.transform.childCount; i++)
            {
                if (go.transform.GetChild(i).gameObject.name == name) return go.transform.GetChild(i).gameObject;
                GameObject found = FindInChildrenIncludingInactive(go.transform.GetChild(i).gameObject, name);
                if (found != null) return found;
            }

            return null;  //couldn't find crap
        }
        //hideously slow as it iterates all objects, so don't overuse!
        public static GameObject FindIncludingInactive(string name)
        {
            Scene scene = SceneManager.GetActiveScene();
            List<GameObject> game_objects = new List<GameObject>();
            scene.GetRootGameObjects(game_objects);
            foreach (GameObject obj in game_objects)
            {
                GameObject found = FindInChildrenIncludingInactive(obj, name);
                if (found) return found;
            }

            return null;
        }





        public static Canvas GetCanvasFromObject(GameObject gameObject)
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();

            while (canvas == null && gameObject != null)
            {
                gameObject = gameObject.transform.parent.gameObject;
                if (gameObject)
                    canvas = gameObject.GetComponent<Canvas>();

            }

            return canvas;

        }


        public static IEnumerator RefreshContainerCoRoutine(GameObject container)
        {
            yield return new WaitForEndOfFrame();
            RefreshContainer(container);
        }

        public static void RefreshContainer(GameObject container)
        {

            container.GetComponentsInChildren<LayoutGroup>().ToList().ForEach(p => { if (p.enabled) { p.enabled = false; p.enabled = true; } });

            //new test
            LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());

        }



        public static IEnumerator CreateTextTimer(TimeSpan time, TextMeshProUGUI textToUpdate, string dayString, string hourString, string minuteString, bool showSec = true, bool showHourMinimum = true, Action timerFinishCallback = null, bool showMinuteOnDays = false)
        {

            int startSec = 0;

            while (time.Ticks > 0)
            {

                time = time.Subtract(new TimeSpan(0, 0, 0, 1, 0));

                textToUpdate.text = StringUtils.GetDateFormatted(time, dayString, hourString, minuteString, showSec, showHourMinimum, showMinuteOnDays);


                startSec++;
                yield return new WaitForSeconds(1f);
            }


            timerFinishCallback();

        }





        public static GameObject AddPlaceHolder(GameObject gameObject)
        {

            //for each object, create a placeholder
            GameObject newPlaceHolder = new GameObject();
            newPlaceHolder.transform.SetParent(gameObject.transform.parent, false);

            // newPlaceHolder.AddComponent<RectTransform>();
            CopyComponent(gameObject.GetComponent<RectTransform>(), newPlaceHolder);
            CopyComponent(gameObject.GetComponent<LayoutElement>(), newPlaceHolder);



            /*
            //if layout element, we copy it
            if (gameObject.GetComponent<LayoutElement>() != null)
            {
                CopyComponent(gameObject.GetComponent<LayoutElement>(), newPlaceHolder);

                newPlaceHolder.GetComponent<LayoutElement>().preferredWidth = gameObject.GetComponent<RectTransform>().sizeDelta.x;
                newPlaceHolder.GetComponent<LayoutElement>().preferredHeight = gameObject.GetComponent<RectTransform>().sizeDelta.y;
            }*/

            newPlaceHolder.name = gameObject.name + "_PlaceHolder";



            //newPlaceHolder.GetComponent<RectTransform>().transform.position

            newPlaceHolder.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex());

            return newPlaceHolder;

        }

        public static Component CopyComponent(Component original, GameObject destination)
        {
            if (original == null) return null;

            Component copy = null;

            if (original is RectTransform)
            {
                RectTransform originalRect = (RectTransform)original;
                RectTransform copyRect = destination.GetComponent<RectTransform>() == null ? destination.AddComponent<RectTransform>() : destination.GetComponent<RectTransform>();

                // These four properties are to be copied
                copyRect.anchorMin = originalRect.anchorMin;
                copyRect.anchorMax = originalRect.anchorMax;
                copyRect.anchoredPosition = originalRect.anchoredPosition;
                copyRect.sizeDelta = originalRect.sizeDelta;

                copyRect.localScale = originalRect.localScale;
                copyRect.localPosition = originalRect.localPosition;

                copy = copyRect;

            }
            if (original is LayoutElement)
            {
                LayoutElement originaLE = (LayoutElement)original;
                LayoutElement copyLE = destination.GetComponent<LayoutElement>() == null ? destination.AddComponent<LayoutElement>() : destination.GetComponent<LayoutElement>();



                copyLE.flexibleHeight = originaLE.flexibleHeight;
                copyLE.flexibleWidth = originaLE.flexibleWidth;
                copyLE.ignoreLayout = originaLE.ignoreLayout;
                copyLE.layoutPriority = originaLE.layoutPriority;
                copyLE.minHeight = originaLE.minHeight;
                copyLE.minWidth = originaLE.minWidth;
                copyLE.preferredHeight = originaLE.preferredHeight;
                copyLE.preferredWidth = originaLE.preferredWidth;

                copy = copyLE;
            }

            /*
            System.Type type = original.GetType();
            Component copy = destination.GetComponent(type) != null ? destination.GetComponent(type) : destination.AddComponent(type);
            

            
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }

            System.Reflection.PropertyInfo[] properties = type.GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                //Debug.Log("prop = " + property.Name);

                if (property.CanWrite && property.Name.ToLower() != "parent")
                    property.SetValue(copy, property.GetValue(original, null), null);
            }*/


            return copy;
        }

        public static string RandomString(int length)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            //test
            string returnString = "";

            for (int i = 0; i < length; i++)
                returnString += chars[UnityEngine.Random.Range(0, chars.Length - 1)];

            return returnString;

            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

        }


        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp);
            return dtDateTime;
        }

        public static DateTime UnixTimeStampInSecondsToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }


        public static long RandomLong(int length)
        {
            string input = "0123456789";
            StringBuilder builder = new StringBuilder();
            char ch;

            System.Random rdn = new System.Random();

            for (int i = 0; i < length; i++)
            {
                int startInt = (i == 0) ? 0 : 1;
                ch = input[rdn.Next(startInt, input.Length)];
                builder.Append(ch);
            }

            return Convert.ToInt64(builder.ToString());
        }

        public static string FormatDate(DateTime date)
        {
            return date.Day + " " + date.ToString("MMM") + " " + date.Year;

        }

        public static string FormatNumber(float number)
        {
            return Mathf.Round(number).ToString();
        }

        public static List<Material> GetAllMaterials(GameObject gameObject, List<Material> materialsList)
        {


            if (gameObject.GetComponent<Renderer>())
            {
                materialsList.AddRange(gameObject.GetComponent<Renderer>().materials.ToList());
            }



            int childCount = gameObject.transform.childCount;

            for (int i = 0; i < childCount; ++i)
            {

                materialsList = GetAllMaterials(gameObject.transform.GetChild(i).gameObject, materialsList);

            }

            return materialsList;

        }




        public static string[] FilterEnum(Type enumType, int min = 0, int max = 9999999)
        {
            List<string> results = new List<string>();
            string[] allItems = Enum.GetNames(enumType);
            foreach (string item in allItems)
            {
                int value = (int)Enum.Parse(enumType, item);
                if (value >= min && value <= max) results.Add(item);
            }
            return results.ToArray();
        }

        public static string MillisecondsToSecondString(int value)
        {
            return (value / 1000).ToString("0.00");
        }

        public static string FloatToString(float num, int numOfDecimal)
        {
            double value = Math.Round(num, numOfDecimal);
            return value.ToString();
        }


        public static string FirstCharToLower(string input)
        {
            return input.First().ToString().ToLower() + input.Substring(1);
        }

        public static string FirstCharToUpper(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }




        public static Vector2 GetOutOfScreenDiff(RectTransform rect, Canvas canvas, Camera camera)
        {

            float canvasScale = canvas.scaleFactor;

            //check overflow
            Vector3[] tooltipCheckOverflowCorners = new Vector3[4];

            Vector2 Diff = new Vector2();

            //i guess it depend on screenspace!

            rect.GetWorldCorners(tooltipCheckOverflowCorners);

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                for (int i = 0; i < tooltipCheckOverflowCorners.Length; i++)
                {
                    tooltipCheckOverflowCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, tooltipCheckOverflowCorners[i]);
                }
            }

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            bool bottomLeftCorner = !screenRect.Contains(tooltipCheckOverflowCorners[0]);
            bool topLeftCorner = !screenRect.Contains(tooltipCheckOverflowCorners[1]);
            bool topRightCorner = !screenRect.Contains(tooltipCheckOverflowCorners[2]);
            bool bottomRightCorner = !screenRect.Contains(tooltipCheckOverflowCorners[3]);


            if (bottomLeftCorner && topLeftCorner)
            {

                if (tooltipCheckOverflowCorners[0].x < 0)
                {

                    Diff.x = tooltipCheckOverflowCorners[0].x / canvasScale;

                }

            }

            if (bottomRightCorner && topRightCorner)
            {


                if (tooltipCheckOverflowCorners[3].x > screenRect.width)
                {

                    Diff.x = -(screenRect.width - tooltipCheckOverflowCorners[3].x) / canvasScale;

                }

            }






            return Diff;



        }



        public static UnityEngine.Object FindGameObject(string name, System.Type type)
        {
            UnityEngine.Object[] objs = Resources.FindObjectsOfTypeAll(type);

            foreach (UnityEngine.Object obj in objs)
            {
                if (obj.name == name)
                {

                    return obj;
                }
            }

            return null;
        }



        public static void FitColliderToParent(BoxCollider fitInCollider, BoxCollider scaleBoxCollider, BoxCollider realSizeBoxCollider, FitType fitType = FitType.Top)
        {


            GameObject childToMove = scaleBoxCollider.gameObject;

            //it need absolutely to be activate
            fitInCollider.gameObject.SetActive(true);
            scaleBoxCollider.gameObject.SetActive(true);
            realSizeBoxCollider.gameObject.SetActive(true);


            Vector3 masterBoxSize = fitInCollider.bounds.size;


            //transform.scale * collider.scale

            //try to fix something
            // masterBoxSize.x /= fitInCollider.gameObject.transform.localScale.x;
            // masterBoxSize.y /= fitInCollider.gameObject.transform.localScale.y;
            // masterBoxSize.z /= fitInCollider.gameObject.transform.localScale.z;


            Vector3 itemBoxSize = scaleBoxCollider.bounds.size;

            // itemBoxSize.x /= scaleBoxCollider.gameObject.transform.localScale.x;
            //  itemBoxSize.y /= scaleBoxCollider.gameObject.transform.localScale.y;
            //  itemBoxSize.z /= scaleBoxCollider.gameObject.transform.localScale.z;



            masterBoxSize.x = Math.Abs(masterBoxSize.x);
            masterBoxSize.y = Math.Abs(masterBoxSize.y);
            masterBoxSize.z = Math.Abs(masterBoxSize.z);
            itemBoxSize.x = Math.Abs(itemBoxSize.x);
            itemBoxSize.y = Math.Abs(itemBoxSize.y);
            itemBoxSize.z = Math.Abs(itemBoxSize.z);

            //Debug.Log(masterBoxSize + " " + itemBoxSize);

            //checking scale on each axis
            Vector3 scaleAxis = new Vector3();
            scaleAxis.x = masterBoxSize.x / itemBoxSize.x;
            scaleAxis.y = masterBoxSize.y / itemBoxSize.y;
            scaleAxis.z = masterBoxSize.z / itemBoxSize.z;

            //the biggest win
            float goodScale = scaleAxis.x < scaleAxis.y ? scaleAxis.x : scaleAxis.y;
            goodScale = goodScale < scaleAxis.z ? goodScale : scaleAxis.z;

            scaleBoxCollider.gameObject.transform.localScale *= goodScale;

            //get the bounds now that it's scaled of the real box
            Vector3 realSize = realSizeBoxCollider.bounds.size;


            float heightDiff = masterBoxSize.y - realSize.y;

            switch (fitType)
            {

                case FitType.Top:

                    childToMove.transform.position = new Vector3(childToMove.transform.position.x, childToMove.transform.position.y + heightDiff, childToMove.transform.position.z);
                    break;

                case FitType.Bottom:
                    childToMove.transform.localPosition = new Vector3(0, 0, 0);
                    break;
            }



        }


        public static void AnimateNumberInText(TextMeshProUGUI text, float startNum, float endNum, float duration = 2f, int numOfDecial = 0,string replaceString = "")
        {

            float to = endNum;
            DOTween.To(() => startNum, x => startNum = x, to, duration).OnUpdate(() => UpdateTextBox(text, startNum, numOfDecial,replaceString)).SetEase(Ease.Linear);

        }

        private static void UpdateTextBox(TextMeshProUGUI text, float startNum, int numOfDecial = 0,string replaceString = "")
        {
            //here we need something to edit correctly the number

            var stringToUse = Math.Round(startNum, numOfDecial).ToString();

            if (replaceString == "")
            {
                text.text = stringToUse;
            } else
            {
                text.text = replaceString.Replace("{0}", stringToUse);
            }
        }




        public static void SetPositionToItemInScrollRect(GameObject item, float padding = 0f, FitType fitType = FitType.Top)
        {


            ScrollRect useScrollRect = item.GetComponentInParent<ScrollRect>();

            float viewportHeight = useScrollRect.viewport.GetComponent<RectTransform>().rect.height;
            float contentHeight = useScrollRect.content.GetComponent<RectTransform>().rect.height;
            float itemPositionY = -item.GetComponent<RectTransform>().localPosition.y;

            float useHeight = contentHeight - viewportHeight;

            switch (fitType)
            {
                case FitType.Center:
                    padding = viewportHeight / 2 - item.GetComponent<RectTransform>().rect.height / 2 - padding / 2;
                    break;

                case FitType.Bottom:
                    padding -= viewportHeight;
                    break;
            }

            useScrollRect.verticalNormalizedPosition = 1 - ((itemPositionY - padding) / useHeight);

            //Debug.Log("itemposition = " + item.GetComponent<RectTransform>().localPosition.y);
            //Debug.Log("viewport = " + useScrollRect.viewport.GetComponent<RectTransform>().rect.height);
            //Debug.Log("content = " + useScrollRect.content.GetComponent<RectTransform>().rect.height);



        }


        public static List<GameObjectWithParent> SetGameObjectOver(List<GameObject> allGameObject, GameObject over, bool moveObject = true, string sortingLayerName = "Default", int sortingOrder = 0)
        {

            List<GameObjectWithParent> allObjects = new List<GameObjectWithParent>();

            int useSorting = sortingOrder;

            foreach (GameObject gameObject in allGameObject)
            {
                GameObjectWithParent newObject = new GameObjectWithParent();
                newObject.AffiliatedGameObject = gameObject;
                newObject.Order = gameObject.transform.GetSiblingIndex();
                newObject.parent = gameObject.transform.parent;
                newObject.moveObject = moveObject;
                allObjects.Add(newObject);

            }

            //when all done, we set them

            if (moveObject)
            {
                allObjects.ForEach(p =>
                {
                    //place holder
                    p.Placeholder = Utils.AddPlaceHolder(p.AffiliatedGameObject);
                    p.PlaceholderRT = p.Placeholder.GetComponent<RectTransform>();
                    p.GameObjectRT = p.AffiliatedGameObject.GetComponent<RectTransform>();
                    p.GameObjectCanvas = GetCanvasFromObject(p.AffiliatedGameObject);
                    p.PlaceHolderCanvas = GetCanvasFromObject(over);

                    float zPos = p.AffiliatedGameObject.transform.position.z;
                    p.AffiliatedGameObject.transform.SetParent(over.transform, true);


                    p.OnUpdate = () =>
                    {

                        WorldToCanvasPosition(p.PlaceHolderCanvas.GetComponent<RectTransform>(), p.PlaceHolderCanvas.worldCamera, p.Placeholder.transform.position);

                        //Vector2 pos = p.Placeholder.transform.position;  // get the game object position
                        //Vector2 viewportPoint = Camera.main.WorldToViewportPoint(pos);  //convert game object position to VievportPoint
                        //p.GameObjectRT.anchorMin = viewportPoint;
                        //p.GameObjectRT.anchorMax = viewportPoint;

                        //Vector2 worldOutPosition = RectTransformUtility.WorldToScreenPoint(p.GameObjectCanvas.worldCamera, p.PlaceholderRT.position);
                        //Vector2 newOutPosition = new Vector2();
                        //RectTransformUtility.ScreenPointToLocalPointInRectangle(over.GetComponent<RectTransform>(), worldOutPosition, p.PlaceHolderCanvas.worldCamera, out newOutPosition);

                        //Vector2 localPos;
                        //RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, camera, out localPos);
                        // p.GameObjectRT.anchoredPosition = newOutPosition;



                    };

                    //test here
                    UpdateCaller.OnUpdate += p.OnUpdate;

                    /* DOVirtual.DelayedCall(0.001f, () =>
                     {
                         p.gameObject.transform.position = new Vector3(p.gameObject.transform.position.x, p.gameObject.transform.position.y, zPos);
                     });*/

                });
            }
            else
            {



                //because of a problem because disease are on the back, and i didn't find any way to change position from canvas to canvas, i decided to try with changing sorting order instead
                allObjects.Select(p => p.AffiliatedGameObject).Cast<GameObject>().ToList().ForEach(p =>
                {

                    //if (sortingOrder != 0 && sortingLayerName != "default")
                    // {
                    Canvas canvas;
                    if (p.GetComponent<Canvas>() == null)
                    {
                        canvas = p.AddComponent<Canvas>();
                    }
                    else
                    {
                        canvas = p.GetComponent<Canvas>();
                    }

                    if (p.GetComponent<GraphicRaycaster>() == null)
                        p.AddComponent<GraphicRaycaster>();

                    canvas.overrideSorting = true;
                    canvas.sortingOrder = useSorting;
                    useSorting++;
                    canvas.sortingLayerName = sortingLayerName;
                    // }
                });


            }

            return allObjects;

        }

        public static void SetGameObjectOverBack(List<GameObjectWithParent> allGameObject)
        {

            if (allGameObject != null && allGameObject.Count > 0)
            {
                if (allGameObject[0].moveObject)
                {

                    allGameObject.ForEach(p =>
                    {
                        p.AffiliatedGameObject.transform.SetParent(p.parent, true);
                        GameObject.Destroy(p.Placeholder);
                        UpdateCaller.OnUpdate -= p.OnUpdate;
                    });
                    allGameObject.ForEach(p => p.AffiliatedGameObject.transform.SetSiblingIndex(p.Order));
                }
                else
                {

                    //because of a problem because disease are on the back, and i didn't find any way to change position from canvas to canvas, i decided to try with changing sorting order instead
                    allGameObject.Select(p => p.AffiliatedGameObject).Cast<GameObject>().ToList().ForEach(p =>
                    {

                        GameObject.Destroy(p.GetComponent<GraphicRaycaster>());
                        GameObject.Destroy(p.GetComponent<Canvas>());


                    });


                }
            }
        }




        public static Vector2 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 temp = camera.WorldToViewportPoint(position);

            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;

            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.

            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        }


        public static IEnumerator WaitForRealSeconds(float time)
        {
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + time)
            {
                yield return null;
            }
        }

        public static Vector2 GetAnchoredDiff(RectTransform parent, RectTransform child)
        {
            Vector2 returnValue = new Vector2(0, 0);

            while (child != null && child != parent)
            {
                returnValue += child.anchoredPosition;

                child = child.parent.GetComponent<RectTransform>();

            }

            return returnValue;

        }


    }
}
