using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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