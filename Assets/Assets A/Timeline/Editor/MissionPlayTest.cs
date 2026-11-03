using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GslvMission.EditorTools
{
    /// <summary>
    /// One-time Play-mode run of the whole mission, started by MISSIONTEST_ONCE.txt next to this script.
    /// Runs the Timeline at 4x speed with every gate answered automatically (quiz: one wrong answer first),
    /// photographs the learner's view at the key moments, checks that each gate button can be reached by the
    /// pointer, and writes a report and contact sheets to the system temp folder. Does nothing without the trigger.
    /// </summary>
    [InitializeOnLoad]
    static class MissionPlayTest
    {
        const string Trigger = "Assets/Assets A/Timeline/Editor/MISSIONTEST_ONCE.txt";
        const string SessionKey = "GSLV.MissionPlayTest";
        const double TimeoutSeconds = 600;
        const float TimeScale = 4f;
        const int Tile = 640;
        const float HeadsetFov = 96f;

        static readonly string Temp = Path.GetTempPath();
        static readonly StringBuilder report = new StringBuilder();
        static readonly List<string> errors = new List<string>();
        static readonly Dictionary<string, int> errorCounts = new Dictionary<string, int>();
        static readonly List<Texture2D> tiles = new List<Texture2D>();
        static readonly List<string> tileNames = new List<string>();
        static List<KeyValuePair<string, double>> shots;
        static double started, endTime, gateTime;
        static int next, sheets;
        static PlayableDirector director;
        static MissionFlow flow;
        static string lastGate;
        static int pausedTicks;
        static bool savedRunInBackground;

        static MissionPlayTest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            if (File.Exists(Trigger)) EditorApplication.delayCall += Begin;
        }

        static void Begin()
        {
            if (!File.Exists(Trigger)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Begin;
                return;
            }
            AssetDatabase.DeleteAsset(Trigger);
            if (File.Exists(Trigger)) File.Delete(Trigger);
            SessionState.SetBool(SessionKey, true);
            EditorApplication.isPlaying = true;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, false);
            report.Length = 0;
            errors.Clear();
            errorCounts.Clear();
            tiles.Clear();
            tileNames.Clear();
            next = 0;
            sheets = 0;
            lastGate = null;
            director = null;
            flow = null;
            started = EditorApplication.timeSinceStartup;
            shots = new List<KeyValuePair<string, double>>();
            string times = Path.Combine(Temp, "GSLV_Timeline_Times.txt");
            if (File.Exists(times))
                foreach (string line in File.ReadAllLines(times))
                {
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    double v = double.Parse(line.Substring(eq + 1), CultureInfo.InvariantCulture);
                    string name = line.Substring(0, eq);
                    if (name == "end") endTime = v;
                    else shots.Add(new KeyValuePair<string, double>(name, v));
                }
            shots = shots.OrderBy(s => s.Value).ToList();
            savedRunInBackground = Application.runInBackground;
            Application.runInBackground = true;   // keep playing if Unity loses focus during the run
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }

        // Groups repeated errors (e.g. one thrown every frame) into one line with a count and where it came from.
        static void OnLog(string condition, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string where = (stack ?? "").Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0) ?? "";
            string key = type + ": " + condition + (where.Length > 0 ? "  @ " + where : "");
            int n;
            errorCounts.TryGetValue(key, out n);
            if (n == 0 && errors.Count < 40) errors.Add(key);
            errorCounts[key] = n + 1;
        }

        static void Tick()
        {
            double real = EditorApplication.timeSinceStartup - started;
            try
            {
                // The Console's "Error Pause" would freeze the game on the first (QWR editor) error; keep it running.
                if (EditorApplication.isPaused) { pausedTicks++; EditorApplication.isPaused = false; }
                if (!EditorApplication.isPlaying) { Finish("stopped: Play mode ended early"); return; }
                if (real > TimeoutSeconds) { Finish("stopped: timed out at director time " + (director != null ? director.time.ToString("0.0") : "?")); return; }
                if (director == null)
                {
                    if (real < 3) return; // let QWR pick its rig and the director start
                    flow = Object.FindObjectOfType<MissionFlow>();
                    director = flow != null ? flow.director : null;
                    if (director == null) { Finish("failed: no MissionFlow / PlayableDirector in the scene"); return; }
                    // Scene 1 has no Exit Menu yet (a template step), and QWR's gaze pointer reads ExitMenu.instance whenever it
                    // hovers a button: it throws every frame and the Console's Error Pause freezes the run. Inert stand-in:
                    if (ExitMenu.instance == null)
                    {
                        var stub = new GameObject("GSLV Test ExitMenu Stand-in");
                        stub.SetActive(false);   // inactive, so ExitMenu.Awake (encoder and socket set-up) never runs
                        ExitMenu.instance = stub.AddComponent<ExitMenu>();
                        report.AppendLine("no Exit Menu in the scene: an inert stand-in was used for this run");
                    }
                    flow.autoPlay = true;
                    flow.autoPlayDelay = 1.0f;
                    AudioListener.volume = 0f;
                    Time.timeScale = TimeScale;
                    report.AppendLine("editor was paused " + pausedTicks + " time(s) so far; focused=" + UnityEditorInternal.InternalEditorUtility.isApplicationActive);
                    report.AppendLine(string.Format("director: state={0} duration={1:0.0}s time={2:0.0} playOnAwake={3}", director.state, director.duration, director.time, director.playOnAwake));
                    return;
                }

                double t = director.time;
                string gate = flow.OpenGateId;
                if (gate != lastGate)
                {
                    if (gate != null) { gateTime = t; GateOpened(gate, t); }
                    else report.AppendLine(string.Format("  gate '{0}' closed, director time {1:0.00} (was {2:0.00} when it opened)", lastGate, t, gateTime));
                    lastGate = gate;
                }
                while (next < shots.Count && t >= shots[next].Value)
                {
                    Capture(shots[next].Key, t);
                    next++;
                }
                if (t >= endTime - 0.3 || (director.state != PlayState.Playing && t > 5 && gate == null))
                {
                    Capture("end", t);
                    Finish(t >= endTime - 0.5 ? "done" : "stopped: director stopped at " + t.ToString("0.0"));
                }
            }
            catch (Exception e)
            {
                Finish("failed\n" + e);
            }
        }

        static Camera Eye()
        {
            var rig = qwr.QwrCameraRig._instance;
            return rig != null ? rig._center_eye : Camera.main;
        }

        static void GateOpened(string id, double t)
        {
            var eye = Eye();
            report.AppendLine(string.Format("GATE '{0}' opened at director time {1:0.00}", id, t));
            var buttons = new List<Button>();
            if (id == "Quiz") buttons.AddRange(flow.quiz.optionButtons);
            else foreach (var g in flow.gateButtons) if (g.id == id && g.button != null) buttons.Add(g.button);
            foreach (var b in buttons)
            {
                var col = b.GetComponent<Collider>();
                string result;
                if (col == null || !col.enabled) result = "NO ACTIVE COLLIDER";
                else if (!b.gameObject.activeInHierarchy) result = "BUTTON NOT ACTIVE IN HIERARCHY";
                else
                {
                    Vector3 to = col.bounds.center - eye.transform.position;
                    RaycastHit hit;
                    if (Physics.Raycast(eye.transform.position, to.normalized, out hit, to.magnitude + 2f))
                        result = hit.collider == col ? "reachable (" + to.magnitude.ToString("0.0") + " m)" : "BLOCKED by " + hit.collider.name;
                    else result = "ray hit nothing";
                }
                report.AppendLine("  " + b.name + ": interactable=" + b.interactable + ", " + result);
            }
        }

        static void Capture(string name, double t)
        {
            var reference = Eye();
            var go = new GameObject("GSLV Test Camera") { hideFlags = HideFlags.HideAndDontSave };
            var rt = new RenderTexture(Tile, Tile, 24);
            try
            {
                var cam = go.AddComponent<Camera>();
                if (reference != null) cam.CopyFrom(reference);
                cam.stereoTargetEye = StereoTargetEyeMask.None;
                cam.fieldOfView = HeadsetFov;
                cam.targetTexture = rt;
                cam.enabled = false;
                if (reference != null) cam.transform.SetPositionAndRotation(reference.transform.position, reference.transform.rotation);
                cam.Render();
                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(Tile, Tile, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, Tile, Tile), 0, 0);
                tex.Apply();
                RenderTexture.active = prev;
                tiles.Add(tex);
                tileNames.Add(name);
                int highlights = Object.FindObjectsOfType<PartHighlight>().Length;
                report.AppendLine(string.Format("shot {0,-14} t={1,7:0.0}  inSpace={2}  highlights={3}  rig={4}", name, t,
                    flow.space != null && flow.space.InSpace, highlights, reference != null ? reference.transform.position.ToString("F1") : "?"));
                if (tiles.Count == 9) FlushSheet();
            }
            finally
            {
                Object.DestroyImmediate(go);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        static void FlushSheet()
        {
            if (tiles.Count == 0) return;
            var sheet = new Texture2D(Tile * 3, Tile * 3, TextureFormat.RGB24, false);
            var fill = new Color[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = new Color(0.05f, 0.05f, 0.08f);
            sheet.SetPixels(fill);
            for (int i = 0; i < tiles.Count; i++)
                sheet.SetPixels((i % 3) * Tile, (2 - i / 3) * Tile, Tile, Tile, tiles[i].GetPixels());
            sheet.Apply();
            sheets++;
            string file = Path.Combine(Temp, "GSLV_MissionTest_Sheet" + sheets + ".png");
            File.WriteAllBytes(file, sheet.EncodeToPNG());
            report.AppendLine("  sheet " + sheets + " (row-major from top-left): " + string.Join(", ", tileNames.ToArray()));
            foreach (var t in tiles) Object.DestroyImmediate(t);
            tiles.Clear();
            tileNames.Clear();
            Object.DestroyImmediate(sheet);
        }

        static void Finish(string result)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            try { FlushSheet(); } catch (Exception e) { report.AppendLine("sheet failed: " + e.Message); }
            report.AppendLine();
            report.AppendLine("editor pauses undone during the run: " + pausedTicks);
            report.AppendLine("errors/exceptions logged during the run: " + errorCounts.Values.Sum() + " (" + errorCounts.Count + " different)");
            foreach (var e in errors) report.AppendLine("  " + errorCounts[e] + "x  " + e);
            File.WriteAllText(Path.Combine(Temp, "GSLV_MissionTest.txt"), result + "\n" + report);
            Time.timeScale = 1f;
            AudioListener.volume = 1f;
            Application.runInBackground = savedRunInBackground;
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }
    }
}
// run 111408
