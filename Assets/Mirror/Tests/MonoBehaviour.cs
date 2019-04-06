using System;
using System.Collections;
using UnityEngine;
using NSubstitute;
using System.Runtime.Serialization;
using NUnit.Framework;

using MonoBehaviour = Mirror.Buffers.Test.MockMonoBehaviour;

namespace Mirror.Buffers.Test
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
            //TestableNetworkBehaviour nb = TestableObjectFactory.Create<TestableNetworkBehaviour>();
            TestableNetworkBehaviour nb = new TestableNetworkBehaviour();
            int abc = 123;
        }
    }

    public class ExtensionOfNativeClass : Attribute { }

    [ExtensionOfNativeClass]
    public class MockMonoBehaviour : UnityEngine.Object
    {
        public bool enabled = true;
        public GameObject gameObject = null;
        public Transform transform = null;

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

        public Component[] GetComponentsInChildren<T>(bool includeInactive = false)
        {
            return new Component[0];
        }

        public Component GetComponentInChildren<T>(bool includeInactive = false)
        {
            return new Component();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return null;
        }
    }
}
