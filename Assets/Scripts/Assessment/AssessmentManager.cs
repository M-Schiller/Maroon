using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AssessmentManager : MonoBehaviour {

    private static AssessmentManager instance_;

    public static AssessmentManager Instance {
        get {
            return instance_;
        }
    }
  
    public void Start()
    {
        instance_ = this;
    }


    public void RegisterValue(IAssessmentValue Value)
    {
        
    }



    public void Update()
    {

    }
}

