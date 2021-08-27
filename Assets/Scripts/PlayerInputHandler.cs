using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


public enum Action
{
    Jump
};

public struct InputActionData
{

    //every time the inputs are cleared, this number will reset.
    public int actionNumber;

    public InputActionPhase phase;
    public Action action;

    public double timeActionOccured;
};

[System.Serializable]
public struct InputParameters
{
    public int playerInputIndex;

    public Vector2 moveDirection;
    public List<InputActionData> jumpActionHistory;
};

public struct InputActionDataComparer : IComparer<InputActionData> 
{
    public int Compare(InputActionData a, InputActionData b) 
    {
        return a.actionNumber - b.actionNumber;
    }
}

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    public InputParameters parameters = new InputParameters { jumpActionHistory = new List<InputActionData>() };

    PlayerInput input;

    int numEventsSinceLastCleared = 0;

    public void clearActions()
    {
        parameters.jumpActionHistory.Clear();
    }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        parameters.playerInputIndex = input.playerIndex;

        PlayerInputListenerSystem.addInputHandler(this);
    }

    private void OnDestroy() {
        PlayerInputListenerSystem.removeInputHandler(this);
    }

    // Update is called once per frame
    void Update()
    {
        // if(jumpActionTrace.count != 0 || moveDirection.sqrMagnitude > 0.05) {

        //     Debug.Log(moveDirection);
        //     Debug.Log(jumpActionTrace.count);
        // }
    }

    //only called when value is changed or something
    public void Move(InputAction.CallbackContext context)
    {
        // Debug.Log("performed " + context.performed);
        // Debug.Log("cancelled " + context.canceled);
        // Debug.Log(context.ReadValue<Vector2>());
        // Debug.Log("phase " + context.phase);

        parameters.moveDirection = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        parameters.jumpActionHistory.Add(new InputActionData { phase = context.phase, action = Action.Jump, timeActionOccured = context.time, actionNumber = ++numEventsSinceLastCleared });
    }
}
