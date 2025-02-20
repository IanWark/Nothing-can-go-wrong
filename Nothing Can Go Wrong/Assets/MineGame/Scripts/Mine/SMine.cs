using UnityEngine;

public class SMine : Explosive
{
    [Header("Launch sequence")]
    [Tooltip("")]
    [SerializeField]
    private float LaunchForce;
    [Tooltip("")]
    [SerializeField]
    private float LaunchTimeSeconds;

    [Header("Audio")]
    [SerializeField]
    private AudioClip LaunchSound;

    [Header("References")]
    [SerializeField]
    private Rigidbody Rigidbody;
    [SerializeField]
    private AudioSource AudioSource;

    bool isLaunching = false;
    float timeSinceLaunch = 0f;

    public override void Activate(Collider collider)
    {
        if (!isLaunching)
        {
            Debug.Log($"LAUNCH MINE!!! Triggered by: {collider.name}");

            Rigidbody.useGravity = true;
            Rigidbody.AddForce(transform.up * LaunchForce, ForceMode.VelocityChange);

            AudioSource.PlayOneShot(LaunchSound);
            isLaunching = true;
        }
    }

    private void Update()
    {
        if (isLaunching)
        {
            timeSinceLaunch += Time.deltaTime;

            if (timeSinceLaunch > LaunchTimeSeconds)
            {
                Explode();
            }
        }
    }
}
