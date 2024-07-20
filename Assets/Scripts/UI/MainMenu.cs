using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueBtn;
    Button quitBtn;

    PlayableDirector director;
    private bool isContinuingGame = false; // 新增变量来表示是否继续游戏

    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        quitBtn = transform.GetChild(3).GetComponent<Button>();


        newGameBtn.onClick.AddListener(delegate { PlayTimeline(false); }); // 开始新游戏
        continueBtn.onClick.AddListener(delegate { PlayTimeline(true); }); // 继续游戏
        quitBtn.onClick.AddListener(QuitGame);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += DetermineGameAction;
    }

    void PlayTimeline(bool isContinue)
    {
        isContinuingGame = isContinue; // 设置是否继续游戏的标记
        director.Play();
    }

    void DetermineGameAction(PlayableDirector obj)
    {
        if (isContinuingGame)
        {
            ContinueGame(); // 如果是继续游戏
        }
        else
        {
            NewGame(); // 如果是开始新游戏
        }
    }

    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        // 转换场景
        SceneController.Instance.TransitionToFirstLevel();
    }

    void ContinueGame()
    {
        // 转换场景
        // 读取进度
        SceneController.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("退出游戏");
    }
}