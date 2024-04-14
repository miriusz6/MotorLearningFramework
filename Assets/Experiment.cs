using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Experiment : MonoBehaviour
{
    public Canvas MsgDisplay;

    public DefaultExperimentSetting defaultExperimentSetting;
    public List<ExperimentSetting> experimentSettings;

    public float  startPointFromEdgeOffset;
    public float  startToEndDistance;
    public float coneToTargetReqProximity;

    public string toDisplayAtFinish;



    private OVRInput.Button trialStartButton = OVRInput.Button.One;
    private OVRInput.Button comebackStartButton = OVRInput.Button.Two;

    private Setting[] settings;

    private State activeState = State.Calibration;

    private bool isFinished = false;
    private bool calibrationCompleted = true; // change

    public GameObject Arm;
    public GameObject Room;
    public Transform RightHandAnchor;

    // defined in calibration ?
    private Vector3 endPointPos;
    private Vector3 startPointPos;

    private Setting currentSetting;


    private Setting defaultSetting;

    public Material checkPointMaterial;
    public Material optimalPathMaterial;
    public float checkPointTransparency;
    private GameObject startPoint;
    private GameObject endPoint;
    private GameObject optimalPath;


    // private Text display;


    enum State
    {
        Calibration,
        BeforeTrial,
        Trail,
        AfterTrial,
        Comeback,
        Finished
    }


    void Start()
    {

        Debug.Log("STARTED!: ");
        createSettings();
        startPoint = GameObject.Instantiate(defaultSetting.Arm.transform.Find("Cone").gameObject, this.transform);
        startPoint.name = "StartPoint";
        Renderer rend = startPoint.GetComponent<Renderer>();
        rend.material = checkPointMaterial;
        Color col = rend.material.color;
        col.a = checkPointTransparency;
        rend.material.color = col;
        endPoint = GameObject.Instantiate(startPoint, this.transform);
        endPoint.name = "EndPoint";

        startPoint.SetActive(false);
        endPoint.SetActive(false);


        if ( settings.Length < 1)
        {
            Debug.Log("There must be at least 1 valid setting ! Shutting down.");
        }
        currentSetting = defaultSetting;
        InitCalibration();
        SwitchSetting(defaultSetting);

    }

    void ShowEnviroment() { SetActiveEnviroment(true); }
    void HideEnviroment() { SetActiveEnviroment(false); }

    void SetActiveEnviroment(bool active)
    {
        currentSetting.Arm.SetActive(active);
        currentSetting.Room.SetActive(active);
        startPoint.SetActive(active);
        endPoint.SetActive(active);
        optimalPath.SetActive(active);
    }

  

    // Update is called once per frame
    void Update()
    {
        trySwitchState();
        DisplayMsg("");
    }

    void Calibrate()
    {
        //Vector3 conePos = defaultSetting.Arm.transform.Find("Cone").position;
        Vector3 tablePos = defaultSetting.Room.transform.Find("Table").position;
        Vector3 offset = new Vector3(0f, 0.5f, -0.43f + startPointFromEdgeOffset); // dummy
        startPoint.transform.position = tablePos + offset;
        endPoint.transform.position = startPoint.transform.position + new Vector3(0, 0, startToEndDistance);
        optimalPath = GameObject.CreatePrimitive(PrimitiveType.Cube);
        optimalPath.transform.localScale = new Vector3(0.01f, 0.001f, startToEndDistance);
        optimalPath.transform.position = startPoint.transform.position + new Vector3(0, 0, startToEndDistance / 2);
        Renderer rend = optimalPath.GetComponent<Renderer>();
        rend.material = optimalPathMaterial;

        startPointPos = startPoint.transform.position;
        endPointPos = endPoint.transform.position;


    }


    void trySwitchState()
    {
        switch (activeState)
        {
            case State.Calibration:
                if (IsPressed(trialStartButton)) //(calibrationCompleted)
                {
                    // put startPoint and endPoint
                    Calibrate();
                    //InitBeforeTrial();
                    InitComeback();
                }
                break;
            case State.BeforeTrial:
                if (IsPressed(trialStartButton))
                {
                    Debug.Log("button pressed, starting trial");
                    InitTrial();
                }
                break;
            case State.Trail:
                if (IsConeInPosition(endPointPos))
                {
                    InitAfterTrial();
                }
                break;
            case State.AfterTrial:
                if (IsPressed(comebackStartButton))
                {
                    InitComeback();
                }
                break;
            case State.Comeback:
                if (isFinished)
                {
                    InitFinished();
                }
                else if (IsConeInPosition(startPointPos))
                {
                    InitBeforeTrial();
                }
                break;
            case State.Finished:
                break;
        }

    }



    void InitBeforeTrial()
    {
        // show BeforeTrial msg
        Debug.Log("Initiating BeforeTrial");
        SwitchSetting(DrawNextSetting());
        DisplayMsg(currentSetting.TextBeforeTrial);
        HideEnviroment();
        activeState = State.BeforeTrial;
        Debug.Log("BeforeTrial Initiated");
    }
    void InitTrial()
    {
        Debug.Log("Initiating Trial");
        HideDisplay();

        // apply new body
        // start logging
        StartLogging();
        ShowEnviroment();
        endPoint.SetActive(true);
        startPoint.SetActive(false);
        activeState = State.Trail;
        Debug.Log("Trial Initiated");
    }

    void InitAfterTrial()
    {
        Debug.Log("Initiating AfterTrial");
        // show AfterTrial msg
        DisplayMsg(currentSetting.TextAfterTrial);
        // stop Logging
        StopLogging();
        HideEnviroment();
        activeState = State.AfterTrial;
        Debug.Log("AfterTrial Initiated");
    }

    void InitComeback()
    {
        Debug.Log("Initiating Comeback");
        HideDisplay();
        ShowEnviroment();
        // switch to default body
        endPoint.SetActive(false);
        startPoint.SetActive(true);
        optimalPath.SetActive(false);
        SwitchSetting(defaultSetting);
        activeState = State.Comeback;
        Debug.Log("Comeback Initiated");
    }

    void InitFinished()
    {
        DisplayMsg(toDisplayAtFinish);
        activeState = State.Finished;
    }

    void InitCalibration() 
    {
        // use to get offset from edgde ! GetComponent<Renderer>().bounds.size
    }


   

    void StartLogging() { }
    void StopLogging() { }

    // Draws next Exper. setting randomly
    // Consider 0 trials!
    Setting DrawNextSetting()
    {
        System.Random rnd = new System.Random();
        int candidate = rnd.Next(0, settings.Length);
        settings[candidate].TrialsLeft--;
        Setting chosen = settings[candidate];
        if (settings[candidate].TrialsLeft < 1)
        {
            // pop setting when no trials left
            var buffer = new List<Setting>();
            for (int i = 0; i < settings.Length; i++)
            {
                if (i != candidate) { buffer.Add(settings[i]); }
            }
            settings = buffer.ToArray();
            // if no trials left, finish experiment
            if (settings.Length == 0) { isFinished = true; }
        }
        return chosen;
    }

    // Displays Msg on canvas
    void DisplayMsg(string msg)
    {
        MsgDisplay.gameObject.SetActive(true);
        MsgDisplay.gameObject.GetComponent<Text>().text = "(" + activeState.ToString() + ")" + msg;
        MsgDisplay.gameObject.GetComponent<Text>().text = msg;


    }

    void HideDisplay()
    {
        //MsgDisplay.gameObject.SetActive(false);
    }

    bool IsPressedMockup()
    {
        return Input.GetKeyUp(KeyCode.Space);
    }

    bool IsPressed(OVRInput.Button button)
    {
        //Debug.Log("Awaiting button");
        return OVRInput.GetUp(button) || IsPressedMockup();
    }

    bool IsConeInPosition(Vector3 targetPos)
    {
        float dist = Vector3.Distance(targetPos, currentSetting.Arm.transform.Find("Cone").transform.position);
        return dist < coneToTargetReqProximity || IsPressedMockup();
        //return true;
    }

    void SwitchSetting(Setting nextSetting)
    {
        currentSetting.Arm.SetActive(false);
        currentSetting.Room.SetActive(false);
        nextSetting.Arm.transform.position = currentSetting.Arm.transform.position;
        currentSetting = nextSetting;
        currentSetting.Arm.SetActive(true);
        currentSetting.Room.SetActive(true);

    }

    GameObject createDefaultArm()
    {
        // make the template objects of arm inactive and all its future clones
        Arm.gameObject.SetActive(false);
        // create copy of arm template
        // create copy of arm template
        GameObject defaultArm = GameObject.Instantiate(Arm, Arm.transform.parent);
        defaultArm.name = "Default Arm";
        // find all elements of the arm
        GameObject hand = defaultArm.transform.Find("Hand").gameObject;
        GameObject cone = defaultArm.transform.Find("Cone").gameObject;
        GameObject sleeve = defaultArm.transform.Find("Sleeve").gameObject;
        // if default materials provided apply them to default arm
        if (defaultExperimentSetting.HandMaterial != null) {
            hand.GetComponent<Renderer>().material = defaultExperimentSetting.HandMaterial; }
        if (defaultExperimentSetting.ConeMaterial != null) {
            cone.GetComponent<Renderer>().material = defaultExperimentSetting.ConeMaterial; }
        if (defaultExperimentSetting.SleeveMaterial != null) {
            sleeve.GetComponent<Renderer>().material = defaultExperimentSetting.SleeveMaterial; }
        
        ArmMapping mapping = defaultArm.AddComponent<ArmMapping>();
        attachMapping(mapping, defaultExperimentSetting);

        return defaultArm;
    }

    void attachMapping(ArmMapping mapping, DefaultExperimentSetting setting)
    {
        (bool, bool, bool) rotationFreeze =
            (setting.freezeRotationX,
            setting.freezeRotationY,
            setting.freezeRotationZ);
        (bool, bool, bool) translationFreeze =
            (setting.freezeTranslationAxisX,
            setting.freezeTranslationAxisY,
            setting.freezeTranslationAxisZ);
        mapping.Configure(RightHandAnchor, rotationFreeze, translationFreeze,
            setting.translationDegrees, setting.translationOffset);
    }

    GameObject createDefaultRoom()
    {
        // make the template objects of room inactive and all its future clones
        Room.gameObject.SetActive(false);
        // create copy of room template
        GameObject defaultRoom = GameObject.Instantiate(Room);
        defaultRoom.name = "Default Room";
        // find all elements of the room
        GameObject ceiling = defaultRoom.transform.Find("Ceiling").gameObject;
        GameObject floor = defaultRoom.transform.Find("Floor").gameObject;
        GameObject wall1 = defaultRoom.transform.Find("Wall1").gameObject;
        GameObject wall2 = defaultRoom.transform.Find("Wall2").gameObject;
        GameObject wall3 = defaultRoom.transform.Find("Wall3").gameObject;
        GameObject wall4 = defaultRoom.transform.Find("Wall4").gameObject;
        // if defualt materials provided apply them to defualt room
        if (defaultExperimentSetting.CeilingMaterial != null) {
            ceiling.GetComponent<Renderer>().material = defaultExperimentSetting.CeilingMaterial; }
        if (defaultExperimentSetting.FloorMaterial != null) 
        { floor.GetComponent<Renderer>().material = defaultExperimentSetting.FloorMaterial; }
        if (defaultExperimentSetting.WallMaterial!= null) { 
            wall1.GetComponent<Renderer>().material = defaultExperimentSetting.WallMaterial;
            wall1.GetComponent<Renderer>().material = defaultExperimentSetting.WallMaterial;
            wall1.GetComponent<Renderer>().material = defaultExperimentSetting.WallMaterial;
            wall1.GetComponent<Renderer>().material = defaultExperimentSetting.WallMaterial;
        }
        return defaultRoom;
    }

    GameObject createCustomArm(ExperimentSetting experSetting)
    {
        // copy the default arm
        GameObject customArm = GameObject.Instantiate(defaultSetting.Arm, defaultSetting.Arm.transform.parent);
        customArm.name = "Custom Arm "+experSetting.name;
        // find all elements of the arm
        GameObject hand = customArm.transform.Find("Hand").gameObject;
        GameObject cone = customArm.transform.Find("Cone").gameObject;
        GameObject sleeve = customArm.transform.Find("Sleeve").gameObject;
        Material customHandMat = experSetting.HandMaterial;
        Material customConeMat = experSetting.ConeMaterial;
        Material customSleeveMat = experSetting.SleeveMaterial;
        
        // if custom materials provided apply them to custom arm
        if (customHandMat != null) { hand.GetComponent<Renderer>().material = customHandMat; }
        if (customConeMat != null) { cone.GetComponent<Renderer>().material = customConeMat; }
        if (customSleeveMat != null) { sleeve.GetComponent<Renderer>().material = customSleeveMat; }
        attachMapping(customArm.GetComponent<ArmMapping>(), experSetting);
        return customArm;
    }

    GameObject createCustomRoom(ExperimentSetting exprSetting)
    {
        // copy the default room
        GameObject customRoom = GameObject.Instantiate(defaultSetting.Room);
        customRoom.name = "Custom Room " + exprSetting.name;
        // find all elements of the room
        GameObject table = customRoom.transform.Find("Table").gameObject;
        GameObject ceiling = customRoom.transform.Find("Ceiling").gameObject;
        GameObject floor = customRoom.transform.Find("Floor").gameObject;
        GameObject wall1 = customRoom.transform.Find("Wall1").gameObject;
        GameObject wall2 = customRoom.transform.Find("Wall2").gameObject;
        GameObject wall3 = customRoom.transform.Find("Wall3").gameObject;
        GameObject wall4 = customRoom.transform.Find("Wall4").gameObject;
        // if custom materials provided apply them to custom room
        if (exprSetting.TableMaterial!= null) { table.GetComponent<Renderer>().material = exprSetting.TableMaterial; }
        if (exprSetting.CeilingMaterial != null) { ceiling.GetComponent<Renderer>().material = exprSetting.CeilingMaterial; }
        if (exprSetting.FloorMaterial != null) { floor.GetComponent<Renderer>().material = exprSetting.FloorMaterial; }
        if (exprSetting.WallMaterial != null) { wall1.GetComponent<Renderer>().material = exprSetting.WallMaterial; }
        if (exprSetting.WallMaterial != null) { wall2.GetComponent<Renderer>().material = exprSetting.WallMaterial; }
        if (exprSetting.WallMaterial  != null) { wall3.GetComponent<Renderer>().material = exprSetting.WallMaterial; }
        if (exprSetting.WallMaterial != null) { wall4.GetComponent<Renderer>().material = exprSetting.WallMaterial; }
        return customRoom;
    }

    /// <summary>
    // creates Setting objects from ExperimentSetting
    /// </summary>
    void createSettings()
    {
        // create default setting
        GameObject defArm = createDefaultArm();
        GameObject defRoom = createDefaultRoom();
        defaultSetting = new Setting("DefaultSetting",0,0, 
            defArm,defRoom, 
            defaultExperimentSetting.toDisplayBeforeEachTrial, 
            defaultExperimentSetting.toDisplayAfterEachTrial);

        // create custom settings
        var buff = new List<Setting>();
        foreach (ExperimentSetting exprSett in experimentSettings)
        {
            // skip if trial count < 1
            if (exprSett.amountOfTrials < 1) { 
                Debug.Log("Setting \"" + exprSett.name + "\" must have at least 1 trialcount !"); 
                continue; }

            GameObject customArm = createCustomArm(exprSett);
            GameObject customRoom = createCustomRoom(exprSett);
            String txtBeforeTrial = exprSett.toDisplayBeforeEachTrial;
            String txtAfterTrial = exprSett.toDisplayAfterEachTrial;
            if (txtBeforeTrial.Length == 0) { txtBeforeTrial = defaultExperimentSetting.toDisplayBeforeEachTrial; }
            if (txtAfterTrial.Length == 0) { txtAfterTrial = defaultExperimentSetting.toDisplayAfterEachTrial; }
            int id = exprSett.settingID;
            if( id <1) { id = generateUniqueID(buff); }
            Setting new_setting = new Setting(exprSett.name, id, exprSett.amountOfTrials,
                customArm,customRoom, txtBeforeTrial,txtAfterTrial);
            buff.Add(new_setting);
        }
        settings = buff.ToArray();
    }

    // defensive programming
    // if a bad id provided the method will genereate a valid one
    int generateUniqueID(List<Setting> seenSettings)
    {
        var seenIDs = new List<int>();
        foreach (Setting setting in seenSettings)
        {
            seenIDs.Add(setting.id);
        }

        int candidate_id = 1;
        while (true)
        {
            if (!seenIDs.Contains(candidate_id)) { break; }
            candidate_id++;
        }
        return candidate_id;
    }


    // simpler and more ssecure version of ExperimentSetting
    // carrying only the necessary info for the rest of the app lifetime 
    public class Setting
    {
        public string name { get; private set; }
        public int id { get; private set; }
        public int TrialsLeft { get; set;}
        public GameObject Arm { get; private set; }
        public GameObject Room { get; private set; }
        public string TextBeforeTrial { get; private set; }
        public string TextAfterTrial { get; private set; }
        
        public Setting(string name, int id, int trialCnt, GameObject arm,
            GameObject room, string textBefore, 
            string textAfter)
        {
            this.name = name;
            this.id = id;
            this.TrialsLeft = trialCnt;
            this.Arm = arm;
            this.Room = room;
            this.TextBeforeTrial = textBefore;
            this.TextAfterTrial = textAfter;

        }
    }


}
 