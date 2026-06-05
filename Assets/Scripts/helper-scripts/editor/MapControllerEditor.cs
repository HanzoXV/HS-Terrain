// Place this file in any Editor/ folder (e.g. Assets/Editor/MapControllerEditor.cs)
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapController))]
public class MapControllerEditor : Editor
{
    // Only the fields we draw manually — everything else renders automatically.
    static readonly string[] _manualFields = { "HeightMapPreviewTexture", "ComplexGeometry", "m_Script" };
    static readonly string[] _noiseFields = { "Octaves", "Persistence", "Lacunarity", "LoD" };

    SerializedProperty _complexGeometry;
    SerializedProperty _previewTexture;

    GUIStyle _headerStyle;
    GUIStyle _previewLabelStyle;
    bool _stylesInitialized;

    void OnEnable()
    {
        _complexGeometry = serializedObject.FindProperty("ComplexGeometry");
        _previewTexture = serializedObject.FindProperty("HeightMapPreviewTexture");
    }

    void InitStyles()
    {
        if (_stylesInitialized) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft,
        };
        _headerStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

        _previewLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
        };

        _stylesInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        InitStyles();

        var ctrl = (MapController)target;

        // ── All fields except manually-controlled ones ────────────────────────
        // New fields you add to MapController will appear here automatically,
        // respecting their [Header], [Range], and other attributes as normal.
        var excludes = CombineArrays(_manualFields, _noiseFields);

        DrawPropertiesExcluding(serializedObject, excludes);

        // ── ComplexGeometry toggle + conditional noise block ─────────────────
        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(_complexGeometry);

        if (_complexGeometry.boolValue)
        {
            EditorGUILayout.Space(6);
            SectionHeader("〰  Noise Settings");
            EditorGUI.indentLevel++;
            foreach (var name in _noiseFields)
                EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            EditorGUI.indentLevel--;
        }

        // ── Generate button ───────────────────────────────────────────────────
        EditorGUILayout.Space(10);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▶  Generate Terrain", GUILayout.Height(30), GUILayout.Width(200)))
                ctrl.GenerateMap();
            GUILayout.FlexibleSpace();
        }

        // ── Preview ───────────────────────────────────────────────────────────
        EditorGUILayout.Space(10);
        SectionHeader("🗺  Height Map Preview");

        var tex = _previewTexture.objectReferenceValue as Texture2D;

        if (tex != null)
        {
            float availableWidth = EditorGUIUtility.currentViewWidth - 32f;
            float previewSize = Mathf.Min(availableWidth, 512f);
            Rect previewRect = GUILayoutUtility.GetRect(previewSize, previewSize,
                                       GUILayout.ExpandWidth(true), GUILayout.Height(previewSize));

            EditorGUI.DrawRect(previewRect, new Color(0.08f, 0.08f, 0.08f));
            EditorGUI.DrawPreviewTexture(previewRect, tex, null, ScaleMode.ScaleToFit);
            EditorGUILayout.LabelField($"{tex.width} × {tex.height} px", _previewLabelStyle);
        }
        else
        {
            float size = EditorGUIUtility.currentViewWidth - 32f;
            Rect r = GUILayoutUtility.GetRect(size, Mathf.Min(size, 256f),
                              GUILayout.ExpandWidth(true), GUILayout.Height(Mathf.Min(size, 256f)));
            EditorGUI.DrawRect(r, new Color(0.1f, 0.1f, 0.1f));
            GUI.Label(r, "No preview — click Generate Terrain", _previewLabelStyle);
        }

        EditorGUILayout.Space(6);
        serializedObject.ApplyModifiedProperties();
    }

    void SectionHeader(string label)
    {
        EditorGUILayout.Space(4);
        Rect r = GUILayoutUtility.GetRect(1, 22, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(r, new Color(0.18f, 0.18f, 0.18f));
        GUI.Label(new RectOffset(8, 0, 0, 0).Remove(r), label, _headerStyle);
        EditorGUILayout.Space(4);
    }

    static string[] CombineArrays(string[] a, string[] b)
    {
        var result = new string[a.Length + b.Length];
        a.CopyTo(result, 0);
        b.CopyTo(result, a.Length);
        return result;
    }
}