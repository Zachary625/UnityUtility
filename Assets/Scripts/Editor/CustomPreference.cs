using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;


// make sure symbols are unicque across editor session
// use symbol to control [MenuItem] to modify the editor tool bar 
public class CustomPreference : MonoBehaviour
{
    private class PreferenceInfo: IEnumerator, IEnumerable
    {
        public string Name;
        public string Description = string.Empty;
        public string SymbolSegment;
        public string Symbol
        {
            get
            {
                if (SuperiorPreference == null)
                {
                    return SymbolSegment;
                }
                else
                {
                    return SuperiorPreference.Symbol + SYMBOL_CONCATINATOR + SymbolSegment;
                }
            }
        }
        private bool _Default;
        public bool Default
        {
            get
            {
                return _Default;
            }
        }

        private bool _Value;
        public bool Value
        {
            get
            {
                if (SuperiorPreference == null)
                {
                    return _Value;
                }
                else
                {
                    return SuperiorPreference.Value && _Value;
                }
            }
            set
            {
                _Value = value;
            }
        }

        public bool? this[string symbol]
        {
            get
            {
                if (!symbol.StartsWith(this.SymbolSegment))
                {
                    return null;
                }
                if (symbol.Equals(this.SymbolSegment))
                {
                    return this.Value;
                }
                if (SubordinatePreferences == null)
                {
                    return null;
                }
                string leftover = symbol.Substring((symbol + SYMBOL_CONCATINATOR).Length);
                foreach (PreferenceInfo sub in SubordinatePreferences)
                {
                    bool? subResult = sub[leftover];
                    if (subResult.HasValue)
                    {
                        return subResult;
                    }
                }
                return null;
            }
            set
            {
                if (!value.HasValue)
                {
                    return;
                }
                if (!symbol.StartsWith(this.SymbolSegment))
                {
                    return;
                }
                if (symbol.Equals(this.SymbolSegment))
                {
                    this.Value = value.Value;
                    return;
                }
                if (SubordinatePreferences == null)
                {
                    return;
                }
                string leftover = symbol.Substring((symbol + SYMBOL_CONCATINATOR).Length);
                foreach (PreferenceInfo sub in SubordinatePreferences)
                {
                    sub[leftover] = value;
                }
            }
        }

        public object Current
        {
            get
            {
                if (enumerateIndex.HasValue)
                {
                    if (SubordinatePreferences != null && 0 <= enumerateIndex.Value && enumerateIndex.Value < SubordinatePreferences.Size)
                    {
                        return SubordinatePreferences[enumerateIndex.Value].Current;
                    }
                    return this;
                }
                return null;
            }
        }

        private int? enumerateIndex = null;

        private PreferenceInfo SuperiorPreference;
        public PreferenceInfoList SubordinatePreferences;

        public PreferenceInfo(string name, string symbolSegment, bool defaultValue = false, string description = null, PreferenceInfoList subs = null)
        {
            Name = name;
            SymbolSegment = symbolSegment;
            Description = description;
            SubordinatePreferences = subs;
            _Default = defaultValue;
            _Value = _Default;
            
            if (SubordinatePreferences != null)
            {
                foreach (PreferenceInfo sub in SubordinatePreferences)
                {
                    sub.SuperiorPreference = this;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (!enumerateIndex.HasValue)
            {
                enumerateIndex = -1;
                return true;
            }

            if (SubordinatePreferences == null || SubordinatePreferences.Size <= 0 || SubordinatePreferences.Size <= enumerateIndex)
            {
                return false;
            }

            if (0 <= enumerateIndex && SubordinatePreferences[enumerateIndex.Value].MoveNext())
            {
                return true;
            }

            enumerateIndex++;
            return enumerateIndex < SubordinatePreferences.Size && SubordinatePreferences[enumerateIndex.Value].MoveNext();
        }

        public void Reset()
        {
            enumerateIndex = null;
            if (SubordinatePreferences != null)
            {
                foreach (PreferenceInfo info in SubordinatePreferences)
                {
                    info.Reset();
                }
            }
        }
    }

    private class PreferenceInfoList: IEnumerable
    {
        private List<PreferenceInfo> list = new List<PreferenceInfo>();
        private Dictionary<string, PreferenceInfo> dict = new Dictionary<string, PreferenceInfo>();
        public int Size
        {
            get
            {
                return list.Count;
            }
        }
        public PreferenceInfo this[int index]
        {
            get
            {
                return list[index];
            }
        }

        public PreferenceInfo this[string segment]
        {
            get
            {
                return dict[segment];
            }
        }

        public void Add(string name, string symbolSegment)
        {
            Add(name, symbolSegment, false, string.Empty, null);
        }

        public void Add(string name, string symbolSegment, bool defaultValue)
        {
            Add(name, symbolSegment, defaultValue, string.Empty, null);
        }

        public void Add(string name, string symbolSegment, string description)
        {
            Add(name, symbolSegment, false, description, null);
        }

        public void Add(string name, string symbolSegment, PreferenceInfoList subs)
        {
            Add(name, symbolSegment, false, string.Empty, subs);
        }

        public void Add(string name, string symbolSegment, bool defaultValue, string description)
        {
            Add(name, symbolSegment, defaultValue, description, null);
        }

        public void Add(string name, string symbolSegment, bool defaultValue, PreferenceInfoList subs)
        {
            Add(name, symbolSegment, defaultValue, string.Empty, subs);
        }

        public void Add(string name, string symbolSegment, string description, PreferenceInfoList subs)
        {
            Add(name, symbolSegment, false, description, subs);
        }


        public void Add(string name, string symbolSegment, bool defaultValue, string description, PreferenceInfoList subs)
        {
            if (dict.ContainsKey(symbolSegment))
            {
                return;
            }
            list.Add(new PreferenceInfo(name, symbolSegment, defaultValue, description, subs));
            dict[symbolSegment] = list[list.Count - 1];
        }

        public IEnumerator GetEnumerator()
        {
            foreach (PreferenceInfo info in list)
            {
                yield return info;
            }
        }
    }

    private static readonly string SYMBOL_CONCATINATOR = "_";
    private static readonly string SYMBOL_PREFIX = "CUSTOM_PREF_";

    private static readonly PreferenceInfo Roles = new PreferenceInfo("Roles", "ROLE", false, string.Empty, new PreferenceInfoList() {
        { "Manager", "MANAGER", new PreferenceInfoList() {
            { "Manager Tool 1", "TOOL1", "Tool 1 for manager." },
        } },
        { "Designer", "DESIGNER", new PreferenceInfoList() {
            { "Designer Tool 1", "TOOL1", "Tool 1 for designer." },
            { "Designer Tool 2", "TOOL2", "Tool 2 for designer." },
        } },
        { "Artist", "ARTIST", new PreferenceInfoList() {
            { "Artist Tool 1", "TOOL1", "Tool 1 for artist." },
            { "Artist Tool 2", "TOOL2", "Tool 2 for artist." },
            { "Artist Tool 3", "TOOL3", "Tool 3 for artist." },
        } },
        { "Programmer", "PROGRAMMER", new PreferenceInfoList() {
            { "Programmer Tool 1", "TOOL1", "Tool 1 for programmer." },
            { "Programmer Tool 2", "TOOL2", "Tool 2 for programmer." },
            { "Programmer Tool 3", "TOOL3", "Tool 3 for programmer." },
            { "Programmer Tool 4", "TOOL4", "Tool 4 for programmer." },
        } },
        { "Tester", "TESTER", new PreferenceInfoList() {
            { "Tester Tool 1", "TOOL1", "Tool 1 for tester." },
            { "Tester Tool 2", "TOOL2", "Tool 2 for tester." },
            { "Tester Tool 3", "TOOL3", "Tool 3 for tester." },
            { "Tester Tool 4", "TOOL4", "Tool 4 for tester." },
            { "Tester Tool 5", "TOOL5", "Tool 5 for tester." },
        } },
        { "Jibber", "JIBBER", new PreferenceInfoList() {
            { "Jibber Tool 1", "TOOL1", "Tool 1 for jibber." },
            { "Jibber Tool 2", "TOOL2", "Tool 2 for jibber." },
            { "Jibber Tool 3", "TOOL3", "Tool 3 for jibber." },
            { "Jibber Tool 4", "TOOL4", "Tool 4 for jibber." },
            { "Jibber Tool 5", "TOOL5", "Tool 5 for jibber." },
            { "Jibber Tool 6", "TOOL6", "Tool 6 for jibber." },
        } },
    });

    private static bool loaded = false;
    private static bool saved = true;
    //private static Dictionary<string, bool> preferences = new Dictionary<string, bool>();

    private static Vector2 scrollPosition = new Vector2();

    [PreferenceItem("Custom")]
    public static void PreferenceGUI()
    {
        if (!loaded)
        {
            LoadPreferences();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();

        PreferenceInfoGUI(Roles);

        EditorGUILayout.Separator();
        if (!saved)
        {
            if (GUILayout.Button("Save"))
            {
                SavePreferences();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private static void PreferenceInfoListGUI(PreferenceInfoList infos)
    {
        if (infos == null || infos.Size <= 0)
        {
            return;
        }

        EditorGUILayout.BeginVertical();
        foreach (PreferenceInfo info in infos)
        {
            PreferenceInfoGUI(info);
        }
        EditorGUILayout.EndVertical();
    }

    private static void PreferenceInfoGUI(PreferenceInfo info)
    {
        if (info == null)
        {
            return;
        }
        EditorGUI.indentLevel++;
        string text = info.Name;
        if (!string.IsNullOrEmpty(info.Description))
        {
            text += " - " + info.Description;
        }

        bool value = EditorGUILayout.ToggleLeft(text, info.Value);
        if (value)
        {
            PreferenceInfoListGUI(info.SubordinatePreferences);
        }

        if (value != info.Value)
        {
            info.Value = value;
            saved = false;
        }

        EditorGUI.indentLevel--;

    }

    private static void LoadPreferenceList(PreferenceInfoList infos)
    {
        if (infos == null || infos.Size <= 0)
        {
            return;
        }
        foreach (PreferenceInfo info in infos)
        {
            LoadPreference(info);
        }
    }

    private static void LoadPreference(PreferenceInfo info)
    {
        if (info == null)
        {
            return;
        }
        info.Value = EditorPrefs.GetBool(info.Symbol, info.Default);
        LoadPreferenceList(info.SubordinatePreferences);
    }

    private static void LoadPreferences()
    {
        LoadPreference(Roles);

        loaded = true;
    }

    private static void SavePreferences()
    {
        foreach (PreferenceInfo info in Roles)
        {
            EditorPrefs.SetBool(info.Symbol, info.Value);
        }
        saved = true;

        string symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> symbols = new List<string>(symbolString.Split(';'));
        foreach (PreferenceInfo info in Roles)
        {
            if (info.Value)
            {
                if (!symbols.Contains(info.Symbol))
                {
                    symbols.Add(info.Symbol);
                }
            }
            else
            {
                symbols.RemoveAll((string symbol) => { return symbol.Equals(info.Symbol); });
            }
        }
        symbolString = string.Join(";", symbols.ToArray());
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbolString);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
