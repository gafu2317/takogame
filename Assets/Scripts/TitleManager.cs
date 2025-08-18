using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UI References")]
    public Button courseAButton;
    public Button courseBButton;
    
    [Header("Course Settings")]
    public string courseASceneName = "CourseA";
    public string courseBSceneName = "CourseB";
    
    void Start()
    {
        // ボタンにイベントを登録
        if (courseAButton != null)
        {
            courseAButton.onClick.AddListener(() => StartCourse("A"));
        }
        
        if (courseBButton != null)
        {
            courseBButton.onClick.AddListener(() => StartCourse("B"));
        }
    }
    
    public void StartCourse(string courseType)
    {
        Debug.Log($"コース{courseType}を選択しました");
        
        // 選択したコースを保存
        PlayerPrefs.SetString("SelectedCourse", courseType);
        PlayerPrefs.Save();
        
        // コースに応じたシーンに遷移
        string targetScene = courseType == "A" ? courseASceneName : courseBSceneName;
        SceneManager.LoadScene(targetScene);
    }
    
    // ボタンから直接呼び出す用（Inspector経由）
    public void OnCourseAButtonClicked()
    {
        StartCourse("A");
    }
    
    public void OnCourseBButtonClicked()
    {
        StartCourse("B");
    }
}