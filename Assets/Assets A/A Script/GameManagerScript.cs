using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using Pvr_UnitySDKAPI;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;
    public Material overlayMat;
    public List<GameObject> _uObjects;
    public int _counter;
    //public int _countCriteria;
    public int dragCounter;
    //public List<RectTransform> _drawLines;
    public GameObject dummy;
    public GameObject menuPanel;
    public GameObject menuPanelBasicTerm;
    public GameObject menuPanelAngles;
    public GameObject menuPanelPairOfAngles;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        _counter = 0;
        
        QualitySettings.SetQualityLevel(5, true);
        QualitySettings.antiAliasing = 8;
    }

    // Update is called once per frame

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadScene(int SceneNo)
    {
        SceneManager.LoadScene(SceneNo);
    }

    //........Line and Angle...........//

    public List<GameObject> menuList;
    public void ClickMainMenuPanel(GameObject item)
    {
        for (int i = 0; i < menuList.Count; i++)
        {
            menuList[i].SetActive(false);
        }
        menuPanel.SetActive(false);
        item.SetActive(true);
    }

    public void ClickRemark(GameObject obj)
    {
        if (obj.activeInHierarchy)
        {
            obj.SetActive(false);
        }
        else
        {
            obj.SetActive(true);
        }
    }

   public void ClickHistogram(Transform obj)
    {
        SoundManager.sManager.PlayClickSound();
        obj.parent.GetComponent<DelayCreate1>().enabled = true;
        //transform.parent.DOScaleY(new Vector3(1, _height,1), 2.5f).SetEase(Ease.Linear).OnComplete(() => {
        //    //foreach (var i in _next)
        //    //{
        //    //    i.SetActive(true);
        //    //}
        //});
    }

    /*
    public List<GameObject> basicTermList;

    bool basicTerm1;
    bool basicTerm2;
    bool basicTerm3;
    bool basicTerm4;
    bool basicTerm5;
    bool basicTerm6;
    public void ClickBasicTerm(GameObject item)
    {
        for (int i = 0; i < basicTermList.Count; i++)
        {
            basicTermList[i].SetActive(false);
            menuPanelBasicTerm.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
        }
        item.SetActive(true);

        if (item.CompareTag("basicTerm1"))
        {
            if (!basicTerm1)
            {
                _counter++;
            }
            basicTerm1 = true;
        }
        else if (item.CompareTag("basicTerm2"))
        {
            if (!basicTerm2)
            {
                _counter++;
            }
            basicTerm2 = true;
        }
        else if (item.CompareTag("basicTerm3"))
        {
            if (!basicTerm3)
            {
                _counter++;
            }
            basicTerm3 = true;
        }
        else if (item.CompareTag("basicTerm4"))
        {
            if (!basicTerm4)
            {
                _counter++;
            }
            basicTerm4 = true;
        }
        else if (item.CompareTag("basicTerm5"))
        {
            if (!basicTerm5)
            {
                _counter++;
            }
            basicTerm5 = true;
        }

        if (_counter == 5)
        {
            _uObjects[0].SetActive(true);
        }
    }

    public void EnableImg(GameObject obj)
    {
        for (int i = 0; i < menuPanelBasicTerm.transform.childCount; i++)
        {
            menuPanelBasicTerm.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; i < menuPanelAngles.transform.childCount; i++)
        {
            menuPanelAngles.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; i < menuPanelPairOfAngles.transform.childCount; i++)
        {
            menuPanelPairOfAngles.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
        }
        StartCoroutine(EnableSelectedImg(obj));
    }
    IEnumerator EnableSelectedImg(GameObject obj)
    {
        yield return new WaitForSeconds(.01f);
        obj.SetActive(true);
    }

    public void ClickNext_1()
    {
        SoundManager.sManager.PlayClickSound();
        basicTerm1 = false;
        basicTerm2 = false;
        basicTerm3 = false;
        basicTerm4 = false;
        basicTerm5 = false;
        basicTerm6 = false;
        _counter = 0;
        for (int i = 0; i < menuList.Count; i++)
        {
            menuList[i].SetActive(false);
        }
        menuPanel.SetActive(true);
        menuPanel.transform.GetChild(1).GetComponent<BoxCollider>().enabled = true;
    }

    public void ClickNext_2()
    {
        SoundManager.sManager.PlayClickSound();
        basicTerm1 = false;
        basicTerm2 = false;
        basicTerm3 = false;
        basicTerm4 = false;
        basicTerm5 = false;
        basicTerm6 = false;
        _counter = 0;
        for (int i = 0; i < menuList.Count; i++)
        {
            menuList[i].SetActive(false);
        }
        menuPanel.SetActive(true);
        menuPanel.transform.GetChild(2).GetComponent<BoxCollider>().enabled = true;
    }

    public void ClickNext_3()
    {
        SoundManager.sManager.PlayClickSound();
        for (int i = 0; i < menuList.Count; i++)
        {
            menuList[i].SetActive(false);
        }
        menuPanel.SetActive(true);
        menuPanel.transform.GetChild(3).GetComponent<BoxCollider>().enabled = true;
    }

    public List<GameObject> anglesList;

    public void ClickAnglesTerm(GameObject item)
    {
        for (int i = 0; i < anglesList.Count; i++)
        {
            anglesList[i].SetActive(false);
            menuPanelAngles.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
        }
        item.SetActive(true);

        if (item.CompareTag("basicTerm1"))
        {
            if (!basicTerm1)
            {
                _counter++;
            }
            basicTerm1 = true;
        }
        else if (item.CompareTag("basicTerm2"))
        {
            if (!basicTerm2)
            {
                _counter++;
            }
            basicTerm2 = true;
        }
        else if (item.CompareTag("basicTerm3"))
        {
            if (!basicTerm3)
            {
                _counter++;
            }
            basicTerm3 = true;
        }
        else if (item.CompareTag("basicTerm4"))
        {
            if (!basicTerm4)
            {
                _counter++;
            }
            basicTerm4 = true;
        }
        else if (item.CompareTag("basicTerm5"))
        {
            if (!basicTerm5)
            {
                _counter++;
            }
            basicTerm5 = true;
        }
        else if (item.CompareTag("basicTerm6"))
        {
            if (!basicTerm6)
            {
                _counter++;
            }
            basicTerm6 = true;
        }

        if (_counter == 6)
        {
            _uObjects[1].SetActive(true);
        }
    }

    public List<GameObject> pairOfAnglesList;
    public void ClickPairOfAngles(GameObject item)
    {
        for (int i = 0; i < pairOfAnglesList.Count; i++)
        {
            pairOfAnglesList[i].SetActive(false);
            menuPanelPairOfAngles.transform.GetChild(i).GetComponent<BoxCollider>().enabled = true;
        }
        item.SetActive(true);

        if (item.CompareTag("basicTerm1"))
        {
            if (!basicTerm1)
            {
                _counter++;
            }
            basicTerm1 = true;
        }
        else if (item.CompareTag("basicTerm2"))
        {
            if (!basicTerm2)
            {
                _counter++;
            }
            basicTerm2 = true;
        }
        else if (item.CompareTag("basicTerm3"))
        {
            if (!basicTerm3)
            {
                _counter++;
            }
            basicTerm3 = true;
        }
        else if (item.CompareTag("basicTerm4"))
        {
            if (!basicTerm4)
            {
                _counter++;
            }
            basicTerm4 = true;
        }
        else if (item.CompareTag("basicTerm5"))
        {
            if (!basicTerm5)
            {
                _counter++;
            }
            basicTerm5 = true;
        }

        if (_counter == 5)
        {
            _uObjects[4].SetActive(true);
        }
    }

    public List<Image> wrongImgList;
    public void ClickAxiomWrong(Image img)
    {
        img.color = new Color32(250, 70, 44, 255);
    }

    public void ClickAxiomRight(Image img)
    {
        img.color = new Color32(56, 224, 90, 255);
        for (int i = 0; i < wrongImgList.Count; i++)
        {
            wrongImgList[i].GetComponent<Animator>().enabled = false;
            wrongImgList[i].color = new Color32(250, 70, 44, 255);
            wrongImgList[i].GetComponent<BoxCollider>().enabled = false;
        }
    }
    */

}
