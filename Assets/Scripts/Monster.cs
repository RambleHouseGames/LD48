using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField]
    ParticleSystem deathBlast;

    [SerializeField]
    Collider2D myCollider;

    private Animator myAnimator = null;

    private float deathTimer = 3f;
    private bool amDead = false;

    public virtual void Update()
    {
        if (amDead)
        {
            deathTimer -= Time.deltaTime;
            if (deathTimer <= 0f)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            MainCharacter player = collision.GetComponent<MainCharacter>();
            SignalManager.Inst.FireSignal(new MonsterAttackedPlayerSignal(player, this));
        }

        if(collision.tag == "PlayerAttack")
        {
            myCollider.enabled = false;
            if (myAnimator == null)
                myAnimator = GetComponent<Animator>();
            myAnimator.SetTrigger("Invisible");
            deathBlast.Play();
            amDead = true;
        }
    }
}
