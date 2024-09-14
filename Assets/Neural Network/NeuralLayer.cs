using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NeuralLayer
{
    //REPRESENTS THE NODES WITHIN A LAYER AND IT'S WEIGHTS AND BIASES
    public int inputNumber;
    public int nodeNumber;

    public float[] nodeArray;
    public float[,] weightsArray;

    public float[] biasesArray;

    //CREATES NEW LAYER
    public NeuralLayer(int inputNumber, int nodeNumber)
    {
        this.inputNumber = inputNumber;
        this.nodeNumber = nodeNumber;

        weightsArray = new float[nodeNumber, inputNumber];
        biasesArray = new float[nodeNumber];
    }

    
    //TAKES IN INPUT ARRAY AND RETURNS OUTPUT ARRAY
    //OUTPUT USED AS INPUT FOR NEXT LAYER (FORWARD PROPAGATION)
    public void Forward(float[] inputsArray)
    {
        nodeArray = new float[nodeNumber];

        for (int i = 0; i < nodeNumber; i++)
        {
            //SUM OF WEIGHTS X INPUTS
            for (int j = 0; j < inputNumber; j++)
            {
                nodeArray[i] += weightsArray[i, j] * inputsArray[j];
            }

            //ADDS THE NODE'S BIAS
            nodeArray[i] += biasesArray[i];
        }
    }

    //DECIDES WHICH NODE WILL BE ACTIVATED
    public void Activation()
    {
        //RELU ACTIVATION FUNCTION
        for (int i = 0; i < nodeArray.Length; i++)
        {
            if (nodeArray[i] < 0)
            {
                nodeArray[i] = 0;
            }
        }
    }

    //MUTATES THE LAYER FOR EVOLUTION
    //RANDOMLY CHANGES WEIGHTS AND BIASES
    public void MutateLayer(float mutationChance, float mutationAmount)
    {
        for (int i = 0; i < nodeNumber; i++)
        {
            for (int j = 0; j < inputNumber; j++)
            {
                if (Random.value < mutationChance)
                {
                    weightsArray[i, j] += Random.Range(-1.0f, 1.0f) * mutationAmount;
                }
            }

            if (Random.value < mutationChance)
            {
                biasesArray[i] += Random.Range(-1.0f, 1.0f) * mutationAmount;
            }
        }
    }
}
