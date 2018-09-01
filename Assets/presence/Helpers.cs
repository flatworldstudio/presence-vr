using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PresenceEngine
{


    public enum NuiSkeletonPositionIndex : int
    {
        HipCenter = 0,
        Spine = 1,
        ShoulderCenter = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19,
        Count = 20
    }


    public class RovingValue
    {


        int span;
        int index;
        float[] values;
        float total;
        float value;

        public RovingValue(int Span)
        {
            span = Span;
            values = new float[span];
            total = 0;
            index = 0;
            value = 0;
        }

        public void Rove(float Value)
        {
            // stash value

            values[index] = Value;

            // add value

            total += Value;

            // inc and subtract oldest value

            index = (index + 1) % span;

            total -= values[index];



            value = total / span;

        }

        public int Int
        {
            get
            {

                return (Mathf.RoundToInt(value));
            }


        }


        public float Float
        {
            get
            {

                return value;
            }
        }






    }
}