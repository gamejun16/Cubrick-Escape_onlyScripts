using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileDeviceSupporter : MonoBehaviour
{
    static public MobileDeviceSupporter instance;
    
    // UI 활성화시 해당 UI 탭을 닫는(Close) 함수를 저장
    // UI 비활성화시 함수 삭제
    // 담긴 함수가 없다면 Application.Quit() 호출
    public delegate void UICloser();

    UICloser uiCloser;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiCloser == null)
            {
                Application.Quit();
            }

            else
            {
                uiCloser();
            }
        }

#endif
    }
    
    public void AddToUICloser(UICloser func) => uiCloser += func;
    
    public void RemoveFromUICloser(UICloser func) => uiCloser -= func;

}
