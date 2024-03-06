using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager instance;

    public List<UnitAIBase> deck;
    public List<UnitAIBase> units;

    public int selectionInt = -100;

   // bool debugArrowsBool = false;
    public enum TurnState
    {
        Deployment,
        Movement,
        Enemy
    }

    public TurnState currentTurnState;

    void Awake()
    {
        instance = this;
    }

    // Start is called before sthe first frame update
    void Start()
    {
        currentTurnState = TurnState.Deployment;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTurnState == TurnState.Deployment && Input.GetMouseButtonDown(0))
        {
            PlayCard();
        }

        if (Input.GetMouseButtonDown (0))
        {
            switch(currentTurnState)
            {
                case TurnState.Deployment:

                    break;

                case TurnState.Movement:

                    break;
            }
        }
    }

    public void StartTurn()
    {
        currentTurnState = TurnState.Deployment;
    }

    public void DrawCard(int input = 0)
    {
        UnitSO newSO;

        switch (input)
        {
            case -3:

                newSO = Catalouge.instance.debugAirUnit_SO;
                break;

            case -2:

                newSO = Catalouge.instance.debugArmouredUnit_SO;
                break;

            case -1:

                newSO = Catalouge.instance.debugGroundUnit_SO;
                break;

            default:

                int selection = Random.Range(0, deck.Count);
                break;
        }
    }

    public void PlayCard()
    {

        if (selectionInt < -3)
        {
            return;
        }

        // Find Position to play


        //GameManager.instance.SpawnUnit(selectionInt, spawnPos);


        switch (selectionInt)
        {
            case -1:

                break;
        }
    }


}
