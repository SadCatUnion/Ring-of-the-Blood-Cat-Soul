using System.IO;
using System;
using UnityEngine;

public class GameScriptLoader : Puerts.ILoader
{
    public string debugRoot { get; private set; }

    public GameScriptLoader(string debugRoot)
    {
        this.debugRoot = debugRoot;
    }

    private string PathToUse(string filepath)
    {
        return
            // .cjs asset is only supported in unity2018+
#if UNITY_2018_1_OR_NEWER
            filepath.EndsWith(".cjs") || filepath.EndsWith(".mjs") ?
            filepath.Substring(0, filepath.Length - 4) :
#endif
                filepath;
    }

    public bool FileExists(string filepath)
    {
        if (filepath.StartsWith("puerts/")) return true;

        return File.Exists(Path.Combine(Application.streamingAssetsPath, filepath + ".txt").Replace("\\", "/"));
    }


    public string ReadFile(string filepath, out string debugpath)
    {

        debugpath = Path.Combine(debugRoot, filepath);
        if (filepath.StartsWith("puerts/"))
        {
            string pathToUse = this.PathToUse(filepath);
            UnityEngine.TextAsset file = (UnityEngine.TextAsset)UnityEngine.Resources.Load(pathToUse);

            return file == null ? null : file.text;
        }

        return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, filepath + ".txt").Replace("\\", "/"));
    }

    public void Close() { }
}


