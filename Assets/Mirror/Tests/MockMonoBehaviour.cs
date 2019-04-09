using System;
using System.Collections;
using UnityEngine;
using System.Runtime.Serialization;
using NUnit.Framework;
using NSubstitute;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor.Compilation;
using Mirror.Weaver;

//using MonoBehaviour = Mirror.Buffers.Test.MockMonoBehaviour;


namespace Mirror
{
    public static class TestableObjectFactory
    {
        public static T Create<T>()
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }
    }

    public class TestableNetworkBehaviour : NetworkBehaviour
    {
    }

    public class NetworkBehaviourFixture
    {
        [Test]
        public void DURPADURP()
        {
            WeaverMocker.DoStuff();
        }
    }

    public class ExtensionOfNativeClass : Attribute { }

    [ExtensionOfNativeClass]
    public class MockMonoBehaviour : UnityEngine.Object
    {
        public bool enabled = true;
        public GameObject gameObject = null;
        public Transform transform = null;

        public T AddComponent<T>() where T : class
        {
            return Substitute.For<T>();
        }

        public T GetComponent<T>() where T : class
        {
            return Substitute.For<T>();
        }

        public void Invoke(string methodName, float time)
        {
            return;
        }

        public void InvokeRepeating(string methodName, float time, float repeatRate)
        {
            return;
        }

        public Component[] GetComponentsInChildren(Type t, bool includeInactive = false)
        {
            return new Component[0];
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : class
        {
            return new T[] { Substitute.For<T>() };
        }

        public T GetComponentInChildren<T>(bool includeInactive = false) where T : class
        {
            return Substitute.For<T>();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return null;
        }
    }
}
