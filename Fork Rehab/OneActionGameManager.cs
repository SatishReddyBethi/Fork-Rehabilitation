using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneActionGameManager : MonoBehaviour
{
    public float EulerAngleZ;
    public GameObject Gate;
    public GameObject Ring;
    public float PressurePadValue;
    const float MaxPressurePadValue = 3.0f;
    public float ForkPressure;
    public float KnifePressure;
    public float KnifeGraspPressure;
    const float MaxKnifePressure = 20.0f;
    const float MaxKnifeGraspPressure = 20.0f;
    const float MaxForkPressure = 10.0f;
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
            ForkPressure = Conn.ForkPressure;
            PressurePadValue = Conn.PressurePadValue;
            KnifePressure = Conn.KnifePressure;
            KnifeGraspPressure = Conn.KnifeGPressure;

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
                if (ForkPressure > 6.0f)
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
            
            Elevator.transform.localPosition = new Vector3(0.0f, -1.1f - (ScaledKnifePressure * 1.9f), 0.0f);
            

            

        }
    }


    public void FixedUpdate()
    {
        if (start)
        {
            Ring.transform.localRotation = Quaternion.Euler(0f, 0f, Conn.CalbEulerX);
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

    public void GameStart()
    {
        Score = 0;
        start = true;
    }

    public void GameStop()
    {
        start = false;
    }
}
