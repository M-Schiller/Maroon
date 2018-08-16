using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskEntryManager : MonoBehaviour {

    [SerializeField]
    private GameObject TaskEntryPrefab;

    private bool Active;

    private bool Finished;
    
    private RectTransform Canvas;
    private RectTransform Content;
    private RectTransform Panel;

    private float visibleY;

    private void Awake()
    {
        if (TaskEntryPrefab == null)
            throw new Exception("Task Entry Prefab must be provided to ensure full functionallity of the Assignment Sheet");

        Canvas = transform as RectTransform;
        Content = Canvas.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
        Panel = Content.Find("Panel") as RectTransform;
    }

    // Use this for initialization
    void Start() {

        updateHeight();
        visibleY = transform.position.y;
        Hide();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void Hide()
    {
        //don't have a better solutiuon, so i will just push it through the roof
        transform.position.Set(transform.position.x, transform.position.y + 10000, transform.position.z);
    }
    private void Show()
    {
        //don't have a better solutiuon, so i will just push it through the roof
        transform.position.Set(transform.position.x, visibleY, transform.position.z);
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

    public void AddElement(string Text, bool Input, string Type = "String" )
    {
        var inst = Instantiate(TaskEntryPrefab);

        var text = inst.transform.Find("Text");
        text.GetComponent<Text>().text = Text;
        text.transform.parent = Panel;
        SetCorrectRotationAndScale(text.transform as RectTransform);

        if (Input)
        {
            if(Type == "Boolean")
            {
                var btn = inst.transform.Find("Button");
                btn.transform.parent = Panel;
                btn.transform.Find("Text").GetComponent<Text>().text = "Yes";
                SetCorrectRotationAndScale(btn.transform as RectTransform);

                var inst2 = Instantiate(TaskEntryPrefab);
                var btn2 = inst2.transform.Find("Button");
                btn2.transform.parent = Panel;
                btn2.transform.Find("Text").GetComponent<Text>().text = "No";
                SetCorrectRotationAndScale(btn2.transform as RectTransform);
            } else
            {
                var inp = inst.transform.Find("Input");
                inp.transform.parent = Panel;
                var btn = inst.transform.Find("Button");
                btn.transform.parent = Panel;
                SetCorrectRotationAndScale(inp.transform as RectTransform);
                SetCorrectRotationAndScale(btn.transform as RectTransform);
            }
        }

        Destroy(inst);

        updateHeight();
        Show();
    }

    private void SetCorrectRotationAndScale(RectTransform obj)
    {
        obj.localScale = new Vector3(1, 1, 1);
        obj.localRotation = Quaternion.identity;
    }
}
