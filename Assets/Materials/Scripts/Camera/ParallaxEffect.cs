using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxChunk
    {
        public Transform[] layers;
    }

    [Range(0, 1)] public float[] parallaxFactors; 
    public Transform camTransform;
    public Transform backgroundRoot;
    private ParallaxChunk[] chunks;
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        BuildChunks();
    }

    private void Start()
    {
        lastCameraPosition = camTransform.position;
    }
    
    // Обновление параллакса
    private void LateUpdate()
    {
        Vector3 cameraDelta = camTransform.position - lastCameraPosition;

        foreach (ParallaxChunk chunk in chunks)
        {
            for (int i = 0; i < chunk.layers.Length; i++)
            {
                Transform layer = chunk.layers[i];

                // берём коэффициент по индексу слоя
                float factor = (i < parallaxFactors.Length) ? parallaxFactors[i] : 0f;

                float moveX = cameraDelta.x * (1 - factor);
                float moveY = cameraDelta.y * (1 - factor);

                layer.position += new Vector3(moveX, moveY, 0);
            }
        }

        lastCameraPosition = camTransform.position;
    }

    // Сборка структуры заднего фона
    private void BuildChunks()
    {
        List<ParallaxChunk> chunkList = new List<ParallaxChunk>();

        foreach (Transform chunkTransform in backgroundRoot)
        {
            ParallaxChunk chunk = new ParallaxChunk();

            List<Transform> layers = new List<Transform>();

            foreach (Transform layer in chunkTransform)
            {
                layers.Add(layer);
            }

            chunk.layers = layers.ToArray();
            chunkList.Add(chunk);
        }

        chunks = chunkList.ToArray();
    }
}
