// https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using System;
using System.Linq;
using System.Collections.Generic;

using Mono.MirrorCecil;


namespace Mirror.Weaver
{
    // InitializeOnLoad is needed for Unity to call the static constructor on load
    [InitializeOnLoad]
    public class CompilationFinishedHook
    {
        // used to determine string operation on ExcludedAssembly
        public enum ExcludedAssemblyType
        {
            StartsWith,
            EndsWith,
            Contains,
            Equals,
        }

        // defines a string operation and string token to determine if an assembly should be excluded from weaving
        public class ExcludedAssembly
        {
            public ExcludedAssemblyType type;
            public string token;
            public ExcludedAssembly(ExcludedAssemblyType excludeType, string excludeToken)
            {
                type = excludeType;
                token = excludeToken;
            }
        }

        public static Action<string> OnMessageMethod; // delegate to subscribe to debug messages
        public static Action<string> OnWarningMethod; // delegate to subscribe to warning messages
        public static Action<string> OnErrorMethod; // delete to subscribe to error messages

        public static bool HooksDisabled { get; set; } // controls whether Weaving is enabled or disabled
        public static bool UnityLogDisabled { get; set; } // controls weather Weaver reports errors to the Unity console
        public static bool WeaveFailed { get; private set; } // holds the status of our latest Weave operation
        public static List<ExcludedAssembly> ExcludedAssemblies { get; private set; } // holds all of the assemblies we will not weave

        // static constructor installs our callbacks into the CompilationPipeline
        static CompilationFinishedHook()
        {
            // configure everything we will not weave (eg. our own assemblies, editor assemblies)
            // TODO: add everything known from Unity components to speed up weaving, as none will have Mirror code in them
            ExcludedAssemblies = new List<ExcludedAssembly>();
            ExcludedAssemblies.Add(new ExcludedAssembly(ExcludedAssemblyType.Equals, "Telepathy.dll")); // ours
            ExcludedAssemblies.Add(new ExcludedAssembly(ExcludedAssemblyType.Equals, "Mirror.dll")); // ours
            ExcludedAssemblies.Add(new ExcludedAssembly(ExcludedAssemblyType.Equals, "Mirror.Weaver.dll")); // ours
            ExcludedAssemblies.Add(new ExcludedAssembly(ExcludedAssemblyType.EndsWith, "Editor.dll")); // editor DLLs like Assembly-CSharp-Editor.dll
            ExcludedAssemblies.Add(new ExcludedAssembly(ExcludedAssemblyType.StartsWith, "Unity.Entities")); // new ECS assemblies that we can't weave

            try
            {
                EditorApplication.LockReloadAssemblies();

                // weave assemblies every time after they are compiled
                CompilationPipeline.assemblyCompilationFinished += AssemblyCompilationFinishedHandler;

                // weave all existing assemblies
                WeaveAssemblies();
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
        }

        // check if the assembly is excluded from weaving
        static bool IsExcludedAssembly(string assemblyName)
        {
            foreach (ExcludedAssembly exclude in ExcludedAssemblies)
            {
                switch (exclude.type)
                {
                case ExcludedAssemblyType.StartsWith:
                    if (assemblyName.StartsWith(exclude.token)) return true;
                    break;
                case ExcludedAssemblyType.EndsWith:
                    if (assemblyName.EndsWith(exclude.token)) return true;
                    break;
                case ExcludedAssemblyType.Contains:
                    if (assemblyName.Contains(exclude.token)) return true;
                    break;
                case ExcludedAssemblyType.Equals:
                    if (assemblyName.Equals(exclude.token)) return true;
                    break;
                }
            }
            return false;
        }

        // debug message handler that also calls OnMessageMethod delegate
        static void HandleMessage(string msg)
        {
            if (!UnityLogDisabled) Debug.Log(msg);
            OnMessageMethod?.Invoke(msg);
        }

        // warning message handler that also calls OnWarningMethod delegate
        static void HandleWarning(string msg)
        {
            if (!UnityLogDisabled) Debug.LogWarning(msg);
            OnWarningMethod?.Invoke(msg);
        }

        // error message handler that also calls OnErrorMethod delegate
        static void HandleError(string msg)
        {
            if (!UnityLogDisabled) Debug.LogError(msg);
            OnErrorMethod?.Invoke(msg);
        }

        // manually weave all assemblies (only called once from ctor)
        static void WeaveAssemblies()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                HandleMessage("Weaving " + assembly.outputPath);
                AssemblyCompilationFinishedHandler(assembly.outputPath, new CompilerMessage[] { } );               
            }
        }

        // this handler is added to Unity's CompilationPipeline.assemblyCompilationFinished delegate in ctor
        // this is where we set up and perform the weave every time Unity finishes compiling an assembly
        static void AssemblyCompilationFinishedHandler(string assemblyPath, CompilerMessage[] messages)
        {
            if (HooksDisabled)
            {
                return;
            }

            // if user scripts can't be compiled because of errors,
            // assemblyCompilationFinished is still called but assemblyPath
            // file won't exist. in that case, do nothing.
            if (!File.Exists(assemblyPath))
            {
                HandleWarning("Weaving skipped because assembly doesnt exist: " + assemblyPath);
                return;
            }

            string assemblyName = Path.GetFileName(assemblyPath);
            if (IsExcludedAssembly(assemblyName))
            {
                return;
            }

            // UnityEngineCoreModule.DLL path:
            string unityEngineCoreModuleDLL = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();

            // outputDirectory is the directory of assemblyPath
            string outputDirectory = Path.GetDirectoryName(assemblyPath);

            string mirrorRuntimeDll = FindMirrorRuntime();
            if (!File.Exists(mirrorRuntimeDll))
            {
                // this is normal, it happens with any assembly that is built before mirror
                // such as unity packages or your own assemblies
                // those don't need to be weaved
                // if any assembly depends on mirror, then it will be built after
                return;
            }

            Console.WriteLine("Weaving: " + assemblyPath);
            // assemblyResolver: unity uses this by default:
            //   ICompilationExtension compilationExtension = GetCompilationExtension();
            //   IAssemblyResolver assemblyResolver = compilationExtension.GetAssemblyResolver(editor, file, null);
            // but Weaver creates it's own if null, which is this one:
            IAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            if (Program.Process(unityEngineCoreModuleDLL, mirrorRuntimeDll, outputDirectory, new string[] { assemblyPath }, GetExtraAssemblyPaths(assemblyPath), assemblyResolver, HandleWarning, HandleError))
            {
                WeaveFailed = false;
                Console.WriteLine("Weaving succeeded for: " + assemblyPath);
            } else
            {
                WeaveFailed = true;
                if (!UnityLogDisabled) Debug.LogError("Weaving failed for: " + assemblyPath);
            }
        }

        // Weaver needs the path for all the extra DLLs like UnityEngine.UI.
        // otherwise if a script that is being weaved (like a NetworkBehaviour)
        // uses UnityEngine.UI, then the Weaver won't be able to resolve it and
        // throw an error.
        // (the paths can be found by logging the extraAssemblyPaths in the
        //  original Weaver.Program.Process function.)
        static string[] GetExtraAssemblyPaths(string assemblyPath)
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.outputPath == assemblyPath)
                {
                    return assembly.compiledAssemblyReferences.Select(Path.GetDirectoryName).ToArray();
                }
            }

            if (!UnityLogDisabled) Debug.LogWarning("Unable to find configuration for assembly " + assemblyPath);
            return new string[] { };
        }

        // returns the full path of Mirror or an empty string if not found
        static string FindMirrorRuntime()
        {
            Assembly[] assemblies = CompilationPipeline.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.name == "Mirror")
                {
                    return assembly.outputPath;
                }
            }
            return "";
        }
    }
}
