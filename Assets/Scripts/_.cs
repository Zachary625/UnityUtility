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
        if (GUI.Button(new Rect(0, 100, 200, 50), "Test Overhead"))
        {
            TestOverhead();
        }
    }

    void TestPredicate()
    {
        int i = 0;
        CodeLog.I.PredicateDelegate = () =>
        {
            return i % 2 == 0;
        };
        CodeLog.I.Options.LogDuration = true;
        for (i = 0; i < 10; i++)
        {
            CodeLog.I.Options.Name = "TestPredicate(" + i + ")";
            CodeLog.I.Log(() =>
            {
                int shit = i * i;
            });
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
        }, new CodeLog.LogOptions()
        {
            Name = "TestOverhead(without log)",
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
        }, new CodeLog.LogOptions()
        {
            Name = "TestOverhead(with log)",
            LogDuration = true,
        });

    }
}
