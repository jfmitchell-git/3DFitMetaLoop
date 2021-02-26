#if !BACKOFFICE
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace MetaLoop.Common.PlatformCommon.Unity.Utils
{
    public class CameraUtils
    {


        public static void ShakeCamera(Camera camera, Vector3 movement)
        {

            LoopShakeCamera(camera, camera.transform.position, movement);

        }

        private static void LoopShakeCamera(Camera camera, Vector3 StartPos, Vector3 movement)
        {

            Vector3 maxMovement = movement;

            Vector3 newPos = new Vector3();
            newPos.x = StartPos.x + UnityEngine.Random.Range(-maxMovement.x, maxMovement.x);
            newPos.y = StartPos.y + UnityEngine.Random.Range(-maxMovement.y, maxMovement.y);
            newPos.z = StartPos.z + UnityEngine.Random.Range(-maxMovement.z, maxMovement.z);

            float time = UnityEngine.Random.Range(0.25f, 0.5f);

            camera.transform.DOMove(newPos, time).SetEase(Ease.Linear).OnComplete(() => LoopShakeCamera(camera, StartPos, movement));

        }

        public static Vector2 GetFrustumFromCamera(Camera camera, Vector3 position)
        {

            var frustumHeight = 2.0f * Vector3.Distance(camera.transform.position, position) * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * camera.aspect;

            return new Vector2(frustumWidth, frustumHeight);

        }

        /// <summary>
        /// Get distance to camera to a width and a height of a element
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="uiElementToFit">optional : if given, will calculate the size to fix the UI element</param>
        /// <returns></returns>
        public static float GetDistanceToCamera(Camera camera, float width, float height, RectTransform uiElementToFit = null)
        {

            //check if there is a uiElementToFit
            if (uiElementToFit)
            {
                Vector2 uiSize = GetSizeOfUIElement(uiElementToFit);

                //not sure about the Mathf.Max but seem to work so
                float widthScale = uiSize.x / Screen.width;
                float heightScale = uiSize.y / Screen.height;

                // float scaleRatio = Mathf.Min(uiSize.x / Screen.width, uiSize.y / Screen.height);



                width /= widthScale;
                height /= heightScale;

            }

            //first we check the aspectRatio to make sure it does fit
            float cameraAspectRatio = camera.aspect;
            float widthRatio = height * cameraAspectRatio;

            //fix the height to fit width
            if (widthRatio < width)
            {
                height = width / cameraAspectRatio;
            }

            return height * 0.5f / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        }

        private static Vector2 GetSizeOfUIElement(RectTransform uiElementRect)
        {

            //maybe use transformPoint could work too
            //uiElementRect.TransformPoint

            Vector3[] corners = new Vector3[4];
            uiElementRect.GetWorldCorners(corners);

            float uiWidth = corners[2].x - corners[0].x;
            float uiHeight = corners[2].y - corners[0].y;

            return new Vector2(uiWidth, uiHeight);


        }

        public static Vector3 GetPositionOfGameObjectToUI(Camera camera, GameObject GameObjectToCenter, Vector3 cameraPositionToGameObject, RectTransform uiElement)
        {

            //let hack it first
            Vector3 originalCameraPos = camera.transform.position;

            //set camera to position to calculate the difference
            camera.transform.position = cameraPositionToGameObject;


            Vector3 zIn2d = camera.WorldToScreenPoint(new Vector3(0, 0, GameObjectToCenter.transform.position.z));

            //get corner
            Vector3[] corners = new Vector3[4];
            uiElement.GetWorldCorners(corners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float screenWidthMiddle = screenWidth / 2;
            float screenHeightMiddle = screenHeight / 2;


            float posX = corners[0].x + (corners[2].x - corners[0].x) / 2;
            float posY = corners[0].y + (corners[2].y - corners[0].y) / 2;

            Vector3 positionDifference = camera.ScreenToWorldPoint(new Vector3(posX, posY, zIn2d.z));

            //set back camera position
            camera.transform.position = originalCameraPos;

            //testing purpose
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = new Vector3(positionDifference.x, positionDifference.y, positionDifference.z);

            //return the correct position to center to ui element
            return new Vector3(cameraPositionToGameObject.x - (positionDifference.x - cameraPositionToGameObject.x), cameraPositionToGameObject.y - (positionDifference.y - cameraPositionToGameObject.y), cameraPositionToGameObject.z);


        }


    }
}
#endif