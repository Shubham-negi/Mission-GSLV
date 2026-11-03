using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using Pvr_UnitySDKAPI;

public class CircleGameManager : MonoBehaviour
{
    public static CircleGameManager instance;

    public List<GameObject> _uObjects;

    public int _counter;
    public int _countCriteria;
    public List<RectTransform> _drawLines;
    public GameObject dummy;
    public GameObject menuPanel;
    public GameObject circle;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
        _counter = 0;
        _countCriteria = 0;

        //_uObjects[0].SetActive(true);
        QualitySettings.SetQualityLevel(5, true);
        QualitySettings.antiAliasing = 8;
    }

    void Update()
    {
        if (Controller.UPvr_GetKeyDown(Pvr_ControllerManager.controllerlink.mainHandID, Pvr_KeyCode.APP))
        {
            Application.Quit();
        }
    }

    public GameObject[] circleAndItsPartsObj;
    public void Btn_CircleAndItsParts()
    {
        circleAndItsPartsObj[1].SetActive(true);
        circleAndItsPartsObj[2].SetActive(true);
        circleAndItsPartsObj[6].SetActive(true);
        //circleInstruments[3].DOLocalRotate(new Vector3(0f, 0f, -380f), 3f).SetEase(Ease.Linear);
        circleAndItsPartsObj[2].transform.DOLocalRotate(new Vector3(0f, 0f, -180f), 3f).SetEase(Ease.Linear).SetEase(Ease.Linear).OnComplete(() =>
          {
              circleAndItsPartsObj[2].transform.DOLocalRotate(new Vector3(0f, 0f, -360f), 3f).SetEase(Ease.Linear).SetEase(Ease.Linear);

              dummy.transform.DOScale(Vector3.one, 4f).SetEase(Ease.Linear).OnComplete(() =>
              {
                  circleAndItsPartsObj[3].SetActive(true);
                  circleAndItsPartsObj[5].SetActive(true);
                  circleAndItsPartsObj[2].SetActive(false);
              });

              
          });

        EnablePoints();

    }

    public List<GameObject> points1;
    public List<GameObject> points2;

    void EnablePoints()
    {
        StartCoroutine(EnablePointsCorotine());
    }
    IEnumerator EnablePointsCorotine()
    {
        for (int i = 0; i < points1.Count; i++)
        {
            points1[i].SetActive(true);
            yield return new WaitForSeconds( 6f / 78f );
            points2[i].SetActive(true);
            yield return new WaitForSeconds( 6f / 78f );
        }

    }

    public void Btn_Next()
    {
        circleAndItsPartsObj[0].SetActive(false);
        menuPanel.SetActive(true);
        circle.SetActive(true);
    }

    public void Btn_QuesAns()
    {
        btn_QuesAns.SetActive(false);
        menuPanel.SetActive(false);
        circle.SetActive(false);
        ques1.SetActive(true);
    }


    bool isRadius, isChord, isSegment, isSector, isCircum, isArc;
    public void Btn_RadiusDia()
    {
        if (!isRadius)
            _counter++;
        isRadius = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }
        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[0].SetActive(true);
        _uObjects[5].SetActive(false);

        _uObjects[0].SetActive(true);
        _uObjects[2].SetActive(true);
        menuPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

        _drawLines[0].DOSizeDelta(new Vector2(250, 4f), 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            _drawLines[0].GetChild(0).gameObject.SetActive(true);
            _drawLines[0].GetChild(1).gameObject.SetActive(true);
            
            dummy.transform.DOScale(Vector3.one, 3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                _uObjects[1].SetActive(true);
                _uObjects[4].SetActive(true);
                _drawLines[2].DOSizeDelta(new Vector2(500, 4f), 2f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    _drawLines[2].GetChild(0).gameObject.SetActive(true);
                    _drawLines[2].GetChild(1).gameObject.SetActive(true);

                });


            });
        });

    }

    public List<GameObject> circleObj;
    
    public void Btn_Chord()
    {
        if (!isChord)
            _counter++;
        isChord = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }
        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[1].SetActive(true);
        _uObjects[5].SetActive(true);
        _drawLines[1].DOSizeDelta(new Vector2(448, 4f), 2f).SetEase(Ease.Linear).OnComplete(() =>
        {

            _drawLines[1].transform.GetChild(0).gameObject.SetActive(true);
            _drawLines[1].transform.GetChild(1).gameObject.SetActive(true);

        });

    }

    public Image segmentImg;
    public void Btn_Segment()
    {
        if (!isSegment)
            _counter++;
        isSegment = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }
        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[2].SetActive(true);
        _uObjects[5].SetActive(true);
        segmentImg.DOFillAmount(0.28f, 2).SetEase(Ease.Linear).OnComplete(() =>
        {

            _uObjects[6].SetActive(true);
            _uObjects[7].SetActive(true);
            _uObjects[8].SetActive(true);

        });

    }

    public Image sectorImg;
    public Image sectorImg1;
    public void Btn_Sector()
    {
        if (!isSector)
            _counter++;
        isSector = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }
        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[3].SetActive(true);
        _uObjects[5].SetActive(false);

        sectorImg.DOFillAmount(0.213f, 2).SetEase(Ease.Linear).OnComplete(() =>
        {

            _uObjects[9].SetActive(true);
            _uObjects[10].SetActive(true);
            _uObjects[11].SetActive(true);
            _uObjects[12].SetActive(true);

            dummy.transform.DOScale(Vector3.one, 1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                _uObjects[3].SetActive(true);

                sectorImg1.DOFillAmount(0.5f, 2).SetEase(Ease.Linear).OnComplete(() =>
                {
                    _uObjects[19].SetActive(true);
                    _uObjects[20].SetActive(true);
                    _uObjects[21].SetActive(true);
                    

                });


            });

        });

    }

    public Image arcImg1;
    public Image arcImg2;
    public void Btn_Arc()
    {
        if (!isArc)
            _counter++;
        isArc = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }
        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[4].SetActive(true);
        _uObjects[5].SetActive(true);
        arcImg1.DOFillAmount(0.285f, 2).SetEase(Ease.Linear).OnComplete(() =>
        {
            _uObjects[13].SetActive(true);
            _uObjects[14].SetActive(true);

        });

        arcImg2.DOFillAmount(0.622f, 2).SetEase(Ease.Linear).OnComplete(() =>
        {
            _uObjects[15].SetActive(true);
            _uObjects[16].SetActive(true);

        });


    }

    public Image circumImg;
    public GameObject btn_QuesAns;
    public GameObject ques1;
    public void Btn_Circumference()
    {
        if (!isCircum)
            _counter++;
        isCircum = true;
        if (_counter == 6)
        {
            btn_QuesAns.SetActive(true);
        }

        EnableMenuPanelCollider();
        DisableAllCircleObj();
        circleObj[5].SetActive(true);
        _uObjects[5].SetActive(true);
        circumImg.DOFillAmount(1, 2).SetEase(Ease.Linear).OnComplete(() =>
        {
            _uObjects[17].SetActive(true);
            _uObjects[18].SetActive(true);

        });


    }

    

    void DisableAllCircleObj()
    {
        for (int i = 0; i < circleObj.Count; i++)
        {
            circleObj[i].SetActive(false);
        }

        for (int i = 0; i < _drawLines.Count; i++)
        {
            _drawLines[i].sizeDelta = new Vector2(0, 4f);
        }

        for (int i = 0; i < _uObjects.Count; i++)
        {
            _uObjects[i].SetActive(false);
        }

        for (int i = 0; i < _drawLines.Count; i++)
        {
            _drawLines[i].GetChild(0).gameObject.SetActive(false);
            _drawLines[i].GetChild(1).gameObject.SetActive(false);
        }
        

        segmentImg.fillAmount = 0;
        sectorImg.fillAmount = 0;
        sectorImg1.fillAmount = 0;
        arcImg1.fillAmount = 0;
        arcImg2.fillAmount = 0;
        circumImg.fillAmount = 0;
    }


    void EnableMenuPanelCollider()
    {
        for (int i = 0; i < menuPanel.transform.childCount; i++)
        {
            menuPanel.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;

        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadScene(int SceneNo)
    {
        SceneManager.LoadScene(SceneNo);
    }
}
