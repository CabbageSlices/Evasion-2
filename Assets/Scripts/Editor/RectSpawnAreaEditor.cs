using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

//how to always draw gizmos tutorial
//https://learn.unity.com/tutorial/editor-scripting#5c7f8528edbc2a002053b5fb

//render the spawn area box bounds inside scene view, and make it easy to edit
[CustomEditor(typeof(RectSpawnAreaAuthoring))]
public class RectSpawnAreaEditor : Editor
{
    private BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    void OnEnable()
    {
        boundsHandle.axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
        boundsHandle.midpointHandleSizeFunction = (Vector3 position) => {
            return HandleUtility.GetHandleSize(position) / 11;
        };

    }

    //draw a rect to display the spawner area
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    static void DrawAreaBoundsDisplayGizmo(RectSpawnAreaAuthoring spawnArea, GizmoType gizmoType)
    {
        Vector3 scale = spawnArea.transform.lossyScale;
        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.Min(scale.x, scale.y) < 5 ? 10 : 25;
        style.fixedWidth = 250;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        
        Handles.Label(spawnArea.transform.position + new Vector3(0, style.fontSize / 10, -10), "Rect Spawn Area", style);

        Gizmos.color = spawnArea.spawnAreaColor;

        Gizmos.DrawCube(spawnArea.transform.position, spawnArea.transform.localScale);
    }

    public void OnSceneGUI()
    {
        RectSpawnAreaAuthoring area = (target as RectSpawnAreaAuthoring);

        Vector3 pos = area.transform.position;
        Vector3 scale = area.transform.localScale;
        
        //first handle the translation of the spawner area
        EditorGUI.BeginChangeCheck();

        //controls the position of the spawner. used ot compute the translation of the spawner center.
        Vector3 snap = Vector3.one * 0.25f;

        //box is too small in one direction, need to make move handle smaller 
        float sizeOfMoveHandle = HandleUtility.GetHandleSize(pos) / 3;

        //blue to move
        Handles.color = Color.blue;
        Vector3 newPositionFromMoveHandle = Handles.FreeMoveHandle(pos, Quaternion.identity, sizeOfMoveHandle, snap, Handles.CubeHandleCap);

        if(EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(area.transform, "Changed Rect Spawn Area Location");
            area.transform.position = newPositionFromMoveHandle;
        }



        //hnandle the spawn area sizing
        Handles.color = new Color(0.75f, 1.0f, 0.0f, 1.0f);
        pos = area.transform.position; //update position which dmight be changed due to translation

        boundsHandle.center = pos;
        boundsHandle.size = scale;

        boundsHandle.center = pos;
        boundsHandle.size = scale;

        EditorGUI.BeginChangeCheck();
        //controls the size of the spawn area
        boundsHandle.DrawHandle();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(area.transform, "Changed Rect Spawn Area Size");
            area.transform.position = boundsHandle.center;
            area.transform.rotation = Quaternion.identity;
            area.transform.localScale = boundsHandle.size;
        }
    }
}
