using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    [SerializeField] private int[] _networkShape = { 0, 0, 0, 0};
    [SerializeField] public NeuralLayer[] layers;

    public void Awake()
    {
        layers = new NeuralLayer[_networkShape.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new NeuralLayer(_networkShape[i], _networkShape[i + 1]);
        }

        //This ensures that the random numbers we generate aren't the same pattern each time. 
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    //FORWARDS PROPAGATES THE INPUT THROUGH THE NETWORK
    public float[] Brain(float[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0) //FIRST HIDDEN LAYER
            {
                layers[i].Forward(inputs);
                layers[i].Activation();
            }
            else if (i == layers.Length - 1) //FINAL LAYER
            {
                layers[i].Forward(layers[i - 1].nodeArray);
            }
            else
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].Activation();
            }
        }

        return (layers[layers.Length - 1].nodeArray); //RETURNS THE OUTPUT WHICH WILL BE USED
    }

    //COPIES THE WEIGHTS AND BIASES TO ANOTHER LAYER
    public NeuralLayer[] CopyLayers()
    {
        NeuralLayer[] tmpLayers = new NeuralLayer[_networkShape.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            tmpLayers[i] = new NeuralLayer(_networkShape[i], _networkShape[i + 1]);
            System.Array.Copy(layers[i].weightsArray, tmpLayers[i].weightsArray, layers[i].weightsArray.GetLength(0) * layers[i].weightsArray.GetLength(1));
            System.Array.Copy(layers[i].biasesArray, tmpLayers[i].biasesArray, layers[i].biasesArray.GetLength(0));
        }
        return (tmpLayers);
    }

    //RANDOMLY MUTATES EVERY LAYER IN NETWORK
    public void MutateNetwork(float mutationChance, float mutationAmount)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].MutateLayer(mutationChance, mutationAmount);
        }
    }
}
