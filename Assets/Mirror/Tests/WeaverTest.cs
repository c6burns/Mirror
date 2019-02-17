﻿//#define LOG_WEAVER_OUTPUTS

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.Compilation;

using Mirror.Weaver;

namespace Mirror
{
    [TestFixture]
    public class WeaverTest
    {
        #region Private
        List<string> m_weaverErrors = new List<string>();
        void HandleWeaverError(string msg)
        {
#if LOG_WEAVER_OUTPUTS
            Debug.LogError(msg);
#endif
            m_weaverErrors.Add(msg);
        }

        List<string> m_weaverWarnings = new List<string>();
        void HandleWeaverWarning(string msg)
        {
#if LOG_WEAVER_OUTPUTS
            Debug.LogWarning(msg);
#endif
            m_weaverWarnings.Add(msg);
        }

        private void BuildAndWeaveTestAssembly(string baseName)
        {
            WeaverAssembler.OutputFile = baseName + ".dll";
            WeaverAssembler.AddSourceFiles(new string[] { baseName + ".cs" });
            WeaverAssembler.Build();

            Assert.That(WeaverAssembler.CompilerErrors, Is.False);
            if (m_weaverErrors.Count > 0)
            {
                Assert.That(m_weaverErrors[0], Does.StartWith("Mirror.Weaver error: "));
            }
        }
        #endregion

        #region Setup and Teardown
        [OneTimeSetUp]
        public void FixtureSetup()
        {
            // TextRenderingModule is only referenced to use TextMesh type to throw errors about types from another module
            WeaverAssembler.AddReferencesByAssemblyName(new string[] { "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.TextRenderingModule.dll", "Mirror.dll" });

            CompilationFinishedHook.UnityLogDisabled = true;
            CompilationFinishedHook.OnErrorMethod += HandleWeaverError;
            CompilationFinishedHook.OnWarningMethod += HandleWeaverWarning;
        }

        [OneTimeTearDown]
        public void FixtureCleanup()
        {
            CompilationFinishedHook.UnityLogDisabled = false;
        }

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestCleanup()
        {
            WeaverAssembler.DeleteOutputOnClear = true;
            WeaverAssembler.Clear();

            m_weaverWarnings.Clear();
            m_weaverErrors.Clear();
        }
        #endregion

        #region General tests
        [Test] // -----------------------------------------------------------------------------------------
        public void InvalidType()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.AtLeast(1));
            Assert.That(m_weaverErrors[0], Does.Match("please make sure to use a valid type"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void RecursionCount()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.AtLeast(1));
            Assert.That(m_weaverErrors[0], Does.Match("Check for self-referencing member variables"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void ClientGuardWrongClass()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverWarnings.Count, Is.EqualTo(1));
            Assert.That(m_weaverWarnings[0], Does.Match("\\[Client\\] guard on non-NetworkBehaviour script"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void ServerGuardWrongClass()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverWarnings.Count, Is.EqualTo(1));
            Assert.That(m_weaverWarnings[0], Does.Match("\\[Server\\] guard on non-NetworkBehaviour script"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void GuardCmdWrongClass()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverWarnings.Count, Is.AtLeast(4));
            Assert.That(m_weaverWarnings[0], Does.Match("\\[Server\\] guard on non-NetworkBehaviour script"));
            Assert.That(m_weaverWarnings[1], Does.Match("\\[Server\\] guard on non-NetworkBehaviour script"));
            Assert.That(m_weaverWarnings[2], Does.Match("\\[Client\\] guard on non-NetworkBehaviour script"));
            Assert.That(m_weaverWarnings[3], Does.Match("\\[Client\\] guard on non-NetworkBehaviour script"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void JaggedArray()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.AtLeast(1));
            Assert.That(m_weaverErrors[0], Does.Match("Jagged and multidimensional arrays are not supported"));
        }
        #endregion

        #region SyncVar tests
        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsNoHook()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar Hook function .* not found for"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsNoHookParams()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* must have one argument"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsTooManyHookParams()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* must have one argument"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsWrongHookType()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar Hook function .* has wrong type signature for"));
        }

       [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsDerivedNetworkBehaviour()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be derived from NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsDerivedScriptableObject()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be derived from ScriptableObject"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsStatic()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be static"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsGenericParam()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsInterface()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be an interface"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsDifferentModule()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be a different module"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsCantBeArray()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("SyncVar .* cannot be an array"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsSyncList()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
            Assert.That(m_weaverWarnings.Count, Is.EqualTo(2));
            Assert.That(m_weaverWarnings[0], Does.Match("SyncLists should not be marked with SyncVar"));
            Assert.That(m_weaverWarnings[1], Does.Match("SyncLists should not be marked with SyncVar"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncVarsMoreThan63()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script class .* has too many SyncVars"));
        }
        #endregion

        #region SyncList tests
        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListMissingParamlessCtor()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Missing parameter-less constructor"));
        }
        #endregion

        #region SyncListStruct tests
        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListStructValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListStructGenericGeneric()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Struct passed into SyncListStruct<T> can't have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListStructMemberGeneric()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("member cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListStructMemberInterface()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("member cannot be an interface"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncListStructMemberBasicType()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(2));
            Assert.That(m_weaverErrors[0], Does.Match("please make sure to use a valid type"));
            Assert.That(m_weaverErrors[1], Does.Match("member variables must be basic types"));
        }
        #endregion

        #region NetworkBehaviour tests
        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourAbstractBaseValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourGeneric()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("NetworkBehaviour .* cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdGenericParam()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command .* cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdCoroutine()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command .* cannot be a coroutine"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdVoidReturn()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command .* must have a void return type"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcGenericParam()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc .* cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcCoroutine()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc .* cannot be a coroutine"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcVoidReturn()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc .* must have a void return type"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamOut()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* cannot have out parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamOptional()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpcfunction .* cannot have optional parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamRef()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* cannot have ref parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamAbstract()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* cannot have abstract parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamComponent()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* You cannot pass a Component to a remote call"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamNetworkConnection()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcParamNetworkConnectionNotFirst()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc .* first parameter must be a NetworkConnection"));
        }
        

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourTargetRpcDuplicateName()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Duplicate Target Rpc name"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcGenericParam()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc .* cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcCoroutine()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc .* cannot be a coroutine"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcVoidReturn()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc .* must have a void return type"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamOut()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc function .* cannot have out parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamOptional()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpcfunction .* cannot have optional parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamRef()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc function .* cannot have ref parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamAbstract()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc function .* cannot have abstract parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamComponent()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc function .* You cannot pass a Component to a remote call"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcParamNetworkConnection()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(2));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc .* cannot use a NetworkConnection as a parameter"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourClientRpcDuplicateName()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Duplicate ClientRpc name"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamOut()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* cannot have out parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamOptional()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Commandfunction .* cannot have optional parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamRef()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* cannot have ref parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamAbstract()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* cannot have abstract parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamComponent()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* You cannot pass a Component to a remote call"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdParamNetworkConnection()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(2));
            Assert.That(m_weaverErrors[0], Does.Match("Command .* cannot use a NetworkConnection as a parameter"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void NetworkBehaviourCmdDuplicateName()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Duplicate Command name"));
        }
        #endregion

        #region Command tests
        [Test] // -----------------------------------------------------------------------------------------
        public void CommandValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void CommandStartsWithCmd()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* doesnt have 'Cmd' prefix"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void CommandCantBeStatic()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Command function .* cant be a static method"));
        }
        #endregion

        #region ClientRpc tests
        [Test] // -----------------------------------------------------------------------------------------
        public void ClientRpcValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void ClientRpcStartsWithRpc()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Rpc function .* doesnt have 'Rpc' prefix"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void ClientRpcCantBeStatic()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("ClientRpc function .* cant be a static method"));
        }
        #endregion

        #region TargetRpc tests
        [Test] // -----------------------------------------------------------------------------------------
        public void TargetRpcValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void TargetRpcStartsWithTarget()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* doesnt have 'Target' prefix"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void TargetRpcCantBeStatic()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("TargetRpc function .* cant be a static method"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void TargetRpcNetworkConnectionMissing()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* must have a NetworkConnection as the first parameter"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void TargetRpcNetworkConnectionNotFirst()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Target Rpc function .* first parameter must be a NetworkConnection"));
        }
        #endregion

        #region TargetRpc tests
        [Test] // -----------------------------------------------------------------------------------------
        public void SyncEventValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncEventStartsWithEvent()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Event .* doesnt have 'Event' prefix"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void SyncEventParamGeneric()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Event .* cannot have generic parameters"));
        }
        #endregion

        #region MonoBehaviour tests
        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourSyncVar()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses \\[SyncVar\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourSyncList()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* defines field .* with type .*, but it's not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourCommand()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses \\[Command\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourClientRpc()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses \\[ClientRpc\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourTargetRpc()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses \\[TargetRpc\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourServer()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses the attribute \\[Server\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourServerCallback()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses the attribute \\[ServerCallback\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourClient()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses the attribute \\[Client\\] .* but is not a NetworkBehaviour"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MonoBehaviourClientCallback()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("Script .* uses the attribute \\[ClientCallback\\] .* but is not a NetworkBehaviour"));
        }
        #endregion

        #region Message tests
        [Test] // -----------------------------------------------------------------------------------------
        public void MessageValid()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.False);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(0));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MessageSelfReferencing()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("GenerateSerialization for .* member cannot be self referencing"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MessageInvalidSerializeFieldType()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(2));
            Assert.That(m_weaverErrors[0], Does.Match("please make sure to use a valid type"));
            Assert.That(m_weaverErrors[1], Does.Match("GenerateSerialization for .* member variables must be basic types"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MessageInvalidDeserializeFieldType()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(3));
            Assert.That(m_weaverErrors[0], Does.Match("please make sure to use a valid type"));
            Assert.That(m_weaverErrors[1], Does.Match("GetReadFunc unable to generate function"));
            Assert.That(m_weaverErrors[2], Does.Match("GenerateDeSerialization for .* member variables must be basic types"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MessageMemberGeneric()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("GenerateSerialization for .* member cannot have generic parameters"));
        }

        [Test] // -----------------------------------------------------------------------------------------
        public void MessageMemberInterface()
        {
            BuildAndWeaveTestAssembly(TestContext.CurrentContext.Test.Name);

            Assert.That(CompilationFinishedHook.WeaveFailed, Is.True);
            Assert.That(m_weaverErrors.Count, Is.EqualTo(1));
            Assert.That(m_weaverErrors[0], Does.Match("GenerateSerialization for .* member cannot be an interface"));
        }
        #endregion
    }
}
