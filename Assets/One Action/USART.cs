using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;
using System.IO;

public class USART : MonoBehaviour
{
    private SerialPort SP;
    private SerialPort SP2;
    public Vector3 InputEu;
    public string Line1;
    public string Line2;
    public float ForkPressure;
    public float PressurePadValue;
    public float KnifePressure;
    public float KnifeGPressure;
    public int no_devices = 5;
    bool[] _device;
    private bool SaveStatics;
    private int timeout = 0;
    public bool start;
    public Text DebugText;
    static float DataSendRate = 100.0f;
    static float TimeTakenforOneMsg = 1.0f / DataSendRate;
    public bool debug = true;
    private static string savedDataPath;
    Quaternion RotQuat;
    public float CalbEulerX;
    public List<int[]> Calibrations = new List<int[]>();
    private string[] LvlSavepath;
    public Dropdown lvl;
    // Use this for initialization
    void Start()
    {
        LvlSavepath = new string[3];
        savedDataPath = Application.persistentDataPath + "/savedData";
        LvlSavepath[0] = savedDataPath + "/Level_1_Data";
        LvlSavepath[1] = savedDataPath + "/Level_2_Data";
        LvlSavepath[2] = savedDataPath + "/Level_3_Data";
        Scan();
    }

    #region Code Connecting With Arduino

    public List<string> portExists;
    public Dropdown ComPort;
    public Dropdown ComPort2;

    public void Scan()
    {
        portExists = new List<string>();
        portExists.AddRange(SerialPort.GetPortNames());

        for (int i = 0; i < portExists.Count; i++)
        {
            int port_ = int.Parse(portExists[i].Split('M')[1]);
            if (port_ > 9)
            {
                portExists[i] = "\\\\.\\" + portExists[i];
            }
        }

        if (portExists.Count != 0)
        {
            ComPort.ClearOptions();
            ComPort.AddOptions(portExists);
            ComPort2.ClearOptions();
            ComPort2.AddOptions(portExists);
        }
        else
        {
            ComPort.ClearOptions();
            ComPort.AddOptions(new List<string> { "No Ports" });
            ComPort2.ClearOptions();
            ComPort2.AddOptions(new List<string> { "No Ports" });
        }
    }

    public void Initialize()
    {
        if (portExists.Count > 1)
        {            
            SP = new SerialPort(portExists[ComPort.value], 9600);
            SP.NewLine = "\n";
            SP.DtrEnable = true;
            SP.ReadTimeout = 25;//25 for query
            SP.WriteTimeout = 5;

            try
            {
                SP.Open();
                Verbose_Logging("Initialized " + portExists[ComPort.value]);
            }
            catch (System.Exception)
            {
                Verbose_Logging("No Device Conneced to " + portExists[ComPort.value]);
            }

            
            SP2 = new SerialPort(portExists[ComPort2.value], 9600);
            SP2.NewLine = "\n";
            SP2.DtrEnable = true;
            SP2.ReadTimeout = 25;//25 for query
            SP2.WriteTimeout = 5;

            try
            {
                SP2.Open();
                Verbose_Logging("Initialized " + portExists[ComPort2.value]);
            }
            catch (System.Exception)
            {
                Verbose_Logging("No Device Conneced to " + portExists[ComPort2.value]);
            }
            

        }
    }

    private void OnDisable()
    {
        if (SP != null)
        {
            SP.Close();
        }
        else
        {
            Debug.Log("COM Port Does Not Exist");
        }

        if (SP2 != null)
        {
            SP2.Close();
        }
        else
        {
            Debug.Log("COM Port Does Not Exist");
        }
    }

    private void OnApplicationQuit()
    {
        if (SP != null)
        {
            SP.Close();
        }

        if (SP2 != null)
        {
            SP2.Close();
        }
    }
    #endregion

    #region Code for Data Streaming and Parsing

   /* void Invokeup_date()
    {
            Quaternion _Left_Forearm_ = new Quaternion(x[0], y[0], z[0], w[0]);           //   A IMU
            Quaternion _right_Forearm_ = new Quaternion(x[1], y[1], z[1], w[1]);          //   B IMU
            Quaternion _Left_Arm_ = new Quaternion(x[2], y[2], z[2], w[2]);               //   C IMU
            Quaternion _Right_Arm_ = new Quaternion(x[3], y[3], z[3], w[3]);              //   D IMU
            Quaternion _Back_ = new Quaternion(x[4], y[4], z[4], w[4]);              //   E IMU

            // RightAngles = getrightarm(_Back_, Left_Arm, Left_Forearm);
            // LeftAngles = getleftarm(_Back_, Right_Arm, right_Forearm);

            // Quaternion Left_Forearm = new Quaternion(z[0], -x[0], y[0], w[0]);           //   A IMU
            //Quaternion right_Forearm = new Quaternion(z[1], -x[1], y[1], w[1]);          //   B IMU
            //Quaternion Left_Arm = new Quaternion(z[2], -x[2], y[2], w[2]);               //   C IMU
            // Quaternion Right_Arm = new Quaternion(z[3], -x[3], y[3], w[3]);              //   D IMU
            //Quaternion _Back_ = new Quaternion(y[4], -x[4], -z[4], w[4]);              //   E IMU


            //LeftArm = LeftArm * L_Arm;//C Testing
            //RightArm = RightArm * R_Arm;//D Testing
            //LeftForearm = LeftForearm * L_Forearm;//A Testing
            //rightForearm = rightForearm * R_Forearm;//B 

            Quaternion Left_Arm = Quaternion.Inverse(RotateLeftArm(_Back_)) * _Left_Arm_;
            Quaternion Left_Forearm = Quaternion.Inverse(RotateLeftForeArm(_Left_Arm_)) * _Left_Forearm_;
            Quaternion Right_Arm = Quaternion.Inverse(_Back_) * _Right_Arm_;
            //Quaternion right_Forearm = Quaternion.Inverse(_Right_Arm_) * _right_Forearm_;
            Quaternion right_Forearm = Quaternion.Inverse(RotateRightForeArm(_Right_Arm_)) * _right_Forearm_;

            //_Back_.ToAngleAxis(out angle, out axis);
            //Back = Quaternion.AngleAxis(angle, new Vector3(axis.x,-axis.z,axis.y));
            Vector3 axis;
            float angle;

            Left_Forearm.ToAngleAxis(out angle, out axis);
            //LeftForearm = Quaternion.AngleAxis(-angle, new Vector3(axis.x, axis.z, axis.y));//Old Design
            LeftForearm = Quaternion.AngleAxis(-angle, new Vector3(-axis.y, axis.z, axis.x));//New Design

            Left_Arm.ToAngleAxis(out angle, out axis);
            //LeftArm = Quaternion.AngleAxis(-angle, new Vector3(axis.x, axis.z, axis.y));//Old Design
            LeftArm = Quaternion.AngleAxis(-angle, new Vector3(-axis.y, axis.x, -axis.z));//New Design

            Right_Arm.ToAngleAxis(out angle, out axis);
            //RightArm = Quaternion.AngleAxis(-angle, new Vector3(axis.x, axis.z, axis.y));//Old Design
            RightArm = Quaternion.AngleAxis(-angle, new Vector3(axis.y, -axis.x, -axis.z));//New Design

            right_Forearm.ToAngleAxis(out angle, out axis);
            //rightForearm = Quaternion.AngleAxis(-angle, new Vector3(axis.x, axis.z, axis.y));//Old Design
            rightForearm = Quaternion.AngleAxis(-angle, new Vector3(axis.y, axis.z, -axis.x));//New Design

    }*/
    
    public float Hts;//Highest Time Stamp
    void Run()//SerialPort sp)
    {
        if (start)
        {
            if (SP != null)
            {
                float ts = 0;
                ts = Time.realtimeSinceStartup;
                string Line = ReadFromArduino();
                Line1 = Line;
                if (Line != "")
                {
                    ParseAngles(Line,0);
                }
                ts = Time.realtimeSinceStartup - ts;
                if (ts > Hts)
                {
                    Hts = ts;
                }                
            }
            else
            {
                Verbose_Logging("No Device Connected");
            }

            if (SP2 != null)
            {
                float ts = 0;
                ts = Time.realtimeSinceStartup;
                string Line = ReadFromArduino2();
                Line2 = Line;
                if (Line != "")
                {
                    ParseAngles(Line, 1);
                }
                ts = Time.realtimeSinceStartup - ts;
                if (ts > Hts)
                {
                    Hts = ts;
                }
            }
            else
            {
                Verbose_Logging("No Device Connected");
            }
        }
    }

    public void WriteToArduino(SerialPort sp, string message)
    {
        sp.WriteLine(message);
        sp.BaseStream.Flush();
    }

    public string ReadFromArduino()
    {
        string Line = "";
        if (SP != null && SP.IsOpen)
        {
            try
            {
                Line = SP.ReadLine();
                DebugText.text = Line;
                timeout = 0;
                SP.BaseStream.Flush();
            }
            catch (System.TimeoutException)
            {
                Line = "";
                timeout = timeout + 1;
                Verbose_Logging("Receive Data Error. Read Timeout");
            }

            if (timeout > 50)
            {
                SP.Close();
                timeout = 0;
            }
        }
        else
        {
            Initialize();
        }
        return Line;
    }

    public string ReadFromArduino2()
    {
        string Line = "";
        if (SP2 != null && SP2.IsOpen)
        {
            try
            {
                Line = SP2.ReadLine();
                DebugText.text = Line;
                timeout = 0;
                SP2.BaseStream.Flush();
            }
            catch (System.TimeoutException)
            {
                Line = "";
                timeout = timeout + 1;
                Verbose_Logging("Receive Data Error. Read Timeout");
            }

            if (timeout > 50)
            {
                SP2.Close();
                timeout = 0;
            }
        }
        else
        {
            Initialize();
        }
        return Line;
    }
    public float timer;
    Quaternion NewQuat;
    public void ParseAngles(string Line, int SerialPortIndex)
    {
        bool ReadStatus = true;
        string[] forces = Line.Split(',');
        switch (SerialPortIndex)
        {
            case 0://Fork
                if (forces.Length == 4)
                {
                    for (int i = 0; i < forces.Length; i++)
                    {
                        if (forces[i] == "")
                        {
                            ReadStatus = false;
                        }
                    }
                    if (ReadStatus)
                    {
                        try
                        {
                            if (firstEulerData)
                            {
                                RotQuat = Quaternion.Euler(0f, 0f, float.Parse(forces[1])-45.0f);
                                RotQuat = Quaternion.Inverse(RotQuat);
                                firstEulerData = false;
                            }
                            else
                            {
                                // Assign each variable in the string
                                InputEu = new Vector3(float.Parse(forces[1]), float.Parse(forces[2]), float.Parse(forces[3]));
                                ForkPressure = float.Parse(forces[0]);
                                if (ForkPressure > MaxForkGraspForceLimit)
                                {
                                    ForkPressure = MaxForkGraspForceLimit;
                                }
                                NewQuat = Quaternion.Euler(0f ,0f, InputEu.x);
                                NewQuat = NewQuat * RotQuat;
                                CalbEulerX = NewQuat.eulerAngles.z;
                                Verbose_Logging("Got Data 1");
                            }
                            
                        }
                        catch (System.FormatException)
                        {
                            Verbose_Logging("Format Error");
                        }

                    }
                }
                break;

            case 1://Knife
                
                if (forces.Length == 4)
                {
                    for (int i = 0; i < forces.Length; i++)
                    {
                        if (forces[i] == "")
                        {
                            ReadStatus = false;
                        }
                    }
                    
                    if (ReadStatus)
                    {
                        try
                        {
                            // Assign each variable in the string
                            KnifePressure = float.Parse(forces[0]);                            
                            PressurePadValue = float.Parse(forces[1]);
                            KnifeGPressure = float.Parse(forces[2]);

                            if (KnifePressure > MaxKnifeForceLimit)
                            {
                                KnifePressure = MaxKnifeForceLimit;
                            }
                            if (KnifeGPressure > MaxKnifeGraspForceLimit)
                            {
                                KnifeGPressure = MaxKnifeGraspForceLimit;
                            }
                            if (PressurePadValue > MaxForkForceLimit)
                            {
                                PressurePadValue = MaxForkForceLimit;
                            }
                            Verbose_Logging("Got Data 2");
                        }
                        catch (System.FormatException)
                        {
                            Verbose_Logging("Format Error");
                        }

                    }
                }
                break;
        }
        timer += Time.deltaTime;
        SaveData(timer.ToString("F2") + "," + ForkPressure.ToString("F2") + "," + KnifePressure.ToString("F2") + "," + KnifeGPressure.ToString("F2") + "," + PressurePadValue.ToString("F2") + "\n", false,LvlSavepath[lvl.value]);
    }
    #endregion

    #region Buttons
    string Time_;
    public InputField Name;
    public Dropdown Gender;
    public Dropdown Age;
    public Dropdown Dexterity;
    bool firstEulerData = false;
    public void Save_Statics()
    {
        timer = 0f;
        //CancelInvoke();
        start = true;
        InvokeRepeating("Run", 2.0f, 0.05f);
        firstEulerData = true;
        Time_ = System.DateTime.Now.ToString("_dd_MM_yyyy_HH_mm_ss");
        if (Name.text == "")
        {
            Name.text = "Unknown_Subject";
        }

        SaveData(Name.text + "," + Gender.options[Gender.value].text + "," + Age.options[Age.value].text + "\n", false, LvlSavepath[lvl.value]); //+ "," + Dexterity.options[Dexterity.value].text 
    }

    public void Exit()
    {
        timer = 0f;
        start = false;
        //StopAllCoroutines();
        CancelInvoke();
        firstEulerData = false;
    }

    public Slider[] CalibrateBars;
    public Text[] CalibrateTexts;
    public float CalForkPressure = 10.0f;
    public float CalKnifePressure = 20.0f;
    public float CalPressurePadValue = 3.0f;
    public float CalKnifeGPressure = 20.0f;

    const float MaxForkForceLimit = 10.0f; //Poking Force
    const float MaxForkGraspForceLimit = 20.0f;// Fork Grasp Force
    const float MaxKnifeForceLimit = 10.0f;//Knife cutting force
    const float MaxKnifeGraspForceLimit = 20.0f;//Knife Grasp Force

    public void Calibrate(int DeviceNo)
    {
        switch (DeviceNo)
        {
            case 0:
                CalForkPressure = ForkPressure;
                break;
            case 1:
                CalKnifeGPressure = KnifeGPressure;
                break;
            case 2:
                CalKnifePressure = KnifePressure;
                break;
            case 3:
                CalPressurePadValue = PressurePadValue;
                break;
        }
    }

    public void StartCalibrationDisplay()
    {
        InvokeRepeating("Calibrating", 0f, 0.01f);
    }

    void Calibrating()
    {
        CalibrateBars[0].value = ForkPressure/MaxForkGraspForceLimit;
        CalibrateTexts[0].text = ForkPressure.ToString();
        CalibrateBars[1].value = KnifeGPressure/MaxKnifeGraspForceLimit;
        CalibrateTexts[1].text = KnifeGPressure.ToString();
        CalibrateBars[2].value = KnifePressure/MaxKnifeForceLimit;
        CalibrateTexts[2].text = KnifePressure.ToString();
        CalibrateBars[3].value = PressurePadValue/MaxForkForceLimit;
        CalibrateTexts[3].text = PressurePadValue.ToString();
    }

    public void CloseSerialPort()
    {
        if (SP != null)
        {
            SP.Close();
        }
        if (SP2 != null)
        {
            SP2.Close();
        }
    }

    public void GetComPort()
    {
        if (SP != null)
        {
            SP.Close();
        }
        if (SP2 != null)
        {
            SP2.Close();
        }
        Initialize();
    }

    public void Verbose_Logging(string msg)
    {
        if (debug)
        {
            Debug.Log(msg);
            DebugText.text = msg;
        }
    }
    #endregion

    #region Saving Data
    public void SaveData(string Data, bool Replace, string Path)
    {

        string FileName = Path + "/" + "RF_" + Time_ + ".txt";

        if (!File.Exists(FileName))
        {
            Directory.CreateDirectory(Path);
        }

        if (Replace)
        {
            File.WriteAllText(FileName, Data);
        }
        else
        {
            File.AppendAllText(FileName, Data);
        }
    }
    #endregion

}
