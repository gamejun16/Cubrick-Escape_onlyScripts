using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StageMakerNotice : MonoBehaviour
{
    static public StageMakerNotice instance;

    public Transform noticeSpawnPoint;
    public GameObject noticeDescPref;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }


    
    public void AsyncNoticer(string desc, Color textColor)
    {
        StartCoroutine(CoNoticer(desc, textColor));
    }

    IEnumerator CoNoticer(string desc, Color textColor)
    {
        GameObject g = Instantiate(noticeDescPref, noticeSpawnPoint);        

        Text text = g.GetComponentInChildren<Text>();
        text.text = desc;
        text.color = textColor;

        float timer = 0f;
        
        while (timer < .5f)
        {
            timer += Time.deltaTime;
            g.transform.Translate(Vector3.up * Time.deltaTime * 0.1f);

            yield return null;
        }

        timer = 0f;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;

            g.transform.Translate(Vector3.up * Time.deltaTime * 0.1f);

            Color dColor = text.color;
            dColor.a = 1 - timer * 2;
            text.color = dColor;

            yield return null;
        }
        
        Destroy(g);
    }
}
