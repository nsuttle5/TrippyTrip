using UnityEngine;

public class SideObjectScroll : MonoBehaviour
{
    public float m_destroyZ;

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, 0, -CarMovement.scrollSpeed * Time.deltaTime * 10);

        if (transform.position.z < m_destroyZ)
            Destroy(gameObject);
    }
}
