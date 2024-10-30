using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamX
{
    public class PlayerObserver : MonoBehaviour
    {
        private float updateInterval = 0.15f;
        private float timer = 0f;

        private Vector3 position = Vector3.zero;
        private Vector3 euler = Vector3.zero;
        private Vector3 lastPosition = Vector3.zero;
        private Vector3 lastEuler = Vector3.zero;

        public Action<PlayerTransformData> TransformChange;

        public void Update()
        {
            position = transform.position;
            euler = transform.eulerAngles;

            timer += Time.deltaTime;

            if(timer >= updateInterval)
            {
                if(position != lastPosition || euler != lastEuler)
                {
                    TransformChange?.Invoke(new PlayerTransformData() { playerID = -1, position = position, euler = euler });
                }

                lastPosition = position;
                lastEuler = euler;

                timer = 0f;
            }
        }

        private void OnDestroy()
        {
            TransformChange = null;
        }
    }
}
