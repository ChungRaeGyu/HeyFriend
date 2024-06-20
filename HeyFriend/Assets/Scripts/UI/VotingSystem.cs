using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class VotingSystem : MonoBehaviourPunCallbacks
{
    private int pauseVote = 0; // �Ͻ����� ������ǥ��
    private int unpauseVote = 0; // �Ͻ����� �ݴ���ǥ��
    private int quitVote = 0; // �������� ������ǥ��
    private int continueVote = 0; // �������� �ݴ���ǥ��

    private int totalPlayer = 4; // �ִ� �ο�
    private int requiredVote = 3; // ���� �ο�

    public GameObject pauseMenu; // �Ͻ����� �޴�
    public GameObject pausevotingPanel; // �Ͻ����� ��ǥ �г�
    public GameObject quitVotingPanel; // �������� ��ǥ �г�
    public float pauseDuration; // �Ͻ������Ǵ� �ð�

    private bool isPaused = false; // �Ͻ����� bool��
    private bool isVoting = false; // ��ǥ���� bool��

    private HashSet<int> votedPlayers = new HashSet<int>(); // ��ǥ�� �÷��̾� ���

    private enum VoteType { None, Pause, Quit } // ��ǥ Ÿ�԰�
    private VoteType currentVoteType = VoteType.None; // ���� ���� ���� ��ǥ ����

    private void ResetVote() // ��ǥ ���� �Լ�
    {
        pauseVote = 0;
        unpauseVote = 0;
        quitVote = 0;
        continueVote = 0;
        isVoting = false;
        currentVoteType = VoteType.None;
        votedPlayers.Clear(); // ��ǥ�ο� �ʱ�ȭ
    }

    public void VotePause() // �Ͻ����� ������ǥ
    {
        if (isPaused || isVoting || votedPlayers.Contains(PhotonNetwork.LocalPlayer.ActorNumber)) return;
        // ��ǥ�� ���� ���̰ų� �Ͻ����� ���°ų� ������ ���Ե� ��ǥ �ο��̸� �����ض�
        StartVoting(VoteType.Pause); // ��ǥ�� �����ض� (��ǥ���� �Ͻ�����)
        votedPlayers.Add(PhotonNetwork.LocalPlayer.ActorNumber); // �������� ��ǥ �ο��� ���ض�
        photonView.RPC("RPC_VotePause", RpcTarget.All); // RPC_VotePause �� RPC�� ���Ե� ��ο��� �߻����Ѷ�
    }

    public void VoteUnpause() // �Ͻ����� �ݴ���ǥ
    {
        if (isPaused || isVoting || votedPlayers.Contains(PhotonNetwork.LocalPlayer.ActorNumber)) return;
        // ��ǥ�� ���� ���̰ų� �Ͻ����� ���°ų� ������ ���Ե� ��ǥ �ο��̸� �����ض�
        StartVoting(VoteType.Pause); // ��ǥ�� �����ض� (��ǥ���� �Ͻ�����)
        votedPlayers.Add(PhotonNetwork.LocalPlayer.ActorNumber); // �������� ��ǥ �ο��� ���ض�
        photonView.RPC("RPC_VoteUnpause", RpcTarget.All); // RPC_VoteUnpause �� RPC�� ���Ե� ��ο��� �߻����Ѷ�
    }

    public void VoteQuit() // �������� ������ǥ
    {
        if (isVoting || votedPlayers.Contains(PhotonNetwork.LocalPlayer.ActorNumber)) return;
        // ��ǥ�� ���� ���̰ų� ������ ���Ե� ��ǥ �ο��̸� �����ض�
        StartVoting(VoteType.Quit); // ��ǥ�� �����ض� (��ǥ���� ��������)
        votedPlayers.Add(PhotonNetwork.LocalPlayer.ActorNumber); // �������� ��ǥ �ο��� ���ض�
        photonView.RPC("RPC_VoteQuit", RpcTarget.All); // RPC_VoteQuit �� RPC�� ���Ե� ��ο��� �߻����Ѷ�
    }

    public void VoteContinue()
    {
        if (isVoting || votedPlayers.Contains(PhotonNetwork.LocalPlayer.ActorNumber)) return;
        // ��ǥ�� ���� ���̰ų� ������ ���Ե� ��ǥ �ο��̸� �����ض�
        StartVoting(VoteType.Quit); // ��ǥ�� �����ض� (��ǥ���� ��������)
        votedPlayers.Add(PhotonNetwork.LocalPlayer.ActorNumber); // �������� ��ǥ �ο��� ���ض�
        photonView.RPC("RPC_VoteContinue", RpcTarget.All); // RPC_VoteContinue �� RPC�� ���Ե� ��ο��� �߻����Ѷ�
    }

    [PunRPC]
    private void RPC_VotePause() // RPC �Ͻ����� ������ǥ
    {
        pauseVote++; // �Ͻ����� ���� ��ǥ�� ���ض�
        CheckPauseVotes(); // �ش� �Լ� ����
    }

    [PunRPC]
    private void RPC_VoteUnpause() // RPC �Ͻ����� �ݴ���ǥ
    {
        unpauseVote++; // �Ͻ����� �ݴ� ��ǥ�� ���ض�
        CheckPauseVotes(); // �ش� �Լ� ����
    }

    [PunRPC]
    private void RPC_VoteQuit() // RPC �������� ������ǥ
    {
        quitVote++; // �������� ���� ��ǥ�� ���ض�
        CheckQuitVotes(); // �ش� �Լ� ����
    }

    [PunRPC]
    private void RPC_VoteContinue() // RPC �������� �ݴ���ǥ
    {
        continueVote++; // �������� �ݴ� ��ǥ�� ���ض�
        CheckQuitVotes(); // �ش� �Լ� ����
    }

    private void CheckPauseVotes() // �Ͻ����� ��ǥ Ȯ��
    {
        if (pauseVote >= requiredVote) // �Ͻ����� ������ǥ�� ���ݼ����� ���ų� ������
        {
            StartCoroutine(PauseGame()); // ���� �Ͻ����� �ڷ�ƾ�� �����ض�
        }
        else if (unpauseVote >= requiredVote) // �Ͻ����� �ݴ���ǥ�� ���ݼ����� ���ų� ������
        {
            ResetVote(); // ��ǥ�� �����ض�
            CloseVotingPanels(); // ��ǥ �г��� �ݾƶ�
        }
    }

    private void CheckQuitVotes() // �������� ��ǥ Ȯ��
    {
        if (quitVote >= requiredVote) // �������� ������ǥ�� ���ݼ����� ���ų� ������
        {
            SceneManager.LoadScene("StartScene"); // ��ŸƮ���� �ҷ��Ͷ�
        }
        else if (continueVote >= requiredVote) // �������� �ݴ���ǥ�� ���ݼ����� ���ų� ������
        {
            ResetVote(); // ��ǥ�� �����ض�
            CloseVotingPanels(); // ��ǥ �г��� �ݾƶ�
        }
    }

    private IEnumerator PauseGame() // �ڷ�ƾ �Ͻ��������� ����
    {
        isVoting = true; // ��ǥ���� �Ѷ�
        isPaused = true; // �Ͻ����� �Ѷ�
        Time.timeScale = 0f; // �ð��� �����
        pauseMenu.SetActive(true); // �Ͻ����� �޴��� �Ѷ�
        CloseVotingPanels(); // ��ǥ �г��� �ݾƶ�
        yield return new WaitForSecondsRealtime(pauseDuration); // ���� �ð��� �������� ���� �ð����� �Ͻ����� �ð��� ���� �ض�
        Time.timeScale = 1f; // �������� �ٽ� �ð��� �帣�� �ض�
        pauseMenu.SetActive(false); // �Ͻ����� �޴��� �ݾƶ�
        ResetVote(); // ��ǥ�� �����ض�
        isPaused = false; // �Ͻ����� Ǯ���
    }

    private void StartVoting(VoteType voteType) // ��ǥ����
    {
        if (currentVoteType == VoteType.None) // ���� ��ǥ���� �ƹ��͵� ���ٸ�
        {
            currentVoteType = voteType; // ���� ��ǥ���� voteType ������ ����
            isVoting = true; // ��ǥ���� �Ѷ�
            OpenVotingPanel(voteType); // voteType �������� ����� �г��� �Ѷ�
        }
    }

    private void OpenVotingPanel(VoteType voteType) // ��ǥ �г� ����
    {
        switch (voteType) // voteType �� ���̽�
        {
            case VoteType.Pause: // �Ͻ����� ��ǥ
                pausevotingPanel.SetActive(true); // �Ͻ����� ��ǥ �г� �Ѷ�
                break;
            case VoteType.Quit: // �������� ��ǥ
                quitVotingPanel.SetActive(true); // �������� ��ǥ �г� �Ѷ�
                break;
        }
    }

    private void CloseVotingPanels() // ��ǥ �г� �ݱ�
    {
        pausevotingPanel.SetActive(false); // �Ͻ����� ��ǥ �г� ����
        quitVotingPanel.SetActive(false); // �������� ��ǥ �г� ����
    }
}
