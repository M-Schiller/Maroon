using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class AssessmentToleranceValue : IAssessmentValue {

    public double TargetValue;
    public double Tolerance ;
    public string ComponentName;
    public string PropertyName;

    private PropertyInfo propInfo;
    private Component component;

    //frequenzformel f = 1 / (2pi) * wurzel(g/l)
    // g = gravity, l = length of rope
    // Use this for initialization
    void Start () {

        if((component = GetComponent(ComponentName)) == null)
        {
            throw new Exception(String.Format("The component '{0}' was not found on the object '{1}'", ComponentName, name));
        }
        
        foreach (PropertyInfo attr in component.GetType().GetProperties())
        {
            if (attr.Name == PropertyName)
            {
                propInfo = attr;
                try
                {
                    Convert.ChangeType(propInfo.GetValue(component, null), typeof(double));
                } catch (Exception e)
                {
                    throw new Exception(String.Format("The type '{0}' is not arithmeticaly modifyable. Please use AssessmentFixedValue. Error: {1}", propInfo.GetType().Name, e.Message));
                }

                AssessmentManager.Instance().RegisterValue(this);
                return;
            }
        }
        
        throw new System.Exception(String.Format("Could not find property '{0}' in Component '{1}'", PropertyName, component.name));
	}
	
	// Update is called once per frame
	void Update () {
        if (OK && Persistent)
            return;

        double val = (double) Convert.ChangeType(propInfo.GetValue(component, null), typeof(double));
        OK = ((TargetValue - Tolerance <= val && val <= TargetValue + Tolerance) == !Negate);
    }

    
    private static bool HasSubtract<T>(T testObject)
    {
        var c = Expression.Constant(default(T), typeof(T));
        try
        {
            Expression.Subtract(c, c); // Throws an exception if + is not defined
            return true;
        } catch
        {
            return false;
        }
    }
}