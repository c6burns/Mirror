using System;
using System.IO;
using System.Collections.Generic;
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
        public const string OutputTempDirectory = WeaverAssembler.OutputBaseDirectory + "Temp/";

        // caches CompilationPipeline.Assemblies
        public static Assembly[] PipelineAssemblies { get; private set; }

        // all directories for resolver
        public static string[] ReferenceDirectories { get; private set; }

        // key assemblies guaranteed to be in CompilationPipeline
        public static Assembly MainAssembly { get; private set; }
        public static Assembly MirrorAssembly { get; private set; }
        public static Assembly MirrorTestsAssembly { get; private set; }

        public static string MirrorMockAssemblyPath { get; private set; }
        public static string MainAssemblyPath { get; private set; }
        public static string MirrorAssemblyPath { get; private set; }
        public static string MirrorTestsAssemblyPath { get; private set; }
        public static string UnityEngineAssemblyPath { get; private set; }
        public static string UnityEngineCoreAssemblyPath { get; private set; }
        public static string UnityEditorAssemblyPath { get; private set; }

        public static TypeDefinition MockMBTypeDef { get; private set; }
        public static TypeDefinition MBTypeDef { get; private set; }
        public static TypeDefinition NBTypeDef { get; private set; }

        #region Initialization: assign required Assembly and Type definitions
        static WeaverMocker()
        {
            Setup();
        }

        private static void AssignAssemblyPaths()
        {
            PipelineAssemblies = CompilationPipeline.GetAssemblies();
            HashSet<string> refPaths = new HashSet<string>();
            foreach (Assembly asm in PipelineAssemblies)
            {
                foreach (string refPath in asm.compiledAssemblyReferences)
                {
                    refPaths.Add(Path.GetDirectoryName(refPath));
                }
                refPaths.Add(Path.GetDirectoryName(asm.outputPath));

                switch (asm.name)
                {
                    case "Mirror":
                        MirrorAssembly = asm;
                        MirrorAssemblyPath = MirrorAssembly.outputPath;
                        break;
                    case "Mirror.Tests":
                        MirrorTestsAssembly = asm;
                        MirrorTestsAssemblyPath = MirrorAssembly.outputPath;
                        break;
                    case "Assembly-CSharp":
                        MainAssembly = asm;
                        MainAssemblyPath = MainAssembly.outputPath;
                        break;
                }
            }
            ReferenceDirectories = new string[refPaths.Count];
            refPaths.CopyTo(ReferenceDirectories);

            UnityEditorAssemblyPath = UnityEditorInternal.InternalEditorUtility.GetEditorAssemblyPath();
            UnityEngineAssemblyPath = UnityEditorInternal.InternalEditorUtility.GetEngineAssemblyPath();
            UnityEngineCoreAssemblyPath = UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath();

            if (string.IsNullOrEmpty(MirrorAssemblyPath))
            {
                throw new InvalidOperationException("Mirror.dll not found");
            }
            if (string.IsNullOrEmpty(MirrorTestsAssemblyPath))
            {
                throw new InvalidOperationException("Mirror.Tests.dll not found");
            }
            if (string.IsNullOrEmpty(UnityEngineAssemblyPath))
            {
                throw new InvalidOperationException("UnityEngine.dll not found");
            }
            if (string.IsNullOrEmpty(UnityEngineCoreAssemblyPath))
            {
                throw new InvalidOperationException("UnityEngine.CoreModule.dll not found");
            }
            if (string.IsNullOrEmpty(UnityEditorAssemblyPath))
            {
                throw new InvalidOperationException("UnityEditor.dll not found");
            }
            if (string.IsNullOrEmpty(MainAssemblyPath))
            {
                throw new InvalidOperationException("Assembly-CSharp.dll not found");
            }

            MirrorMockAssemblyPath = OutputTempDirectory + Path.GetFileName(MirrorAssemblyPath);
        }

        // make sure the needed directory exists
        static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(OutputBaseDirectory))
            {
                throw new DirectoryNotFoundException("Missing WeaverAssembly base output folder");
            }

            if (!Directory.Exists(OutputTempDirectory))
            {
                Directory.CreateDirectory(OutputTempDirectory);
            }
        }

        public static void Setup()
        {
            EnsureOutputDirectory();
            AssignAssemblyPaths();
        }
        #endregion

        public static MethodDefinition ResolveMethod(TypeDefinition typeDef, string methodName)
        {
            foreach (MethodDefinition method in typeDef.Methods)
            {
                //Debug.LogFormat("{0} = {1}", method.Name, methodName);
                if (method.Name == methodName)
                {
                    return method;
                }
            }
            return null;
        }

        public static void SendCommandInternal(Type invokeClass, string cmdName, NetworkWriter writer, int channelId)
        {
            Debug.LogFormat("HandleSendCommandInternal called: {0} - {1} - {2} - {3}", invokeClass.Name, cmdName, writer.Position, channelId);
        }

        public static void DoStuff()
        {
            bool needsWrite = false;
            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            foreach (string refPath in ReferenceDirectories) {
                assemblyResolver.AddSearchDirectory(refPath);
            }

            ReaderParameters readParams = new ReaderParameters { AssemblyResolver = assemblyResolver };
            using (AssemblyDefinition mirrorAsmDef = AssemblyDefinition.ReadAssembly(MirrorAssemblyPath, readParams))
            using (AssemblyDefinition unityCoreAsmDef = AssemblyDefinition.ReadAssembly(UnityEngineCoreAssemblyPath, readParams))
            using (AssemblyDefinition mirrorTestsAsmDef = AssemblyDefinition.ReadAssembly(MirrorTestsAssemblyPath, readParams))
            {
                ModuleDefinition mirrorModDef = mirrorAsmDef.MainModule;
                ModuleDefinition mirrorTestsModDef = mirrorTestsAsmDef.MainModule;

                TypeDefinition networkBehaviourTD = mirrorModDef.GetType("Mirror.NetworkBehaviour");
                TypeDefinition mockMonoBehaviourTD = mirrorTestsAsmDef.MainModule.GetType("Mirror.MockMonoBehaviour");
                TypeDefinition weaverMockerTD = mirrorTestsAsmDef.MainModule.ImportReference(typeof(Mirror.WeaverMocker)).Resolve();
                TypeDefinition monoBehaviourTD = unityCoreAsmDef.MainModule.GetType("UnityEngine.MonoBehaviour");

                MethodDefinition mockCommand = ResolveMethod(weaverMockerTD, "SendCommandInternal");
                MethodDefinition realCommand = ResolveMethod(networkBehaviourTD, "SendCommandInternal");

                //mirrorModDef.ImportReference(weaverMockerTD);
                MethodReference mockCommandRef = mirrorModDef.ImportReference(mockCommand);

                ILProcessor ilp = realCommand.Body.GetILProcessor();
                realCommand.Body.Instructions.Clear();
                ilp.Append(ilp.Create(OpCodes.Nop));
                for (int i = 1; i <= realCommand.Parameters.Count; i++)
                {
                    ilp.Append(ilp.Create(OpCodes.Ldarg, i));
                }
                ilp.Append(ilp.Create(OpCodes.Call, mockCommandRef));
                ilp.Append(ilp.Create(OpCodes.Ret));

                mirrorAsmDef.Write(MirrorMockAssemblyPath);

                //foreach (TypeDefinition type in mirrorModDef.Types)
                //{
                //    if (type != NBTypeDef && !type.IsDerivedFrom(NBTypeDef)) continue;

                //    //Debug.LogFormat("Type: {0}", type.FullName);
                //    foreach (MethodDefinition method in type.Methods)
                //    {
                //        if (method.Name == "SendCommandInternal")
                //        {
                //            ILProcessor ilp = method.Body.GetILProcessor();
                //            foreach (Instruction op in method.Body.Instructions)
                //            {
                //                Debug.LogFormat("op: {0}", op.ToString());
                //            }

                //            needsWrite = true;
                //        }
                //    }
                //}

                //if (needsWrite)
                //{
                //    mirrorAsmDef.Write(MirrorMockAssemblyPath);
                //}
            }
        }
    }
}
