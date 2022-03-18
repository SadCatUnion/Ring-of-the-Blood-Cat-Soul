/*
 * Tencent is pleased to support the open source community by making InjectFix available.
 * Copyright (C) 2019 THL A29 Limited, a Tencent company.  All rights reserved.
 * InjectFix is licensed under the MIT License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
 * This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
 */

using Puerts;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

//1、配置类必须打[Configure]标签
//2、必须放Editor目录
[Configure]
public class ExamplesCfg
{
    static IEnumerable<Type> Bindings
    {
        get
        {
            return new List<Type>()
            {
                //直接指定的类型
                typeof(JsEnv),
                typeof(ILoader),
                typeof(JsBehaviour),
                typeof(System.Array),
                typeof(UnityEngine.Events.UnityEvent),
            };
        }
    }
    [Binding]
    static IEnumerable<Type> DynamicBindings
    {
        get
        {
            // 在这里添加名字空间
            var namespaces = new List<string>()
            {
                "UnityEngine",
                "UnityEngine.UI",
            };
            var unityTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                              where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                              from type in assembly.GetExportedTypes()
                              where type.Namespace != null && namespaces.Contains(type.Namespace) && !IsExcluded(type)
                              select type);
            string[] customAssemblys = new string[] {
                "Assembly-CSharp",
            };
            var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                               where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                               from type in assembly.GetExportedTypes()
                               where type.Namespace == null || !type.Namespace.StartsWith("Puerts")
                                    && !IsExcluded(type)
                               select type);
            return unityTypes
                .Concat(customTypes)
                .Concat(Bindings)
                .Distinct();
        }
    }
    static bool IsExcluded(Type type)
    {
        if (type == null)
            return false;

        string assemblyName = Path.GetFileName(type.Assembly.Location);
        if (excludeAssemblys.Contains(assemblyName))
            return true;

        string fullname = type.FullName != null ? type.FullName.Replace("+", ".") : "";
        if (excludeTypes.Contains(fullname))
            return true;
        return IsExcluded(type.BaseType);
    }
    //需要排除的程序集
    static List<string> excludeAssemblys = new List<string>{
        "UnityEditor.dll",
        "Assembly-CSharp-Editor.dll",
    };
    //需要排除的类型
    static List<string> excludeTypes = new List<string>
    {
        "UnityEngine.iPhone",
        "UnityEngine.iPhoneTouch",
        "UnityEngine.iPhoneKeyboard",
        "UnityEngine.iPhoneInput",
        "UnityEngine.iPhoneAccelerationEvent",
        "UnityEngine.iPhoneUtils",
        "UnityEngine.iPhoneSettings",
        "UnityEngine.AndroidInput",
        "UnityEngine.AndroidJavaProxy",
        "UnityEngine.BitStream",
        "UnityEngine.ADBannerView",
        "UnityEngine.ADInterstitialAd",
        "UnityEngine.RemoteNotification",
        "UnityEngine.LocalNotification",
        "UnityEngine.LightingSettings",
        "UnityEngine.NotificationServices",
        "UnityEngine.MasterServer",
        "UnityEngine.Network",
        "UnityEngine.NetworkView",
        "UnityEngine.ParticleSystemRenderer",
        "UnityEngine.ParticleSystem.CollisionEvent",
        "UnityEngine.ProceduralPropertyDescription",
        "UnityEngine.ProceduralTexture",
        "UnityEngine.ProceduralMaterial",
        "UnityEngine.ProceduralSystemRenderer",
        "UnityEngine.TerrainData",
        "UnityEngine.HostData",
        "UnityEngine.RPC",
        "UnityEngine.AnimationInfo",
        "UnityEngine.UI.IMask",
        "UnityEngine.Caching",
        "UnityEngine.Handheld",
        "UnityEngine.MeshRenderer",
        "UnityEngine.UI.DefaultControls",
        "UnityEngine.AnimationClipPair", //Obsolete
        "UnityEngine.CacheIndex", //Obsolete
        "UnityEngine.SerializePrivateVariables", //Obsolete
        "UnityEngine.Networking.NetworkTransport", //Obsolete
        "UnityEngine.Networking.ChannelQOS", //Obsolete
        "UnityEngine.Networking.ConnectionConfig", //Obsolete
        "UnityEngine.Networking.HostTopology", //Obsolete
        "UnityEngine.Networking.GlobalConfig", //Obsolete
        "UnityEngine.Networking.ConnectionSimulatorConfig", //Obsolete
        "UnityEngine.Networking.DownloadHandlerMovieTexture", //Obsolete
        "AssetModificationProcessor", //Obsolete
        "AddressablesPlayerBuildProcessor", //Obsolete
        "UnityEngine.WWW", //Obsolete
        "UnityEngine.EventSystems.TouchInputModule", //Obsolete
        "UnityEngine.MovieTexture", //Obsolete[ERROR]
        "UnityEngine.NetworkPlayer", //Obsolete[ERROR]
        "UnityEngine.NetworkViewID", //Obsolete[ERROR]
        "UnityEngine.NetworkMessageInfo", //Obsolete[ERROR]
        "UnityEngine.UI.BaseVertexEffect", //Obsolete[ERROR]
        "UnityEngine.UI.IVertexModifier", //Obsolete[ERROR]
        //Windows Obsolete[ERROR]
        "UnityEngine.EventProvider",
        "UnityEngine.UI.GraphicRebuildTracker",
        "UnityEngine.GUI.GroupScope",
        "UnityEngine.GUI.ScrollViewScope",
        "UnityEngine.GUI.ClipScope",
        "UnityEngine.GUILayout.HorizontalScope",
        "UnityEngine.GUILayout.VerticalScope",
        "UnityEngine.GUILayout.AreaScope",
        "UnityEngine.GUILayout.ScrollViewScope",
        "UnityEngine.GUIElement",
        "UnityEngine.GUILayer",
        "UnityEngine.GUIText",
        "UnityEngine.GUITexture",
        "UnityEngine.ClusterInput",
        "UnityEngine.ClusterNetwork",
        //System
        "System.Tuple",
        "System.Double",
        "System.Single",
        "System.ArgIterator",
        "System.SpanExtensions",
        "System.TypedReference",
        "System.StringBuilderExt",
        "System.IO.Stream",
        "System.Net.HttpListenerTimeoutManager",
        "System.Net.Sockets.SocketAsyncEventArgs",
    };

    [BlittableCopy]
    static IEnumerable<Type> Blittables
    {
        get
        {
            return new List<Type>()
            {
                //打开这个可以优化Vector3的GC，但需要开启unsafe编译
                //typeof(Vector3),
            };
        }
    }

    [Typing]
    static IEnumerable<Type> Typings
    {
        get
        { 
            //静态或动态列表
            return new List<Type>()
            {
                
            };
        }
    }
    
    [Filter]
    static bool FilterMethods(System.Reflection.MemberInfo mb)
    {
        // 排除 MonoBehaviour.runInEditMode, 在 Editor 环境下可用发布后不存在
        //if (mb.DeclaringType == typeof(MonoBehaviour) && mb.Name == "runInEditMode") {
        //    return true;
        //}
        //return false;

        return PuertsFilter.Filter(mb);
    }
}
