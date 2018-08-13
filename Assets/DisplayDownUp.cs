using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayDownUp : MonoBehaviour {

    Animator anim;
    int upHash = Animator.StringToHash("Up");
    int downHash = Animator.StringToHash("Down");

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Show();
        else if (Input.GetKeyDown(KeyCode.M))
            Hide();
        
    }

    public void Show()
    {
        anim.SetTrigger(downHash);
    }

    public void Hide()
    {
        anim.SetTrigger(upHash);
    }
}
