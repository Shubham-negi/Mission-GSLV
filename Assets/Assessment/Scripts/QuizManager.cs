using SmartLocalization;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Rwizen.Assessment
{
    public class QuizManager : MonoBehaviour
    {

        [Header("Enter number of questions and select the respective correct options for each")]
        public Question[] m_Questions;

        [Space(10)]
        public GameObject m_InstructionPanel;
        public GameObject m_QuizPanel;
        public GameObject m_ResultPanel;

        public TextMeshProUGUI m_QuestionTitle;
        public Text m_QuestionNumber;
        public Text m_Score;

        public GraphicRaycaster m_UIRaycaster;

        public TextMeshProUGUI[] m_OptionsText;
        public Button[] m_OptionsButton;
        public AudioClip[] m_AudioClips;

        public AudioSource m_AudioSource;

        private int currentQuesIndex = 0;
        private int currentAttempt;
        private int currentScore;
        private int totalScore;
        private int totalQuestions;


        private void Start()
        {

             

            m_InstructionPanel.SetActive(true);
            m_QuizPanel.SetActive(false);
            m_ResultPanel.SetActive(false);

            totalQuestions = m_Questions.Length;
            totalScore = totalQuestions * 2;
            currentAttempt = 0;
            currentScore = 0;
            currentQuesIndex = 0;


           
           _QuizData = new QuizData();

            if (PlayerPrefs.HasKey("MasterJson"))
            {
                MasterData masterData = JsonUtility.FromJson<MasterData>(PlayerPrefs.GetString("MasterJson"));
                _QuizData.chapterName= masterData.chapterName;
                _QuizData.className = masterData.className;
                _QuizData.topicName= masterData.topicName;
                _QuizData.subjectName= masterData.subjectName;
                accessToken = masterData.accessToken;

            }

            _QuizData.qa = new List<Quiz>();

            SendMultiScore(); //Check for saved data if any and then send API for multi scores

        }


        public void StartQuiz()
        {

            m_AudioSource.PlayOneShot(m_AudioClips[0]);
            m_InstructionPanel.SetActive(false);

            SetQuizUI();
        }


        private void SetQuizUI()
        {
            ResetQuizUI();

            string quesCode = "Quiz" + (currentQuesIndex + 1);
            m_QuestionTitle.gameObject.name = quesCode;


           
            Quiz quiz = new Quiz();

            quiz.question = LanguageManager.Instance.GetTextValue(m_QuestionTitle.name); ;
            quiz.answer = new string[m_OptionsButton.Length];
            quiz.studentAnswer = new List<string>();


            for (int i = 0; i < m_OptionsButton.Length; i++)
            {

                string optionCode = quesCode + "_" + (i + 1);
                m_OptionsButton[i].gameObject.name = optionCode;
                quiz.answer[i] = LanguageManager.Instance.GetTextValue(m_OptionsButton[i].name);


            }

            _QuizData.qa.Add(quiz);

            m_QuizPanel.SetActive(true);
            StartCoroutine(ForceUpdateLayout(m_QuizPanel));


            
               

        }

        private IEnumerator ForceUpdateLayout(GameObject panel)
        {
            var layoutgroup = panel.GetComponentInChildren<LayoutGroup>();
            if (layoutgroup != null)
            {
                yield return null;
                layoutgroup.enabled = false;
                layoutgroup.enabled = true;
            }

            
        }




        public void OnOptionClick(int optionIndex)
        {

            m_UIRaycaster.enabled = false;

            _QuizData.qa[currentQuesIndex].studentAnswer.Add (LanguageManager.Instance.GetTextValue(m_OptionsButton[optionIndex].name));

            LanguageHandler.instance.StopVoiceOver();

            int correctIndex = m_Questions[currentQuesIndex].correctOption;

            

            if (correctIndex == optionIndex)
            {
                m_AudioSource.PlayOneShot(m_AudioClips[2]);
                ShowCorrectOption();
                _QuizData.qa[currentQuesIndex].isCorrect = true;
            }

            else
            {
                m_AudioSource.PlayOneShot(m_AudioClips[1]);
                SetOptionUI(m_OptionsButton[optionIndex], false, Color.red);
                OnWrongAnswer();

            }

           

        }

        private void OnWrongAnswer()
        {
            currentAttempt++;

            if (currentAttempt == 2)
            {
                Invoke(nameof(ShowCorrectOption), 1f);
                _QuizData.qa[currentQuesIndex].isCorrect = false;

            }

            else
                m_UIRaycaster.enabled = true;

        }

        private void ShowCorrectOption()
        {

            int correctIndex = m_Questions[currentQuesIndex].correctOption;
            SetOptionUI(m_OptionsButton[correctIndex], false, Color.green);

            _QuizData.qa[currentQuesIndex].correct = LanguageManager.Instance.GetTextValue(m_OptionsButton[correctIndex].name);
            _QuizData.qa[currentQuesIndex].attempts = _QuizData.qa[currentQuesIndex].studentAnswer.Count;

            currentScore += 2 - currentAttempt;
            currentAttempt = 0;

            Invoke(nameof(NextQuestion), 1f);

        }


        private void NextQuestion()
        {
            currentQuesIndex++;

            if (currentQuesIndex == totalQuestions)
            {
                m_QuizPanel.SetActive(false);
                m_ResultPanel.SetActive(true);
                m_UIRaycaster.enabled = true;
                m_Score.text = (currentScore) + "/" + totalScore;

                _QuizData.score = m_Score.text;
                _QuizData.totalScore =totalScore.ToString();

                SendScore(_QuizData);

            }

            else
            {
                m_QuizPanel.SetActive(false);
                SetQuizUI();
            }

        }


        private void ResetQuizUI()
        {



            m_QuestionNumber.text = (currentQuesIndex + 1) + "/" + totalQuestions;

            for (int i = 0; i < m_OptionsButton.Length; i++)
                SetOptionUI(m_OptionsButton[i], true, Color.white);

            m_UIRaycaster.enabled = true;

        }

        private void SetOptionUI(Button button, bool val, Color color)
        {
            button.image.raycastTarget = val;
            button.image.color = color;

        }


        public void Home()
        {
            Loader.LoadScene("MainMenu");
        }


        public void PlayAgain()
        {
            Loader.LoadScene("Assessment");
        }

        public void Exit()
        {
            Application.Quit();
        }




        #region API Region

        [Header("API Region")]
        public QuizData _QuizData;
        public string accessToken; // your access token here
        public const string scoreUrl = "https://metabookxrservices.metabookxr.com/v1/scores";
        public const string multiScoreUrl = "https://metabookxrservices.metabookxr.com/v1/scores/multi";

        public void SendScore(QuizData quizData)
        {
            string json = JsonUtility.ToJson(quizData);

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Internet connection is not available");

                SaveDataLocally(quizData);
                return;
            }


            StartCoroutine(PostScore(json,scoreUrl));
        }

        public void SendMultiScore()
        {
           
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Internet connection is not available");
                return;
            }

            if (PlayerPrefs.HasKey("SavedData")) {

                Root rootData = JsonUtility.FromJson<Root>(PlayerPrefs.GetString("SavedData"));

                StudentData studentData = rootData.students.Find(x => x.accessKey == accessToken);

                if (studentData != null)
                {

                   
               
                    Scores scores = new Scores();
                    scores.scores = studentData.scores;

                    string jsonArray = JsonUtility.ToJson(scores);

                    Debug.Log("SavedData " + JsonUtility.ToJson(scores));

                    StartCoroutine(PostScore(jsonArray, multiScoreUrl));

                }

               
               
            }
  
        }

        private IEnumerator PostScore(string json, string url)
        {

            byte[] postData = Encoding.UTF8.GetBytes(json);
            UnityWebRequest request = UnityWebRequest.Post(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log("Score post request failed: " + request.error);

                
            }

            else
            {
                Debug.Log("Score posted successfully");
                DeleteData();

            }
        }

        private void DeleteData()
        {

            Root rootData = JsonUtility.FromJson<Root>(PlayerPrefs.GetString("SavedData"));

            if (rootData == null)
                return;

            StudentData studentData = rootData.students.Find(x => x.accessKey == accessToken);

            if (studentData != null)
            {

                rootData.students.Remove(studentData);

                if(rootData.students.Count ==0)
                    PlayerPrefs.DeleteKey("SavedData");

                else
                    PlayerPrefs.SetString("SavedData", JsonUtility.ToJson(rootData));
            }

            
        }

        private void SaveDataLocally(QuizData quizData)
        {

            if (PlayerPrefs.HasKey("SavedData")){

                Root rootData = JsonUtility.FromJson<Root>(PlayerPrefs.GetString("SavedData"));

                StudentData studentData = rootData.students.Find(x => x.accessKey == accessToken);

                if(studentData!= null)
                {
                    studentData.scores.Add(quizData);
                }

                else
                {
                    StudentData newStudent = new StudentData();
                    newStudent.scores = new List<QuizData> { quizData };
                    newStudent.accessKey = accessToken;   
                    rootData.students.Add(newStudent);
                }


                PlayerPrefs.SetString("SavedData", JsonUtility.ToJson(rootData)); 

            }

            else
            {

                Root rootData = new Root();
                rootData.students = new List<StudentData>();

                StudentData newStudent = new StudentData();
                newStudent.scores = new List<QuizData> { quizData };
                newStudent.accessKey = accessToken;
                rootData.students.Add(newStudent);

                PlayerPrefs.SetString("SavedData", JsonUtility.ToJson(rootData));

            }
            
          
            
        }


        #endregion



    }

    [System.Serializable]
    public class Question
    {
        [Range(0,3)]
        public int correctOption;
    }

    [System.Serializable]
    public class Root
    {

        public List<StudentData> students;
    }


    [System.Serializable]
    public class StudentData
    {
      
        public List<QuizData> scores;
        public string accessKey;
    }

    [System.Serializable]
    public class Scores
    {
        public List<QuizData> scores;
    }


    [System.Serializable]
    public class QuizData
    {
        public List<Quiz> qa;
        public string score;
        public string totalScore;
        public string className;
        public string chapterName;
        public string topicName;
        public string subjectName;
    }

    [System.Serializable]
    public class Quiz
    {
        public string question;
        public string[] answer;
        public string correct;
        public List<string> studentAnswer;
        public bool isCorrect;
        public int attempts;
    }

}
