using System;
using UnityEngine;
using UnityEditor.Compilation;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mirror.Weaver;

namespace Mirror
{
    public static class WeaverMocker
    {
        // loosely coupled to WeaverAssembler for now
        public const string OutputBaseDirectory = WeaverAssembler.OutputBaseDirectory;

        // caches CompilationPipeline.Assemblies
        public static Assembly[] PipelineAssemblies { get; private set; }

        // key assemblies guaranteed to be in CompilationPipeline
        public static Assembly MainAssembly { get; private set; }
        public static Assembly MirrorAssembly { get; private set; }
        public static Assembly MirrorTestsAssembly { get; private set; }

        // required AssemblyDefinitions
        public static AssemblyDefinition MainAsmDef { get; private set; }
        public static AssemblyDefinition MirrorAsmDef { get; private set; }
        public static AssemblyDefinition MirrorTestsAsmDef { get; private set; }
        public static AssemblyDefinition UnityEngineAsmDef { get; private set; }
        public static AssemblyDefinition UnityEngineCoreAsmDef { get; private set; }
        public static AssemblyDefinition UnityEditorAsmDef { get; private set; }

        // required ModuleDefinitions
        public static ModuleDefinition MainModDef { get; private set; }
        public static ModuleDefinition MirrorModDef { get; private set; }
        public static ModuleDefinition MirrorTestsModDef { get; private set; }
        public static ModuleDefinition UnityEngineModDef { get; private set; }
        public static ModuleDefinition UnityEngineCoreModDef { get; private set; }
        public static ModuleDefinition UnityEditorModDef { get; private set; }

        // required TypeDefinitions
        public static TypeDefinition MockMBTypeDef { get; private set; }
        public static TypeDefinition MBTypeDef { get; private set; }
        public static TypeDefinition NBTypeDef { get; private set; }

        #region Initialization: assign required Assembly and Type definitions
        static WeaverMocker()
        {
            Setup();
        }

        private static void AssignAssemblies()
        {
            PipelineAssemblies = CompilationPipeline.GetAssemblies();
            foreach (Assembly asm in PipelineAssemblies)
            {
                Debug.LogFormat("{0}", asm.name);
                switch (asm.name)
                {
                    case "Mirror":
                        MirrorAssembly = asm;
                        MirrorAsmDef = AssemblyDefinition.ReadAssembly(MirrorAssembly.outputPath);
                        break;
                    case "Mirror.Tests":
                        MirrorTestsAssembly = asm;
                        MirrorTestsAsmDef = AssemblyDefinition.ReadAssembly(MirrorAssembly.outputPath);
                        break;
                    case "Assembly-CSharp":
                        MainAssembly = asm;
                        MainAsmDef = AssemblyDefinition.ReadAssembly(MainAssembly.outputPath);
                        break;
                }
            }

            string fullpath;
            fullpath = UnityEditorInternal.InternalEditorUtility.GetEditorAssemblyPath();
            if (!string.IsNullOrEmpty(fullpath))
            {
                UnityEditorAsmDef = AssemblyDefinition.ReadAssembly(fullpath);
            }

            fullpath = UnityEditorInternal.InternalEditorUtility.GetEngineAssemblyPath();
            if (!string.IsNullOrEmpty(fullpath))
            {
                UnityEngineAsmDef = AssemblyDefinition.ReadAssembly(fullpath);
            }

            fullpath = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
            if (!string.IsNullOrEmpty(fullpath))
            {
                UnityEngineCoreAsmDef = AssemblyDefinition.ReadAssembly(fullpath);
            }


            if (MirrorAsmDef == null)
            {
                throw new InvalidOperationException("Mirror.dll not found");
            }
            if (MirrorTestsAsmDef == null)
            {
                throw new InvalidOperationException("Mirror.Tests.dll not found");
            }
            if (UnityEngineAsmDef == null)
            {
                throw new InvalidOperationException("UnityEngine.dll not found");
            }
            if (UnityEngineCoreAsmDef == null)
            {
                throw new InvalidOperationException("UnityEngine.CoreModule.dll not found");
            }
            if (UnityEditorAsmDef == null)
            {
                throw new InvalidOperationException("UnityEditor.dll not found");
            }
            if (MainAsmDef == null)
            {
                throw new InvalidOperationException("Assembly-CSharp.dll not found");
            }
        }

        private static void AssignModules()
        {
            MainModDef = MainAsmDef.MainModule;
            MirrorModDef = MirrorAsmDef.MainModule;
            MirrorTestsModDef = MirrorTestsAsmDef.MainModule;
            UnityEngineModDef = UnityEngineAsmDef.MainModule;
            UnityEngineCoreModDef = UnityEngineCoreAsmDef.MainModule;
            UnityEditorModDef = UnityEditorAsmDef.MainModule;

            if (MirrorModDef == null)
            {
                throw new InvalidOperationException("Mirror.dll MainModule not found");
            }
            if (MirrorTestsModDef == null)
            {
                throw new InvalidOperationException("Mirror.Tests.dll MainModule not found");
            }
            if (UnityEngineModDef == null)
            {
                throw new InvalidOperationException("UnityEngine.dll MainModule not found");
            }
            if (UnityEngineCoreModDef == null)
            {
                throw new InvalidOperationException("UnityEngine.CoreModule.dll MainModule not found");
            }
            if (UnityEditorModDef == null)
            {
                throw new InvalidOperationException("UnityEditor.dll MainModule not found");
            }
            if (MainModDef == null)
            {
                throw new InvalidOperationException("Assembly-CSharp.dll MainModule not found");
            }
        }

        private static void AssignTypes()
        {
            NBTypeDef = MirrorModDef.GetType("Mirror.NetworkBehaviour");
            MockMBTypeDef = MirrorTestsModDef.GetType("Mirror.MockMonoBehaviour");
            MBTypeDef = UnityEngineCoreModDef.GetType("UnityEngine.MonoBehaviour");
        }

        private static void Setup()
        {
            AssignAssemblies();
            AssignModules();
            AssignTypes();
        }
        #endregion

        public static void DoStuff()
        {

            foreach (TypeDefinition type in MirrorModDef.Types)
            {
                if (type != NBTypeDef && !type.IsDerivedFrom(NBTypeDef)) continue;

                //Debug.LogFormat("Type: {0}", type.FullName);
                foreach (MethodDefinition method in type.Methods)
                {
                    if (method.Name == "SendCommandInternal")
                    {
                        ILProcessor ilp = method.Body.GetILProcessor();
                        foreach (Instruction op in method.Body.Instructions)
                        {
                            Debug.LogFormat("op: {0}", op.ToString());
                        }
                    }
                }
            }
        }
    }
}
