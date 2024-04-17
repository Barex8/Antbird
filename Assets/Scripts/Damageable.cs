using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    int healht;

    public virtual void GetDamage(int damage)
        {
        healht -= damage;
        if (healht <= 0)
            {
            Die();
            }
        else
            {
            ReceiveImpact();
            }
        }

    public virtual void Die()
        {
         Destroy(gameObject);
        }

    public virtual void ReceiveImpact()
        {
        print("Impacto recibido");
        }
    }
