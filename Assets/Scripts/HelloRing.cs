using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puerts;

public class HelloRing : MonoBehaviour
{
    void Start()
    {
        var jsEnv = new JsEnv(new GameScriptLoader(""));
        jsEnv.Eval("require('GameRoot')");
    }
}
