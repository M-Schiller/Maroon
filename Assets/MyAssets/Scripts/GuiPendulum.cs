using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Evaluation.UnityInterface;


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
                TEM.AddElement(fb.Text, true, fb.VariableType);
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



}
