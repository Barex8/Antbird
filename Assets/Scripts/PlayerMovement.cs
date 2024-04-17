using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
    {

    Animator anim;
    Rigidbody rb;
    Transform checkSlopeForward;
    Transform checkSlopeBackward;


    [SerializeField]
    LayerMask whatIsGround;

    [SerializeField]
    float speed;
    [SerializeField]
    float speedTurning;
    [SerializeField]
    float runSpeedTurning;
    [SerializeField]
    float runSpeed;
    [SerializeField]
    bool running;       //Ocultar


    bool moving;
    bool grounded;

    [SerializeField]
    AnimationCurve gravity;
    float timeFalling;
    Vector3 axisApplyGravity = Vector3.up;
    public bool canMove;
    public bool isStick;
    float lastVerticalInput;

    [SerializeField]
    float radiusCheckGround = 0.1f;
    [SerializeField]                   //Ocultar los dos
    float lenghtCheckSlope = 0.15f;

    float timerTransitionSlopes;
    [SerializeField]
    AnimationCurve slopesCurve;
    Vector3 actualSlopeNormal;

    Ray rayCheckSlope;
    Ray rayCenter,rayFront,rayBack;

    public Image image;/// <summary>
    public Text Euler;
    public Text Euler2;

    Transform hitBoxAttack;
    [SerializeField]
    LayerMask whatIsDamageable;

    [SerializeField]
    int damage;

    private void Start()
        {
        anim = transform.GetChild(0).GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        checkSlopeForward = transform.Find("CheckSlopeForward");
        checkSlopeBackward = transform.Find("CheckSlopeBackward");
        hitBoxAttack = transform.Find("HitBoxAttack");

        }

    private void Update()
        {
        grounded = CheckGround();
        canMove = grounded;

        if (grounded)
            {
            image.color = Color.blue;
            timeFalling = 0f;
            }
        else
            {
            image.color = Color.red;
            timeFalling += Time.deltaTime;
            print("Aplicando Gravedad");
            }

        if (Input.GetMouseButtonDown(1))
            {
            Attack();
            }

        MovementHandler();
        }

    void MovementHandler()
        {
        float axisZ = Input.GetAxis("Vertical");
        float axisX = Input.GetAxis("Horizontal");
        if (axisZ != 0) lastVerticalInput = axisZ;
        
        Vector3 movement = axisApplyGravity * gravity.Evaluate(timeFalling);
        if (grounded)
            {
            if (axisX == 0) movement = transform.forward * axisZ * (running ? runSpeed:speed);
            transform.Rotate(transform.up, Time.deltaTime*(running ? runSpeedTurning:speedTurning)*axisX,Space.World);

            moving = axisX != 0 || axisZ != 0;
            }

        movement = AdjustVelocityToSlope(movement);
        rb.velocity = movement;

        anim.SetFloat("Hor", axisX);
        anim.SetFloat("Ver", axisZ);
        anim.SetBool("Walking", moving);
        }

    void Attack()
        {
        Collider [] colliders = Physics.OverlapSphere(hitBoxAttack.position, .56f, whatIsDamageable, QueryTriggerInteraction.Collide);
        print(colliders);
        foreach (var item in colliders)
            {
            
            try
                {
                item.GetComponent<Damageable>().GetDamage(damage);
                }
            catch (System.Exception)
                {
                print("No se puede atacar");
                throw;
                }
            }
        }

    bool CheckGround()
        {
        rayCenter = new Ray(new Vector3(transform.position.x,transform.position.y+.3f,transform.position.z), -transform.up);
        rayFront = new Ray(new Vector3(transform.position.x,transform.position.y+.3f,transform.position.z) + -transform.forward / 2, -transform.up);
        rayBack = new Ray(new Vector3(transform.position.x, transform.position.y + .3f, transform.position.z) + transform.forward / 2, -transform.up);

       
        return Physics.Raycast(rayCenter, radiusCheckGround,whatIsGround) ||
            Physics.Raycast(transform.position + -transform.forward / 2, -transform.up, radiusCheckGround, whatIsGround) ||
            Physics.Raycast(transform.position + transform.forward / 2, -transform.up, radiusCheckGround, whatIsGround);
        }

    private Vector3 AdjustVelocityToSlope(Vector3 velocity)
        {

        Transform checker = lastVerticalInput < 0 ? checkSlopeBackward : checkSlopeForward;
        rayCheckSlope = new Ray(checker.position, checker.transform.forward);

        //Impulso en caso de que no llegue
        Physics.Raycast(rayCheckSlope, out RaycastHit hitInfo, lenghtCheckSlope, whatIsGround);
        if (hitInfo.distance > .10f)
            {
            rb.position += transform.forward * (checker == checkSlopeBackward ? -2f : 2f)*Time.deltaTime;
            print("Empujon");
            }
        if (hitInfo.normal == Vector3.zero)
            {
            Physics.Raycast(transform.position, -transform.up, out hitInfo, 5, whatIsGround);
            axisApplyGravity = Vector3.up;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down,out hit,5,  whatIsGround) && timeFalling >.4f)
                {
                print("Ejecuting");
                hitInfo.normal =hit.normal;
                }
            
            }
        else
            {
            axisApplyGravity = transform.up;
            
            }

         //Comprobación de normales para saber si ha tocado una cara nueva
        Vector3 newSlopeNormal = hitInfo.normal;
        if (newSlopeNormal != actualSlopeNormal)
            {
            actualSlopeNormal = newSlopeNormal;
            timerTransitionSlopes = 0f; 
            }
        else
            {
            timerTransitionSlopes += Time.deltaTime;
            //axisApplyGravity = transform.up;
        }
        //Cálculo del angulo al que va a girar.
        var slopeRotation = Quaternion.FromToRotation(transform.up, actualSlopeNormal);
        
        var rot = Quaternion.Lerp(transform.rotation,
                                slopeRotation * transform.rotation, slopesCurve.Evaluate(timerTransitionSlopes));
        Euler.text = (newSlopeNormal).ToString();
        //Euler.text = (lastSlopeNormal).ToString() + " Ultima Rampa";
        Euler2.text = (actualSlopeNormal).ToString();


        transform.rotation = rot;

        var adjustedVelocity = slopeRotation * velocity;

        if (adjustedVelocity.y < 0)
            {
            return adjustedVelocity;
            }

        return velocity;
        }

    void OnDrawGizmosSelected()
        {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        //Gizmos.DrawRay(transform.position, -transform.up);
        //Gizmos.DrawRay(transform.position + transform.forward, -transform.up);
        //Gizmos.DrawRay(transform.position + -transform.forward, -transform.up);
        Gizmos.DrawRay(rayCenter);
        Gizmos.DrawRay(rayFront);
        Gizmos.DrawRay(rayBack);

        //Gizmos.DrawSphere(transform.position, radiusCheckGround);
        Gizmos.color = Color.red;

        Gizmos.DrawRay(rayCheckSlope);

        }

    }
