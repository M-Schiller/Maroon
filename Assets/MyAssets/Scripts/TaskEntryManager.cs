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


    // Use this for initialization
    void Start() {

        if (TaskEntryPrefab == null)
            throw new Exception("Task Entry Prefab must be provided to ensure full functionallity of the Assignment Sheet");

        Canvas = transform as RectTransform;
        Content = Canvas.Find("Scroll View").Find("Viewport").Find("Content") as RectTransform;
        Panel = Content.Find("Panel") as RectTransform;

        updateHeight();
        updateHeight();
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    private void updateHeight()
    {

        //init the contentHeight or set at least 
        float contentHeight = 0;

        var vlg = Panel.GetComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        foreach (Text text in GetComponentsInChildren<Text>())
        {
            text.rectTransform.rect.Set(0, 0, Canvas.rect.width, Canvas.rect.height);
            contentHeight += text.preferredHeight + 5 ; // add some for the border of the Input field
        }


        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.childForceExpandWidth = true;

        contentHeight = (contentHeight > 0) ? contentHeight : Canvas.rect.height;

        Content.sizeDelta = new Vector2(Content.rect.width, contentHeight);
    }

    public void AddElement(string Text, bool Input)
    {
        var inst = Instantiate(TaskEntryPrefab);

        var text = inst.transform.Find("Text");
        text.GetComponent<Text>().text = Text;
        text.transform.parent = Panel;


        if (Input)
        {
            var inp = inst.transform.Find("Input");
            inp.transform.parent = Panel;
        }
        
        Destroy(inst);
    }
}
