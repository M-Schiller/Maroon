using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Evaluation.UnityInterface.EWS;
public class TaskEntryManager : MonoBehaviour {

    private GameObject TaskEntryPrefab_text;
    private GameObject TaskEntryPrefab_inp;
    private GameObject TaskEntryPrefab_btn;

    private GameObject Trash;

    private bool Active;

    private bool Finished;
    
    private RectTransform Canvas;
    private RectTransform Content;
    private RectTransform Panel;

    private float visibleY;

    public List<ElementGroup> Elements = new List<ElementGroup>();

    private void Awake()
    {
        Canvas = transform as RectTransform;
        Content = Canvas.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
        Panel = Content.Find("Panel") as RectTransform;
        Trash = Content.Find("Trash").gameObject;

        TaskEntryPrefab_text = Content.Find("TaskEntryPrefab").Find("Text").gameObject;
        TaskEntryPrefab_inp = Content.Find("TaskEntryPrefab").Find("InputField").gameObject;
        TaskEntryPrefab_btn = Content.Find("TaskEntryPrefab").Find("Button").gameObject;


        if (TaskEntryPrefab_text == null || TaskEntryPrefab_inp == null || TaskEntryPrefab_btn == null )
            throw new Exception("Task Entry Prefab must be provided to ensure full functionallity of the Assignment Sheet");
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
        Content.offsetMax = new Vector2(0, Content.offsetMax.y);
    }



    public void AddElement(AddElementArguments args)
    {
        args.CheckConsistency();

        var group = new ElementGroup();
        var text = Instantiate(TaskEntryPrefab_text);
        text.transform.SetParent(Panel);
        group.VisibleText = text.GetComponent<Text>();
        group.VisibleText.text = args.Text;
        SetCorrectRotationAndScale(text.transform as RectTransform);

        if (args.VariableName != null)
        {
            if(args.VariableType == DataType.Boolean)
            {
                var btn = Instantiate(TaskEntryPrefab_btn);
                var btn2 = Instantiate(TaskEntryPrefab_btn);
                btn.transform.SetParent(Panel);
                btn2.transform.SetParent(Panel);

                group.Button1 = btn.GetComponent<Button>();
                group.Button2 = btn2.GetComponent<Button>();

                btn.transform.Find("Text").GetComponent<Text>().text = "Yes";
                SetCorrectRotationAndScale(btn.transform as RectTransform);
                btn.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        group,
                        true,
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler
                    );
                });

                btn2.transform.Find("Text").GetComponent<Text>().text = "No";
                SetCorrectRotationAndScale(btn2.transform as RectTransform);
                btn2.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        group,
                        false,
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler
                    );
                });
            } else
            {
                var inp = Instantiate(TaskEntryPrefab_inp);
                var btn = Instantiate(TaskEntryPrefab_btn);
                inp.transform.SetParent(Panel);
                btn.transform.SetParent(Panel);
                SetCorrectRotationAndScale(inp.transform as RectTransform);
                SetCorrectRotationAndScale(btn.transform as RectTransform);

                group.Button1 = btn.GetComponent<Button>();
                group.InputField = inp.GetComponent<InputField>();

                btn.GetComponent<Button>().onClick.AddListener(delegate {
                    internalEventHandler(
                        group,
                        false,
                        args.VariableName,
                        args.VariableType,
                        args.SendHandler 
                    );
                });
            }
        }

        Elements.Add(group);

        updateHeight();
        Show();
    }

    public void Clear()
    {
        Trash.SetActive(false);

        foreach (Transform tr in Panel)
            tr.SetParent( Trash.transform);

        //some f*** buttons stays behind and no one knows why...
        foreach (Transform tr in Panel)
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

    private static void internalEventHandler(ElementGroup group, bool yes, string varName, DataType varType, Func<ButtonPressedEvent, bool> handler)
    {
        var ret = new ButtonPressedEvent()
        {
           Value = (group.InputField != null) ? group.InputField.text : null,
           ComponentGroup = group,
           VariableName = varName,
           VariableType = varType,
           SystemTime = DateTime.Now,
           UnityTime = new Time()
        };
        

        if (ret.Value == null)
            ret.Value = yes;


        if (handler(ret))
        {
            group.VisibleText.color = new Color(group.VisibleText.color.r, group.VisibleText.color.g, group.VisibleText.color.b, 0.5f);

        }
}




    public class ButtonPressedEvent : EventArgs
    {
        public object Value;
        public ElementGroup ComponentGroup;
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
