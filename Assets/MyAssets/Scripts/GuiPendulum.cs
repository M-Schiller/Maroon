using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Evaluation.UnityInterface;
using Evaluation.UnityInterface.EWS;

public class GuiPendulum : MonoBehaviour {

    public float MinimumTimeTextIsVisible = 1f;
    public float MaximumTimeTextIsVisible = 5f;

    [SerializeField]
    private Text InfoText = null;

    [SerializeField]
    private GameObject AssignmentSheet = null;
    
    private static GuiPendulum Instance = null;

    private DateTime lastSet;

    List<string> textToSet = new List<string>();

    private void Awake()
    {
        Instance = this;
        lastSet = DateTime.Now;

        if (AssignmentSheet == null || AssignmentSheet.GetComponent<TaskEntryManager>() == null)
            throw new InvalidProgramException("The given Assignment Sheet is not valid. It must contain a TaskEntryManager component.");
    }

    void Start ()
    {
        defaultText();
        lastSet = DateTime.Now;
    }
    private void Update()
    {
        if (textToSet.Count > 0)
        {
            if (lastSet.AddSeconds(MinimumTimeTextIsVisible) < DateTime.Now)
                lock (textToSet)
                {
                    customText(textToSet[0]);
                    textToSet.RemoveAt(0);
                    lastSet = DateTime.Now;
                }
        } else
            if (lastSet.AddSeconds(MaximumTimeTextIsVisible) < DateTime.Now)
            defaultText();
    }
    private void defaultText()
    {
        InfoText.text = GamificationManager
            .instance.l_manager
            .GetString("Info Pendulum")
            .Replace("NEWLINE ", "\n");
    }

    private void customText(string text)
    {
        InfoText.text = text;
    }

    public static void ShowFeedback(FeedbackEntry[] feedback)
    {
        if (feedback.Length == 0)
            return;

        var TEM = Instance.AssignmentSheet.GetComponent<TaskEntryManager>();

        foreach (var fb in feedback)
        {
            if(fb.IsQuestion)
            {
                var args = new TaskEntryManager.AddElementArguments() {
                    VariableName = fb.VariableName,
                    VariableType = fb.VariableType,
                    Text = fb.Text,
                    SendHandler = ButtonSendPressed,
                };
                TEM.AddElement(args);
            } else
            {
                ShowText(fb.Text);
            }
        }
    }

    public static void ShowText(string text)
    {
        if (text.Length > 0 && Instance != null)
            lock (Instance.textToSet)
            {
                Instance.textToSet.Add(text);
            }
    }
    

    public static void ShowText(string[] text)
    {
        ShowText(String.Join(", ", text).Trim());
    }

    private static void ButtonBoolPressed(string VariableName, bool value)
    {
        AssessmentManager.Instance.Send(
            GameEventBuilder.EnvironmentVariable("assessment", VariableName, value)
        );
    }
    
    public static void Clear()
    {
        Instance.defaultText();
        Instance.AssignmentSheet.GetComponent<TaskEntryManager>().Clear();
        
    }

    private static bool ButtonSendPressed(TaskEntryManager.ButtonPressedEvent evt)
    {

        switch (evt.VariableType)
        {
            case DataType.Float:
                double dbl;
                if(!double.TryParse(evt.Value.ToString(), out dbl))
                {
                    Debug.Log(string.Format("Could not parse {0} to double, falling back to string", evt.Value.ToString()));
                    Send(evt.VariableName, evt.Value.ToString());
                } else
                    Send(evt.VariableName, dbl);

                break;

            case DataType.Integer:
                int i;
                if (!int.TryParse(evt.Value.ToString(), out i))
                {
                    Debug.Log(string.Format("Could not parse {0} to integer, falling back to string", evt.Value.ToString()));
                    Send(evt.VariableName, evt.Value.ToString());
                } else
                    Send(evt.VariableName, i);

                break;

            case DataType.Boolean:
                Send(evt.VariableName, (bool)evt.Value);
                break;

            default:
                Send(evt.VariableName, evt.Value.ToString());
                break;

        }

        return true;

    }


    private static void Send<T>(string name, T value)
    {
        AssessmentManager.Instance.Send(
            GameEventBuilder.EnvironmentVariable("assessment", name, value)
        );
    }

}
