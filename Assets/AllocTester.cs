using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyClass
{
}

public class AllocTester : MonoBehaviour
{
    internal class TestClassA
    {
        public int a;
    };

    TestClassA _classA;

    private int _iters = 10000;
    byte[] buf;
    void Start()
    {
        //buf = new byte[256];
        //for (int i = 0; i < _iters; i++)
        //{
        //    //Mirror.NetworkReader r = new Mirror.NetworkReader(buf);
        //    EmptyClass ec = new EmptyClass();
        //}

        _classA = new TestClassA();
        _classA.a = 79;

        if (_classA == CheckClassA(_classA))
        {
            Debug.Log("Equal by return value");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    TestClassA CheckClassA(TestClassA cl)
    {
        if (cl == _classA) Debug.Log("Equal by param value");
        return cl;
    }
}
