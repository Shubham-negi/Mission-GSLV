using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.VR;
using qwr;
using QWR.Rwizen.Demo;
using UnityEngine.UI;
using System;
using Rwizen.Assessment;

public class ExitMenu : MonoBehaviour
{
    public static ExitMenu instance;


    AudioSource[] _AllAudioSrc;
    List<bool> audioPlaying;

    public GameObject _QuitMenu, _CameraFade, _MainMenu;

    [Tooltip("Set object's scale 0. Put Canvas and Scalable GameObjects here to hide.")]
    public GameObject[] _HideObjects;
    Vector3[] _HideObjectsScale;
    [Tooltip("Disable object in scene. Put Halo, Line Renderer etc here to disable.")]
    public GameObject[] _DisableObjects;
    bool[] _DisableObjectsState;

    public bool IsQuitPanelOpen;
    float _dist = 1;


    internal GameObject CamY;
    Camera MainCam;

    public static string VoiceOverPath = "VoiceOvers/";
    AudioClip _Clip;
    public MovementController m_moveController;
    bool cStatus;

    public GameObject _teacherMenu;
    public GameObject _teacherClosed;

    public bool teacherControl;
    [HideInInspector]
    public GameViewEncoder gameViewEncoder;
    private bool pausedByTeacher;


    public void Action_ProcessStringData(string _string)
    {

        Debug.Log("Processing String : " + _string);

        if (_string.StartsWith("Play"))
            UnpauseByTeacher();
        else if (_string.StartsWith("Pause"))
            EnableTeacherMenu();
        else if (_string.StartsWith("Quit"))
            StartCoroutine(TeacherQuit());
    }



    public void Action_OnServerLeft(string _string)
    {
        Debug.Log("Server Left" + _string);
        StartCoroutine(TeacherQuit());
    }



    void Awake()
    {
        instance = this;

        if (PlayerPrefs.HasKey("TeacherControl"))
            teacherControl = PlayerPrefs.GetInt("TeacherControl") == 1;

        gameViewEncoder = GameObject.Find("GameViewEncoder").GetComponent<GameViewEncoder>();

        audioPlaying = new List<bool>();
        audioPlaying.Clear();
        _HideObjectsScale = new Vector3[_HideObjects.Length];
        _DisableObjectsState = new bool[_DisableObjects.Length];
        _QuitMenu.SetActive(false);
        _MainMenu.SetActive(!(SceneManager.GetActiveScene().name == "MainMenu"));
        _teacherMenu.SetActive(false);
        _CameraFade.SetActive(false);
        CamY = new GameObject();
        m_moveController = (MovementController)FindObjectOfType<MovementController>();

       
    }



    void UpdateCamY()
    {
        CamY.transform.position = MainCam.transform.position;
        CamY.transform.eulerAngles = new Vector3(0, MainCam.transform.eulerAngles.y, 0);
    }

    void Start()
    {
        MainCam = QwrCameraRig._instance._center_eye;
        CamY.transform.SetParent(MainCam.transform.parent);

        EnableFM();
     
       
    }



    void EnableFM()
    {
        if (teacherControl && PlayerPrefs.HasKey("DeviceIP"))
        {

            Debug.Log("Connecting to FM...1");
            FMSocketIOManager.instance.Action_SetIP(PlayerPrefs.GetString("DeviceIP"));
            FMSocketIOManager.instance.InitAsClient();
        }
    }


    void LateUpdate()
    {
        if (PlayerPrefs.HasKey("Label"))
        {
            gameViewEncoder.label = PlayerPrefs.GetInt("Label");
        }

        UpdateCamY();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!teacherControl)
                ToggleBackMenu();
            else
                ToggleTeacherMenu();
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			 if(PlayerPrefs.HasKey("TeacherControl") && PlayerPrefs.GetInt("TeacherControl") == 0 )
                ToggleBackMenu();
            }
#endif

    }



    IEnumerator TeacherQuit()
    {
        Time.timeScale = 1;
        _teacherMenu.SetActive(false);
        _teacherClosed.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        Application.Quit();
    }


    public void ToggleBackMenu()
    {
        if (IsQuitPanelOpen)
            ResumeLevel();
        else
            EnableBackMenu();
    }

    public void ToggleTeacherMenu()
    {
        if (pausedByTeacher)
            UnpauseByTeacher();
        else
            EnableTeacherMenu();
    }

    void PauseSimulation()
    {
        Time.timeScale = 0;
        _AllAudioSrc = Resources.FindObjectsOfTypeAll<AudioSource>();

        if (_AllAudioSrc.Length == 0)
            return;

        audioPlaying.Clear();

        for (int i = 0; i < _AllAudioSrc.Length; i++)
        {
            if (_AllAudioSrc[i].isPlaying)
            {
                audioPlaying.Add(true);
                _AllAudioSrc[i].Pause();
            }
            else
                audioPlaying.Add(false);
        }
    }

    void PlaySimulation()
    {
        Time.timeScale = 1;
        _AllAudioSrc = Resources.FindObjectsOfTypeAll<AudioSource>();

        if (_AllAudioSrc.Length == 0)
            return;

        if (audioPlaying.Count == 0)
            return;

        for (int i = 0; i < _AllAudioSrc.Length; i++)
        {
            if (audioPlaying[i])
            {
                _AllAudioSrc[i].UnPause();
            }
        }
    }

    public void EnableBackMenu()
    {
        IsQuitPanelOpen = true;

        if (Time.timeScale != 0)
            PauseSimulation();


        transform.eulerAngles = CamY.transform.eulerAngles;
        transform.position = CamY.transform.position + CamY.transform.forward * _dist;

        _QuitMenu.SetActive(true);

        _CameraFade.SetActive(true);

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjectsScale[i] = _HideObjects[i].transform.localScale;
        }

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjects[i].transform.localScale = Vector3.zero;
        }


        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjectsState[i] = _DisableObjects[i].activeSelf;
        }

        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjects[i].SetActive(false);
        }

        SoundManager.sManager.PlayClickSound();
        if (m_moveController)
        {
            cStatus = m_moveController.enabled;
            m_moveController.enabled = false;
        }
        else
        {
            cStatus = false;
        }

    }

    public void EnableTeacherMenu()
    {
        pausedByTeacher = true;
        if (Time.timeScale != 0)
            PauseSimulation();


        transform.eulerAngles = CamY.transform.eulerAngles;
        transform.position = CamY.transform.position + CamY.transform.forward * _dist;

        _teacherMenu.SetActive(true);

        _CameraFade.SetActive(true);

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjectsScale[i] = _HideObjects[i].transform.localScale;
        }

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjects[i].transform.localScale = Vector3.zero;
        }


        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjectsState[i] = _DisableObjects[i].activeSelf;
        }

        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjects[i].SetActive(false);
        }

        //  SoundManager.sManager.PlayClickSound();
        if (m_moveController)
        {
            cStatus = m_moveController.enabled;
            m_moveController.enabled = false;
        }
        else
        {
            cStatus = false;
        }

    }



    public void SetTargetPosition(Transform target)
    {

        target.eulerAngles = CamY.transform.eulerAngles;
        target.position = CamY.transform.position + CamY.transform.forward * _dist;
    }

    public void GOHome()
    {
        SoundManager.sManager.PlayClickSound();
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SoundManager.sManager.PlayClickSound();
        ResumeLevel();
        Loader.LoadScene("MainMenu");
    }

    public void ResumeLevel()
    {
        IsQuitPanelOpen = false;
        Time.timeScale = 1;
        _QuitMenu.SetActive(false);
        _CameraFade.SetActive(false);

        PlaySimulation();

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjects[i].transform.localScale = _HideObjectsScale[i];
        }

        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjects[i].SetActive(_DisableObjectsState[i]);
        }

        Debug.Log("Resume");
        m_moveController.enabled = cStatus;
    }

    public void UnpauseByTeacher()
    {
        pausedByTeacher = false;
        Time.timeScale = 1;
        _teacherMenu.SetActive(false);
        _CameraFade.SetActive(false);

        PlaySimulation();

        for (int i = 0; i < _HideObjects.Length; i++)
        {
            _HideObjects[i].transform.localScale = _HideObjectsScale[i];
        }

        for (int i = 0; i < _DisableObjects.Length; i++)
        {
            _DisableObjects[i].SetActive(_DisableObjectsState[i]);
        }

        Debug.Log("Resume");
        m_moveController.enabled = cStatus;
    }
}