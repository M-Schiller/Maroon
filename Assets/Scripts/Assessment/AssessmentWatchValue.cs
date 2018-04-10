using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class AssessmentWatchValue : MonoBehaviour {

    [Header("Attributes")]
    public string Attributes;

    private List<Property> properties = new List<Property>();

    // Use this for initialization
	void Start () {
        
        foreach(string line in Attributes.Split('\n'))
        {
            if (line.Trim() == "")
                continue;

            Property prop = new Property(line);
            string[] parts = line.Split('.');
            string ComponentName = parts[0];
            if ((prop.RootComponent = GetComponent(ComponentName)) == null)
                throw new Exception(String.Format("The component '{0}' was not found on the object '{1}'", ComponentName, name));
            
            parts = parts.Skip(1).ToArray();
            foreach (string part in parts)
            {
                Debug.Log("Type " + prop.GetType());
                var ppi = GetMember(part, prop.GetType());

                if (ppi == null)
                    throw new Exception(String.Format("The attribute '{0}.{1}' was not found", prop.ToString(), part));

                prop.ParentLine.Add(ppi);
            }

            if(! prop.IsOK()) 
                throw new Exception(String.Format("At least one property needs to be specified for {0}, line: {1}", prop.RootComponent.name, line));
            else
                properties.Add(prop);
        }

    }
	
    private ParentProp GetMember(string name, Type ParentType)
    {
        ParentProp ret = null;
        foreach (PropertyInfo pi in ParentType.GetProperties())
        {
            Debug.Log(String.Format("{0}, {1}", name, pi.Name.ToLower()));
            if (pi.Name.ToLower() == name.ToLower())
            {
                ret = new ParentProp(pi);
                break;
            }
        }


        if ( ret == null)
            foreach (FieldInfo fi in ParentType.GetFields())
            {
                Debug.Log(String.Format("{0}, {1}", name, fi.Name.ToLower()));
                if (fi.Name.ToLower() == name.ToLower())
                {
                    ret = new ParentProp(fi);
                    break;
                }
            }

        return ret;
    }

	// Update is called once per frame
	void Update () {

        foreach(Property prop in properties)
        {
            Debug.Log(String.Format("{0}: {1}", prop.FullName, prop.GetValue()));
        }
		
	}

    public class Property
    {
        public string FullName;
        public Component RootComponent;
        public List<ParentProp> ParentLine;

        public Property(string name)
        {
            FullName = name;
            ParentLine = new List<ParentProp>();
        }

        public Property(Property other)
        {
            FullName = other.FullName;
            ParentLine = new List<ParentProp>(other.ParentLine);
        }

        public object GetValue()
        {
            if (! IsOK())
                return null;
            else
                return GetValue(ParentLine.Count - 1);
        }

        private object GetValue(int i)
        {
            if (i == 0)
                return ParentLine[0].GetValue(RootComponent, null);
            else
                return ParentLine[i].GetValue(ParentLine[i - 1], null);
        }
        
        public new Type GetType()
        {
            if (ParentLine.Count > 0)
                return ParentLine[ParentLine.Count - 1].GetType();
            else if (RootComponent)
                return RootComponent.GetType();
            else
                return null;
        }

        public bool IsOK()
        {
            return (ParentLine.Count > 0);
        }
    }

    public class ParentProp
    {
        private MemberInfo info;
        private Type type;
        public ParentProp(PropertyInfo pi)
        {
            info = (MemberInfo)pi;
            type = pi.PropertyType;
        }
        public ParentProp(FieldInfo fi)
        {
            info = (MemberInfo)fi;
            type = fi.FieldType;
        }

        public new Type GetType()
        {
            return type;
        }
        

        public object GetValue(object obj, object[] index)
        {
                   
            if (type == Type.GetType("PropertyInfo"))
                return ((PropertyInfo)info).GetValue(obj, index);
            else
                return ((FieldInfo)info).GetValue(obj);
        }

    }
}


