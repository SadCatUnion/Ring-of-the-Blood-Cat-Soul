using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puerts;
using System.IO;

public class GlobalJSEnv
{
    private static JsEnv _Env;
    public static JsEnv Env
    {
        get
        {
            if (_Env == null)
            {
                _Env = new JsEnv(new GameScriptLoader(""),4396);
            }
            return _Env;
        }
    }

}
