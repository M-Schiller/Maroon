using System;
using System.Linq;
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

    private TaskEntryManager TEM = null;

    private static GuiPendulum Instance = null;

    private DateTime lastSet;

    private List<Evaluation.UnityInterface.Feedback> textToSet = new List<Evaluation.UnityInterface.Feedback>();

    private void Awake()
    {
        Instance = this;
        TEM = AssignmentSheet.GetComponent<TaskEntryManager>();

        lastSet = DateTime.Now;

        if (AssignmentSheet == null || TEM == null)
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
                    Color clr = new Color(1,1,1);
                    switch(textToSet[0].Item.Code) {
                        case ColorCode.Success:
                            clr = new Color(0, 1, 0);
                            break;
                        case ColorCode.Mistake:
                            clr = new Color(1, 0, 0);
                            break;
                    }

                    customText(textToSet[0].Item.Text, clr);
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
        InfoText.color = new Color(1,1,1);
    }

    private void customText(string text, Color color )
    {
        InfoText.text = text;
        InfoText.color = color;
    }

    public static void ShowFeedback(FeedbackEntry[] feedback)
    {
        if (feedback.Length == 0)
            return;

       

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
                Instance.TEM.AddElement(args);
            } else
            {
                ShowText(fb);
            }
        }
    }



    public static void ShowText(Evaluation.UnityInterface.Feedback feedback)
    {
        if (feedback.Item.Text.Length > 0 && Instance != null)
            lock (Instance.textToSet)
            {
                Instance.textToSet.Add(feedback);
            }
    }


    public static void ShowText(FeedbackEntry feedbackEntry)
    {
        var fb = new Evaluation.UnityInterface.Feedback();
        fb.Item = new TextFeedbackContent();
        fb.Item.Text = feedbackEntry.Text;
        fb.Item.Code = feedbackEntry.ColorCode;
        ShowText(fb);
    }

    public static void ShowText(string[] text)
    {
        var fb = new Evaluation.UnityInterface.Feedback();
        fb.Item = new TextFeedbackContent();
        fb.Item.Text = String.Join(", ", text).Trim();
        fb.Item.Code = ColorCode.Hint;
        ShowText(fb);
    }

    private static void ButtonBoolPressed(string VariableName, bool value)
    {
        AssessmentManager.Instance.Send(
            GameEventBuilder.EnvironmentVariable("assessment", VariableName, value)
        );
    }

    public static bool isFocused()
    {
        return Instance.TEM.Elements.Where(e => e.InputField &&  e.InputField.isFocused).FirstOrDefault() != null;
    }
    
    public static void Clear()
    {
        Instance.defaultText();
        Instance.AssignmentSheet.GetComponent<TaskEntryManager>().Clear();
    }

    private static bool ButtonSendPressed(TaskEntryManager.ButtonPressedEvent evt)
    {
        object value = null;

        switch (evt.VariableType)
        {
            case DataType.Float:
                double dbl;
                if (!double.TryParse(evt.Value.ToString(), out dbl))
                {
                    Debug.Log(string.Format("Could not parse {0} to double, falling back to string", evt.Value.ToString()));
                    value = evt.Value.ToString();
                } else
                    value = dbl;
                break;

            case DataType.Integer:
                int i;
                if (!int.TryParse(evt.Value.ToString(), out i))
                {
                    Debug.Log(string.Format("Could not parse {0} to integer, falling back to string", evt.Value.ToString()));
                    value = evt.Value.ToString();
                } else
                    value = i;

                break;

            case DataType.Boolean:
                value = (bool)evt.Value;
                break;

            default:
                value = evt.Value.ToString();
                break;

        }

        Send(evt.VariableName, value);
        return true;

    }


    private static void Send<T>(string name, T value)
    {
        var fb = AssessmentManager.Instance.Send(
            GameEventBuilder.AnswerQuestion(name, value)
        ).Feedback;

        ShowFeedback(fb);

        if (fb.Where(f => !f.IsQuestion && f.Text == "Congratulations! You finished the test!").FirstOrDefault() != null)
            PendulumManager.ExitExperiment();
        
            
    }

}
