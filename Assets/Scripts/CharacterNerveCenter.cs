using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterNerveCenter : MonoBehaviour
{
    float InputTimeout=2.0f;
    int poise = 5;

    Animator animator;

    StatWarden statWarden;
    PhysicalInput physicalInput;
    Behavior behavior;
    public bool IsPlayer = false;
    KnowledgeBase knowledgeBase;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        statWarden = GetComponent<StatWarden>();
        statWarden.updateLivingStatEvent += UpdateLivingStats;
        physicalInput = GetComponent<PhysicalInput>();
        behavior = GetComponent<Behavior>();
        knowledgeBase = GetComponent<KnowledgeBase>();
    }



    private void Update()
    {
        animator.SetFloat("Speed", physicalInput.moveInput.magnitude);//mov cnc
    }
    public void Move(Vector2 input)
    {
        SendMessage("MoveInput", input);
       
    }

    public void PlayerLook(Vector2 input)
    {
        SendMessage("PlayerLookInput", input);
    }

    public void NPCLook(Vector3 direction)
    {
        SendMessage("NPCLookInput", direction);
    }

    public void Jump(bool isPressed)
    {
        animator.SetBool("JumpBool", isPressed);
        if (isPressed)
        {
            SetTrigger("JumpTrigger");
        }
    }


    public void Attack()
    {
        SetTrigger("Attack");
        
    }


    public void Kick()
    {
        SetTrigger("Kick");//mov cnc
    }

    public void Interact()
    {
        SetTrigger("Interact");
    }





    //physcial input
    public void IsGrounded(GroundInfo info)
    {
        animator.SetBool("Grounded", info.detectGround);
        animator.SetFloat("Slope", info.angle);
    }

    public void CollisionAngle(ControllerColliderHit hit)
    {
        animator.SetFloat("CollisionAngle", Vector3.Angle(hit.normal, Vector3.up));
    }

    //Environmental Interaction
    float resetPoiseTime = 0;
    private int lastHitboxID;
    public void SruckByHitBox(HitBox hitBox,HurtBox hurtBox)
    {
        if (hitBox.ID == lastHitboxID)
        {
            return;
        }
        else
        {
            lastHitboxID = hitBox.ID;
        }
        foreach (KeyValuePair<string, float> damage in hitBox.GetDamage())
        {
            float val = statWarden.ImpactStat("Health", -damage.Value, new List<string>() { damage.Key });
        }
        GetComponent<HitStop>().ActivateHitStop();//0.13 is the default


        // poise/knockback stuff
        if (resetPoiseTime <= Time.time)
        {
            Debug.Log(Time.time + "     "+resetPoiseTime);
            Debug.Log("resetpoise");
            statWarden.ImpactStat("Poise", 999f);
        }
        statWarden.ImpactStat("Poise", -hitBox.GetPoiseDamage());
        statWarden.GetLivingStat("Poise", out float currentPoise, out _);
        animator.SetInteger("Poise", (int) currentPoise);
        animator.SetInteger("KnockDownType", (int)hitBox.GetKnockdownType());
        resetPoiseTime = Time.time + statWarden.characterStats.EmergentStats()["PoiseReset"];

        Debug.Log(statWarden.characterStats.EmergentStats()["PoiseReset"]);
        //done with poise stuff

        hitBox.GetKnockbackDistance(hurtBox, out Vector3 enemyKnockback , out _);
        GetComponent<HitStop>().ActivateKnockBack(enemyKnockback);
    }
    public void StrikeHurtBox(HitBox hitBox, HurtBox hurtBox)
    {
        if (IsPlayer)
        {
            Debug.Log(transform.name + " Struck hurtbox " + hurtBox.cnc.transform.name);
            GetComponent<HitStop>().ActivateHitStop();
            
            hitBox.GetKnockbackDistance(hurtBox, out Vector3 _,out Vector3 selfKnockback);
            GetComponent<HitStop>().ActivateKnockBack(selfKnockback);
            
        }
    }

    public void Death()
    {
        if (IsPlayer)
        {
            SceneManager.LoadScene("GameOver");
        }
    }

    public void ResetPoise()
    {
        statWarden.GetLivingStat("Poise", out _, out float maxPoise);
        statWarden.ImpactStat("Poise", 999f);
        animator.SetInteger("Poise", (int)maxPoise);
    }

    //stat Warden
    public float ImpactStat(string stat, float impact, List<string> quirks)
    {
        return (statWarden.ImpactStat(stat, impact, quirks));
    }

    public void GetLivingStat(string stat, out float currentValue, out float maxValue)
    {
        statWarden.GetLivingStat(stat, out currentValue, out maxValue);
    }
    public void UpdateLivingStats()
    {
        statWarden.GetLivingStat("Health", out float currentValue, out float maxValue);
        if (currentValue <= 0)
        {
            animator.SetTrigger("Die");
        }
        Debug.Log("Health is now  "+ currentValue );
    }
   
    //KnowledgeBase
    public void ImpactAgro(GameObject source,float impact)//impact is the amountof time the character will be agroed for.
    {
        knowledgeBase.HostileImpact(source, impact);//the impact is converted to a percent and clamped between 0 and 1
    }

    // character Stat

    public ReadOnlyDictionary<string, float> PrimalStats()
    {
        return statWarden.characterStats.PrimalStats();
    }
    public ReadOnlyDictionary<string, float> EmergentStats()
    {
        return statWarden.characterStats.EmergentStats();
    }
    public ReadOnlyDictionary<string, float> QuirkAugurs()
    {
        return statWarden.characterStats.QuirkAugurs();
    }
    public void AddStat(StatComponent stat)
    {
        statWarden.characterStats.AddStat(stat);
    }
    public void RemoveStat(int id)
    {
        statWarden.characterStats.RemoveStat(id);
    }


    //Character Mood

    public string GetBehavior()
    {
        return behavior.CurrentBehavior;
    }
    [SerializeField]
    private Transform focus;
    public void SetFocus(Transform focus)
    {
        this.focus = focus;
    }
    public Transform GetFocus()
    {
        return focus;
    }




    //TODO: Refine and move to utility
    bool isRunning;
    string previousName;
    private void SetTrigger(string name)
    {
        if (isRunning && previousName != "") 
        {
            StopCoroutine(SetTriggerProcess(previousName));
            animator.ResetTrigger(previousName);
        }
        StartCoroutine(SetTriggerProcess(name));
        previousName = name;
    }


    IEnumerator SetTriggerProcess(string name)
    {
        isRunning = true;
        animator.SetTrigger(name);
        yield return new WaitForSeconds(InputTimeout);
        animator.ResetTrigger(name);
        isRunning = false;
        previousName = "";
    }


}
