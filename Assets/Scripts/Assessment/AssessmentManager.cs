using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssessmentManager : MonoBehaviour {

    public bool Persistent;
    public bool Negate;
    public bool OK;
    public int Length;


    private static AssessmentManager instance_ ;
    private List<IAssessmentValue> values_ = new List<IAssessmentValue>();
    private List<IAssessmentSuccessAction> actions_ = new List<IAssessmentSuccessAction>();
    
    public void Start()
    {
        instance_ = this;
    }

    public static AssessmentManager Instance()
    {
        return instance_;
    }

    public void RegisterValue(IAssessmentValue Value)
    {
        if (values_.Contains(Value))
            return;

        values_.Add(Value);
        Length = values_.Count;
    }
    
    public bool IsSuccessfull()
    {
        return values_.TrueForAll(x => x.OK);
    }


    public void Update()
    {
        if (OK && Persistent)
            return;

        OK = (IsSuccessfull() == !Negate);
        
        if (OK && !this.enabled)
            this.enabled = true;
        else if (!OK && this.enabled)
            this.enabled = false;
    }
}
