using Evaluation.UnityInterface.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class IAssessmentValue : MonoBehaviour
{
    public bool ContinousUpdate = false;
    public abstract EnvironmentalChange GetEvalEvent();
}
