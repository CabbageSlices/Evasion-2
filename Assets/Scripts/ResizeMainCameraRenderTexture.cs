using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//force the main camera render texture to be the same size as the screen resolution
//that way the scene render isn't messed up
[ExecuteInEditMode]
public class ResizeMainCameraRenderTexture : MonoBehaviour
{

    private Vector2Int screenResolution;
    
    [SerializeField]
    Camera mainCamera;

    private void Awake() {
        screenResolution = new Vector2Int(Screen.width, Screen.height);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!mainCamera) {
            mainCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(screenResolution.x != Screen.width || screenResolution.y != Screen.height) {
            screenResolution.x = Screen.width;
            screenResolution.y = Screen.height;
            
            //resolution changed
            RenderTexture texture = mainCamera.targetTexture;
            texture.Release();
            texture.width = Screen.width;
            texture.height = Screen.height;
            texture.Create();
        }
    }
}
