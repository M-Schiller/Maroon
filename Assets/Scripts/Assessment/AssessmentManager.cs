using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Evaluation;
using Evaluation.UnityInterface;
using Evaluation.UnityInterface.Events;

public class AssessmentManager : MonoBehaviour {
    
    
    private static AssessmentManager instance_;
    private EvaluationService evalService_;
    private List<IAssessmentValue> values_;

    public static AssessmentManager Instance {
        get {
            return instance_;
        }
    }
  
    public void Start()
    {
        values_ = new List<IAssessmentValue>();
        instance_ = this;

        Debug.Log("Connecting to WebService");
        evalService_ = new EvaluationService("http://localhost:51166/EvaluationService.asmx", "pendulum_maroon");
        Debug.Log("Got ID: " + evalService_.ContextID);
        IterationResult result = Send(new EnterSection("pendulum-workplace"));
        Debug.Log("Immediate Feedback count: " + result.ImmediateFeedackStrings.Length);
        foreach(String fb in result.ImmediateFeedackStrings )
        {
            Debug.Log("Feedback: " + fb);
        }

    }

    public void UpdateEnvironment()
    {
        foreach (IAssessmentValue val in values_)
            if (!val.ContinousUpdate)
            {
                Debug.Log("Sending " + val.name);
                Send(val.GetEvalEvent());
            }
    }

    public IterationResult Send(IEvalEvent Event)
    {
        return evalService_.Send(Event);
    }

    public void RegisterValue(IAssessmentValue Value)
    {
        Debug.Log("registering value: " + Value.gameObject.name);
        values_.Add(Value);
        IterationResult res = evalService_.Send(Value.GetEvalEvent(), true);
        if(res.ImmediateFeedackStrings.Length > 0)
        {
            Debug.Log("Got a result during registration:");
            Debug.Log(String.Join(", ", res.ImmediateFeedackStrings));
        }
    }

    public void PrintSummary()
    {
        Debug.Log(evalService_.GetSummary().ToString());
    }

    public void Update()
    {
        foreach (IAssessmentValue val in values_)
            if (val.ContinousUpdate)
                Send(val.GetEvalEvent());
    }
}

