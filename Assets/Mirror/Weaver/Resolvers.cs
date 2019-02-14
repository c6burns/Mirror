// all the resolve functions for the weaver
// NOTE: these functions should be made extensions, but right now they still
//       make heavy use of Weaver.WeavingFailed and we'd have to check each one's return
//       value for null otherwise.
//       (original FieldType.Resolve returns null if not found too, so
//        exceptions would be a bit inconsistent here)
using Mono.Cecil;

namespace Mirror.Weaver
{
    public static class Resolvers
    {
        public static MethodReference ResolveMethod(TypeReference tr, AssemblyDefinition asmDef, string name)
        {
            //Console.WriteLine("ResolveMethod " + t.ToString () + " " + name);
            if (tr == null)
            {
                Log.Error("Type missing for " + name);
                Weaver.WeavingFailed = true;
                return null;
            }
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    return asmDef.MainModule.ImportReference(methodRef);
                }
            }
            Log.Error("ResolveMethod failed " + tr.Name + "::" + name + " " + tr.Resolve());

            // why did it fail!?
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                Log.Error("Method " + methodRef.Name);
            }

            Weaver.WeavingFailed = true;
            return null;
        }

        // TODO reuse ResolveMethod in here after Weaver.WeavingFailed was removed
        public static MethodReference ResolveMethodInParents(TypeReference tr, AssemblyDefinition asmDef, string name)
        {
            if (tr == null)
            {
                Log.Error("Type missing for " + name);
                Weaver.WeavingFailed = true;
                return null;
            }
            foreach (MethodDefinition methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    return asmDef.MainModule.ImportReference(methodRef);
                }
            }
            // Could not find the method in this class,  try the parent
            return ResolveMethodInParents(tr.Resolve().BaseType, asmDef, name);
        }

        // System.Byte[] arguments need a version with a string
        public static MethodReference ResolveMethodWithArg(TypeReference tr, AssemblyDefinition asmDef, string name, string argTypeFullName)
        {
            foreach (var methodRef in tr.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    if (methodRef.Parameters.Count == 1)
                    {
                        if (methodRef.Parameters[0].ParameterType.FullName == argTypeFullName)
                        {
                            return asmDef.MainModule.ImportReference(methodRef);
                        }
                    }
                }
            }
            Log.Error("ResolveMethodWithArg failed " + tr.Name + "::" + name + " " + argTypeFullName);
            Weaver.WeavingFailed = true;
            return null;
        }

        // reuse ResolveMethodWithArg string version
        public static MethodReference ResolveMethodWithArg(TypeReference tr, AssemblyDefinition asmDef, string name, TypeReference argType)
        {
            return ResolveMethodWithArg(tr, asmDef, name, argType.FullName);
        }

        public static MethodDefinition ResolveDefaultPublicCtor(TypeReference variable)
        {
            foreach (MethodDefinition methodRef in variable.Resolve().Methods)
            {
                if (methodRef.Name == ".ctor" &&
                    methodRef.Resolve().IsPublic &&
                    methodRef.Parameters.Count == 0)
                {
                    return methodRef;
                }
            }
            return null;
        }

        public static GenericInstanceMethod ResolveMethodGeneric(TypeReference t, AssemblyDefinition asmDef, string name, TypeReference genericType)
        {
            foreach (MethodDefinition methodRef in t.Resolve().Methods)
            {
                if (methodRef.Name == name)
                {
                    if (methodRef.Parameters.Count == 0)
                    {
                        if (methodRef.GenericParameters.Count == 1)
                        {
                            MethodReference tmp = asmDef.MainModule.ImportReference(methodRef);
                            GenericInstanceMethod gm = new GenericInstanceMethod(tmp);
                            gm.GenericArguments.Add(genericType);
                            if (gm.GenericArguments[0].FullName == genericType.FullName)
                            {
                                return gm;
                            }
                        }
                    }
                }
            }

            Log.Error("ResolveMethodGeneric failed " + t.Name + "::" + name + " " + genericType);
            Weaver.WeavingFailed = true;
            return null;
        }

        public static FieldReference ResolveField(TypeReference tr, AssemblyDefinition asmDef, string name)
        {
            foreach (FieldDefinition fd in tr.Resolve().Fields)
            {
                if (fd.Name == name)
                {
                    return asmDef.MainModule.ImportReference(fd);
                }
            }
            return null;
        }

        public static MethodReference ResolveProperty(TypeReference tr, AssemblyDefinition asmDef, string name)
        {
            foreach (PropertyDefinition pd in tr.Resolve().Properties)
            {
                if (pd.Name == name)
                {
                    return asmDef.MainModule.ImportReference(pd.GetMethod);
                }
            }
            return null;
        }
    }
}