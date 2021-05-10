#if !BACKOFFICE
using UnityEngine;
using System.IO;
#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using System.Collections.Generic;

namespace MetaLoop.Common.PlatformCommon.Utils
{
    public class Path
    {
        /// <summary>
        /// Gets the streaming assets persistant path. This function copy the filename in the Application.persistentDataPath so that we can then read/write data on it. Perfect for a Database file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetStreamingAssetsPersistantPath(string filename)
        {

            // return Application.streamingAssetsPath + "/" + filename;
            string outputPath = "";

            //we always overwrite now (stupid way of doing it)

#if UNITY_EDITOR || UNITY_STANDALONE

            outputPath = Application.streamingAssetsPath + "/" + filename;
            var filepath = string.Format("{0}/{1}", Application.persistentDataPath, filename);

            File.Copy(outputPath, filepath, true);

            outputPath = filepath;

#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, filename);

        //if (!File.Exists(filepath))
          if(filepath != null)
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID

                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
           
                var loadFile = new WWW("jar:file://" + Application.dataPath + "!/assets/" + filename);  // this is the path to your StreamingAssets in android
                while (!loadFile.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
                // then save to Application.persistentDataPath
                File.WriteAllBytes(filepath, loadFile.bytes);
#elif UNITY_IOS
                     var loadFile = Application.dataPath + "/Raw/" + filename;  // this is the path to your StreamingAssets in iOS
                    // then save to Application.persistentDataPath
                    File.Copy(loadFile, filepath, true);
                    Debug.Log("iOS Database copied to " + filepath);

#elif UNITY_WP8
                    var loadFile = Application.dataPath + "/StreamingAssets/" + filename;  // this is the path to your StreamingAssets in iOS
                    // then save to Application.persistentDataPath
                    File.Copy(loadFile, filepath);

#elif UNITY_WINRT
		    var loadFile = Application.dataPath + "/StreamingAssets/" + filename;  // this is the path to your StreamingAssets in iOS
		    // then save to Application.persistentDataPath
		    File.Copy(loadFile, filepath);
#else
	    var loadFile = Application.dataPath + "/StreamingAssets/" + filename;  // this is the path to your StreamingAssets in iOS
	    // then save to Application.persistentDataPath
	    File.Copy(loadFile, filepath);

#endif

        }

        outputPath = filepath;
#endif
            Debug.Log("Final PATH: " + outputPath);
            return outputPath;

        }

        /// <summary>
        /// Gets the streaming assets path for a specific filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetStreamingAssetsPath(string filename)
        {
            string path;
#if UNITY_EDITOR || UNITY_STANDALONE
            path = "file:" + Application.dataPath + "/StreamingAssets/{0}";
#elif UNITY_ANDROID
     path = "jar:file://"+ Application.dataPath + "!/assets/{0}";
#elif UNITY_IOS
     path = "file:" + Application.dataPath + "/Raw/{0}";
#else
     //Desktop (Mac OS or Windows)
     path = "file:"+ Application.dataPath + "/StreamingAssets/{0}";
#endif

            return string.Format(path, filename);
        }


    }

}
#endif