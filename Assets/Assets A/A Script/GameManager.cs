using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	private int i;
    public int CounterDrag;
    public Transform _ovrPlayer;
    public List<GameObject> _instA,_uObjects,_nxtBtns;
    public GameObject _activityCompleteMsg, _mainObject, dummy,_zoomObj;
    public RectTransform _midLine;
    public int _counter, _countCriteria;

    public List<RectTransform> _drawLines;

    void Start ()
	{
		i = 0;
        _counter = 0;
        _countCriteria = 0;
        Instance = this;
       _uObjects[0].SetActive(true);
        QualitySettings.SetQualityLevel(5, true);
        QualitySettings.antiAliasing = 8;
    }
    public void Btn_Equilateral()
    {
        _counter++;
        _uObjects[0].SetActive(false);
        _uObjects[1].SetActive(true);
        _uObjects[2].SetActive(true);
        _uObjects[7].SetActive(true);
        _uObjects[2].transform.DOLocalMoveX(2.5f, 2f).SetEase(Ease.Linear);
        _drawLines[0].DOSizeDelta(new Vector2(500f, 4f), 2f).SetEase(Ease.Linear).OnComplete(()=> {
           // _uObjects[2].SetActive(false);
           // _uObjects[7].SetActive(false);
            dummy.transform.DOScale(Vector3.one, 1f).SetEase(Ease.Linear).OnComplete(() => {
                _uObjects[2].SetActive(false);
                _uObjects[7].SetActive(false);
                _uObjects[3].SetActive(true);
                _uObjects[4].SetActive(true);
                _nxtBtns[0].SetActive(true);
            });
        });
        _instA[0].SetActive(true);
    }

    public void BtnNxt_0()
    {
        _uObjects[5].SetActive(true);
        _instA[1].SetActive(true);
        _uObjects[6].transform.DOLocalRotate(new Vector3(89.25f, 0f,0f), 2f).SetEase(Ease.Linear);
        _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 0f, -44.6f), 2f).SetEase(Ease.Linear);
        dummy.transform.DOScale(Vector3.one, 2f).SetEase(Ease.Linear).OnComplete(() => {
           // dummy.transform.DOScale(Vector3.one, AudioSourceA.Instance.aSourceVO.clip.length+0.5f).SetEase(Ease.Linear).OnComplete(() => {
                
                
                _nxtBtns[1].SetActive(true);
            //});
        });
    }

    public void BtnNxt_1()
    {
        _instA[2].SetActive(true);
        _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 0f, 0.5f), 0f).SetEase(Ease.Linear).OnComplete(() => {
            _uObjects[8].SetActive(true);
            _uObjects[8].GetComponent<Image>().DOFillAmount(0.08f, 2f).SetEase(Ease.Linear);
            _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 0f, 29.2f), 2f).SetEase(Ease.Linear).OnComplete(() => {
               // _uObjects[5].SetActive(false);
               // _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 180f, 0.5f), 0f).SetEase(Ease.Linear);
               // _uObjects[5].transform.DOLocalMoveX(2.5f, 0f).SetEase(Ease.Linear);
                _nxtBtns[2].SetActive(true);
            });
        });
    }

    public void BtnNxt_2()
    {
        _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 180f, 0.5f), 0f).SetEase(Ease.Linear);
        _uObjects[5].transform.DOLocalMoveX(2.5f, 0f).SetEase(Ease.Linear).OnComplete(() => {
            _instA[3].SetActive(true);
            _uObjects[5].SetActive(true);
            _uObjects[9].SetActive(true);
            _uObjects[9].GetComponent<Image>().DOFillAmount(0.08f, 2f).SetEase(Ease.Linear);
            _uObjects[5].transform.DOLocalRotate(new Vector3(0f, 180f, 29.2f), 2f).SetEase(Ease.Linear).OnComplete(() => {
               // _uObjects[5].SetActive(false);
                _nxtBtns[3].SetActive(true);
            });
        });
    }

    public void BtnNxt_3()
    {
        _uObjects[5].SetActive(false);
        _instA[4].SetActive(true);
        _uObjects[7].transform.DOLocalMove(new Vector3(1.05f, 0f, -0.1125f), 0f).SetEase(Ease.Linear);
        _uObjects[7].transform.DOLocalRotate(new Vector3(0f, 0f, -60f), 0f).SetEase(Ease.Linear);
        _uObjects[7].SetActive(true);
        _drawLines[1].gameObject.SetActive(true);
        _drawLines[1].DOSizeDelta(new Vector2(500f, 4f), 2f).SetEase(Ease.Linear);
        _uObjects[2].SetActive(true);
        _uObjects[2].transform.DOLocalMove(new Vector3(0f, 5.08f, 0f), 2f).SetEase(Ease.Linear).OnComplete(() => {
            _nxtBtns[4].SetActive(true);
            _uObjects[7].SetActive(false);
            _uObjects[10].SetActive(true);
        });
    }

    public void BtnNxt_4()
    {
        _instA[5].SetActive(true);
        _uObjects[7].transform.DOLocalMove(new Vector3(-1.05f, 0f, -0.1125f), 0f).SetEase(Ease.Linear);
        _uObjects[7].transform.DOLocalRotate(new Vector3(0f, 0f, 60f), 0f).SetEase(Ease.Linear);
        _uObjects[7].SetActive(true);
        _drawLines[2].gameObject.SetActive(true);
        _drawLines[2].DOSizeDelta(new Vector2(500f, 4f), 2f).SetEase(Ease.Linear);
        
        _uObjects[2].transform.DOLocalMove(new Vector3(-2.5f, 0.76f, 0f), 2f).SetEase(Ease.Linear).OnComplete(() => {
            _nxtBtns[5].SetActive(true);
           // _uObjects[2].SetActive(false);
           // _uObjects[7].SetActive(false);
        });
    }

    public void BtnNxt_5()
    {
        _instA[6].SetActive(true);
        _uObjects[2].SetActive(false);
        _uObjects[7].SetActive(false);
    }

    public void BtnNxt_6()
    {
        _instA[6].SetActive(true);
        _uObjects[2].SetActive(false);
        _uObjects[7].SetActive(false);
    }

    public void Btn_Isosceles()
    {
        _counter++;
        _uObjects[0].SetActive(false);
        _uObjects[1].SetActive(true);
        _instA[0].SetActive(true);
    }


    public void Btn_Introduction()
    {
        _counter++;
        _uObjects[0].SetActive(false);
        _uObjects[1].SetActive(true);
        _instA[0].SetActive(true);
    }

    public void ActivityIntro()
    {
        dummy.transform.DOScale(Vector3.one, 2.6f).SetEase(Ease.Linear).OnComplete(ActivityFinish);
    }

    public void Btn_Similarity()
    {
        _counter++;
        _uObjects[0].SetActive(false);
        _instA[3].SetActive(true);
    }


    public void BtnNxt_0_()
    {
        _instA[0].SetActive(false);
        _instA[1].SetActive(true);
    }




    void ActivityFinish()
    {
        if (_counter > 2)
        {
            _activityCompleteMsg.SetActive(true);
            _counter = 0;
        }
        else
        {
            _uObjects[0].SetActive(true);
        }
    }

    public void QuitGame ()
	{
		Application.Quit ();
	}

	public void LoadScene (int SceneNo)
	{
		SceneManager.LoadScene (SceneNo);
	}
}