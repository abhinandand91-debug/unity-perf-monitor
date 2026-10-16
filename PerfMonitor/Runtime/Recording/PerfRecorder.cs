using System;
using System.IO;
using PerfMonitor.Core;
using UnityEngine;

namespace PerfMonitor.Recording
{
    // Starts and stops a recording session: a CSV of every frame plus a summary text file
    public sealed class PerfRecorder
    {
        readonly SessionSummary _summary = new SessionSummary();

        CsvWriter _csv;
        string _basePath;

        public bool IsRecording => _csv != null;
        public string LastFileName { get; private set; }

        public static string Folder => Path.Combine(Application.persistentDataPath, "PerfMonitor");

        public void Start()
        {
            if (IsRecording)
            {
                return;
            }

            Directory.CreateDirectory(Folder);
            _basePath = Path.Combine(Folder, "perf_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            _csv = new CsvWriter(_basePath + ".csv");
            _summary.Reset();
            LastFileName = Path.GetFileName(_basePath) + ".csv";
        }

        public void Add(in PerfSample sample)
        {
            if (!IsRecording)
            {
                return;
            }

            _csv.WriteRow(sample);
            _summary.Add(sample);
        }

        public void Stop()
        {
            if (!IsRecording)
            {
                return;
            }

            _csv.Dispose();
            _csv = null;
            File.WriteAllText(_basePath + "_summary.txt", _summary.Build());
        }
    }
}
