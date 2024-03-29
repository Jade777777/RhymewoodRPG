using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    public static readonly float hitPauseTime = 0.033f;//how long the animation initialy stops at 0 for( 1 frame at 30 fps )
    public static readonly float cleaveTime = 0.6f;
    public static readonly float cleaveSpeed = 0.4f;//how fast the hitstop starts at
    public static readonly float impactPauseTime = 0.25f;// how long the impact pauses for before the animation resumes.
    public static readonly float hitBoostSpeed = 2f;// the speed at which the animation accelerates to
    public static readonly float impactAccelerateTime = 0.1f;// how long the animation accelerates to this speed for;
    
    private Animator animator;
    private float impactTime;// How long this hitstop occurs for in total;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void ActivateHitStop()
    {
        ActivateHitStop(0.13f);
        
    }
    Coroutine cref;
    public void ActivateHitStop(float impactTime)
    {

        //ScreenShake.Shake(impactTime * 1, 2, 20);
        if (impactTime == 0f)
        {
            return;
        }
        Mathf.Clamp(impactTime,0.07f, 0.75f);
        if (impactTime >= this.impactTime)
        {

            if (cref != null) 
            { 
                StopCoroutine(cref); 
            }
            
            this.impactTime = impactTime;
   
            cref = StartCoroutine(HitStopProcess());
        }

    }

    IEnumerator HitStopProcess()
    {
        //Allow user to visualy process initial hit
        
        animator.speed = 0;
        yield return new WaitForSeconds(hitPauseTime);
        

        //cleave through the target, put force and motion into the hit
        float cleaveTimer = 0;
        float scaledCleaveTime = cleaveTime * impactTime;
        float scaledCleaveSpeed = cleaveSpeed * Mathf.Pow(0.13f / (impactTime),2); 
        while (cleaveTimer < scaledCleaveTime)
        {
            animator.speed = Mathf.Lerp(scaledCleaveSpeed, 0.5f*scaledCleaveSpeed, cleaveTimer/scaledCleaveTime);
            cleaveTimer += Time.deltaTime;
            yield return null;
        }
        

        //hold and build up power, stretch like a rubberband
        animator.speed = 0;
        yield return new WaitForSeconds(impactTime* impactPauseTime);

        impactTime *= 0.5f;
        //Launch target
        float accelerateTimer = 0;
        float halfAccelerateTime = 0.5f * impactAccelerateTime;
        while (accelerateTimer<halfAccelerateTime)// wait for impactAccelerateTime
        {
            animator.speed = Mathf.SmoothStep(0, hitBoostSpeed, accelerateTimer / halfAccelerateTime);
            accelerateTimer += Time.deltaTime;
            yield return null; 
        }
        accelerateTimer = 0f;
        
        while (accelerateTimer<halfAccelerateTime)
        {
            animator.speed = Mathf.SmoothStep(hitBoostSpeed, 1, accelerateTimer / halfAccelerateTime);
            accelerateTimer += Time.deltaTime;
            yield return null;
        }
        animator.speed = 1f;
        impactTime = 0f;

    }




    float acceleration=20;
    public Vector3 knockBackVelocity = Vector3.zero;
    Coroutine knockBackProcess = null;
    public void ActivateKnockBack(Vector3 distance)
    {
        Vector3 knockBackVelocity = Mathf.Sqrt(2 * acceleration * distance.magnitude) * distance.normalized;
        if(knockBackProcess!=null) StopCoroutine(KnockBackProcess());
        this.knockBackVelocity = knockBackVelocity;
        knockBackProcess=StartCoroutine(KnockBackProcess());
    }

    IEnumerator KnockBackProcess()
    {
        while (knockBackVelocity.magnitude > 0.1)
        {
            yield return null;
            knockBackVelocity -= knockBackVelocity.normalized * (acceleration*Time.deltaTime)*animator.speed;
        }
        knockBackVelocity = Vector3.zero;
    }
}
