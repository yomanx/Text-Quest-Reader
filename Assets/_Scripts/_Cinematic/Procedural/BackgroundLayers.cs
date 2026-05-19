using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Cinematic.Procedural
{
    public class GradientLayer : MonoBehaviour
    {
        private Image image;

        public void Apply(Color top, Color bottom)
        {
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = ProceduralTextureFactory.CreateVerticalGradientSprite(top, bottom, 256);
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private void OnDestroy()
        {
            if (image != null && image.sprite != null)
            {
                Texture2D tex = image.sprite.texture;
                Destroy(image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class ScanlinesLayer : MonoBehaviour
    {
        private Image image;
        private float drift;

        public void Apply(float opacity)
        {
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = ProceduralTextureFactory.CreateScanlinesSprite(opacity, 4);
            image.color = Color.white;
            image.type = Image.Type.Tiled;
            image.preserveAspect = false;
            image.pixelsPerUnitMultiplier = 1f;
            image.material = null;
        }

        private void Update()
        {
            if (image == null) return;
            drift += Time.deltaTime * 12f;
            if (drift > 1024f) drift = 0f;
            RectTransform rt = image.rectTransform;
            Vector2 anchored = rt.anchoredPosition;
            anchored.y = (Mathf.Sin(Time.time * 0.4f) * 4f);
            rt.anchoredPosition = anchored;
        }

        private void OnDestroy()
        {
            if (image != null && image.sprite != null)
            {
                Texture2D tex = image.sprite.texture;
                Destroy(image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class VignetteLayer : MonoBehaviour
    {
        private Image image;

        public void Apply(Color color)
        {
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = ProceduralTextureFactory.CreateVignetteSprite(color, 256);
            image.color = Color.white;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }

        private void OnDestroy()
        {
            if (image != null && image.sprite != null)
            {
                Texture2D tex = image.sprite.texture;
                Destroy(image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class RadialGlowLayer : MonoBehaviour
    {
        private Image image;
        private float phase;
        private Color baseColor;

        public void Apply(Color color, float radius01, Vector2 offset01)
        {
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = ProceduralTextureFactory.CreateRadialGlowSprite(color, 1.8f, 256);
            image.color = Color.white;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
            baseColor = color;

            RectTransform parent = transform.parent as RectTransform;
            RectTransform rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            if (parent != null)
            {
                Vector2 sz = parent.rect.size;
                rt.sizeDelta = sz * Mathf.Clamp(radius01 * 2f, 0.3f, 2.5f);
                rt.anchoredPosition = new Vector2(sz.x * offset01.x, sz.y * offset01.y);
            }
            else
            {
                rt.sizeDelta = new Vector2(800f, 800f);
                rt.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            if (image == null) return;
            phase += Time.deltaTime * 0.6f;
            float wave = (Mathf.Sin(phase) * 0.5f + 0.5f);
            image.color = new Color(1f, 1f, 1f, 0.85f + 0.15f * wave);
        }

        private void OnDestroy()
        {
            if (image != null && image.sprite != null)
            {
                Texture2D tex = image.sprite.texture;
                Destroy(image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class GridLayer : MonoBehaviour
    {
        private Image image;
        private float drift;

        public void Apply(Color color, int spacing)
        {
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = ProceduralTextureFactory.CreateGridSprite(color, spacing, 1);
            image.color = Color.white;
            image.type = Image.Type.Tiled;
            image.preserveAspect = false;
            image.pixelsPerUnitMultiplier = 1f;
        }

        private void Update()
        {
            if (image == null) return;
            drift += Time.deltaTime * 6f;
            if (drift > 2048f) drift = 0f;
            RectTransform rt = image.rectTransform;
            Vector2 p = rt.anchoredPosition;
            p.y = Mathf.Sin(Time.time * 0.3f) * 6f;
            rt.anchoredPosition = p;
        }

        private void OnDestroy()
        {
            if (image != null && image.sprite != null)
            {
                Texture2D tex = image.sprite.texture;
                Destroy(image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class PulseTintLayer : MonoBehaviour
    {
        private Image image;
        private Color baseColor;
        private float period;
        private float phase;

        public void Apply(Color color, float period)
        {
            this.baseColor = color;
            this.period = Mathf.Max(0.1f, period);
            this.phase = Random.Range(0f, Mathf.PI * 2f);
            image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = baseColor;
            image.sprite = null;
            image.preserveAspect = false;
            image.type = Image.Type.Simple;
        }

        private void Update()
        {
            if (image == null) return;
            phase += Time.deltaTime * (Mathf.PI * 2f / period);
            float wave = (Mathf.Sin(phase) * 0.5f + 0.5f);
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (0.40f + 0.60f * wave));
        }
    }

    public class LightBeamsLayer : MonoBehaviour
    {
        private struct Beam
        {
            public RectTransform rt;
            public Image image;
            public float baseRotation;
            public float speed;
            public float phaseOffset;
        }

        private Beam[] beams;
        private Color baseColor;

        public void Apply(Color color, int count)
        {
            baseColor = color;
            beams = new Beam[Mathf.Max(1, count)];
            for (int i = 0; i < beams.Length; i++)
            {
                GameObject go = new GameObject("Beam" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(80f, 1200f);
                Image img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = ProceduralTextureFactory.CreateBeamSprite(color, 16, 256);
                img.color = Color.white;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;

                float baseRot = i * (360f / beams.Length) + Random.Range(-20f, 20f);
                rt.localEulerAngles = new Vector3(0f, 0f, baseRot);

                beams[i] = new Beam
                {
                    rt = rt,
                    image = img,
                    baseRotation = baseRot,
                    speed = Random.Range(2f, 6f),
                    phaseOffset = Random.Range(0f, Mathf.PI * 2f)
                };
            }
        }

        private void Update()
        {
            if (beams == null) return;
            for (int i = 0; i < beams.Length; i++)
            {
                float angle = beams[i].baseRotation + Mathf.Sin(Time.time * 0.15f + beams[i].phaseOffset) * 8f;
                beams[i].rt.localEulerAngles = new Vector3(0f, 0f, angle);

                float pulse = Mathf.Abs(Mathf.Sin(Time.time * 0.8f + beams[i].phaseOffset));
                beams[i].image.color = new Color(1f, 1f, 1f, 0.6f + 0.4f * pulse);
            }
        }

        private void OnDestroy()
        {
            if (beams == null) return;
            for (int i = 0; i < beams.Length; i++)
            {
                if (beams[i].image != null && beams[i].image.sprite != null)
                {
                    Texture2D tex = beams[i].image.sprite.texture;
                    Destroy(beams[i].image.sprite);
                    if (tex != null) Destroy(tex);
                }
            }
        }
    }

    public class StarsLayer : MonoBehaviour
    {
        private struct Star
        {
            public RectTransform rt;
            public Image image;
            public float twinkleSpeed;
            public float phase;
            public float baseAlpha;
        }

        private Star[] stars;
        private Color baseColor;
        private float twinkleSpeed;

        public void Apply(Color color, int count, float twinkleSpeed)
        {
            baseColor = color;
            this.twinkleSpeed = twinkleSpeed;

            RectTransform parent = transform as RectTransform;
            Vector2 size = parent.rect.size;
            if (size.x < 16f || size.y < 16f) size = new Vector2(800f, 600f);
            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            stars = new Star[Mathf.Max(1, count)];
            Sprite shared = ProceduralTextureFactory.CreateRadialGlowSprite(color, 2.5f, 32);
            for (int i = 0; i < stars.Length; i++)
            {
                GameObject go = new GameObject("Star" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                float starSize = Random.Range(2f, 6f);
                rt.sizeDelta = new Vector2(starSize, starSize);
                rt.anchoredPosition = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));

                Image img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = shared;
                img.color = Color.white;
                img.type = Image.Type.Simple;
                img.preserveAspect = false;

                stars[i] = new Star
                {
                    rt = rt,
                    image = img,
                    twinkleSpeed = Random.Range(0.5f, 1.6f) * twinkleSpeed,
                    phase = Random.Range(0f, Mathf.PI * 2f),
                    baseAlpha = Random.Range(0.55f, 1f)
                };
            }
        }

        private void Update()
        {
            if (stars == null) return;
            for (int i = 0; i < stars.Length; i++)
            {
                float a = Mathf.Abs(Mathf.Sin(Time.time * stars[i].twinkleSpeed + stars[i].phase));
                stars[i].image.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * stars[i].baseAlpha * (0.4f + 0.6f * a));
            }
        }

        private void OnDestroy()
        {
            if (stars != null && stars.Length > 0 && stars[0].image != null && stars[0].image.sprite != null)
            {
                Texture2D tex = stars[0].image.sprite.texture;
                Destroy(stars[0].image.sprite);
                if (tex != null) Destroy(tex);
            }
        }
    }

    public class SparkleLayer : MonoBehaviour
    {
        private struct Sparkle
        {
            public RectTransform rt;
            public Image image;
            public float lifetime;
            public float age;
            public float baseSize;
        }

        private Sparkle[] sparkles;
        private Color baseColor;
        private Sprite sharedSprite;

        public void Apply(Color color, int count = 32)
        {
            baseColor = color;
            sparkles = new Sparkle[Mathf.Max(1, count)];
            sharedSprite = ProceduralTextureFactory.CreateRadialGlowSprite(color, 2.2f, 24);
            for (int i = 0; i < sparkles.Length; i++)
                Spawn(ref sparkles[i], randomAge: true);
        }

        private void Spawn(ref Sparkle s, bool randomAge)
        {
            if (s.rt == null)
            {
                GameObject go = new GameObject("Sparkle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(transform, false);
                s.rt = (RectTransform)go.transform;
                s.rt.anchorMin = new Vector2(0.5f, 0.5f);
                s.rt.anchorMax = new Vector2(0.5f, 0.5f);
                s.rt.pivot = new Vector2(0.5f, 0.5f);
                s.image = go.GetComponent<Image>();
                s.image.raycastTarget = false;
                s.image.sprite = sharedSprite;
                s.image.preserveAspect = false;
                s.image.type = Image.Type.Simple;
            }

            RectTransform parent = transform as RectTransform;
            Vector2 size = parent.rect.size;
            if (size.x < 16f || size.y < 16f) size = new Vector2(800f, 600f);
            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            s.rt.anchoredPosition = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
            s.baseSize = Random.Range(6f, 14f);
            s.rt.sizeDelta = new Vector2(s.baseSize, s.baseSize);
            s.lifetime = Random.Range(1.0f, 2.4f);
            s.age = randomAge ? Random.Range(0f, s.lifetime) : 0f;
        }

        private void Update()
        {
            if (sparkles == null) return;
            for (int i = 0; i < sparkles.Length; i++)
            {
                ref Sparkle s = ref sparkles[i];
                s.age += Time.deltaTime;
                if (s.age >= s.lifetime)
                {
                    Spawn(ref s, randomAge: false);
                    continue;
                }
                float t = s.age / s.lifetime;
                float alpha = Mathf.Sin(t * Mathf.PI);
                s.image.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha);
                float scale = s.baseSize * (0.8f + 0.4f * alpha);
                s.rt.sizeDelta = new Vector2(scale, scale);
            }
        }

        private void OnDestroy()
        {
            if (sharedSprite != null)
            {
                Texture2D tex = sharedSprite.texture;
                Destroy(sharedSprite);
                if (tex != null) Destroy(tex);
            }
        }
    }
}
