using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BrickMakerBtn : MonoBehaviour
{
    public Transform selectFrame;
    GameObject myBrick;

    public void SetMyBrick(GameObject g)
    {
        myBrick = g;
    }

    public void GetMyBrick()
    {
        print($"get ma brick");
        
        if (SceneMakeUIManager.instance.AsyncCursorTracker(myBrick))
        {
            Select();
            SceneMakeUIManager.instance.SelectEvent += Unselect;
        }
    }

    public void Select()
    {
        selectFrame.gameObject.SetActive(true);
    }

    public void Unselect()
    {
        selectFrame.gameObject.SetActive(false);
    }
}
