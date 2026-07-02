using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MeshBoundMod : MonoBehaviour
{
    public bool isWorld = false;
    public Renderer _renderer = null;
    public Vector3 sizeMultiplier = Vector3.one;
    public Vector3 sizeAddend = Vector3.zero;
    public Vector3 centerMultiplier = Vector3.one;
    public Vector3 centerAddend = Vector3.zero;

    [Space]
    public bool debug = false;

    void OnDrawGizmos()
    {
        if(debug){
            if(_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (isWorld)
            {
                Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.size);
            }else{
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(_renderer.localBounds.center, _renderer.localBounds.size);
            }
        }
    }
    void Start()
    {
        UpdateBounds();
    }

    void OnValidate()
    {
        UpdateBounds();
    }


#if UNITY_EDITOR
    [ContextMenu("Make Absolute")]
    public void MakeAbsolute()
    {
        Undo.RecordObject(this, "Make absolute");

        var bound = isWorld? _renderer.bounds: _renderer.localBounds;
        
        sizeMultiplier = Vector3.zero;
        sizeAddend = bound.size;
        centerMultiplier = Vector3.zero;
        centerAddend = bound.center;

    }
#endif

    void Reset()
    {
        if(_renderer == null)
            _renderer = GetComponent<Renderer>();
    }

    [ContextMenu("Update Bounds")]
    public void UpdateBounds(){
        if(_renderer == null)
            _renderer = GetComponent<Renderer>();
        _renderer.ResetLocalBounds();
        _renderer.ResetBounds();
        
        var bounds = isWorld? _renderer.bounds: _renderer.localBounds;
        bounds.size = Vector3.Scale(bounds.size, sizeMultiplier) + sizeAddend;
        bounds.center = Vector3.Scale(bounds.center, centerMultiplier) + centerAddend;

        if(isWorld)
            _renderer.bounds = bounds;
        else
            _renderer.localBounds = bounds;
    }
}
