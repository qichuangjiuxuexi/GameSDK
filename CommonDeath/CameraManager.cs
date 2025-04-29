using AppBase.Module;
using UnityEngine;

public class CameraManager : ModuleBase
{
    public Camera MainCamera { get; private set; }
    public Camera UICamera { get; private set; }

    protected override void OnInit()
    {
        UICamera = GameObject.Find("UICamera")?.GetComponent<Camera>();
        if (UICamera != null)
        {
            GameObject.DontDestroyOnLoad(UICamera.gameObject);
        }
        else
        {
            Debug.LogError("ui camera doesn't exist!");
        }
        
        MainCamera = GameObject.Find("MainCamera")?.GetComponent<Camera>();
        if (MainCamera != null)
        {
            GameObject.DontDestroyOnLoad(MainCamera.gameObject);
        }
    }
}