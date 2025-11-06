using UnityEngine;
using System.Collections;
public class doorMechanics : MonoBehaviour
{
    public bool isOpen = false;
    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float Speed = 1f;
    [SerializeField] private float RotationAmount = 90f;
    [SerializeField] private float ForwardDirection = 0;

    private Vector3 StartRotation;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;


    private void Awake()
    {
        StartRotation = transform.rotation.eulerAngles;
        Forward = transform.right;
    }

    public void Open(Vector3 Userposition)
    {
        if (!isOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }
            if (isRotatingDoor)
            {
                float dot = Vector3.Dot(Forward,(Userposition - transform.position).normalized);
                Debug.Log($"Dot: {dot.ToString("N3")}");
                AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
        }
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Quaternion startrotation = transform.rotation;
        Quaternion endrotation;

        if (ForwardAmount >= ForwardDirection)
        {
            endrotation = Quaternion.Euler(new Vector3(0, StartRotation.y - RotationAmount, 0));
        }
        else {
            endrotation = Quaternion.Euler(new Vector3(0, StartRotation.y + RotationAmount, 0));
        }

        isOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startrotation, endrotation, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }
    }

    public void Closed()
    {
        if (isOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            if (isRotatingDoor)
            {
                AnimationCoroutine = StartCoroutine(DoRotationClose());
            }
        }
    }

    private IEnumerator DoRotationClose()
    {
        Quaternion startrotation = transform.rotation;
        Quaternion endrotation = Quaternion.Euler(StartRotation);

        isOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startrotation, endrotation, time);
            yield return null;
            time += Time.deltaTime * Speed;
        }
    }
}
