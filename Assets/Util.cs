using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Util {

    public static JointType GetMirrorJoint(JointType joint)
    {
        switch (joint)
        {
            case JointType.ShoulderLeft:
                return JointType.ShoulderRight;
            case JointType.ElbowLeft:
                return JointType.ElbowRight;
            case JointType.WristLeft:
                return JointType.WristRight;
            case JointType.HandLeft:
                return JointType.HandRight;

            case JointType.ShoulderRight:
                return JointType.ShoulderLeft;
            case JointType.ElbowRight:
                return JointType.ElbowLeft;
            case JointType.WristRight:
                return JointType.WristLeft;
            case JointType.HandRight:
                return JointType.HandLeft;

            case JointType.HipLeft:
                return JointType.HipRight;
            case JointType.KneeLeft:
                return JointType.KneeRight;
            case JointType.AnkleLeft:
                return JointType.AnkleRight;
            case JointType.FootLeft:
                return JointType.FootRight;

            case JointType.HipRight:
                return JointType.HipLeft;
            case JointType.KneeRight:
                return JointType.KneeLeft;
            case JointType.AnkleRight:
                return JointType.AnkleLeft;
            case JointType.FootRight:
                return JointType.FootLeft;

            case JointType.HandTipLeft:
                return JointType.HandTipRight;
            case JointType.ThumbLeft:
                return JointType.ThumbRight;

            case JointType.HandTipRight:
                return JointType.HandTipLeft;
            case JointType.ThumbRight:
                return JointType.ThumbLeft;
        }

        return joint;
    }
}
