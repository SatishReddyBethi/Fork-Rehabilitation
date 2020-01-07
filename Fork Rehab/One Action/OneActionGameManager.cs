using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneActionGameManager : MonoBehaviour
{
    public float EulerAngleZ;
    public float ScaledEulerZ;
    public GameObject Gate;
    public GameObject Ring;
    public float PressurePadValue;
    float MaxPressurePadValue = 3.0f;
    public float ForkPressure;
    public float KnifePressure;
    public float KnifeGraspPressure;
    float MaxKnifePressure = 3.0f;
    float MaxKnifeGraspPressure = 10.0f;
    float MaxForkPressure = 10.0f;
    public float AngleOffset = 0f;
    public float AngleScale;
    public GameObject Ball;
    private Rigidbody BallRB;
    float PreviousYPos;
    public bool Gravity;
    public int Score;
    public Text ScoreText;
    public bool start;
    private USART Conn;
    public GameObject Elevator;
    public Slider[] ForceUI;
    float ScaledForkPressure;
    float ScaledPressurePadValue;
    float ScaledKnifePressure;
    float ScaledKnifeGraspPressure;
    public int Level;
    public Dropdown Lvl;
    public GameObject[] GameOverUI;
    public float Timer;
    public Text TimerUI;
    public Text[] Timescores;
    // Start is called before the first frame update
    void Start()
    {
        Conn = GetComponent<USART>();
        BallRB = Ball.GetComponent<Rigidbody>();
        ScoreText.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            Timer += Time.deltaTime;
            TimerUI.text = Timer.ToString("F2");
            if (Score > 2+(3*Lvl.value))
            {
                GameOverUI[Lvl.value].SetActive(true);
                GameStop();
                Conn.Exit();
                Timescores[Lvl.value].text = Timer.ToString("F2") + "s";
            }

            ForkPressure = Conn.ForkPressure;
            PressurePadValue = Conn.PressurePadValue;
            KnifePressure = Conn.KnifePressure;
            KnifeGraspPressure = Conn.KnifeGPressure;
            if(ForkPressure > MaxForkPressure)
            {
                ForkPressure = MaxForkPressure;
            }
            if (KnifeGraspPressure > MaxKnifeGraspPressure)
            {
                KnifeGraspPressure = MaxKnifeGraspPressure;
            }
            if (KnifePressure > MaxKnifePressure)
            {
                KnifePressure = MaxKnifePressure;
            }
            if (PressurePadValue > MaxPressurePadValue)
            {
                PressurePadValue = MaxPressurePadValue;
            }
            ScaledForkPressure = ForkPressure / MaxForkPressure;
            ScaledPressurePadValue = PressurePadValue / MaxPressurePadValue;
            ScaledKnifePressure = KnifePressure / MaxKnifePressure;
            ScaledKnifeGraspPressure = KnifeGraspPressure / MaxKnifeGraspPressure;

            ForceUI[0].value = ScaledForkPressure;
            ForceUI[1].value = ScaledKnifeGraspPressure;
            ForceUI[2].value = ScaledKnifePressure;
            ForceUI[3].value = ScaledPressurePadValue;

            if (Gravity)
            {
                if (PreviousYPos == transform.position.y)
                {
                    BallRB.useGravity = false;
                    BallRB.useGravity = true;
                }
            }
            PreviousYPos = transform.position.y;
            EulerAngleZ = Ring.transform.localRotation.eulerAngles.z;
            if (EulerAngleZ > 15.0f && EulerAngleZ < 75.0f) // The Gate Stays Open
            {
                if (ForkPressure == MaxForkPressure)
                {
                    Gravity = true;
                    BallRB.useGravity = true;
                }

                float AngleDiff = 0;
                if (EulerAngleZ < 45.0f)
                {
                    // For every degree the Pie moves after 15 until 45, the gate opens
                    AngleDiff = EulerAngleZ - 15.0f;
                    Gate.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f - AngleDiff * 3.0f);
                }
                else
                {
                    // For every degree the Pie moves after 45 until 75, the gate closes
                    AngleDiff = EulerAngleZ - 45.0f;
                    Gate.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, AngleDiff * 3.0f);
                }
            }
            else
            {                
                // The position of gate is based on the pressure pad
                
                Gate.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f - (ScaledPressurePadValue * 90.0f));                
            }
            
            if(Elevator.activeInHierarchy)
            {
                Elevator.transform.localPosition = new Vector3(0.0f, -1.1f - (ScaledKnifePressure * 1.9f), 0.0f);
            }
            

            

        }
    }

    public float OldOff;
    public void FixedUpdate()
    {
        
        
        if (start)
        {
            if(Conn.CalbEulerX < AngleScale)
            {
                ScaledEulerZ = (Conn.CalbEulerX / AngleScale) * 170.0f;
            }
            else if(Conn.CalbEulerX > 360.0f- AngleScale)
            {
                ScaledEulerZ = 190.0f + ((Conn.CalbEulerX - (360.0f -AngleScale)) / AngleScale) * 170.0f;
            }
            else
            {
                ScaledEulerZ = 170.0f + ((Conn.CalbEulerX - AngleScale) / 90.0f) * 20.0f;
            }
            Ring.transform.localRotation = Quaternion.Euler(0f, 0f, ScaledEulerZ + 45f);
        }
    }
    
    public void CollisionEffect()
    {
        Score += 1;
        Gravity = false;
        BallRB.useGravity = false;
        BallRB.velocity = new Vector3(0f, 0f, 0f);
        BallRB.angularVelocity = new Vector3(0f, 0f, 0f);
        Ball.transform.position = new Vector3(0f, 5.5f, 0f);
        Ball.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        ScoreText.text = Score.ToString();
        // only for experimental purpose
        ForkPressure = 0;
        PressurePadValue = 0;
    }

    public void BoundaryEffect()
    {
        Gravity = false;
        BallRB.useGravity = false;
        BallRB.velocity = new Vector3(0f, 0f, 0f);
        BallRB.angularVelocity = new Vector3(0f, 0f, 0f);
        Ball.transform.position = new Vector3(0f, 5.5f, 0f);
        Ball.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        // only for experimental purpose
        ForkPressure = 0;
        PressurePadValue = 0;
    }

    public void SetLevel(int Level)
    {
        Lvl.value = Level;
        GameStart();
    }
    
    public void GameStart()
    {
        Timer = 0f;
        Level = Lvl.value;
        Score = 0;
        start = true;
        Ball.transform.position = new Vector3(0f, 5.5f, 0f);
        switch (Level)
        {
            case 0:
                MaxPressurePadValue = Conn.CalPressurePadValue/2.0f;
                MaxForkPressure = Conn.CalForkPressure/2.0f;
                MaxKnifePressure = Conn.CalKnifePressure/2.0f;
                MaxKnifeGraspPressure = Conn.CalKnifeGPressure/2.0f;
                Elevator.SetActive(false);
                AngleScale = 135.0f; // When angle is 135 the scaled angle is 180
                break;

            case 1:
                MaxPressurePadValue = Conn.CalPressurePadValue * (2.0f/3.0f);
                MaxForkPressure = Conn.CalForkPressure * (2.0f / 3.0f);
                MaxKnifePressure = Conn.CalKnifePressure * (2.0f / 3.0f);
                MaxKnifeGraspPressure = Conn.CalKnifeGPressure * (2.0f / 3.0f);
                Elevator.SetActive(false);
                AngleScale = 150.0f;
                break;

            case 2:
                MaxPressurePadValue = Conn.CalPressurePadValue;
                MaxForkPressure = Conn.CalForkPressure;
                MaxKnifePressure = Conn.CalKnifePressure;
                MaxKnifeGraspPressure = Conn.CalKnifeGPressure;
                Elevator.SetActive(true);
                AngleScale = 180.0f;
                break;


        }
    }

    public void GameStop()
    {
        start = false;
    }
}
