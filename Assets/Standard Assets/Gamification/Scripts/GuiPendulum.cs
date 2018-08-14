using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GuiPendulum : MonoBehaviour {

    public Text text_info;
    public float TimeTextIsVisible;

    private static GuiPendulum Instance = null;

    private DateTime lastSet;

    List<string> textToSet = new List<string>();

    void Start ()
    {
        Instance = this;
        lastSet = DateTime.Now;
        defaultText();

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("Laboratory");

        if(lastSet.AddSeconds(TimeTextIsVisible) < DateTime.Now)
        {
            if (textToSet.Count > 0)
                lock (textToSet)
                {
                    customText(textToSet[0]);
                    textToSet.RemoveAt(0);
                } else
                defaultText();

            lastSet = DateTime.Now;
        }
    }

    private void defaultText()
    {
        text_info.text = GamificationManager
            .instance.l_manager
            .GetString("Info Pendulum")
            .Replace("NEWLINE ", "\n");
    }

    private void customText(string text)
    {
        text_info.text = text;
    }

    public static void ShowText(string text)
    {
        if(text.Length > 0 && Instance != null)
            lock(Instance.textToSet)
            {
                Instance.textToSet.Add(text);
            }
    }


    public static void ShowText(string[] text)
    {
        ShowText(String.Join(", ", text).Trim());
    }



}
