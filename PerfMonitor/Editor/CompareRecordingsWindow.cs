using System;
using System.Collections.Generic;
using PerfMonitor.Analysis;
using PerfMonitor.Recording;
using UnityEditor;
using UnityEngine;

namespace PerfMonitor.Editor
{
    // Editor window: pick two recordings and see which metrics got better or worse
    public sealed class CompareRecordingsWindow : EditorWindow
    {
        string _baselinePath = "";
        string _candidatePath = "";
        float _thresholdPct = 5f;

        RecordingStats _baseline;
        RecordingStats _candidate;
        List<ComparisonRow> _rows;
        string _error;
        Vector2 _scroll;

        [MenuItem("Tools/Perf Monitor/Compare Recordings")]
        static void Open()
        {
            GetWindow<CompareRecordingsWindow>("Compare Recordings");
        }

        void OnGUI()
        {
            DrawFilePicker("Baseline (old build)", ref _baselinePath);
            DrawFilePicker("Candidate (new build)", ref _candidatePath);
            _thresholdPct = EditorGUILayout.Slider("Noise threshold %", _thresholdPct, 1f, 25f);

            using (new EditorGUI.DisabledScope(_baselinePath.Length == 0 || _candidatePath.Length == 0))
            {
                if (GUILayout.Button("Compare"))
                {
                    RunComparison();
                }
            }

            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
            if (_rows != null)
            {
                DrawResults();
            }
        }

        static void DrawFilePicker(string label, ref string path)
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(label, path);
            if (GUILayout.Button("Browse", GUILayout.Width(64f)))
            {
                string picked = EditorUtility.OpenFilePanel(label, PerfRecorder.Folder, "csv");
                if (!string.IsNullOrEmpty(picked))
                {
                    path = picked;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void RunComparison()
        {
            _error = null;
            try
            {
                _baseline = RecordingLoader.Load(_baselinePath);
                _candidate = RecordingLoader.Load(_candidatePath);
                _rows = new RecordingComparer { ThresholdPct = _thresholdPct }.Compare(_baseline, _candidate);
            }
            catch (Exception exception)
            {
                _rows = null;
                _error = exception.Message;
            }
        }

        void DrawResults()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (ComparisonRow row in _rows)
            {
                DrawRow(row);
            }
            EditorGUILayout.EndScrollView();

            bool regression = RecordingComparer.HasRegression(_rows);
            EditorGUILayout.HelpBox(regression ? "Regression found" : "No regression", regression ? MessageType.Error : MessageType.Info);

            if (GUILayout.Button("Copy report"))
            {
                EditorGUIUtility.systemCopyBuffer = ComparisonReport.Build(_baseline, _candidate, _rows);
            }
        }

        static void DrawRow(ComparisonRow row)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = GetColor(row.Change);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(row.Name, GUILayout.Width(150f));
            if (row.Change == Change.NotComparable)
            {
                EditorGUILayout.LabelField("n/a");
            }
            else
            {
                EditorGUILayout.LabelField($"{row.Baseline:0.#} -> {row.Candidate:0.#} {row.Unit}", GUILayout.Width(170f));
                EditorGUILayout.LabelField($"{row.DeltaPct:+0.0;-0.0;0.0}%  {row.Change}", style);
            }
            EditorGUILayout.EndHorizontal();
        }

        static Color GetColor(Change change)
        {
            switch (change)
            {
                case Change.Better: return new Color(0.3f, 0.75f, 0.35f);
                case Change.Worse: return new Color(0.9f, 0.3f, 0.25f);
                default: return EditorStyles.label.normal.textColor;
            }
        }
    }
}
