using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class Skeleton : MonoBehaviour {

    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public GameObject SpawnManger;
    public GameObject Camera;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private Vector3 posOffset;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    private Dictionary<Kinect.JointType, Vector3> _InitialPos = new Dictionary<Kinect.JointType, Vector3>();


    // Update is called once per frame
    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    StartCoroutine(ExecuteAfterTime(3));
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    posOffset = GetOffset(body, gameObject.transform.position);
                    
                    _InitialPos = GetInitialPos(body, posOffset);
                }
                
               RefreshBodyObject(body, _Bodies[body.TrackingId]);
               // var pos = _Bodies[body.TrackingId].transform.Find("SpineShoulder").position;
               // var rot = _Bodies[body.TrackingId].transform.Find("SpineShoulder").rotation;
               // Camera.transform.position = pos;
               // Camera.transform.rotation = rot;
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        body.transform.localScale = new Vector3(1,1, 1);
        body.transform.position = gameObject.transform.position;
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);

            jointObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
          if(jt == Kinect.JointType.HandLeft)
            {
                jointObj.AddComponent<HandScript>();
                jointObj.GetComponent<HandScript>().BodySourceManager = this.BodySourceManager;
                jointObj.GetComponent<HandScript>().joint = Kinect.JointType.HandLeft;
                jointObj.GetComponent<HandScript>().SpawnManager = SpawnManger;
            }
            if (jt == Kinect.JointType.HandRight)
            {
                jointObj.AddComponent<HandScript>();
                jointObj.GetComponent<HandScript>().BodySourceManager = this.BodySourceManager;
                jointObj.GetComponent<HandScript>().joint = Kinect.JointType.HandRight;
                jointObj.GetComponent<HandScript>().SpawnManager = SpawnManger;
            }
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.position = GetVector3FromJoint(jt ,body,posOffset);

            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if (targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.position);
                lr.SetPosition(1, GetVector3FromJoint(_BoneMap[jt], body,posOffset));
                lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }

    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return Color.green;

            case Kinect.TrackingState.Inferred:
                return Color.red;

            default:
                return Color.black;
        }
    }

    private Vector3 GetVector3FromJoint(Kinect.JointType joint,Kinect.Body body,Vector3 offset)
    {
        var pos = body.Joints[joint].Position;
    

        Vector3 initialJointPos = _InitialPos[joint];
        Vector3 movement = new Vector3(initialJointPos.x - pos.X, initialJointPos.y - pos.Y, initialJointPos.z - pos.Z);
        movement.x = -movement.x;
        movement.y = -movement.y;
        return (initialJointPos + movement + offset) * 2f;
    }

    private static Dictionary<Kinect.JointType,Vector3> GetInitialPos(Kinect.Body body, Vector3 offset)
    {
        Dictionary<Kinect.JointType, Vector3> positions = new Dictionary<Windows.Kinect.JointType, Vector3>();
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint joint = body.Joints[jt];
            Vector3 position = new Vector3(joint.Position.X,joint.Position.Y,joint.Position.Z);
            positions.Add(jt,position);
        }
        
        return positions;
    }

    private static Vector3 GetOffset(Kinect.Body body,Vector3 initialPos)
    {
        Vector3 initialJointPos;
        var pos = body.Joints[Kinect.JointType.SpineBase].Position;
        
        initialJointPos = new Vector3(pos.X, pos.Y, pos.Z);
   
        return  (initialPos - initialJointPos) * 0.5f;
        
    }
    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        // Code to execute after the delay
    }
}
