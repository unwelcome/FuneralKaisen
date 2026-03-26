using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;
        public bool parallaxY = true;
        [Range(0, 1)] public float parallaxFactor;
    }

    public ParallaxLayer[] layers;
    public Transform camTransform;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        lastCameraPosition = camTransform.position;
    }
    

    private void LateUpdate()
    {
        Vector3 cameraDelta = camTransform.position - lastCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            float moveX = cameraDelta.x * (1 - layer.parallaxFactor);
            float moveY = layer.parallaxY ? cameraDelta.y * (1 - layer.parallaxFactor) : 0;

            layer.layer.position += new Vector3(moveX, moveY, 0);
        }

        lastCameraPosition = camTransform.position;
    }
}
