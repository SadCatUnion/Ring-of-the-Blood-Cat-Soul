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
        if (jsEnv == null) jsEnv = new JsEnv(new GameScriptLoader(""), 9229);
        var varname = "m_" + Time.frameCount;
        var init = jsEnv.Eval<ModuleInit>("const "+varname+" = require('" + ModuleName + "'); "+varname+".init;");

        if (init != null) init(this);

        Application.runInBackground = true;
    }

    private void OnDisable()
    {
        if (JsOnDestroy != null) JsOnDestroy();
    }

    void Start()
    {
        if (JsStart != null) JsStart();
    }

    void Update()
    {
        jsEnv.Tick();
        if (JsUpdate != null) JsUpdate();
    }

    void OnDestroy()
    {
        JsStart = null;
        JsUpdate = null;
        JsOnDestroy = null;
    }
}
