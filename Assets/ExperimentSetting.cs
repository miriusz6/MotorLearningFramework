using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultExperimentSetting : MonoBehaviour
{

    public Vector3 translationDegrees;

    public Vector3 translationOffset = new Vector3(1,1,1);
    

    public bool sleeveVisible;
    public bool handVisible;

    public bool freezeTranslationAxisX;
    public bool freezeTranslationAxisY;
    public bool freezeTranslationAxisZ;

    public bool freezeRotationX;
    public bool freezeRotationY;
    public bool freezeRotationZ;

    // for now support only for material change,
    // not mesh but can be added
    public Material HandMaterial;
    public Material SleeveMaterial;
    public Material ConeMaterial;
    public Material TableMaterial;
    public Material WallMaterial;
    public Material CeilingMaterial;
    public Material FloorMaterial;

    // TODO:
    // visual cues
    // audio cues
    // haptic cues

    public string toDisplayBeforeEachTrial = "";
    public string toDisplayAfterEachTrial = "";


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
