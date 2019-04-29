using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Prediction2 : NetworkBehaviour
{
    public static bool isOn;

    public class Settings
    {
        const float PlayerMoveSpeed = 3.0f;
        public const float PlayerLerpEasing = 0.5f;
        public const float PlayerLerpSpacing = 1.0f;
    }

    public class PlayerInput
    {
        public float x;
        public float y;
    }

    public struct PlayerState
    {
        public int timestamp;
        public Vector2 position;
    }

    private PlayerState predictedState;
    private List<PlayerInput> pendingMoves;

    [SyncVar(hook = "OnServerStateChanged")]
    public PlayerState state;

    // Description: Start function is called at the beginning of the script
    //              used to initialise the variables
    void Start()
    {
        if (!isOn)
        {
            enabled = false;
        }

        if (isLocalPlayer)
        {
            pendingMoves = new List<PlayerInput>();
        }
    }

    // Description: Awake function is called at the beginning of the script
    //              used to initialise the starting state
    void Awake()
    {
        InitState();
    }

    // Description: Initialises the starting state
    private void InitState()
    {
        state = new PlayerState
        {
            timestamp = 0,
            position = Vector2.zero
        };
    }

    // Description: Update is called once per frame and used as the main
    //              game loop to call the functions used for movement and
    //              prediction. For effiecieny functions are only called
    //              when the player inputs something
    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            PlayerInput playerInput = GetPlayerInput();

            if (playerInput != null)
            {
                pendingMoves.Add(playerInput);
                UpdatePredictedState();
                CmdMoveOnServer(playerInput);
            }
        }
        SyncState();
    }

    // Description: Called in the update function every frame. This function syncs the server and client position
    //              using linear interpolation
    private void SyncState()
    {
        if (isServer)
        {
            transform.position = state.position;
            return;
        }

        PlayerState stateToRender = isLocalPlayer ? predictedState : state;

        transform.position = Vector2.Lerp(transform.position,
            stateToRender.position * Settings.PlayerLerpSpacing,
            Settings.PlayerLerpEasing);
    }

    // Description: Called in the update function every frame. Collects player inputs and store them
    //              into a list
    private PlayerInput GetPlayerInput()
    {
        PlayerInput playerInput = new PlayerInput();
        playerInput.x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        playerInput.y = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;
        return playerInput;
    }

    // Description: Called everytime theres a new player input. Stores player input and adds the new
    //              position to the new player state
    public PlayerState ProcessPlayerInput(PlayerState previous, PlayerInput playerInput)
    {
        Vector2 newPosition = previous.position;

        if (playerInput.x != 0 || playerInput.y != 0)
        {
            Vector2 temp;
            temp.x = playerInput.x;
            temp.y = playerInput.y;
            newPosition = previous.position + temp;
        }

        return new PlayerState
        {
            timestamp = previous.timestamp + 1,
            position = newPosition
        };
    }

    // Description: Called in the update function every frame if there is a new player input. 
    //              Sets and sends the new state to the server 
    [Command]
    void CmdMoveOnServer(PlayerInput playerInput)
    {
        state = ProcessPlayerInput(state, playerInput);
    }

    // Description: Called everytime there is a new state. Updates the current state. Removes all outdated player
    //              inputs then calls the prediction function
    public void OnServerStateChanged(PlayerState newState)
    {
        state = newState;
        if (pendingMoves != null)
        {
            while (pendingMoves.Count > (predictedState.timestamp - state.timestamp))
            {
                pendingMoves.RemoveAt(0);
            }
            UpdatePredictedState();
        }
    }

    // Description: Called if there are pending moves that need to be applied to the player. Updates the predicted state to
    //              equal the next move in the list. This is applied after every server update.
    public void UpdatePredictedState()
    {
        predictedState = state;
        foreach (PlayerInput playerInput in pendingMoves)
        {
            predictedState = ProcessPlayerInput(predictedState, playerInput);
        }
    }

    // Description: Overrides the OnStartLocalPlayer fucntion which is locating in unity
    //              network behaviour. Changes the colour of your player black leaving all
    //              other players white
    public override void OnStartLocalPlayer()
    {
        GetComponent<SpriteRenderer>().material.SetColor("_EmissionColor", Color.black);
    }
}
