using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SceneMakeUIManager : MonoBehaviour
{
    static public SceneMakeUIManager instance;

    [Header("UI")]
    public Transform brickListContents;
    public int totalBrickCount;
    public List<Text> brickIconCountText;
    public List<int> brickIconLimit;
    IEnumerator cursorTrackCoroutine;

    [Header("Element")]
    public Transform bricksRoot;

    public int curSelectBrickId; // 현재 선택된 brick의 id

    public delegate void Select();
    public event Select SelectEvent;

    private void Start()
    {
        instance = this;

        InitBrickBtns();
    }

    private void Update()
    {
        if (cursorTrackCoroutine != null)
            return;

        RaycastHit hit;
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if(Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue, LayerMask.NameToLayer("BrickSensor")))
            if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue))
            {
                if (hit.collider.CompareTag("Brick"))
                {
                    // 해당 Brick 파괴
                    RemoveBrick(hit.collider.gameObject);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    private void InitBrickBtns()
    {
        for(int i=0;i<totalBrickCount; i++)
        {
            Transform tr = brickListContents.GetChild(i);

            // set icon img
            tr.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Sprites/BricksImg/" + string.Format("{0:D3}", i));

            // set icon limit count
            // 하나만 존재해야 하는 Brick은 해당 정보를 Text로 표시
            // ( 현재 설치된 수 ) / ( 최대 설치 가능 수 )
            brickIconCountText.Add(tr.GetChild(3).GetChild(0).GetComponent<Text>());
            Text limitText = tr.GetChild(3).GetChild(2).GetComponent<Text>();
            if (i == 0 || i == 1 || i == 9)
            {
                limitText.text = "1";
                brickIconLimit.Add(1);
            }
            else
            {
                limitText.text = "inf";
                brickIconLimit.Add(-1);
            }
            
            // set btn property
            BrickMakerBtn bmb = tr.GetComponent<BrickMakerBtn>();
            string brickName = "[" + string.Format("{0:D3}", i) + "]Brick";
            GameObject res = Resources.Load<GameObject>("Prefabs/Bricks/" + brickName);
            bmb.SetMyBrick(res);
        }
    }

    public bool AsyncCursorTracker(GameObject targetBrickObj)
    {
        if (cursorTrackCoroutine == null)
        {
            cursorTrackCoroutine = CursorTracker(targetBrickObj);
            StartCoroutine(cursorTrackCoroutine);

            return true;
        }

        return false;
    }

    IEnumerator CursorTracker(GameObject targetBrickObj)
    {
        RaycastHit hit;
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //if(Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue, LayerMask.NameToLayer("BrickSensor")))
                if (Physics.Raycast(ray.origin, ray.direction, out hit, float.MaxValue))
                {
                    if(hit.collider.CompareTag("BrickSensor")){

                        // 해당 센서 위치에 오브젝트 생성
                        GameObject obj = Instantiate(targetBrickObj);
                        obj.transform.position = hit.collider.transform.position;
                        obj.transform.eulerAngles = Vector3.zero;

                        obj.transform.parent = bricksRoot;

                        if (!AddBrick(obj))
                            Destroy(obj);
                    }
                }
                else
                {
                    cursorTrackCoroutine = null;
                    SelectEvent();
                    yield break;
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                cursorTrackCoroutine = null;
                SelectEvent();

                yield break;
            }

            yield return null;
        }
    }



    /// <summary>
    /// Brick을 배치할 때 호출
    /// </summary>
    /// <param name="brick">배치하는 brick obj</param>
    /// <returns>true: 설치 가능 / false: 설치 불가능</returns>
    public bool AddBrick(GameObject brick)
    {
        int brickId = int.Parse(brick.GetComponent<BrickActionController>().brickId);
        int curCount = int.Parse(brickIconCountText[brickId].text);

        if (curCount == brickIconLimit[brickId])
        {
            StageMakerNotice.instance.AsyncNoticer("더 이상 배치할 수 없습니다.", Color.red);
            return false;
        }

        curCount += 1;

        brickIconCountText[brickId].text = curCount.ToString();

        if(brickIconLimit[brickId] == curCount)
        {
            brickIconCountText[brickId].color = Color.red;
        }

        return true;
    }

    public bool AddBrick(int brickId)
    {
        int curCount = int.Parse(brickIconCountText[brickId].text);

        if (curCount == brickIconLimit[brickId])
            return false;

        curCount += 1;

        brickIconCountText[brickId].text = curCount.ToString();

        if (brickIconLimit[brickId] == curCount)
        {
            brickIconCountText[brickId].color = Color.red;
        }

        return true;
    }


    /// <summary>
    /// Brick을 제거할 때 호출
    /// </summary>
    /// <param name="brick">제거하는 brick obj</param>
    public void RemoveBrick(GameObject brick)
    {
        int brickId = int.Parse(brick.GetComponent<BrickActionController>().brickId);
        int curCount = int.Parse(brickIconCountText[brickId].text);

        curCount -= 1;

        brickIconCountText[brickId].text = curCount.ToString();
        
        brickIconCountText[brickId].color = Color.black;
        
    }

    public void RemoveBrick(int brickId)
    {
        int curCount = int.Parse(brickIconCountText[brickId].text);

        curCount -= 1;

        brickIconCountText[brickId].text = curCount.ToString();

        brickIconCountText[brickId].color = Color.black;

    }

    public void BrickCountReset()
    {
        for(int i=0;i<brickIconCountText.Count; i++)
        {
            brickIconCountText[i].text = "0";
        }
    }

    public bool IsSavePossible()
    {
        if (int.Parse(brickIconCountText[0].text) != 1
            || int.Parse(brickIconCountText[1].text) != 1)
        {
            StageMakerNotice.instance.AsyncNoticer("필수 brick이 배치되지 않았습니다.", Color.red);
            
            return false;
        }

        return true;
    }
}
