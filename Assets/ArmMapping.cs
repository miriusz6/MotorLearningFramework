using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMapping : MonoBehaviour
{
    //public GameObject trackingSpace;
    //public bool freezeRotationX;
    //public bool freezeRotationY;
    //public bool freezeRotationZ;

    //public bool freezeTranslationAxisX;
    //public bool freezeTranslationAxisY;
    //public bool freezeTranslationAxisZ;

    private Vector3 translationDegrees;

    private Vector3 translationOffset;

    private Vector3 rotationFreeze;

    private Vector3 translationFreeze;

    private Transform trackingSpaceTrans;

    private Vector3 prevTracPos;

    private Vector3 initPos;

    


    //
    public void Configure(Transform trackingSpace, (bool x, bool y ,bool z) freezeRotation,
        (bool x, bool y, bool z) freezeTranslation, Vector3 translationDegrees, Vector3 translationOffset)
    {
        trackingSpaceTrans = trackingSpace;
        initPos = this.transform.position;
        prevTracPos = trackingSpaceTrans.position;

        // create Vector3 rotationFreeze for cleaner calculations
        // of rotation in each update 
        float rotationFreezeX = 1;
        float rotationFreezeY = 1;
        float rotationFreezeZ = 1;
        if (freezeRotation.x) { rotationFreezeX = 0; }
        if (freezeRotation.y) { rotationFreezeY = 0; }
        if (freezeRotation.z) { rotationFreezeZ = 0; }
        rotationFreeze = new Vector3(rotationFreezeX, rotationFreezeY, rotationFreezeZ);

        // create Vector3 translationFreeze for cleaner calculations
        // of translation in each update
        float translationFreezeX = 1;
        float translationFreezeY = 1;
        float translationFreezeZ = 1;
        if (freezeTranslation.x) { translationFreezeX = 0; }
        if (freezeTranslation.y) { translationFreezeY = 0; }
        if (freezeTranslation.z) { translationFreezeZ = 0; }
        translationFreeze = new Vector3(translationFreezeX, translationFreezeY, translationFreezeZ);

        this.translationDegrees = translationDegrees;
        this.translationOffset = translationOffset;


    }

    // Start is called before the first frame update
    void Start()
    {
        //trackingSpaceTrans = trackingSpace.transform;
        //initPos = this.transform.position;
        //prevTracPos = trackingSpaceTrans.position;

        //// create Vector3 rotationFreeze for cleaner calculations
        //// of rotation in each update 
        //float rotationFreezeX = 1;
        //float rotationFreezeY = 1;
        //float rotationFreezeZ = 1;
        //if (freezeRotationX) { rotationFreezeX = 0; }
        //if (freezeRotationY) { rotationFreezeY = 0; }
        //if (freezeRotationZ) { rotationFreezeZ = 0; }
        //rotationFreeze = new Vector3(rotationFreezeX, rotationFreezeY, rotationFreezeZ);


        //// create Vector3 translationFreeze for cleaner calculations
        //// of translation in each update
        //float translationFreezeX = 1;
        //float translationFreezeY = 1;
        //float translationFreezeZ = 1;
        //if (freezeTranslationAxisX) { translationFreezeX = 0; }
        //if (freezeTranslationAxisY) { translationFreezeY = 0; }
        //if (freezeTranslationAxisZ) { translationFreezeZ = 0; }
        //translationFreeze = new Vector3(translationFreezeX, translationFreezeY, translationFreezeZ);

    }

    // Update is called once per frame
    void Update()
    {
        ApplyRotation();
        ApplyTranslation();
    }

    void ApplyTranslation()
    {
        Vector3 currTrackPos = trackingSpaceTrans.position;
        float dist = Vector3.Distance(currTrackPos, prevTracPos);
        // check if the controller moved at least the treshold to minimize rounding errors
        if (dist > 0)
        {
            // direction of controllers movement
            Vector3 tracDir = currTrackPos - prevTracPos;
            tracDir = tracDir.normalized;
            // apply translation rotation i.e. change arm movement direction w.r.t. the controller
            Vector3 armDir = Quaternion.Euler(translationDegrees.x, translationDegrees.y, translationDegrees.z) * tracDir;
            armDir *= dist;
            // move the arm by 'dist' in the calculated direction 
            // dont move along the freezed axis
            // scale by translation offset
            this.transform.position += new Vector3(
                armDir.x * translationFreeze.x * translationOffset.x,
                armDir.y * translationFreeze.y * translationOffset.y,
                armDir.z * translationFreeze.z * translationOffset.z);
            prevTracPos = trackingSpaceTrans.position;
        }
    }
    void ApplyRotation()
    {
        // tracking space rotation in World Coords
        Vector3 trackRot = trackingSpaceTrans.rotation.eulerAngles;
        // apply (optional) rotation freeze setting
        Vector3 new_rot = new Vector3(trackRot.x * rotationFreeze.x,
            trackRot.y * rotationFreeze.y, trackRot.z * rotationFreeze.z);
        // set arms rotation to the new rotation
        this.transform.rotation = Quaternion.Euler(new_rot);
    }
}
