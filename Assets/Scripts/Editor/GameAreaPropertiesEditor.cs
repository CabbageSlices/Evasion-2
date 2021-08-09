using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

//how to always draw gizmos tutorial
//https://learn.unity.com/tutorial/editor-scripting#5c7f8528edbc2a002053b5fb

//render the spawn area box bounds inside scene view, and make it easy to edit
[CustomEditor(typeof(GameAreaProperties))]
public class GameAreaPropertiesEditor : Editor
{
    private BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    void OnEnable()
    {
        boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
        boundsHandle.midpointHandleSizeFunction = (Vector3 position) =>
        {
            return HandleUtility.GetHandleSize(position) / 11;
        };

    }

    //draw a rect to display the spawner area
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    static void DrawAreaBoundsDisplayGizmo(GameAreaProperties properties, GizmoType gizmoType)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.fixedWidth = 250;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        Handles.Label(properties.transform.position, "Game Area", style);

        Gizmos.color = new Color(0.4f, 0.8f, 0, 0.3f);

        Gizmos.DrawCube(properties.transform.position, new Vector3(properties.gameplayArea.size.x, properties.gameplayArea.size.y, 1));
    }

    public void OnSceneGUI()
    {
        //rect to specify the gameplay area
        GameAreaProperties properties = (target as GameAreaProperties);

        Rect gameArea = properties.gameplayArea;

        //hnandle the spawn area sizing
        Handles.color = new Color(0.75f, 1.0f, 0.0f, 1.0f);

        boundsHandle.center = properties.transform.position;
        boundsHandle.size = new Vector3(gameArea.size.x, gameArea.size.y, 1);

        EditorGUI.BeginChangeCheck();
        //controls the size of the spawn area
        boundsHandle.DrawHandle();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(properties, "Changed Game Area Bounds");

            Rect newGameArea = gameArea;

            newGameArea.center = new Vector2(boundsHandle.center.x, boundsHandle.center.y);
            newGameArea.size = new Vector2(boundsHandle.size.x, boundsHandle.size.y);
            properties.gameplayArea = newGameArea;

            
            Undo.RecordObject(properties.transform, "Changed Game Area Bounds position");
            properties.transform.position = boundsHandle.center;
        }
    }
}
