using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class HandScript : MonoBehaviour
{

    public GameObject BodySourceManager;
    public Windows.Kinect.JointType joint;
    public GameObject SpawnManager;
    private BodySourceManager bodyManager;
    private SpawnManager spawnManager;
    private Body[] bodies;

    private bool gripping;
    private GameObject pickedObject;
    private bool pickedUp;


    // Use this for initialization
    void Start()
    {
        if (BodySourceManager == null)
        {
            Debug.Log("No Body Source Manager");
        }
        else
        {
            bodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        }
        if(SpawnManager == null)
        {
            Debug.Log("No Spawn Manager");
        }
        else
        {
            spawnManager = SpawnManager.GetComponent<SpawnManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bodyManager == null)
        {
            return;
        }
        bodies = bodyManager.GetData();

        if (bodies == null)
        {
            return;
        }
        foreach (var body in bodies)
        {
            if (body == null)
            {
                continue;
            }
            if (body.IsTracked)
            {

                if ((joint == Windows.Kinect.JointType.HandLeft && body.HandLeftState.Equals(Windows.Kinect.HandState.Closed))|| (joint == Windows.Kinect.JointType.HandRight && body.HandRightState.Equals(Windows.Kinect.HandState.Closed)))
                {
                    gripping = true;
                }
                else
                {
                    gripping = false;
                }


            }
        }
        if (pickedUp && gripping && pickedObject)
        {
            pickedObject.transform.position = this.transform.position;
        }
        else if (pickedUp && !gripping)
        {
            pickedObject.GetComponent<Collider>().enabled = true;
            pickedObject = null;
            pickedUp = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Draggable" && gripping && !pickedUp)
        {
            pickedObject = other.gameObject;
            pickedObject.GetComponent<Collider>().enabled = false;
            pickedUp = true;
        }
        if (other.gameObject.tag == "Bowl" && pickedUp)
        {
            //points
            Debug.Log("Hit");
            Destroy(pickedObject);
            spawnManager.Spawn();
            pickedUp = false;
            pickedObject = null;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Draggable" && gripping && !pickedUp)
        {
            pickedObject = other.gameObject;
            pickedObject.GetComponent<Collider>().enabled = false;
            pickedUp = true;
        }
        
    }
}