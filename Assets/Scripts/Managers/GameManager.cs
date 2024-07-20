using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharaterStats playerStats;

    private CinemachineFreeLook followCamera;

    List<IEndGameObserver> endGameObserver = new List<IEndGameObserver>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void RigisterPlayer(CharaterStats player) 
    {
        playerStats = player;

        followCamera = FindObjectOfType<CinemachineFreeLook>();

        if (followCamera != null )
        {
            followCamera.Follow = playerStats.transform.GetChild(2);
            followCamera.LookAt = playerStats.transform.GetChild(2);
        }
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObserver.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObserver.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (var observer in endGameObserver)
        {
            observer.EndNotify();
        }
    }

    public Transform GetEnterance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestination>())
        {
            if(item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}
