using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class BrickActionController : MonoBehaviour
{
    [Header("Brick_BrickName[BrickId]")]
    public string brickId;
    
    IEnumerator coroutine;

    AudioSource audioSource;

    #region specific variables

    // 003, 008, 011
    Animation anim;

    // 003 breakable
    int hp = 3;

    // 007 switch
    static public int switchCount; // 모든 switch의 개수
    bool isOn; // 본 switch가 On 되어 있는가(무언가 위에 올라와있는가)

    // 010 laser
    Material laserTex;
    GameObject cubrickBurnOutSmog;

    #endregion

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 생명주기 Start()에서 각 brick 의 동작을 제어
        switch (brickId)
        {
            case "000": // Start brick
                if (SceneManager.GetActiveScene().name == "StageMaker")
                    return;

                MeshDisabler();
                
                if (coroutine == null)
                {
                    coroutine = Brick000();
                    StartCoroutine(coroutine);
                }
                break;

            case "001": // Finish brick
                if (SceneManager.GetActiveScene().name == "StageMaker")
                    return;

                MeshDisabler();

                // 비활성화 되어도 찾을 수 있도록 빈 오브젝트의 하위에서 동작된다.
                transform.parent = GameObject.Find("FinishBrickRoot").transform;

                break;
                
            case "003": // Breakable brick
                anim = GetComponent<Animation>();
                break;

            case "004": // Elevate Up brick
                MeshDisabler();
                break;

            case "005": // Elevate Down brick
                MeshDisabler();
                break;
            case "007": // Switch
                if (SceneManager.GetActiveScene().name == "StageMaker")
                    return;

                MeshDisabler();

                if (BrickDataContainer.SwitchAdd())
                {
                    StartCoroutine(Brick007WaitFinishBrick(GameObject.Find("FinishBrickRoot").transform));
                }

                break;
            case "008":
                MeshDisabler();

                anim = GetComponent<Animation>();
                anim.Play("Spin");

                break;

            case "009":
                MeshDisabler();

                break;

            case "010":
                if (SceneManager.GetActiveScene().name == "StageMaker")
                    return;

                MeshDisabler();

                laserTex = Resources.Load<Material>("Materials/Bricks/[010]Brick/LaserTexture");
                //cubrickBurnOutMat = Resources.Load<Material>("Materials/Bricks/[010]Brick/BurnOutMat");
                cubrickBurnOutSmog = Resources.Load<GameObject>("Prefabs/Bricks/[010]Brick/BurnOutSmog");

                if (coroutine != null) break;

                coroutine = Brick010Detector();
                StartCoroutine(coroutine);

                break;

            case "011":
                if (SceneManager.GetActiveScene().name == "StageMaker")
                    return;

                anim = GetComponent<Animation>();
                anim.Play();

                break;
        }
    }

    /// <summary>
    /// 각 Brick별 동작 제어
    /// </summary>
    /// <param name="isBrick">플레이어가 아닌, 타 brick 에 의해 호출되는 경우.</param>
    /// <returns> true: 겹치기 가능. false: 겹치기 불가능 </returns>
    public bool CallBrickAction(Transform isBrick = null)
    {
        switch (brickId)
        {
            case "000": // Start
                

                break;
            case "001": // Finish

                if (coroutine != null) return true;

                // audio play
                audioSource.Play();
                
                coroutine = Brick001();
                StartCoroutine(coroutine);
                

                return true;
            case "002": // Hard
                
                // audio play
                audioSource.Play();

                break;


            case "003": // Breakable

                // audio play
                audioSource.Play();

                coroutine = Brick003();
                StartCoroutine(coroutine);

                print($"[DEV] ({hp}) Breakable brick 에 막혀 이동할 수 없습니다.");
                break;

            case "004": // Elevate Up

                // audio play
                audioSource.Play();

                coroutine = isBrick == null ? Brick004() : Brick004ByBrick(isBrick);
                StartCoroutine(coroutine);

                print($"[DEV] Elevate brick에 의해 위로 이동되었습니다.");
                return true;
                
            case "005": // Elevate Down

                // audio play
                audioSource.Play();

                coroutine = isBrick == null ? Brick005() : Brick005ByBrick(isBrick);
                StartCoroutine(coroutine);

                print($"[DEV] Elevate brick에 의해 아래로 이동되었습니다.");
                return true;

            case "006": // Pushable

                if (coroutine != null) break;

                // audio play
                audioSource.Play();

                coroutine = Brick006();
                StartCoroutine(coroutine);

                print($"[DEV] Pushable brick이 플레이어에게 밀렸습니다.");
                break;

            case "007": // Switch

                if (coroutine != null) break;

                // audio play
                audioSource.Play();

                coroutine = Brick007();
                StartCoroutine(coroutine);

                return true;

            case "008": // Key
                if (coroutine != null) break;

                // audio play
                audioSource.Play();

                coroutine = Brick008();
                StartCoroutine(coroutine);

                return true;

            case "009": // Lock
                print("[DEV] Lock brick 에 막혀 이동할 수 없습니다.");
                break;

            case "010": // 4-way Laser

                //if (coroutine != null) break;

                //coroutine = Brick010();
                //StartCoroutine(coroutine);

                print("[DEV] 4-way Laser brick 에 막혀 이동할 수 없습니다.");
                break;

            case "011":

                audioSource.Play();

                //coroutine = Brick011();
                //StartCoroutine(coroutine);

                return false;
                
            case "999": // Wall
                // audio play
                audioSource.Play();

                return false;

            default:
                Debug.LogError("[DEV] 정의되지 않은 브릭입니다.");
                
                break;
        }

        return false;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 게임 시작시 Mesh를 제거하는 함수.
    /// 
    /// 스테이지 메이킹을 위해 박스 Mesh가 필수적으로 있어야 한다.
    /// 단, 게임 진행시에는 Mesh가 필요 없으므로
    /// 게임 시작시 Mesh를 제거한다.
    /// </summary>
    void MeshDisabler()
    {
        if (SceneManager.GetActiveScene().name == "StageMaker")
            return;

        GetComponent<MeshRenderer>().enabled = false;
    }

    #region Specific Actions

    IEnumerator Brick000() // Start Brick
    {
        var v = Resources.Load<GameObject>("Prefabs/[Player]Cubrick");
        GameObject obj = Instantiate(v, transform.position, Quaternion.identity);
        obj.GetComponent<PlayerMover>().isMovePossible = false;
        //obj.transform.position = transform.position;

        float timer = 1f;

        Vector3 bottomPos = obj.transform.position;
        Vector3 topPos = obj.transform.position + new Vector3(0, 3, 0);

        while(timer > 0)
        {
            timer -= Time.deltaTime;

            obj.transform.position = Vector3.Lerp(bottomPos, topPos, timer);

            yield return null;
        }

        obj.transform.position = bottomPos;
        obj.GetComponent<PlayerMover>().isMovePossible = true;

        Destroy(gameObject);
    }
    
    IEnumerator Brick001() // Finish Brick
    {
        // 게임 클리어, 기록 갱신
        StageLoader.instance.GameClear();

        // Ingame UI 일괄 제거
        UIContainer.instance.StageClearIngameCanvasOffEff();
        
        // 플레이어 움직임 제어
        PlayerMover.instance.isMovePossible = false;

        // Success 이펙트
        Transform rootParticle = transform.GetChild(0);

        // spining particle
        ParticleSystem ps = rootParticle.GetChild(0).GetComponent<ParticleSystem>();
        var volt = ps.velocityOverLifetime;
        volt.speedModifier = 4; // 4배속

        // disable other particles
        rootParticle.GetChild(1).gameObject.SetActive(false); // Shining
        rootParticle.GetChild(2).gameObject.SetActive(false); // Blinking
        rootParticle.GetChild(3).gameObject.SetActive(false); // CircleUp

        // enable other particle
        rootParticle.GetChild(4).gameObject.SetActive(true);

        Vector3 bottomPos = transform.position;
        Vector3 middlePos = transform.position + new Vector3(0, 3, 0);
        Vector3 topPos = middlePos + new Vector3(0, 4, 0);

        yield return new WaitForSeconds(0.3f);

        float timer = 0;

        while(timer < 1.5f)
        {
            timer += Time.deltaTime;

            PlayerMover.instance.transform.position = Vector3.Lerp(bottomPos, middlePos, timer * 0.66666f);
            transform.position = PlayerMover.instance.transform.position;

            yield return null;
        }
        ps.gameObject.SetActive(false);
        
        // 스테이지 클리어 UI On
        UIContainer.instance.CanvasOn(CanvasType.Clear, true);

        while(timer < 2.5f)
        {
            timer += Time.deltaTime;

            PlayerMover.instance.transform.position = Vector3.Lerp(PlayerMover.instance.transform.position, topPos, 0.05f);
            transform.position = PlayerMover.instance.transform.position;

            yield return null;
        }

        yield return new WaitForSeconds(.5f);

        PlayerMover.instance.gameObject.SetActive(false);
        gameObject.SetActive(false);

    }

    IEnumerator Brick003() // Breakable Brick 
    {
        hp--;
        if(hp == 0)
        {
            anim.Play("Destroy");
        }
        else
        {
            anim.Play("Cracked");
        }

        coroutine = null;

        yield return null;
    }

    IEnumerator Brick004() // Elevate Up brick
    {
        PlayerMover.instance.isMovePossible = false;
        yield return new WaitForSeconds(0.2f);

        PlayerMover.instance.transform.Translate(new Vector3(0, 8, 0), Space.World);

        PlayerMover.instance.isMovePossible = true;
    }

    IEnumerator Brick004ByBrick(Transform brick) // Elevate Up brick. 플레이어가 아닌 타 brick에 의해 호출되는 경우
    {

        yield return new WaitForSeconds(0.25f);

        brick.Translate(new Vector3(0, 8, 0), Space.World);
    }

    IEnumerator Brick005() // Elevate Down brick
    {

        PlayerMover.instance.isMovePossible = false;
        yield return new WaitForSeconds(0.2f);

        PlayerMover.instance.transform.Translate(new Vector3(0, -8, 0), Space.World);

        PlayerMover.instance.isMovePossible = true;
    }

    IEnumerator Brick005ByBrick(Transform brick) // Elevate Down brick. 플레이어가 아닌 타 brick에 의해 호출되는 경우
    {

        yield return new WaitForSeconds(0.25f);

        brick.Translate(new Vector3(0, -8, 0), Space.World);
    }

    IEnumerator Brick006() // Pushable brick
    {

        Vector3 dir = PlayerMover.instance.transform.InverseTransformPoint(transform.position);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, 1))
        {
            BrickActionController bac = hit.collider.gameObject.GetComponent<BrickActionController>();

            // 충돌한 brick의 id
            string brickId = bac.brickId;
            // Elevate brick
            if (brickId == "004" || brickId == "005")
            {
                // Elevate 동작
                bac.CallBrickAction(transform);
            }

            // Switch brick
            else if (brickId == "007")
            {
                bac.CallBrickAction(transform);
            }

            // 막혀서 Push 될 수 없는 경우, 코루틴 종료.
            else
            {
                coroutine = null;
                yield break;
            }
        }
        else
        {
        }


        Vector3 _targetPos = transform.position + dir;
        Vector3 targetPos = new Vector3((int)_targetPos.x, (int)_targetPos.y, (int)_targetPos.z);

        
        float timer = 0f;
        float speed = 15f;

        while (timer < 0.2f)
        {
            timer += Time.deltaTime;

            //transform.position = Vector3.Lerp(transform.position, targetPos, 0.15f);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);

            yield return null;
        }
        transform.position = targetPos;

        coroutine = null;
    }

    IEnumerator Brick007() // Switch brick
    {
        if (BrickDataContainer.SwitchOn())
        {
            GameObject.Find("FinishBrickRoot").transform.GetChild(0).gameObject.SetActive(true);
        }
        StartCoroutine(Brick007Detector());
        
        coroutine = null;

        yield return null;
    }

    IEnumerator Brick007Detector() //
    {
        // 플레이어 등이 본 센서로 진입하는 시간 대기
        yield return new WaitForSeconds(0.2f);

        // 아직도 위에 무언가 올라와 있는가? 를 확인하는 코루틴.
        // 무언가 물체가 위에 올라와있다면 대기
        int layer = LayerMask.NameToLayer("Switchable");
        while (Physics.Raycast(transform.position + Vector3.up, Vector3.down, 1, 1 << layer))
        {
            Debug.DrawRay(transform.position, Vector3.up);
            //print($"[DEV] brick 007 detector is working now");
            yield return null;
        }

        //print($"[DEV] brick 007 detector is done");

        if (BrickDataContainer.SwitchOff())
        {
            GameObject.Find("FinishBrickRoot").transform.GetChild(0).gameObject.SetActive(false);
        }

        yield return null;
    }

    // finish brick이 switch brick보다 늦게 생성된다면
    // switch brick은 finish brick을 찾을 수 없다.
    // 따라서 finish brick이 생성되기 전까지 본 코루틴 루프를 돌며 대기한다.
    IEnumerator Brick007WaitFinishBrick(Transform finishBrickRoot)
    {
        while (finishBrickRoot.childCount == 0)
        {
            yield return null;
        }

        // Finish brick load 완료!!
        // 찾아서 비활성화 실행.
        finishBrickRoot.GetChild(0).gameObject.SetActive(false);
    }

    IEnumerator Brick008() // Key brick
    {
        anim.Play("Acquire");

        GameObject.Find("[009]Brick(Clone)").GetComponent<Animation>().Play("Open");

        coroutine = null;

        yield return null;
    }

    IEnumerator Brick010Detector() // 4-way Laser brick
    {
        LineRenderer[] lr = new LineRenderer[]{
            transform.GetChild(0).GetChild(0).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(1).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(2).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(3).GetComponent<LineRenderer>()
        };

        LineRenderer[] lrShadow = new LineRenderer[]
        {
            transform.GetChild(0).GetChild(4).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(5).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(6).GetComponent<LineRenderer>(),
            transform.GetChild(0).GetChild(7).GetComponent<LineRenderer>()
        };
        
        RaycastHit[] hit = new RaycastHit[4];

        float lazerSpeed = 5f;

        while (true)
        {
            laserTex.mainTextureOffset += new Vector2(Time.deltaTime * lazerSpeed * -1, 0);

            Ray[] ray = new Ray[]{
                new Ray(transform.position, transform.forward),
                new Ray(transform.position, transform.right),
                new Ray(transform.position, transform.forward * -1),
                new Ray(transform.position, transform.right * -1)
            };

            for (int i = 0; i < ray.Length; i++)
            {
                if (Physics.Raycast(ray[i], out hit[i], 10))
                {
                    Vector3 dir = hit[i].point - transform.position;
                    
                    lr[i].SetPosition(1, dir);
                    lrShadow[i].SetPosition(1, dir);
                    
                    // 레이저에 플레이어가 닿아서 게임 종료 -> 재시작
                    if (hit[i].collider.CompareTag("Player") && coroutine != null)
                    {
                        coroutine = null;

                        PlayerMover.instance.CallAnimTrigger("isBurnOut");
                        Instantiate(cubrickBurnOutSmog, PlayerMover.instance.transform.position, Quaternion.Euler(-90, 0, 0));

                        // audio play
                        audioSource.Play();

                        yield return new WaitForSeconds(2f);
                        LoadingManager.instance.LoadScene("IngameScene");
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator Brick011() // 4-way Sword brick
    {
        Vector3[] dirMap =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        // 4방향으로 체크. Sword에 무언가 닿았는지 여부 확인
        RaycastHit hit;
        for (int i = 0; i < dirMap.Length; i++)
        {
            if (Physics.Raycast(transform.position, dirMap[i], out hit, 1))
            {
                print($"hit name : {hit.collider.name}");

                if (hit.collider.CompareTag("Player"))
                {
                    PlayerMover.instance.CallAnimTrigger("isBurnOut");
                    yield return new WaitForSeconds(2f);
                    LoadingManager.instance.LoadScene("IngameScene");
                }
                else
                {
                    BrickActionController bac = hit.collider.gameObject.GetComponent<BrickActionController>();

                    // 충돌한 brick의 id
                    string brickId = bac.brickId;

                    // Pushable brick
                    if (brickId == "006")
                    {
                        //bac.CallBrickAction(transform);
                        GameObject.Destroy(bac.gameObject);
                    }
                }
            }
        }
        yield return null;
    }

    #endregion
}
