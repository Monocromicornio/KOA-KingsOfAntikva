using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalizationEditorWindow : EditorWindow
{
    private const string TableAssetPath = "Assets/Localization/LocalizationTable.asset";
    private const string ResourcesJsonFolder = "Assets/Resources/Localization"; // runtime JSON output
    private const string CsvExportPath = "Assets/Localization/localization.csv"; // translator file
    private const string DialogueKeyPrefix = "dialogue";
    private const string DialogueSpeakerKeyPrefix = "dialogue.speaker";
    private const string DialogueLineIndexFormat = "D2";

    private LocalizationTable _table;
    private string _sourceLang = "pt-br";
    private string _scriptedTextSourceLang = "en"; // language the DialogueBase texts and code-defined options are written in
    private string _languagesCsv = "pt-br,en"; // configure languages here
    private bool _includeScenes = true;
    private bool _includePrefabs = false;

    [MenuItem("Tools/Localization/Manager")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<LocalizationEditorWindow>("Localization");
        wnd.minSize = new Vector2(460, 300);
        wnd.Show();
    }

    void OnEnable() => LoadOrCreateTable();

    void OnGUI()
    {
        EditorGUILayout.LabelField("Localization Pipeline", EditorStyles.boldLabel);
        _table = (LocalizationTable)EditorGUILayout.ObjectField("Table Asset", _table, typeof(LocalizationTable), false);

        _sourceLang = EditorGUILayout.TextField("Source Language", _sourceLang);
        _scriptedTextSourceLang = EditorGUILayout.TextField("Dialogue/Code Source Language", _scriptedTextSourceLang);
        _languagesCsv = EditorGUILayout.TextField("Languages (CSV)", _languagesCsv);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scan Options", EditorStyles.boldLabel);
        _includeScenes = EditorGUILayout.Toggle("Scan Scenes", _includeScenes);
        _includePrefabs = EditorGUILayout.Toggle("Scan Prefabs", _includePrefabs);

        EditorGUILayout.Space();
        if (GUILayout.Button("1) Scan & Assign Keys (Scenes/Prefabs)"))
        {
            ScanAndAssignKeys();
        }

        if (GUILayout.Button("1b) Scan & Assign Keys (Dialogue ScriptableObjects)"))
        {
            ScanDialogues();
        }

        if (GUILayout.Button("1c) Register Code Keys (Settings Dropdowns, Turn Info)"))
        {
            RegisterCodeKeys();
        }

        if (GUILayout.Button("2) Export CSV (Master)"))
        {
            ExportCSV();
        }

        if (GUILayout.Button("3) Import CSV → Update Table"))
        {
            ImportCSV();
        }

        if (GUILayout.Button("4) Export JSON per Language (Resources/Localization/*.json)"))
        {
            ExportJSONs();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Workflow:\n" +
            "• Configure languages\n" +
            "• Scan & assign keys (scenes/prefabs and dialogues)\n" +
            "• Register code keys (settings dropdowns, turn info)\n" +
            "• Export CSV → Send to translators\n" +
            "• Import CSV when updated\n" +
            "• Export JSONs for runtime",
            MessageType.Info);
    }

    void LoadOrCreateTable()
    {
        _table = AssetDatabase.LoadAssetAtPath<LocalizationTable>(TableAssetPath);
        if (_table == null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TableAssetPath)!);
            _table = ScriptableObject.CreateInstance<LocalizationTable>();
            AssetDatabase.CreateAsset(_table, TableAssetPath);
            AssetDatabase.SaveAssets();
        }

        // Sync languages with UI field
        var langs = SplitLangs();
        foreach (var l in langs) _table.EnsureLanguage(l);
        EditorUtility.SetDirty(_table);
    }

    List<string> SplitLangs()
        => _languagesCsv.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

    void ScanAndAssignKeys()
    {
        var langs = SplitLangs();
        foreach (var l in langs) _table.EnsureLanguage(l);

        int changedCount = 0;

        if (_includeScenes)
        {
            var assetFolders = new[] { "Assets" }; // only look inside your project folder
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", assetFolders);

            foreach (var guid in sceneGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
                    continue; // skip read-only package scenes
                if (!AssetDatabase.IsOpenForEdit(path))
                    continue; // skip locked assets

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                changedCount += ProcessScene(scene, Path.GetFileNameWithoutExtension(path));

                if (scene.isDirty && AssetDatabase.IsOpenForEdit(path))
                    EditorSceneManager.SaveScene(scene);

                EditorSceneManager.CloseScene(scene, true);
            }
        }


        if (_includePrefabs)
        {
            var assetFolders = new[] { "Assets" };
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", assetFolders);
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = PrefabUtility.LoadPrefabContents(path);
                bool changed = ProcessRoot(go, "PREFAB", Path.GetFileNameWithoutExtension(path)) > 0;
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                    changedCount++;
                }
                PrefabUtility.UnloadPrefabContents(go);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Localization: scan complete. Modified/assigned {changedCount} objects.");
    }

    int ProcessScene(Scene scene, string context)
    {
        int c = 0;
        foreach (var root in scene.GetRootGameObjects())
            c += ProcessRoot(root, context, root.name);
        return c;
    }

    int ProcessRoot(GameObject root, string context, string topName)
    {
        int count = 0;

        // Find all TMP and legacy Text components (include inactive)
        var tmps = root.GetComponentsInChildren<TMP_Text>(true);
        var uis = root.GetComponentsInChildren<Text>(true);

        foreach (var t in tmps) count += ProcessTextComponent(t.gameObject, t.text, context);
        foreach (var t in uis) count += ProcessTextComponent(t.gameObject, t.text, context);

        return count;
    }

    int ProcessTextComponent(GameObject go, string textValue, string context)
    {
        if (string.IsNullOrWhiteSpace(textValue)) return 0;

        // Dropdown captions/items display the dropdown options (localized in code), so they must never get a LocalizedText.
        if (go.GetComponentInParent<TMP_Dropdown>(true) != null || go.GetComponentInParent<Dropdown>(true) != null) return 0;

        // Texts written by code-localized components (turn info, selected piece panel...) must never get a LocalizedText either.
        if (go.GetComponentInParent<ILocalizedByCode>(true) != null) return 0;

        var lt = go.GetComponent<LocalizedText>();
        if (lt == null) lt = go.AddComponent<LocalizedText>();

        // Generate stable key from path
        string path = GetHierarchyPath(go.transform);
        string key = MakeKey(context, path);

        lt.Key = key;
        lt.SetFallbackForEditor(textValue);

        // Add/Update in table
        _table.Upsert(key, textValue, _sourceLang);
        EditorUtility.SetDirty(lt);
        EditorUtility.SetDirty(_table);
        return 1;
    }

    static string GetHierarchyPath(Transform t)
    {
        var stack = new List<string>();
        while (t != null) { stack.Add(t.name); t = t.parent; }
        stack.Reverse();
        return string.Join("/", stack);
    }

    // --- Dialogue ScriptableObjects ---

    /// <summary>
    /// Assigns missing text/speaker keys to every DialogueBase asset and upserts their texts into the table
    /// (using the dialogue source language). Existing keys and translations are preserved.
    /// </summary>
    public void ScanDialogues()
    {
        foreach (var l in SplitLangs()) _table.EnsureLanguage(l);
        _table.EnsureLanguage(_scriptedTextSourceLang);

        int assetCount = 0;
        int lineCount = 0;

        var dialogueGuids = AssetDatabase.FindAssets($"t:{nameof(DialogueBase)}", new[] { "Assets" });
        foreach (var guid in dialogueGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var dialogue = AssetDatabase.LoadAssetAtPath<DialogueBase>(path);
            if (dialogue == null || dialogue.dialogueInfo == null) continue;

            bool assetChanged = false;
            for (int i = 0; i < dialogue.dialogueInfo.Length; i++)
            {
                var info = dialogue.dialogueInfo[i];
                if (info == null) continue;

                if (!string.IsNullOrWhiteSpace(info.text))
                {
                    if (string.IsNullOrEmpty(info.textKey))
                    {
                        info.textKey = MakeDialogueTextKey(dialogue.name, i);
                        assetChanged = true;
                    }
                    _table.Upsert(info.textKey, info.text, _scriptedTextSourceLang);
                    lineCount++;
                }

                if (!string.IsNullOrWhiteSpace(info.speaker))
                {
                    if (string.IsNullOrEmpty(info.speakerKey))
                    {
                        info.speakerKey = MakeKey(DialogueSpeakerKeyPrefix, info.speaker);
                        assetChanged = true;
                    }
                    _table.Upsert(info.speakerKey, info.speaker, _scriptedTextSourceLang);
                }
            }

            if (assetChanged)
            {
                EditorUtility.SetDirty(dialogue);
                assetCount++;
            }
        }

        EditorUtility.SetDirty(_table);
        AssetDatabase.SaveAssets();
        Debug.Log($"Localization: dialogue scan complete. {lineCount} lines processed, keys assigned in {assetCount} assets.");
    }

    // --- Code-defined texts ---

    /// <summary>
    /// Upserts the translatable texts defined in code (settings dropdown options, turn info) into the table,
    /// using the dialogue/code source language. Existing texts and translations are preserved.
    /// </summary>
    public void RegisterCodeKeys()
    {
        foreach (var l in SplitLangs()) _table.EnsureLanguage(l);
        _table.EnsureLanguage(_scriptedTextSourceLang);

        var codeTexts = VideoSettingsManager.GetTranslatableOptions()
            .Concat(TurnInfoUI.GetTranslatableTexts())
            .Where(text => text.IsTranslatable);

        int textCount = 0;
        foreach (var text in codeTexts)
        {
            _table.Upsert(text.Key, text.Fallback, _scriptedTextSourceLang);
            textCount++;
        }

        EditorUtility.SetDirty(_table);
        AssetDatabase.SaveAssets();
        Debug.Log($"Localization: {textCount} code-defined text keys registered.");
    }

    static string MakeDialogueTextKey(string dialogueAssetName, int lineIndex)
        => MakeKey(DialogueKeyPrefix, $"{dialogueAssetName}_{lineIndex.ToString(DialogueLineIndexFormat)}");

    static string MakeKey(string context, string path)
    {
        // SCENEORPREFAB:Clean/Path/Name -> SCENEORPREFAB.Clean_Path_Name
        string clean = new string(path.Select(ch =>
            char.IsLetterOrDigit(ch) ? ch : '_').ToArray());

        // collapse multiple underscores
        while (clean.Contains("__")) clean = clean.Replace("__", "_");

        return $"{context}.{clean}".Trim('.').ToLowerInvariant();
    }

    // --- CSV Export/Import ---

    void ExportCSV()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(CsvExportPath)!);
        var langs = _table.Languages;
        using (var sw = new StreamWriter(CsvExportPath))
        {
            // Header
            sw.Write("key");
            foreach (var l in langs) sw.Write($",{Csv(l)}");
            sw.WriteLine();

            // Rows
            foreach (var r in _table.Rows.OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase))
            {
                sw.Write(Csv(r.Key));
                for (int i = 0; i < langs.Count; i++)
                {
                    string val = (i < r.Values.Count) ? r.Values[i] : "";
                    sw.Write($",{Csv(val)}");
                }
                sw.WriteLine();
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"Localization: CSV exported → {CsvExportPath}");
    }

   void ImportCSV()
{
    if (!File.Exists(CsvExportPath))
    {
        Debug.LogError($"CSV not found at {CsvExportPath}");
        return;
    }

    using var sr = new StreamReader(CsvExportPath, new UTF8Encoding(false));
    var rows = ReadCsvRows(sr).ToList();
    if (rows.Count == 0) { Debug.LogError("CSV is empty."); return; }

    var header = rows[0];
    if (header.Count < 2 || !string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
    {
        Debug.LogError("CSV header must start with 'key'.");
        return;
    }

    // languages exactly as in header (no filtering)
    var langs = header.Skip(1).ToList();
    foreach (var l in langs) _table.EnsureLanguage(l);

    // lang name -> index in table
    var langToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < _table.Languages.Count; i++)
        langToIndex[_table.Languages[i]] = i;

    int updated = 0;

    for (int r = 1; r < rows.Count; r++)
    {
        var cols = rows[r];
        if (cols.Count == 0) continue;

        var key = cols[0];
        if (string.IsNullOrWhiteSpace(key)) continue;

        var row = _table.Rows.Find(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (row == null)
        {
            row = new LocalizationTable.Row
            {
                Key = key,
                Values = new List<string>(new string[_table.Languages.Count])
            };
            _table.Rows.Add(row);
        }
        while (row.Values.Count < _table.Languages.Count) row.Values.Add(string.Empty);

        for (int i = 0; i < langs.Count; i++)
        {
            if (!langToIndex.TryGetValue(langs[i], out var li)) continue;
            string val = (i + 1 < cols.Count) ? cols[i + 1] : string.Empty; // raw
            row.Values[li] = val; // store exactly as-is (may be "")
        }

        updated++;
    }

    EditorUtility.SetDirty(_table);
    AssetDatabase.SaveAssets();
    Debug.Log($"Localization: CSV import complete. Updated {updated} keys.");
}



    // --------------- helper: robust CSV reader ----------------
    // - comma separated
    // - quotes wrap fields
    // - double quote "" inside quoted field = escaped quote
    // - supports multiline quoted fields
    IEnumerable<List<string>> ReadCsvRows(TextReader reader)
    {
        var rows = new List<List<string>>();
        var curRow = new List<string>();
        var cur = new StringBuilder();
        bool inQuotes = false;
        int ch;

        while ((ch = reader.Read()) != -1)
        {
            char c = (char)ch;

            if (inQuotes)
            {
                if (c == '"')
                {
                    int peek = reader.Peek();
                    if (peek == '"') { reader.Read(); cur.Append('"'); } // escaped quote
                    else inQuotes = false; // closing quote
                }
                else
                {
                    cur.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true; // opening quote
                }
                else if (c == ',')
                {
                    curRow.Add(cur.ToString());
                    cur.Clear();
                }
                else if (c == '\r')
                {
                    // normalize CRLF/LF
                    if (reader.Peek() == '\n') reader.Read();
                    curRow.Add(cur.ToString());
                    rows.Add(curRow);
                    curRow = new List<string>();
                    cur.Clear();
                }
                else if (c == '\n')
                {
                    curRow.Add(cur.ToString());
                    rows.Add(curRow);
                    curRow = new List<string>();
                    cur.Clear();
                }
                else
                {
                    cur.Append(c);
                }
            }
        }

        // flush last field/row
        curRow.Add(cur.ToString());
        rows.Add(curRow);
        return rows;
    }

    // --- JSON Export ---

    /// <summary>
    /// Writes one runtime JSON file per table language into Resources/Localization.
    /// </summary>
    public void ExportJSONs()
    {
        Directory.CreateDirectory(ResourcesJsonFolder);

        foreach (var lang in _table.Languages)
        {
            int li = _table.IndexOfLang(lang);
            if (li < 0) continue;

            var dict = new LocalizationManager.SerializableDict
            {
                items = new List<LocalizationManager.KV>()
            };

            foreach (var r in _table.Rows)
            {
                // Take exactly what's in the table for this language.
                // If missing -> empty string.
                string val = (li < r.Values.Count && r.Values[li] != null)
                    ? r.Values[li]
                    : string.Empty;

                dict.items.Add(new LocalizationManager.KV { key = r.Key, value = val });
            }

            var json = LocalizationManager.ToJson(dict); // uses Newtonsoft.Json
            var path = Path.Combine(ResourcesJsonFolder, $"{lang}.json");
            File.WriteAllText(path, json);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Localization: JSONs exported (raw values only) → {ResourcesJsonFolder}");
    }

    // --- CSV helpers ---

    static string Csv(string s)
    {
        if (s == null) s = "";
        bool needQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        if (s.Contains("\"")) s = s.Replace("\"", "\"\"");
        return needQuotes ? $"\"{s}\"" : s;
    }

    static List<string> ParseCsvLine(string line)
    {
        var res = new List<string>();
        bool inQuotes = false;
        var cur = new System.Text.StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; }
                    else inQuotes = false;
                }
                else cur.Append(c);
            }
            else
            {
                if (c == ',') { res.Add(cur.ToString()); cur.Clear(); }
                else if (c == '"') inQuotes = true;
                else cur.Append(c);
            }
        }
        res.Add(cur.ToString());
        return res;
    }
}
