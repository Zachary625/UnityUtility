using UnityEngine;
using System.Collections;

using com.zachary625.unity_utility;

public class _ : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 50), "Test Predicate"))
        {
            TestPredicate();
        }
        if (GUI.Button(new Rect(0, 50, 200, 50), "Test Overhead"))
        {
            TestOverhead();
        }
        if (GUI.Button(new Rect(0, 100, 200, 50), "Test StackTrace"))
        {
            TestStackTrace();
        }
    }

    void TestPredicate()
    {
        int i = 0;
        CodeLogOptions options = new CodeLogOptions()
        {
            LogDuration = true,
            PredicateDelegate = () =>
            {
                return i % 2 == 0;
            },

        };
        for (i = 0; i < 1000; i++)
        {
            options.Identifier = "TestPredicate(" + i + ")";
            CodeLog.I.Log(() =>
            {
                i+=2;
            }, options);
        }
    }

    void TestOverhead()
    {
        int loops = 1000000;
        CodeLog.I.Log(() =>
        {
            for (int i = 0; i < loops; i++)
            {
                int shit = i * i;
            }
        }, new CodeLogOptions()
        {
            Identifier = "TestOverhead(without log)",
            LogDuration = true,
        });

        CodeLog.I.Log(() =>
        {
            for (int i = 0; i < loops; i++)
            {
                CodeLog.I.Log(() =>
                {
                    int shit = i * i;
                });
            }
        }, new CodeLogOptions()
        {
            Identifier = "TestOverhead(with log)",
            LogDuration = true,
        });

    }

    void TestStackTrace()
    {
        CodeLog.I.Log(()=> {
            int shit = 0;
        }, new CodeLogOptions()
        {
            Identifier = "TestStackTrance",
            LogStack = true,
        });
    }
}
