using Unity.Mathematics;
using UnityEngine;

public class UnderwaterCamera : MonoBehaviour
{
    private Camera myCamera;
    private float cameraPos;
    // Start is called before the first frame update
    void Start()
    {
        myCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        cameraPos = myCamera.transform.position.y;
        if (cameraPos < 0.45)
        {
            RenderSettings.fogEndDistance = 60*math.exp((0.2f)/(0.5f - cameraPos));
            RenderSettings.fog = true;
        }
        else
        {
            RenderSettings.fog = false;
        }
    }
}
