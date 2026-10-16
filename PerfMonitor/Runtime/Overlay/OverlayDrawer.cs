using PerfMonitor.Core;
using UnityEngine;

namespace PerfMonitor.Overlay
{
    // Small helpers for drawing the panel with IMGUI
    public sealed class OverlayDrawer
    {
        static readonly Color Good = new Color(0.3f, 0.85f, 0.4f);
        static readonly Color Warning = new Color(0.95f, 0.8f, 0.2f);
        static readonly Color Bad = new Color(0.95f, 0.3f, 0.25f);

        readonly Texture2D _white;

        GUIStyle _labelStyle;
        GUIStyle _rightLabelStyle;
        GUIStyle _bigStyle;
        GUIStyle _buttonStyle;
        GUIStyle _verdictStyle;

        public OverlayDrawer()
        {
            _white = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        public void Destroy()
        {
            if (_white != null)
            {
                Object.Destroy(_white);
            }
        }

        // Scales the whole panel so it stays readable on phones
        public void BeginFrame()
        {
            float scale = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height) / 540f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            CreateStylesOnce();
        }

        public bool Button(Rect rect, string text)
        {
            return GUI.Button(rect, text, _buttonStyle);
        }

        public void Label(Rect rect, string text)
        {
            GUI.Label(rect, text, _labelStyle);
        }

        public void BigLabel(Rect rect, string text)
        {
            GUI.Label(rect, text, _bigStyle);
        }

        // Colored one line verdict such as "GPU bound"
        public void DrawVerdict(Rect rect, BudgetReport report)
        {
            _verdictStyle.normal.textColor = report.IsHealthy ? Good : Bad;
            GUI.Label(rect, report.Text, _verdictStyle);
        }

        public void FillRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _white);
            GUI.color = previous;
        }

        // Draws a name, the value text and a colored bar. Returns the y for the next row
        public float DrawBar(float x, float y, float width, string name, float percent, string extraText)
        {
            bool available = percent >= 0f;
            string value = available ? $"{percent:0}%" : "n/a";
            if (available && extraText.Length > 0)
            {
                value += "  " + extraText;
            }

            GUI.Label(new Rect(x, y, width * 0.62f, 16f), name, _labelStyle);
            GUI.Label(new Rect(x + width * 0.38f, y, width * 0.62f, 16f), value, _rightLabelStyle);

            var track = new Rect(x, y + 16f, width, 6f);
            FillRect(track, new Color(1f, 1f, 1f, 0.15f));
            if (available)
            {
                var fill = new Rect(track.x, track.y, track.width * Mathf.Clamp01(percent / 100f), track.height);
                FillRect(fill, GetLoadColor(percent));
            }
            return y + 28f;
        }

        // One thin bar per stored frame, taller means slower
        public void DrawFrameGraph(Rect area, FrameHistory history)
        {
            const float graphMaxMs = 50f;

            FillRect(area, new Color(1f, 1f, 1f, 0.08f));
            float barWidth = area.width / history.Capacity;
            int firstSlot = history.Capacity - history.Count;

            for (int i = 0; i < history.Count; i++)
            {
                float ms = history.GetFromOldest(i);
                float height = Mathf.Clamp01(ms / graphMaxMs) * area.height;
                var bar = new Rect(area.x + (firstSlot + i) * barWidth, area.yMax - height, Mathf.Max(barWidth, 1f), height);
                FillRect(bar, GetFrameColor(ms));
            }
        }

        static Color GetLoadColor(float percent)
        {
            if (percent < 60f)
            {
                return Good;
            }
            return percent < 85f ? Warning : Bad;
        }

        static Color GetFrameColor(float ms)
        {
            if (ms <= 17.5f)
            {
                return Good;
            }
            return ms <= 34f ? Warning : Bad;
        }

        void CreateStylesOnce()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, padding = new RectOffset(0, 0, 0, 0) };
            _labelStyle.normal.textColor = Color.white;
            _rightLabelStyle = new GUIStyle(_labelStyle) { alignment = TextAnchor.UpperRight };
            _bigStyle = new GUIStyle(_labelStyle) { fontSize = 22, fontStyle = FontStyle.Bold };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            _verdictStyle = new GUIStyle(_labelStyle) { fontSize = 12, fontStyle = FontStyle.Bold };
        }
    }
}
