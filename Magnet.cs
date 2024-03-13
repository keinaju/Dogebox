using UnityEngine;

public class Magnet : MonoBehaviour
{
    public Vector3 Target;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, Target, 13 * Time.deltaTime);
    }
}