using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections.ObjectModel;

//takes inputs generated by player input handler, and updates the input management component of the associated player entity.
//Essentially translates GameObject input data to ECS input data, which can then be used by other systems to allow player to interact with the world.
[UpdateInGroup(typeof(InputSystemGroup))]
public class PlayerInputListenerSystem : SystemBase
{

    public struct PlayerStateInput
    {
        public float2 moveDirection;
    };

    static List<PlayerInputHandler> inputHandlers = new List<PlayerInputHandler>();

    static public void addInputHandler(PlayerInputHandler handler)
    {
        inputHandlers.Add(handler);
    }

    static public void removeInputHandler(PlayerInputHandler handler)
    {
        inputHandlers.Remove(handler);
    }

    static public void clearInputHandlerActions()
    {
        foreach (var handler in inputHandlers)
        {
            handler.clearActions();
        }
    }

    static void applyActions(ref PlayerInputComponent playerInput, in NativeList<InputActionData> actions)
    {
        //reset the flag since we should be running in a new frame now so it's no longer pressed this single frame, unless he presses it again.
        //in which case it will be handleed 
        playerInput.isJumpKeyPressedThisFrame = false;

        if(actions.Length == 0) {
            return;
        }

        //have to sort because we want to respond to events in the order that they occured
        actions.Sort(new InputActionDataComparer());


        foreach (var action in actions)
        {
            if (action.action == Action.Jump)
            {
                if (action.phase == UnityEngine.InputSystem.InputActionPhase.Performed)
                {
                    playerInput.isJumpKeyPressedThisFrame = true;
                    playerInput.isJumpKeyHeld = true;
                    playerInput.jumpKeyPressStartTime = action.timeActionOccured;
                }
                else
                if (action.phase == UnityEngine.InputSystem.InputActionPhase.Canceled)
                {
                    playerInput.isJumpKeyPressedThisFrame = false;
                    playerInput.isJumpKeyHeld = false;
                }
            }
        }
    }


    protected override void OnUpdate()
    {

        //store the player inputs for each player.
        //Store them separately as state input and action event, because events are an array and arrays can't be stored inside an array
        NativeHashMap<int, PlayerStateInput> currentPlayerInputStates =
            new NativeHashMap<int, PlayerStateInput>(PlayerInput.all.Count, Allocator.TempJob);

        NativeMultiHashMap<int, InputActionData> currentPlayerActions = new NativeMultiHashMap<int, InputActionData>(PlayerInput.all.Count * 20, Allocator.TempJob);


        foreach (PlayerInputHandler handler in inputHandlers)
        {
            currentPlayerInputStates[handler.parameters.playerInputIndex] = new PlayerStateInput { moveDirection = handler.parameters.moveDirection };

            foreach (var actionData in handler.parameters.jumpActionHistory)
            {

                currentPlayerActions.Add(handler.parameters.playerInputIndex, actionData);
            }
        }

        Entities.WithReadOnly(currentPlayerActions)
            .WithReadOnly(currentPlayerInputStates)
            .WithAll<TagPlayer>()
            .ForEach((ref PlayerInputComponent playerInput, in PlayerInputIndexComponent playerInputIndex) =>
        {
            int index = playerInputIndex.playerInputIndex;

            playerInput.inputDirection = float2.zero;

            if(currentPlayerInputStates.ContainsKey(index)) {

                PlayerStateInput stateInput = currentPlayerInputStates[index];
                playerInput.inputDirection = stateInput.moveDirection;
            }

            //DONT skip even if there are no actions.
            //this is because even if ther eare no actions, there might still be an input listenere for this player, it's just that the array will be empty for this player
            //apply actions will handle the case with no actions so it's okay to allow this code to run.
            NativeList<InputActionData> actions = new NativeList<InputActionData>(currentPlayerActions.CountValuesForKey(index), Allocator.Temp);
            var actionsEnumerator = currentPlayerActions.GetValuesForKey(index);

            foreach (var action in actionsEnumerator)
            {
                actions.Add(action);
            }

            applyActions(ref playerInput, actions);

            actions.Dispose();
        })
        .WithDisposeOnCompletion(currentPlayerActions)
        .WithDisposeOnCompletion(currentPlayerInputStates)
        .ScheduleParallel();

        clearInputHandlerActions();
    }
}
