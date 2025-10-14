using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Velocity : MonoBehaviour
{
    Animator animator;
    AnimatorClipInfo[] animatorinfo;
    MeshRenderer meshRenderer;
    public ArduinoInterface arduinoInterface;
    public float grating;
    public float drifting;
    public bool mouseIsStill = false;
    public float smoothingFactor = 0.01f;
    public float speedThreshold = 1.0f;
    public float alpha;
    public float speed;
    public int trialNumber = 0;
    public int[] spatial_wavelength= { 24, 12, 8, 6, 4, 2, 1};
    public string anim_name;
    public bool trial_started = false;
    public EventLogger eventLogger;

    public float smoothedSpeed = 0.0f;
    void Start()
    {
        animator = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
        smoothedSpeed = 0.0f;
        trialNumber = 0;

        //startTrial();

        trial_started = false;
    }

    // Update is called once per frame
    void Update()
    {

        if(trial_started == false & eventLogger.info_entered == true)
        {
            startTrial();
            trial_started = true;
        }

        speed = Mathf.Sqrt(arduinoInterface.Rx* arduinoInterface.Rx + arduinoInterface.Ry * arduinoInterface.Ry + arduinoInterface.Rz * arduinoInterface.Rz);

        float dt = Time.deltaTime;

        alpha = Mathf.Pow(smoothingFactor, dt/(1/120.0f));

        smoothedSpeed = alpha * smoothedSpeed + (1.0f - alpha) * speed;

        animator.SetBool("MouseIsStill", smoothedSpeed < speedThreshold);
        animatorinfo = animator.GetCurrentAnimatorClipInfo(0);
        anim_name= animatorinfo[0].clip.name;

        
    }

    void startTrial()
    {
        animator.enabled = true;


        eventLogger.Add(new Event("New Trial", trialNumber));

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("degreesPerCycle", spatial_wavelength[trialNumber % 7]);
        //animator.Play("Base Layer");
        
        // update cylindar grating frequency
        // start animation
    }

    void TrialEnded()
    {
        trialNumber += 1;
        eventLogger.Add(new Event("New Trial", trialNumber));

        Debug.Log("Trial ended");

        startTrial();
        if (trialNumber == 35)
        { Application.Quit(); }
    }

    void fadeIn()
    {
        eventLogger.Add(new Event("Fade In", spatial_wavelength[trialNumber % 7]));
    }


    void fadeOut()
    {
        eventLogger.Add(new Event("Fade Out", spatial_wavelength[trialNumber % 7]));
    }

    void turnLeft()
    {
        eventLogger.Add(new Event("Turn Left", spatial_wavelength[trialNumber % 7]));
    }

    void turnRight()
    {
        eventLogger.Add(new Event("Turn Right", spatial_wavelength[trialNumber % 7]));
    }

    void noTurnControl()
    {
        eventLogger.Add(new Event("No Turn Control", spatial_wavelength[trialNumber % 7]));
    }

    void turnEnd()
    {
        eventLogger.Add(new Event("Turn End", spatial_wavelength[trialNumber % 7]));
    }

    void fadeComplete()
    {
        eventLogger.Add(new Event("Fade Complete", spatial_wavelength[trialNumber % 7]));
    }

    void waitingForStillMouse()
    {
        eventLogger.Add(new Event("Waiting For Still Mouse", spatial_wavelength[trialNumber % 7]));
    }

}
