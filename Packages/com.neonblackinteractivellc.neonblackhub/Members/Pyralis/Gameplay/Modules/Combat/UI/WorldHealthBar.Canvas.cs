using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class WorldHealthBar
    {
        private void BuildCanvas()
        {
            float ch = CW * (barSize.y / Mathf.Max(0.001f, barSize.x));
            float scale = barSize.x / CW;

            GameObject root = new GameObject("WorldHealthBar");
            root.transform.SetParent(transform);
            root.transform.localPosition = barOffset;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = new Vector3(scale, scale, scale);

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingLayerName = sortingLayerName;
            canvas.sortingOrder = sortingOrderInLayer;
            canvas.overrideSorting = true;
            root.AddComponent<CanvasScaler>();

            _group = root.AddComponent<CanvasGroup>();
            _group.interactable = false;
            _group.blocksRaycasts = false;

            _canvasRoot = root.transform;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(CW, ch);

            Image borderImg = null;
            if (borderPx > 0f)
                borderImg = MakeRect(root, "Border", borderColor, CW + borderPx * 2f, ch + borderPx * 2f);

            Image bgImg = MakeRect(root, "BG", bgColor, CW, ch);
            Image emptyImg = MakeRect(root, "EmptyHP", emptyHpColor, CW, ch);
            if (showGhostBar)
                _ghost = MakeFill(root, "Ghost", ghostColor, CW, ch);

            _fill = MakeFill(root, "Fill", fillColor, CW, ch);
            MakeSegments(root, CW, ch);

            if (showName)
                _nameLabel = MakeLabel(
                    root,
                    "Name",
                    nameColor,
                    nameSize,
                    nameFontStyle,
                    nameFont,
                    new Vector2(CW, 28f),
                    new Vector2(0f, ch * 0.5f + 14f));

            if (showHpNumbers)
                _hpLabel = MakeLabel(
                    root,
                    "HP",
                    hpNumberColor,
                    hpSize,
                    hpFontStyle,
                    hpFont,
                    new Vector2(CW * 0.9f, ch),
                    Vector2.zero);

            if (cornerRadius > 0f)
            {
                Sprite s = GetBarSprite();
                float ppu = BarSpriteTexH / ch;
                if (borderImg != null) SetRounded(borderImg, s, BarSpriteTexH / (ch + borderPx * 2f));
                SetRounded(bgImg, s, ppu);
                SetRounded(emptyImg, s, ppu);
                if (_ghost != null) SetRounded(_ghost, s, ppu);
                SetRounded(_fill, s, ppu);
            }
        }

        private Image MakeRect(
            GameObject parent,
            string goName,
            Color color,
            float w,
            float h,
            Vector2 anchoredPos = default)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent.transform, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = anchoredPos;
            return img;
        }

        private Image MakeFill(GameObject parent, string goName, Color color, float w, float h)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent.transform, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(w, 0f);
            return img;
        }

        private void MakeSegments(GameObject parent, float w, float h)
        {
            if (segmentCount <= 1) return;

            float lineW = Mathf.Max(1f, w * 0.005f);
            for (int i = 1; i < segmentCount; i++)
            {
                float xPos = ((float)i / segmentCount - 0.5f) * w;
                Image line = MakeRect(parent, $"Seg{i}", segmentColor, lineW, h);
                line.rectTransform.anchoredPosition = new Vector2(xPos, 0f);
            }
        }

        private TMP_Text MakeLabel(
            GameObject parent,
            string goName,
            Color color,
            float fontSize,
            FontStyles fontStyle,
            TMP_FontAsset font,
            Vector2 sizeDelta,
            Vector2 anchoredPos)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent.transform, false);
            TMP_Text text = go.AddComponent<TextMeshProUGUI>();
            text.color = color;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            if (font != null) text.font = font;
            text.alignment = TextAlignmentOptions.Center;
            text.overflowMode = TextOverflowModes.Overflow;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return text;
        }

        private Sprite GetBarSprite()
        {
            if (_roundedSprite != null) return _roundedSprite;

            const int W = 64;
            const int H = BarSpriteTexH;
            int radius = Mathf.RoundToInt(cornerRadius * H);

            Texture2D tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    pixels[y * W + x] = InsideRounded(x, y, W, H, radius) ? Color.white : Color.clear;
            tex.SetPixels(pixels);
            tex.Apply();

            _roundedSprite = Sprite.Create(
                tex,
                new Rect(0, 0, W, H),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            return _roundedSprite;
        }

        private static bool InsideRounded(int px, int py, int w, int h, int radius)
        {
            if (radius <= 0) return true;

            int cx = px < radius ? radius : (px >= w - radius ? w - radius - 1 : px);
            int cy = py < radius ? radius : (py >= h - radius ? h - radius - 1 : py);
            int dx = px - cx;
            int dy = py - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static void SetRounded(Image img, Sprite sprite, float pixelsPerUnit)
        {
            if (img == null || sprite == null) return;

            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = pixelsPerUnit;
        }
    }
}
