using UnityEngine;


[ExecuteAlways]
public class WindowShaderBinder : MonoBehaviour
{
    static public WindowShaderBinder instance;
    [Header("Size (in CM)")]
    public Vector3 size;
    public Vector3 restSize;
    [Space]
    public string originProp = "_WindowOrigin";
    public string sizeProp = "_WindowSize";
    public string restSizeProp = "_WindowRestSize";
    int originPropID;
    int sizePropID;
    int restSizePropID;


    private void OnValidate()
    {
        Init();
        UpdateShader();
    }

    void Init()
    {
        originPropID = Shader.PropertyToID(originProp);
        sizePropID = Shader.PropertyToID(sizeProp);
        restSizePropID = Shader.PropertyToID(restSizeProp);
    }
    void Start()
    {
        Init();
        instance = this;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, size/100);
    }

    public void UpdateShader()
    {
        Shader.SetGlobalVector(originPropID, transform.position);
        Shader.SetGlobalVector(sizePropID, size / 100);
        Shader.SetGlobalVector(restSizePropID, restSize / 100);
    }

    void Update()
    {
        UpdateShader();
    }
}
