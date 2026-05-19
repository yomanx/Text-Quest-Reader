using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic
{
    /// <summary>
    /// Lightweight UI particle field — generates N child Images that drift across
    /// the canvas. No Unity ParticleSystem dependency, no shaders. Cheap and
    /// reliable for atmospheric overlays.
    /// </summary>
    public class ParticleField : MonoBehaviour
    {
        public enum ShapeKind { Dot, LineVertical, Twinkle }

        private struct Particle
        {
            public RectTransform rt;
            public Image image;
            public Vector2 pos;
            public Vector2 velocity;
            public float lifetime;
            public float age;
            public float baseAlpha;
            public float twinklePhase;
        }

        private Particle[] particles;
        private Color baseColor;
        private Vector2 baseVelocity;
        private Vector2 sizeRange;
        private ShapeKind shape;
        private RectTransform rectTransform;

        public void Init(Color color, Vector2 velocity, int count, Vector2 sizeRange, ShapeKind shape)
        {
            this.baseColor = color;
            this.baseVelocity = velocity;
            this.sizeRange = sizeRange;
            this.shape = shape;
            this.rectTransform = (RectTransform)transform;
            this.particles = new Particle[Mathf.Max(1, count)];

            for (int i = 0; i < particles.Length; i++)
                SpawnParticle(ref particles[i], randomLife: true);
        }

        private void SpawnParticle(ref Particle p, bool randomLife)
        {
            if (p.rt == null)
            {
                GameObject go = new GameObject("p", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                p.rt = (RectTransform)go.transform;
                p.image = go.GetComponent<Image>();
                p.image.raycastTarget = false;
                p.image.color = baseColor;
            }

            Vector2 size = shape == ShapeKind.LineVertical
                ? new Vector2(Random.Range(sizeRange.x, sizeRange.y) * 0.6f, Random.Range(sizeRange.x, sizeRange.y) * 6f)
                : new Vector2(Random.Range(sizeRange.x, sizeRange.y), Random.Range(sizeRange.x, sizeRange.y));

            p.rt.sizeDelta = size;
            p.rt.anchorMin = new Vector2(0.5f, 0.5f);
            p.rt.anchorMax = new Vector2(0.5f, 0.5f);

            Rect bounds = rectTransform.rect;
            float halfW = bounds.width * 0.5f;
            float halfH = bounds.height * 0.5f;

            float startX, startY;

            if (baseVelocity.y < -10f) { startX = Random.Range(-halfW, halfW); startY = halfH + 20f; }
            else if (baseVelocity.y > 10f) { startX = Random.Range(-halfW, halfW); startY = -halfH - 20f; }
            else if (baseVelocity.x > 10f) { startX = -halfW - 20f; startY = Random.Range(-halfH, halfH); }
            else if (baseVelocity.x < -10f) { startX = halfW + 20f; startY = Random.Range(-halfH, halfH); }
            else { startX = Random.Range(-halfW, halfW); startY = Random.Range(-halfH, halfH); }

            p.pos = new Vector2(startX, startY);
            p.rt.anchoredPosition = p.pos;
            p.velocity = baseVelocity + new Vector2(Random.Range(-6f, 6f), Random.Range(-6f, 6f));
            p.lifetime = 3.5f + Random.Range(0f, 2.5f);
            p.age = randomLife ? Random.Range(0f, p.lifetime) : 0f;
            p.baseAlpha = baseColor.a;
            p.twinklePhase = Random.Range(0f, Mathf.PI * 2f);
            p.image.color = new Color(baseColor.r, baseColor.g, baseColor.b, p.baseAlpha);
        }

        private void Update()
        {
            if (particles == null) return;

            Rect bounds = rectTransform.rect;
            float halfW = bounds.width * 0.5f;
            float halfH = bounds.height * 0.5f;

            for (int i = 0; i < particles.Length; i++)
            {
                ref Particle p = ref particles[i];
                p.age += Time.deltaTime;

                if (shape == ShapeKind.Twinkle)
                {
                    float a = Mathf.Abs(Mathf.Sin(Time.time + p.twinklePhase));
                    p.image.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * a);
                }
                else
                {
                    float fadeIn = Mathf.Clamp01(p.age / 0.5f);
                    float fadeOut = Mathf.Clamp01((p.lifetime - p.age) / 0.8f);
                    float alpha = baseColor.a * Mathf.Min(fadeIn, fadeOut);
                    p.image.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                }

                p.pos += p.velocity * Time.deltaTime;
                p.rt.anchoredPosition = p.pos;

                bool outOfBounds = p.pos.x < -halfW - 40f || p.pos.x > halfW + 40f || p.pos.y < -halfH - 40f || p.pos.y > halfH + 40f;
                if (p.age >= p.lifetime || outOfBounds)
                    SpawnParticle(ref p, randomLife: false);
            }
        }
    }
}
