using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TitleSceneManager : MonoBehaviour
{
    [Header("Components")]
    public Animator envAnim;
    
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            envAnim.SetTrigger("isEnter");
            LoadingManager.instance.LoadScene("HomeScene");
        }
    }
    
}
