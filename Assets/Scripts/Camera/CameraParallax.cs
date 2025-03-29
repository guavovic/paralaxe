using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SilverGames.Metroidvania.Camera
{

    public class CameraParallax : MonoBehaviour
    {
        private float length, startpos;
        public GameObject cam;
        public float parallaxEffect;

        // Start is called before the first frame update
        void Start()
        {
            startpos = transform.position.x;
            length = transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            MoveParallax();
        }

        private void MoveParallax()
        {
            float dist = (cam.transform.position.x * parallaxEffect);
            transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
        }
    }
}