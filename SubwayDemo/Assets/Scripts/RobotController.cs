using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    public CharacterController characterController;

    public float moveSpeed = 1f;

    public float rotateSpeed = 1f;

    public ParticleSystem particle;

    public bool enableCtrl = true;

    public bool enableShot = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (!enableCtrl)
        {
            if (enableShot)
            {
                if (!particle.isPlaying)
                {
                    particle.Play();
                }
            }
            else
            {
                if (particle.isPlaying)
                {
                    particle.Stop();
                }
            }
            return;
        }

        float h = -Input.GetAxis("Horizontal");
        float v = -Input.GetAxis("Vertical");

        if (v != 0f)
        {
            characterController.SimpleMove(transform.forward * -v * Time.deltaTime * moveSpeed);
        }

        if (h != 0f)
        {
            transform.Rotate(Vector3.up * Time.deltaTime * -rotateSpeed * h, Space.Self);
        }


        if (Input.GetKey(KeyCode.Space))
        {
            if (!particle.isPlaying)
            {
                particle.Play();
            }
        }
        else
        {
            if (particle.isPlaying)
            {
                particle.Stop();
            }
        }
    }
}
