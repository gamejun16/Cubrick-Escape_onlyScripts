using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogMonitor : MonoBehaviour
{
    static public LogMonitor instance;

    [SerializeField] Text log;

    float logTimer = 0f;

    IEnumerator logCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        print("data path : " + Application.persistentDataPath);
    }

    public void AddLog(string s)
    {
        logTimer = 0f;
        log.text += "\n>> " + s;

        if (logCoroutine == null)
        {
            logCoroutine = LogDestroyTimer(s);
            StartCoroutine(logCoroutine);
        }
        else
            logTimer = 0f;
        
    }

    IEnumerator LogDestroyTimer(string s)
    {

        while (logTimer < 3f)
        {
            logTimer += Time.deltaTime;
            yield return null;
        }
        log.text = "";
        logCoroutine = null;
    }

}
