using UnityEngine;

[CreateAssetMenu(fileName = "WindowSet", menuName = "Scriptable Objects/WindowSet")]
public class WindowSet : ScriptableObject
{
    public GameObject frameModelDual;
    public GameObject doorModelL;
    public GameObject doorModelR;

    public GameObject frameModelSingular;
    public GameObject doorModelSingular;

    public GameObject frameModelTriple;
    public GameObject doorModelTriple;
}
