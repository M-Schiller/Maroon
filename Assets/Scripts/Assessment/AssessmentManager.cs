using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AssessmentManager : MonoBehaviour {

    public bool Persistent;
    public bool Negate;
    public bool OK;
    public SuccessActionConfig SuccessAction;

    [Header("Success Action")]
    public string TargetObjectName ;
    public string StartActionName = "defaultSuccessStart";
    public string EndActionName = "defaultSuccessEnd";
    public string UpdateActionName = "defaultSuccessUpdate";

    private bool oldOK;
    private GameObject workingObject;
    private GameObject fctReceiverObject;

    private static AssessmentManager instance_ ;
    private List<IAssessmentValue> values_ = new List<IAssessmentValue>();
    private List<IAssessmentSuccessAction> actions_ = new List<IAssessmentSuccessAction>();
    
    public void Start()
    {
        instance_ = this;

        if (TargetObjectName != "")
        {
            workingObject = GameObject.Find(TargetObjectName);
            if (!workingObject)
                throw new System.Exception(string.Format("No GameObject with name '{0}' found", TargetObjectName));

        } else
        {
            workingObject = this.gameObject;
        }


        fctReceiverObject = workingObject;
        MethodInfo temp = workingObject.GetType().GetMethod(StartActionName);
        if (temp == null)
            fctReceiverObject = this.gameObject;
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


        if (OK && !oldOK)
            fctReceiverObject.SendMessage(StartActionName);
        else if (OK && oldOK)
            fctReceiverObject.SendMessage(UpdateActionName);
        else if (!OK && oldOK)
            fctReceiverObject.SendMessage(EndActionName);
        
        oldOK = OK;
    }
    

    public void defaultSuccessStart()
    {
       
            workingObject.GetComponent<MeshRenderer>().enabled = true;

        
    }

    public void defaultSuccessEnd()
    {
        workingObject.GetComponent<MeshRenderer>().enabled = false;
    }

    public void defaultSuccessUpdate()
    {
        //does nothing by default
    }





}


public class SuccessActionConfig
{
    public string TargetObject;
    public string SuccessStartAction;
    public string SuccessEndAction;
    public string SuccessUpdateAction;

    public SuccessActionConfig(string ObjectName)
    {

    }
}