﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Evaluation.UnityInterface.EWS;
using Evaluation.UnityInterface;

public class TaskEntryManager : MonoBehaviour {

    private GameObject TaskEntryPrefab_text;
    private GameObject TaskEntryPrefab_inp;
    private GameObject TaskEntryPrefab_btn;
    private GameObject TaskEntryPrefab_dd;

    private GameObject Trash;

    private bool Active;

    private bool Finished;
    
    private RectTransform Canvas;
    private RectTransform Content;
    private RectTransform MainPanel;
    private RectTransform RowPanel;

    private float visibleY;

    public List<ElementGroup> Elements = new List<ElementGroup>();

    private void Awake()
    {
        Canvas = transform as RectTransform;
        Content = Canvas.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
        MainPanel = Content.Find("Panel") as RectTransform;
        Trash = Content.Find("Trash").gameObject;

        TaskEntryPrefab_text = Content.Find("TaskEntryPrefab").Find("Text").gameObject;
        TaskEntryPrefab_inp = Content.Find("TaskEntryPrefab").Find("InputField").gameObject;
        TaskEntryPrefab_btn = Content.Find("TaskEntryPrefab").Find("Button").gameObject;
        TaskEntryPrefab_dd = Content.Find("TaskEntryPrefab").Find("Dropdown").gameObject;
        RowPanel = Content.Find("TaskEntryPrefab").Find("Panel") as RectTransform;


        if (TaskEntryPrefab_text == null || TaskEntryPrefab_inp == null || TaskEntryPrefab_btn == null || TaskEntryPrefab_dd == null)
            throw new Exception("Task Entry Prefab must be provided to ensure full functionallity of the Assignment Sheet");
    }

    // Use this for initialization
    void Start() {
        updateHeight();
        visibleY = transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
    private void updateHeight()
    {
        //init the contentHeight or set at least 
        float contentHeight = 0;

        var vlg = MainPanel.GetComponent<VerticalLayoutGroup>();
        MainPanel.GetComponent<ContentSizeFitter>().enabled = false;

        var check = new List<object> ();
        foreach (Text text in GetComponentsInChildren<Text>())
        {
            if (!check.Contains(text))
                if (   text.rectTransform.parent.name.StartsWith("InputField") 
                    || text.rectTransform.parent.name.StartsWith("Button"))
                {
                    Component inp = text.rectTransform.parent.GetComponent<InputField>();
                    if(inp == null)
                        inp = text.rectTransform.parent.GetComponent<Button>();

                    if (!check.Contains(inp))
                    {
                        contentHeight += (inp.transform as RectTransform).sizeDelta.y; 
                        check.Add(inp);
                    }
                }  else
                {
                    check.Add(text);
                    contentHeight += text.preferredHeight + 5; // add some for the border of the Input field
                    if (text.gameObject.name != "Label")
                        text.rectTransform.sizeDelta = new Vector2(text.rectTransform.rect.width, text.preferredHeight + 5);
                }
        }

        contentHeight += MainPanel.childCount * vlg.padding.top;//these are the paddings per children
        Content.sizeDelta = new Vector2(Content.rect.width, contentHeight);
        Content.offsetMax = new Vector2(0, Content.offsetMax.y);
    }



    public void AddElement(AddElementArguments args)
    {
        args.CheckConsistency();

        var group = new ElementGroup();
        var text = Instantiate(TaskEntryPrefab_text);
        text.transform.SetParent(MainPanel, false);
        group.VisibleText = text.GetComponent<Text>();
        group.VisibleText.text = args.Text;

        foreach (var row in args.Inputs)
        {
            var Panel = Instantiate(RowPanel);
            Panel.SetParent(MainPanel, false);

            foreach (var inp in row)
                if (inp is FeedbackButton)
                {
                    var btn = Instantiate(TaskEntryPrefab_btn);
                    btn.transform.SetParent(Panel, false);
                    group.Buttons.Add(new InpButton() {
                        GUIButton = btn.GetComponent<Button>(),
                        FBButton = inp as FeedbackButton
                    });
                    btn.transform.Find("Text").GetComponent<Text>().text = inp.Text;
                    btn.GetComponent<Button>().onClick.AddListener(delegate {
                        internalEventHandler(
                            inp,
                            group,
                            inp.DefaultValue,
                            args.SendHandler
                        );
                    });

                    Panel.rect.Set(
                        Panel.rect.x,
                        Panel.rect.y,
                        (Canvas.Find("Scroll View").transform as RectTransform).sizeDelta.x,
                        Math.Max(MainPanel.rect.height, (btn.transform as RectTransform).rect.height)
                    );

                    var rectTrans = (btn.transform as RectTransform);
                    Debug.Log((Canvas.Find("Scroll View").transform as RectTransform).sizeDelta);

                    rectTrans.sizeDelta = new Vector2(
                        (Canvas.Find("Scroll View").transform as RectTransform).sizeDelta.x / row.Count,
                        rectTrans.rect.height
                    );
                } else if (inp is FeedbackDropDown)
                {
                    var dd = Instantiate(TaskEntryPrefab_dd);
                    dd.transform.SetParent(Panel, false);

                    var dropd = dd.GetComponent<Dropdown>();
                    group.DropDowns.Add(new  InpDropdown() {
                        GUIDropdown = dropd,
                        FBDropdown = inp as FeedbackDropDown
                    });

                    dropd.ClearOptions();
                    dropd.AddOptions((inp as FeedbackDropDown).Values);
                    dropd.value = dropd.options.FindIndex((i) => i.text == inp.DefaultValue);
                    var rect = (dd.transform as RectTransform).rect;
                    (dd.transform as RectTransform).sizeDelta = new Vector2(
                        Math.Min(
                            Math.Max(
                                rect.width, dd.transform.Find("Label").GetComponent<Text>().preferredWidth
                            ),

                            (Canvas.Find("Scroll View").transform as RectTransform).sizeDelta.x / row.Count
                        ), 
                        rect.height
                    );
                } else
                {
                    var inpfield = Instantiate(TaskEntryPrefab_inp);
                    inpfield.transform.SetParent(Panel, false);
                    var input = inpfield.GetComponent<InputField>();
                    group.InputFields.Add(new InpInputField() {
                         GUIField = input,
                         FBField = inp as FeedbackInputField
                    });

                    input.text = inp.DefaultValue;
                    input.placeholder.GetComponent<Text>().text = inp.Text;

                    var rectTrans = inpfield.transform as RectTransform;

                    rectTrans.sizeDelta = new Vector2(

                        (Canvas.Find("Scroll View").transform as RectTransform).sizeDelta.x / row.Count,
                        rectTrans.rect.height
                    );
                }
        }
        
        Elements.Add(group);

        updateHeight();
    }

    public void Clear()
    {
        Trash.SetActive(false);

        foreach (Transform tr in MainPanel)
            tr.SetParent( Trash.transform);

        //some f*** buttons stays behind and no one knows why...
        foreach (Transform tr in MainPanel)
            tr.SetParent(Trash.transform);

        updateHeight();
    }

    private void SetCorrectRotationAndScale(RectTransform obj)
    {
        obj.localScale = new Vector3(1, 1, 1);
        obj.localRotation = Quaternion.identity;
        obj.localPosition.Set(obj.localPosition.x, obj.localPosition.y, 0);
        obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, 0);
    }

    private static void internalEventHandler(FeedbackInput sender, ElementGroup group, object value, Func<ButtonPressedEvent, bool> handler)
    {
        var ret = new ButtonPressedEvent()
        {
            Sender = sender,
            Value = value,
            ComponentGroup = group,
            SystemTime = DateTime.Now,
            UnityTime = new Time()
        };
        
        
        if (handler(ret))
            group.VisibleText.color = new Color(group.VisibleText.color.r, group.VisibleText.color.g, group.VisibleText.color.b, 0.5f);
}




    public class ButtonPressedEvent : EventArgs
    {
        public FeedbackInput Sender;
        public object Value;
        public ElementGroup ComponentGroup;
        public DateTime SystemTime;
        public Time UnityTime;
    }
    public class AddElementArguments
    {
        public string Text { get; set; }
        public List<List<FeedbackInput>> Inputs { get; set; } 
        public Func<ButtonPressedEvent, bool> SendHandler { get; set; }

        public void CheckConsistency()
        {
            Func<object, bool> isEmpty = (object var) => var == null || var.ToString() == "";

            if (isEmpty(Text))
                throw new ArgumentNullException("Text is mandatory");
            for(int r = 0; r < Inputs.Count; r++)
                for(int i = 0; i  < Inputs[r].Count; i++)
                { 
                    if (!isEmpty(Inputs[r][i].VariableName) && isEmpty(Inputs[r][i].VariableType))
                        throw new ArgumentNullException(
                            String.Format(
                                "If a VariableName is provided, VariableType is mandatory (Row {0}, element {1})",
                                r, 
                                i
                            )
                        );

                    if (!isEmpty(Inputs[r][i].VariableType) && isEmpty(SendHandler))
                        throw new ArgumentNullException(
                            String.Format(
                                "If a VariableName and VariableType is given, a SendHandler must be provided (Row {0}, element {1})",
                                r,
                                i
                            )
                        );
                }
        }

        public AddElementArguments()
        {
            Inputs = new List<List<FeedbackInput>>();
        }
    }

    public class ElementGroup
    {
        public Text VisibleText;
        public List<InpButton> Buttons = new List<InpButton>();
        public List<InpInputField> InputFields = new List<InpInputField>();
        public List<InpDropdown> DropDowns = new List<InpDropdown>();


    }

    public class InpButton
    {
        public Button GUIButton;
        public FeedbackButton FBButton;
    }

    public class InpInputField
    {
        public InputField GUIField;
        public FeedbackInputField FBField;
    }
    public class InpDropdown
    {
        public Dropdown GUIDropdown;
        public FeedbackDropDown FBDropdown;
    }
}
