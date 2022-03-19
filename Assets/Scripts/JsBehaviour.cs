using UnityEngine;
using Puerts;
using System;
using System.Collections;

public delegate void ModuleInit(JsBehaviour monoBehaviour);

public class JsBehaviour : MonoBehaviour
{
    public string ModuleName;

    public Action JsStart;
    public Action JsUpdate;
    public Action JsOnDestroy;

    static JsEnv jsEnv;

    private void OnEnable()
    {
        jsEnv = GlobalJSEnv.Env;
        var varname = "m_" + Time.frameCount;
        var init = jsEnv.Eval<ModuleInit>($"const {varname} = require('{ModuleName}'); {varname}.init;");

        init?.Invoke(this);

        Application.runInBackground = true;
    }

    private void OnDisable()
    {
        JsOnDestroy?.Invoke();
    }

    void Start()
    {
        JsStart?.Invoke();
    }

    void Update()
    {
        jsEnv.Tick();
        JsUpdate?.Invoke();
    }

    void OnDestroy()
    {
        JsStart = null;
        JsUpdate = null;
        JsOnDestroy = null;
    }
}
