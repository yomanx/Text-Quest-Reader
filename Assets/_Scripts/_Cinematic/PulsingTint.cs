using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    public class PulsingTint : MonoBehaviour
    {
        private Image image;
        private Color baseColor;
        private float period;
        private float phase;

        public void Init(Color color, float period)
        {
            this.image = GetComponent<Image>();
            this.baseColor = color;
            this.period = Mathf.Max(0.1f, period);
            this.phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (image == null) return;

            phase += Time.deltaTime * (Mathf.PI * 2f / period);
            float wave = (Mathf.Sin(phase) * 0.5f + 0.5f);
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (0.55f + 0.45f * wave));
        }
    }
}
