using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/*
Parameters controlled by the DNA:

Number of ray casts
Angle of rays
Ray Size
Forward acceleration
Max forward speed
Turn Speed 

 
 
 
 */




public class AIController : MonoBehaviour
{
        
    //private float turnInput;
    //private float moveInput;
    public Rigidbody sphereRB;
    public Transform raySpawn;

    [Header("MaxValues")]
    [SerializeField] int MaxRays = 12;
    [SerializeField] int maxRaySize = 100;
    [SerializeField] float maxAngleOfRays = 75;
    [SerializeField] float maxForwardSpeed = 4000f;
    [SerializeField] float minForwardSpeed = 125f;
    [SerializeField] float maxForwardIncrement = 2000f;
    [SerializeField] float minForwardIncrement = 300f;
    [SerializeField] float maxTurnSpeed = 350;

    [SerializeField] int rays = 12;
    [SerializeField] float raySize = 100;
    [SerializeField] float angleCovered = 75;
    [SerializeField] float forwardIncrement = 4000f;
    [SerializeField] float turnSpeed = 350;
    [SerializeField] float maxSpeed = 350;

    float sumLeftHits = 0f;
    float sumRightHits = 0f;

    public Transform CheckPointHolder;
    public int CheckPointsPassedInOrder = 0;
    int nextCheckpoint = 0;

    public float timeAlive = 0f;
    public bool wentTheWrongWay = false;

    private void Start()
    {

        sphereRB.transform.parent = null;
    }

    void Update()
    {
        if(this.gameObject.activeInHierarchy)
            timeAlive += Time.deltaTime;
        sumLeftHits = 0f;
        sumRightHits = 0f;

        transform.position = sphereRB.position;

        // cast rays
       
        float angleIncrement = angleCovered / rays;
        LayerMask mask = LayerMask.GetMask("Wall");
        // figure out the vector using the angle increment and the rigth Vector
        for (int i = 0; i <= rays;i++)
        {
            /*  Given a vector A and the angle θ between A and the vector B, the vector B can be calculated as:
                B = ||B|| cos(θ) u + ||B|| sin(θ) v
            //  using unit vectors and u = transform.forward, v = transform.right 
            */
            angleIncrement = (angleCovered / rays) * i + (90 - angleCovered/2);
            float angleInRadians = angleIncrement * Mathf.Deg2Rad;
            Vector3 dir = transform.forward * Mathf.Cos(angleInRadians) + transform.right * Mathf.Sin(angleInRadians);
            Debug.DrawRay(raySpawn.position, dir * raySize, Color.red);
            RaycastHit raycastHit;
            if( Physics.Raycast(raySpawn.position, dir, out raycastHit, raySize, mask))
            {
                // map the values to -1 to 1 using the dot product of -transform.forward
                float dotBackwards = Vector3.Dot(dir, transform.forward * -1);

                // clamp the values
                dotBackwards /= Mathf.Abs(dotBackwards);
                float dotUP = Vector3.Dot(transform.right, dir);

                float rot = dotBackwards * dotUP;

                // update the sum according to the side of the ray

                if (angleIncrement > 90)
                    sumRightHits += rot;
                else if (angleIncrement < 90)
                    sumLeftHits += rot;
            }
                       
        }

        float newRotation;
        if (Mathf.Abs(sumLeftHits) > Mathf.Max(sumRightHits))
        {
            newRotation =  turnSpeed  * Time.deltaTime;
        }
        else 
        {
            newRotation = -turnSpeed  * Time.deltaTime;
        }
        transform.Rotate(0, newRotation, 0, Space.World);

        // if the car has barely moved and enough time has passed then disable it
        if(CheckPointsPassedInOrder < 2 && timeAlive > 30f)
        {
            this.gameObject.SetActive(false);
        }

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (sphereRB.velocity.magnitude < maxSpeed)
        {
            sphereRB.AddForce(transform.right * forwardIncrement, ForceMode.Acceleration);
        }

    }


    public void ApplyGenes(DNA<float> DNA)
    {
        rays = (int)(DNA.Genes[0] * MaxRays);
        angleCovered = DNA.Genes[1] * maxAngleOfRays;
        raySize = DNA.Genes[2] * maxRaySize;
        forwardIncrement = DNA.Genes[3] * maxForwardIncrement;
        if(forwardIncrement < minForwardIncrement)
        {
            forwardIncrement = minForwardIncrement;
        }
        maxSpeed = DNA.Genes[4] * maxForwardSpeed;
        if(maxSpeed > minForwardSpeed)
        {
            maxSpeed = minForwardSpeed;
        }
        turnSpeed = DNA.Genes[5] * maxTurnSpeed;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision != null)
        {
            if (collision.gameObject.CompareTag("Rail"))
            {
                transform.gameObject.SetActive(false);
            }
        }
 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject)
        {
            if (nextCheckpoint < CheckPointHolder.childCount)
            {
                if (other.gameObject.Equals(CheckPointHolder.GetChild(nextCheckpoint).gameObject))
                {
                    nextCheckpoint++;
                    CheckPointsPassedInOrder++;
                    if (nextCheckpoint >= CheckPointHolder.childCount)
                        nextCheckpoint = 0;
                    if(CheckPointsPassedInOrder > CheckPointHolder.childCount)
                        transform.gameObject.SetActive(false);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                    wentTheWrongWay = true;
                }
            }

        }

    }

}
