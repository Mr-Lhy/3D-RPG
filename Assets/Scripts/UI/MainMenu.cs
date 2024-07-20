using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueBtn;
    Button quitBtn;

    PlayableDirector director;
    private bool isContinuingGame = false; // ������������ʾ�Ƿ������Ϸ

    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        quitBtn = transform.GetChild(3).GetComponent<Button>();


        newGameBtn.onClick.AddListener(delegate { PlayTimeline(false); }); // ��ʼ����Ϸ
        continueBtn.onClick.AddListener(delegate { PlayTimeline(true); }); // ������Ϸ
        quitBtn.onClick.AddListener(QuitGame);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += DetermineGameAction;
    }

    void PlayTimeline(bool isContinue)
    {
        isContinuingGame = isContinue; // �����Ƿ������Ϸ�ı��
        director.Play();
    }

    void DetermineGameAction(PlayableDirector obj)
    {
        if (isContinuingGame)
        {
            ContinueGame(); // ����Ǽ�����Ϸ
        }
        else
        {
            NewGame(); // ����ǿ�ʼ����Ϸ
        }
    }

    void NewGame()
    {
        PlayerPrefs.DeleteAll();
        // ת������
        SceneController.Instance.TransitionToFirstLevel();
    }

    void ContinueGame()
    {
        // ת������
        // ��ȡ����
        SceneController.Instance.TransitionToLoadGame();
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("�˳���Ϸ");
    }
}