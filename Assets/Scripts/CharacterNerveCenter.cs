using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class CharacterNerveCenter : MonoBehaviour
{
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
        animator.SetBool("Jump", isPressed);
    }


    public void Attack()
    {
        animator.SetTrigger("Attack");
    }


    public void Kick()
    {
        animator.SetTrigger("Kick");//mov cnc
    }

    public void Interact()
    {
        animator.SetTrigger("Interact");
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



    //stat Warden
    public float ImpactStat(string stat, float impact, List<string> quirks)
    {
        return (statWarden.ImpactStat(stat, impact, quirks));
    }

    public void GetLivingStat(string stat, out float currentValue, out float maxValue)
    {
        statWarden.GetLivingStat(stat, out currentValue, out maxValue);
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

    



}
