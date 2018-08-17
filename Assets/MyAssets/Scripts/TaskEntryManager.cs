using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Evaluation.UnityInterface.EWS;
public class TaskEntryManager : MonoBehaviour {

    private GameObject TaskEntryPrefab;

    private bool Active;

    private bool Finished;
    
    private RectTransform Canvas;
    private RectTransform Content;
    private RectTransform Panel;

    private float visibleY;

    private List<ElementGroup> elements = new List<ElementGroup>();

    private void Awake()
    {
        if (TaskEntryPrefab == null)
            throw new Exception("Task Entry Prefab must be provided to ensure full functionallity of the Assignment Sheet");

        Canvas = transform as RectTransform;
        Content = Canvas.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
        Panel = Content.Find("Panel") as RectTransform;
        TaskEntryPrefab = Content.Find("TaskEntryPrefab").gameObject;
    }

    // Use this for initialization
    void Start() {

        updateHeight();
        visibleY = transform.localPosition.y;
        Hide();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Hide()
    {
        //don't have a better solutiuon, so i will just push it through the roof
        transform.localPosition.Set(transform.localPosition.x, transform.localPosition.y + 10000, transform.localPosition.z);
    }
    private void Show()
    {
        //don't have a better solutiuon, so i will just push it through the roof
        transform.localPosition.Set(transform.localPosition.x, visibleY, transform.localPosition.z);
    }
    private void updateHeight()
    {
        //init the contentHeight or set at least 
        float contentHeight = 0;

        var vlg = Panel.GetComponent<VerticalLayoutGroup>();
        Panel.GetComponent<ContentSizeFitter>().enabled = false;

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
                    text.rectTransform.sizeDelta = new Vector2(text.rectTransform.rect.width, text.preferredHeight + 5);
                }
        }

        contentHeight += Panel.childCount * vlg.padding.top;//these are the paddings per children

        Content.sizeDelta = new Vector2(Content.rect.width, contentHeight);
    }



    public void AddElement(AddElementArguments args)
    {
        args.CheckConsistency();

        var inst = Instantiate(TaskEntryPrefab);

        var text = inst.transform.Find("Text");
        text.GetComponent<Text>().text = args.Text;
        text.transform.parent = Panel;
        SetCorrectRotationAndScale(text.transform as RectTransform);
        

        if (args.VariableName != null)
        {
            if(args.VariableType == DataType.Boolean)
            {
                var btn = inst.transform.Find("Button");
                btn.transform.parent = Panel;
                var inst2 = Instantiate(TaskEntryPrefab);
                var btn2 = inst2.transform.Find("Button");
                btn2.transform.parent = Panel;

                btn.transform.Find("Text").GetComponent<Text>().text = "Yes";
                SetCorrectRotationAndScale(btn.transform as RectTransform);
                btn.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        btn.GetComponent<Button>(),
                        null,
                        true,
                        text.GetComponent<Text>(),
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler
                    );
                });

                btn2.transform.Find("Text").GetComponent<Text>().text = "No";
                SetCorrectRotationAndScale(btn2.transform as RectTransform);
                btn2.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        btn.GetComponent<Button>(),
                        null,
                        false,
                        text.GetComponent<Text>(),
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler
                    );
                });
            } else
            {
                var inp = inst.transform.Find("Input");
                inp.transform.parent = Panel;
                var btn = inst.transform.Find("Button");
                btn.transform.parent = Panel;
                SetCorrectRotationAndScale(inp.transform as RectTransform);
                SetCorrectRotationAndScale(btn.transform as RectTransform);
                btn.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        btn.GetComponent<Button>(),
                        inp.GetComponent<InputField>(),
                        true,
                        text.GetComponent<Text>(),
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler 
                    );
                });
            }
        }

        Destroy(inst);

        updateHeight();
        Show();
    }

    public void Clear()
    {
        foreach (Transform tr in Panel)
        {
            tr.parent = null;
            Destroy(tr.gameObject);
        }

        updateHeight();
    }

    private void SetCorrectRotationAndScale(RectTransform obj)
    {
        obj.localScale = new Vector3(1, 1, 1);
        obj.localRotation = Quaternion.identity;
    }

    private static void internalEventHandler(Button btn, InputField txt, bool yes, Text initText, string varName, DataType varType, Func<ButtonPressedEvent, bool> handler)
    {
        var ret = new ButtonPressedEvent()
        {
           Value = (txt != null) ? txt.text : null,
           Sender = btn,
           Textbar = txt,
           InitialText = initText,
           VariableName = varName,
           VariableType = varType,
           SystemTime = DateTime.Now,
           UnityTime = new Time()
        };
        

        if (ret.Value == null)
            ret.Value = yes;


        if (handler(ret))
        {
            initText.color = new Color(initText.color.r, initText.color.g, initText.color.b, 0.5f);

        }
}




    public class ButtonPressedEvent : EventArgs
    {
        public object Value;
        public Button Sender;
        public InputField Textbar;
        public Component[] ComponentGroup;
        public Text InitialText;
        public string VariableName;
        public DataType VariableType;
        public DateTime SystemTime;
        public Time UnityTime;
    }
    public class AddElementArguments
    {
        public string Text { get; set; }
        public string VariableName { get; set; }
        public DataType VariableType { get; set; }
        public Func<ButtonPressedEvent, bool> SendHandler { get; set; }

        public void CheckConsistency()
        {
            Func<object, bool> isEmpty = (object var) => var == null || var.ToString() == "";

            if (isEmpty(Text))
                throw new ArgumentNullException("Text is mandatory");

            if (!isEmpty(VariableName) && isEmpty(VariableType))
                throw new ArgumentNullException("If a VariableName is provided, VariableType is mandatory");

            if (!isEmpty(VariableType) && isEmpty(SendHandler))
                throw new ArgumentNullException("If a VariableName and VariableType is given, a SendHandler must be provided");

        }
    }

    public class ElementGroup
    {
        public Text VisibleText;
        public InputField InputField;
        public Button Button1;
        public Button Button2;
    }
}
