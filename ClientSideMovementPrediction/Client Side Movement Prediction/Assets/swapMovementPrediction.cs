using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swapMovementPrediction : MonoBehaviour
{
    public void setPredictionMethod(int i)
    {
        switch(i)
        {
            case 1:
                PlayerController.isOn = true;
                break;
            case 2:
                Prediction1.isOn = true;
                break;
            case 3:
                Prediction2.isOn = true;
                break;
        }
    }
}
