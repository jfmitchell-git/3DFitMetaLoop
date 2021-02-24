using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;

namespace MetaLoop.Common.PlatformCommon.Unity.Shaders
{
    public class ShaderUtils
    {

        public static Renderer[] renderers;
        public static Material[] materials;
        public static string[] shaders;


        public static void ForceShaderHead(GameObject gameObject)
        {
            if (gameObject == null) return;
            renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers.Count() == 0) return;

            foreach (var rend in renderers)
            {
                materials = rend.sharedMaterials;
                shaders = new string[materials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    shaders[i] = materials[i].shader.name;
                }

                for (int i = 0; i < materials.Length; i++)
                {
                    Material currentMaterial = materials[i];

                    if (currentMaterial.shader.name.ToLower() == "standard")
                    {

                        // materials[i].shader = Shader.Find(shaders[i]);

                        currentMaterial.shader = Shader.Find(shaders[i]);
                        // StupidGame(materials[i], gameManager);


                    }
                }
            }
        }


        public static void FadeObject(MonoBehaviour fadeObject,float endValue = 0f,float speed = 0.5f)
        {

            List<Renderer> allRenderer = new List<Renderer>();

            if(fadeObject.GetComponent<Renderer>())
            {
                allRenderer.Add(fadeObject.GetComponent<Renderer>());
            }
                //fadeObject.GetComponent<Renderer>().materials.Where(p => p.HasProperty("_Color")).ToList().ForEach(p => p.DOFade(endValue, speed));

            //fade all children
            allRenderer.AddRange(fadeObject.GetComponentsInChildren<Renderer>());
            //.ToList().ForEach(p => p.materials.Where(j => j.HasProperty("_Color")).ToList().ToList().ForEach(j => j.DOFade(endValue, speed)));

            foreach(Renderer renderer in allRenderer)
            {

                //loop all materials
                foreach(Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                        material.DOFade(endValue, speed);


                    if (endValue == 0)
                    {
                        if (material.HasProperty("_EmissiveIntensity"))
                            material.DOFloat(endValue, "_EmissiveIntensity", speed);


                        if (material.HasProperty("_ReflectionGlow"))
                            material.DOFloat(endValue, "_ReflectionGlow", speed);

                    }
                }

            }

            //try some other keywords often found

        }
    }
}
