using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_4 : MonoBehaviour
{
    [Header("Image References")]
    public float Y_HandPosition;
    public float YBottomCap;
    public float YTopCap;
    public float ScaleFactor;
    public float X_offset;
    [Header("Image References")]
    public GameObject[] PacMan;
    public RectTransform Hand;
    public GameObject[] HandImages;
    public GameObject Apple;
    public Rigidbody A_Rg;

    [Header("Miscellaneous")]
    public float AppleTopCap;
    public float AppleBottomCap;
    private float AppleScaleFactor;
    private float ScaledHandPosition;
    private OneActionGameManager OAGM;
    public bool hold;
    // Start is called before the first frame update
    void Start()
    {
        OAGM = GetComponent<OneActionGameManager>();
        ScaleFactor = YTopCap - YBottomCap;
        AppleScaleFactor = AppleTopCap - AppleBottomCap;
    }

    // Update is called once per frame
    void Update()
    {

        if (Y_HandPosition < YBottomCap)
        {
            Y_HandPosition = YBottomCap;
        }

        if (Y_HandPosition > YTopCap)
        {
            Y_HandPosition = YTopCap;
        }

        CursorMapping();

        Hold_Manager();

        Apple_Mover();
    }
    public float newPos;
    private void CursorMapping()
    {
        ScaledHandPosition = (Y_HandPosition - YBottomCap) / ScaleFactor;
        if (ScaledHandPosition > 1.0f)
        {
            ScaledHandPosition = 1.0f;
        }
        if (ScaledHandPosition < 0f)
        {
            ScaledHandPosition = 0f;
        }

        /*if (ScaledHandPosition < -0.5f)
        {
            ScaledHandPosition = -0.5f;
        }
        if (ScaledHandPosition > 0.5f)
        {
            ScaledHandPosition = 0.5f;
        }
        */
        // ScaledHandPosition = ScaledHandPosition.x * 1920;
        //ScaledHandPosition = ScaledHandPosition * 1080;

        newPos = (0.25f + (ScaledHandPosition * 0.75f) - 0.5f) * 1080;
        Hand.anchoredPosition = new Vector3(X_offset, newPos, 0f);
        //Pos = Hand.anchoredPosition;
    }

    private void Hold_Manager()
    {
        float ScaledHandPosition = (Y_HandPosition - YBottomCap)/ ScaleFactor;
        if (ScaledHandPosition < 0.1f) //> 0.2f && ScaledHandPosition < 0.3f)
        {
            if (!hold && OAGM.ForkPressure == OAGM.MaxForkPressure)
            {
                print(ScaledHandPosition);
                PacMan[1].SetActive(false);
                PacMan[0].SetActive(true);
                HandImages[0].SetActive(false);
                HandImages[1].SetActive(true);
                Apple.SetActive(true);
                hold = true;
            }
        }
        if (ScaledHandPosition > 0.7f && ScaledHandPosition < 0.9f)
        {
            if (hold && OAGM.ForkPressure == OAGM.MaxForkPressure)
            {
                PacMan[0].SetActive(false);
                PacMan[1].SetActive(true);
                HandImages[1].SetActive(false);
                HandImages[0].SetActive(true);
                OAGM.Score += 1;
                OAGM.ScoreText.text = OAGM.Score.ToString();
                Apple.SetActive(false);
                hold = false;
                Apple.transform.localPosition = new Vector3(Apple.transform.position.x, -1.0f, Apple.transform.position.z);
            }
        }
        if(OAGM.ForkPressure != OAGM.MaxForkPressure)
        {
            hold = false;
        }
    }

    void Apple_Mover()
    {
        if (hold)
        {
            A_Rg.useGravity = false;
            float Scaled_A_Pos = AppleBottomCap + (ScaledHandPosition * AppleScaleFactor);
            Apple.transform.localPosition = new Vector3(Apple.transform.position.x, Scaled_A_Pos, Apple.transform.position.z);
        }
        else if (Apple.transform.position.y > AppleBottomCap)
        {
            A_Rg.useGravity = true;
        }
        else
        {
            A_Rg.useGravity = false;
        }
    }
}
