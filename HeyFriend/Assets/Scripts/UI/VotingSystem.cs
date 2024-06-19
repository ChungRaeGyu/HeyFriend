using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VotingSystem : MonoBehaviour
{
    private int pauseVote = 0;
    private int unpauseVote = 0;
    private int quitVote = 0;
    private int continueVote = 0;

    private int totalPlayer = 4;
    private int requiredVote = 3;

    public GameObject pauseMenu;
    public float pauseDuration;

    private bool isPaused = false;

    // ��ǥ �ʱ�ȭ
    private void ResetVote()
    {
        pauseVote = 0;
        unpauseVote = 0;
        quitVote = 0;
        continueVote = 0;
    }

    // �Ͻ����� ��ǥ ó��
    public void VotePause()
    {
        if (isPaused) return;
        pauseVote++;
        CheckPauseVotes();
    }

    public void VoteUnpause()
    {
        if (isPaused) return;
        unpauseVote++;
        CheckPauseVotes();
    }

    // ���� ���� ��ǥ ó��
    public void VoteQuit()
    {
        quitVote++;
        CheckQuitVotes();
    }

    public void VoteContinue()
    {
        continueVote++;
        CheckQuitVotes();
    }

    // �Ͻ����� ��ǥ ��� Ȯ��
    private void CheckPauseVotes()
    {
        if (pauseVote >= requiredVote)
        {
            StartCoroutine(PauseGame());
        }
        else if (unpauseVote >= requiredVote)
        {
            ResetVote();
        }
    }

    // ���� ���� ��ǥ ��� Ȯ��
    private void CheckQuitVotes()
    {
        if (quitVote >= requiredVote)
        {
            SceneManager.LoadScene("StartScene");
        }
        else if (continueVote >= requiredVote)
        {
            ResetVote();
        }
    }

    // �Ͻ����� ó��
    private IEnumerator PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        yield return new WaitForSecondsRealtime(pauseDuration);
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        ResetVote();
        isPaused = false;
    }
}
