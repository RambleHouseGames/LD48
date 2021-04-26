using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            MainCharacter player = collision.GetComponent<MainCharacter>();
            SignalManager.Inst.FireSignal(new MonsterAttackedPlayerSignal(player, this));
        }
    }
}
