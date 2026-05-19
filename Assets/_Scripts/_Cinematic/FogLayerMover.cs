using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    public class FogLayerMover : MonoBehaviour
    {
        private float speed;
        private Image image;
        private float phase;

        public void Init(float speed)
        {
            this.speed = speed;
            this.image = GetComponent<Image>();
            this.phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (image == null) return;

            phase += Time.deltaTime * (speed * 0.02f);
            float pulse = 0.55f + 0.35f * Mathf.Sin(phase);
            Color c = image.color;
            image.color = new Color(c.r, c.g, c.b, c.a * (0.8f + 0.2f * pulse));
        }
    }
}
