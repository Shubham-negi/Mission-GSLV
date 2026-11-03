using QWR;
using qwr;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rwizen.Assessment;
using UnityEngine.SceneManagement;

public class AppHandler : MonoBehaviour
{


    [SerializeField] private GameObject warning;
    [Header("Remember to uncheck these before making build")]
    [SerializeField]  private bool calledViaMasterAPP = true;
    [SerializeField]  private bool teacherControl;
    [SerializeField]  private LanguageID languageID;

    private string language;
    private int  label;
    private string deviceIP;

   private MasterData masterData;


    void Start()
    {

        if (!Loader.initialLoad)
            return;

        transform.parent = Camera.main.transform;
        transform.localPosition = Vector3.forward;
        transform.localEulerAngles = Vector3.zero;

        warning.SetActive(false);







#if !UNITY_EDITOR

		AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");



        try
        {


          string masterJson = intent.Call<string>("getStringExtra", "userData");
          masterData = JsonUtility.FromJson<MasterData>(masterJson);
          
           PlayerPrefs.SetString("MasterJson", masterJson);

          calledViaMasterAPP = masterData.calledViaMasterApp; 
          teacherControl = masterData.teacherControl;
          label = int.Parse(masterData.label);
          deviceIP = masterData.deviceIP;
          language = masterData.language;

           
             PlayerPrefs.SetInt("Label", label);
             PlayerPrefs.SetInt("TeacherControl", teacherControl == true ? 1 : 0);
             
            
            Time.timeScale = 1;
           //language = languageID.ToString().Replace("_", "-");
            PlayerPrefs.SetString("currentLanguage", language);

        }
        catch (Exception e)
        {
            AudioListener.volume = 0;
            Debug.Log(e.Message);
            warning.SetActive(true);
            Invoke("QuitAPP", 10f);
            Time.timeScale = 0;
            PlayerPrefs.DeleteKey("MasterJson");
        }


#else

        if (calledViaMasterAPP)
        {
            Time.timeScale = 1;
            language = languageID.ToString().Replace("_", "-");
            PlayerPrefs.SetString("currentLanguage", language);

        }

#endif


        if (!calledViaMasterAPP)
        {
            PlayerPrefs.DeleteKey("MasterJson");
            AudioListener.volume = 0;
            Time.timeScale = 0;
            warning.SetActive(true);
            Invoke(nameof(QuitAPP), 10f);


        }

        if (calledViaMasterAPP && teacherControl)
        {

            PlayerPrefs.SetString("DeviceIP", deviceIP);

            //  FMSocketIOManager.instance.Action_SetIP(deviceIP);
            // FMSocketIOManager.instance.InitAsClient();
        }



    }

	
	void QuitAPP()
    {
		Application.Quit();
	}



}

[System.Serializable]
public class MasterData
{
    public string accessToken;
    public string chapterName;
    public string topicName;
    public string subjectName;
    public string className;

    public bool calledViaMasterApp;
    public bool teacherControl;
    public string label;
    public string deviceIP;
    public string language;

}
