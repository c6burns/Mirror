using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

namespace Mirror
{
    public static class WeaverAssembler
    {
        public const string OutputBaseDirectory = "Assets/Mirror/Tests/WeaverTests~/";
        public const string OutputScratchDirectory = OutputBaseDirectory + "Scratch";

        private static Assembly[] _cachedAssemblies;

        public static string OutputFile { get; set; }
        public static HashSet<string> SourceFiles { get; private set; }
        public static HashSet<string> ReferenceAssemblies { get; private set; }
        public static bool AllowUnsafe { get; set; }
        public static List<CompilerMessage> CompilerMessages { get; private set; }
        public static bool CompilerErrors { get; private set; }
        public static bool DeleteOutputOnClear { get; set; }

        // static constructor to initialize static properties
        static WeaverAssembler()
        {
            EnsureOutputDirectory();

            SourceFiles = new HashSet<string>();
            ReferenceAssemblies = new HashSet<string>();
            CompilerMessages = new List<CompilerMessage>();
        }

        // make sure the needed directory exists
        static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(OutputBaseDirectory))
            {
                throw new DirectoryNotFoundException("Missing WeaverAssembly base output folder");
            }

            if (!Directory.Exists(OutputScratchDirectory))
            {
                Directory.CreateDirectory(OutputScratchDirectory);
            }
        }

        // clear everything from the output directory
        static void ClearOutputDirectory()
        {
            foreach (string tmpFile in Directory.EnumerateFiles(OutputScratchDirectory))
            {
                try
                {
                    File.Delete(tmpFile);

                }
                catch { }
            }
        }

        // cache pipeline asms if not already
        static void EnsureCachedAssemblies()
        {
            if (_cachedAssemblies == null)
            {
                _cachedAssemblies = CompilationPipeline.GetAssemblies();
            }
        }

        // clear cached pipeline asms
        static void ClearCachedAssemblies()
        {
            _cachedAssemblies = null;
        }

        // Add a range of source files to compile
        public static void AddSourceFiles(string[] sourceFiles)
        {
            foreach (string src in sourceFiles)
            {
                SourceFiles.Add(OutputBaseDirectory + src);
            }
        }

        // Add a range of reference files by full path
        public static void AddReferencesByFullPath(string[] refAsms)
        {
            foreach (string asm in refAsms)
            {
                ReferenceAssemblies.Add(asm);
            }
        }

        // Add a range of reference files by assembly name only
        public static void AddReferencesByAssemblyName(string[] refAsms)
        {
            foreach (string asm in refAsms)
            {
                string asmFullPath;
                if (FindReferenceAssemblyPath(asm, out asmFullPath))
                {
                    ReferenceAssemblies.Add(asmFullPath);
                }
            }
        }

        // Find reference assembly specified by asmName and store its full path in asmFullPath
        // do not pass in paths in asmName, just assembly names
        public static bool FindReferenceAssemblyPath(string asmName, out string asmFullPath)
        {
            asmFullPath = "";

            EnsureCachedAssemblies();

            foreach (Assembly asm in _cachedAssemblies)
            {
                foreach (string asmRef in asm.compiledAssemblyReferences)
                {
                    if (asmRef.EndsWith(asmName))
                    {
                        asmFullPath = asmRef;
                        return true;
                    }
                }
            }

            return false;
        }

        // clear referenced asms
        public static void ClearReferences()
        {
            ReferenceAssemblies.Clear();
        }

        // clear all settings except for referenced assemblies (which are cleared with ClearReferences)
        public static void Clear()
        {
            if (DeleteOutputOnClear)
            {
                ClearOutputDirectory();
            }

            CompilerErrors = false;
            OutputFile = "";
            SourceFiles.Clear();
            CompilerMessages.Clear();
            AllowUnsafe = false;
            DeleteOutputOnClear = false;
            ClearCachedAssemblies();
        }

        // build synchronously
        public static void Build()
        {
            BuildAssembly(true);
        }

        // build asynchronously - this isn't currently used
        public static void BuildAsync()
        {
            BuildAssembly(false);
        }

        private static void BuildAssembly(bool wait)
        {
            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(OutputBaseDirectory + OutputFile, SourceFiles.ToArray());
            assemblyBuilder.additionalReferences = ReferenceAssemblies.ToArray();
            if (AllowUnsafe)
            {
                assemblyBuilder.compilerOptions.AllowUnsafeCode = true;
            }

            assemblyBuilder.buildStarted += delegate (string assemblyPath)
            {
            //Debug.LogFormat("Assembly build started for {0}", assemblyPath);
        };

            assemblyBuilder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
            {
                CompilerMessages.AddRange(compilerMessages);
                foreach (CompilerMessage cm in compilerMessages)
                {
                    if (cm.type == CompilerMessageType.Warning)
                    {
                    //Debug.LogWarningFormat("{0}:{1} -- {2}", cm.file, cm.line, cm.message);
                }
                    else if (cm.type == CompilerMessageType.Error)
                    {
                        Debug.LogErrorFormat("{0}:{1} -- {2}", cm.file, cm.line, cm.message);
                        CompilerErrors = true;
                    }
                }
            };

            // Start build of assembly
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}", assemblyBuilder.assemblyPath);
                return;
            }

            if (wait)
            {
                while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
        }
    }
}
