using Assets.Scripts.States;
using UnityEngine;

public class GameManager : MonoBehaviour 
{
    public IGameState CurrentState { get; private set; }

    public Texture2D StartScreenBackground;
    public Texture2D MenuScreenBackground;

    public void Start ()
	{
	    CurrentState = new StartScreenState(this);
	}

    public void Update ()
	{
        if (CurrentState == null) return;
        CurrentState.Update();
	}

    public void OnGUI()
    {
        if (CurrentState == null) return;
        CurrentState.Render();
        CurrentState.OnGUI();
    }

    public void SwitchState(IGameState newState)
    {
        CurrentState = newState;
    }
}


