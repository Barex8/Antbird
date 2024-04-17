using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    Transform player;
    float time;
    bool reach;
    
    [SerializeField]AnimationCurve curve;

    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        reach = transform.position == player.position;
        if (time > 1 || reach)
            {
            time = 0;
            }
        else
            {
             time += Time.deltaTime;
            }
        
        transform.position = Vector3.Lerp(transform.position, player.position,curve.Evaluate(time));  
    }
}
