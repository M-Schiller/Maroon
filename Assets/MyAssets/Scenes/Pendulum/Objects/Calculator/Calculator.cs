using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Calculator : MonoBehaviour {

    private static int visibleDigits = 15;

    private Dictionary<KeyCode, string> buttonNames = new Dictionary<KeyCode, string>();
    private double leftNumber;
    private string rightNumber;
    private string op;
    private bool dot = false;
    private bool error = false;

    private Transform display;

    private Dictionary<string, Func<string, bool>> actions = new Dictionary<string, Func<string, bool>>();

	// Use this for initialization
	void Start () {

        //numberButtons
        var start = (int)KeyCode.Keypad0;
        for (int i = start; i <= (int)KeyCode.Keypad9; i++)
        {
            buttonNames[(KeyCode)i] = (i - start).ToString();
            actions.Add((i - start).ToString(), calcNumber);
        }

        buttonNames[KeyCode.KeypadDivide] = "/";
        buttonNames[KeyCode.KeypadMultiply] = "*";
        buttonNames[KeyCode.KeypadPlus] = "+";
        buttonNames[KeyCode.KeypadMinus] = "-";
        actions["/"] =
        actions["*"] =
        actions["+"] =
        actions["-"] =
            (string x) => {
                reduce();
                leftNumber = parse(rightNumber);
                rightNumber = "";
                op = x;
                return true;
            };

        buttonNames[KeyCode.Comma] = ".";
        actions["."] = calcNumber;

        buttonNames[KeyCode.Backspace] = "clear";
        buttonNames[KeyCode.KeypadEnter] = "enter";
        buttonNames[KeyCode.Return] = "enter";
        actions["enter"] = ((string x) => { reduce(); return true; });
        actions["clear"] = ((string x) => { error = false; leftNumber = 0; rightNumber = ""; return true; });

        buttonNames[KeyCode.S] = "sin";
        buttonNames[KeyCode.C] = "cos";
        actions["sin"] = ((string x) => { rightNumber = Math.Sin(parse(rightNumber)).ToString();  return true; });
        actions["cos"] = ((string x) => { rightNumber = Math.Cos(parse(rightNumber)).ToString(); return true; });
            

        foreach (Transform child in GetComponentInChildren<Transform>())
            if (child.name.ToLower() == "display")
                display = child;
        
        if (display == null)
            throw new Exception("Calculator: Internal Error. Could not find an object named 'Display'");
	}

    private double parse(string s)
    {
        double res;
        if (!double.TryParse(s, out res))
            throw new Exception(string.Format("Calculator: internal error. Could not parse '{0}' to double.", s));
        return res;
    }

    private void reduce()
    {
        switch (op)
        {
            case "/":
                if (rightNumber == "0" || rightNumber == "")
                {
                    rightNumber = "";
                    leftNumber = 0;
                    error = true;
                    op = "";
                }
                else
                    rightNumber = (leftNumber / parse(rightNumber)).ToString();
                break;

            case "+":
                rightNumber = (leftNumber + parse(rightNumber)).ToString();
                break;

            case "-":
                rightNumber = (leftNumber - parse(rightNumber)).ToString();
                break;

            case "*":
                rightNumber = (leftNumber * parse(rightNumber)).ToString();
                break;                
        }

        op = "";
        dot = false;

    }

    private bool calcNumber(string x)
    {
        if (x == "." || x == ",")
        {
            if (dot)
                return false;
            else
                dot = true;

            rightNumber += ".";
            return true;
        }

        Debug.Log(rightNumber);
        rightNumber += x;
        Debug.Log(rightNumber);
        return true;
    }

    private void displayNumber()
    {
        if (error)
            display.GetComponent<TextMesh>().text = "Error: div/0";
        else
        {
            if (rightNumber == "")
                display.GetComponent<TextMesh>().text = "0.";
            else if (rightNumber.Contains("."))
            {
                display.GetComponent<TextMesh>().text = rightNumber.Substring(0, visibleDigits);
            } else
            {
                display.GetComponent<TextMesh>().text = rightNumber.Substring(Math.Max(0, rightNumber.Length - visibleDigits), Math.Min(rightNumber.Length, 15)) + ".";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        string buttonPressed;
        if (!( getButtonNamefromMouse(out buttonPressed)
            || getKeyboardButton(out buttonPressed)))
            return;

        actions[buttonPressed].Invoke(buttonPressed);

        displayNumber();
        }


    private bool getButtonNamefromMouse(out string name)
    {
        name = "";
        if (!Input.GetMouseButtonDown(0))
            return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100))
        {
            if (buttonNames.ContainsValue(hit.transform.name))
            {
                name = hit.transform.name;
                return true;
            }   
        }

        return false;
    }

    private bool getKeyboardButton(out string name)
    {
        name = "";
        foreach(KeyValuePair<KeyCode, string> button in buttonNames)
        {
            if (Input.GetKeyDown(button.Key))
            {
                name = button.Value;
                return true;
            }                
        }
        return false;
    }
}
