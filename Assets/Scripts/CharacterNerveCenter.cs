using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterNerveCenter : MonoBehaviour
{
    float InputTimeout=0.75f;
    int poise = 5;



    Animator animator;
    CharacterController cc;
    StatWarden statWarden;
    PhysicalInput physicalInput;
    Behavior behavior;
    public bool IsPlayer = false;
    public KnowledgeBase knowledgeBase;
    EquipedWeapon equipedWeapon;

    private void Awake()
    {
        
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

        statWarden = GetComponent<StatWarden>();
        statWarden.updateLivingStatEvent += UpdateLivingStats;
        physicalInput = GetComponent<PhysicalInput>();
        behavior = GetComponent<Behavior>();
        knowledgeBase = GetComponent<KnowledgeBase>();
        equipedWeapon = GetComponent<EquipedWeapon>();
    }



    private void Update()
    {
        animator.SetFloat("Speed", physicalInput.moveInput.magnitude);//mov cnc
        IsPushActive();
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
        if (IsPushActive())
        {
            SetTrigger("Push");
        }
        else
        {
            SetTrigger("Attack");
        }
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
    private int lastHitID;
    public void StartBlocking()
    {

    }
    public void StopBlocking()
    {

    }
    public void SruckByHitBox(HitBox hitBox,HurtBox hurtBox)
    {
        WeaponHitBox whb = hitBox as WeaponHitBox;
        if (hitBox.hitID == lastHitID || (whb!=null&&whb.cnc ==this)) 
        {
            return;//skip if the hitbox
        }
        else
        {
            lastHitID = hitBox.hitID;
        }
        float totalDamage=0f;
        foreach (KeyValuePair<string, float> damage in hitBox.GetDamage())
        {
            totalDamage += damage.Value;
            statWarden.ImpactStat("Health", -damage.Value, new List<string>() { damage.Key });

        }
        //agro
        if (whb != null)
        {
            statWarden.GetLivingStat("Health", out _, out float maxValue);
            hurtBox.cnc.ImpactAgro(whb.cnc.gameObject, totalDamage / maxValue);

        }
   


        GetComponent<HitStop>().ActivateHitStop(hitBox.hitStop);//0.13 is the default


        // poise/knockback stuff
        if (resetPoiseTime <= Time.time)
        {
            statWarden.ImpactStat("Poise", 999999f);//resets poise back to max.
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
    public void StrikeHurtBox(WeaponHitBox hitBox, HurtBox hurtBox)
    {

            Debug.Log(transform.name + " Struck hurtbox " + hurtBox.cnc.transform.name);
            GetComponent<HitStop>().ActivateHitStop(hitBox.hitStop);//0.13 is the default

            hitBox.GetKnockbackDistance(hurtBox, out Vector3 _,out Vector3 selfKnockback);
            GetComponent<HitStop>().ActivateKnockBack(selfKnockback);
            
            

    }

    public void Death()
    {
        gameObject.SetActive(false);
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
    Coroutine coroutineRef;
    private void SetTrigger(string name)
    {

            if (isRunning && coroutineRef != null)
            {
                StopCoroutine(coroutineRef);
                animator.ResetTrigger(previousName);

            }
            coroutineRef = StartCoroutine(SetTriggerProcess(name));
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
    



    //Attack Conditions

    private bool IsPushActive()
    {
        Vector3 origin = transform.position + cc.center;
        float width = cc.radius + cc.radius * equipedWeapon.weaponInstance.weapon.pushDistance;
        Vector3 halfExtents = new(width, cc.height * 0.5f + cc.radius, 0.1f);
        Vector3 direction = transform.forward;
        float distance = equipedWeapon.weaponInstance.weapon.pushDistance+cc.radius;
        
        
        
        int layerMask = 1<<3 ;

        if (Physics.BoxCast(origin, halfExtents, direction, transform.rotation, distance, layerMask))
        {
            return true;
        }
        return false;
    }
    

    //Swap Weapons

    private void SwapWeapon(Weapon weapon)
    {
        //equipedWeapon.EquipWeapon(weapon);
    }
    [SerializeField]
    Weapon weapon1;
    [SerializeField]
    int level1;
    [SerializeField]
    WeaponInfusion infusion1;
    public void SwapToWeapon1()
    {
        equipedWeapon.EquipWeapon(new WeaponInstance(weapon1, level1, infusion1));
    }
    [SerializeField]
    Weapon weapon2;
    [SerializeField]
    int level2;
    [SerializeField]
    WeaponInfusion infusion2;
    public void SwapToWeapon2()
    {
        equipedWeapon.EquipWeapon(new WeaponInstance(weapon2, level2, infusion2));
    }
    [SerializeField]
    Weapon weapon3;
    [SerializeField]
    int level3;
    [SerializeField]
    WeaponInfusion infusion3;
    public void SwapToWeapon3()
    {
        equipedWeapon.EquipWeapon(new WeaponInstance(weapon3, level3, infusion3));
    }

}
