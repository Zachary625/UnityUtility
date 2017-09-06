using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


// make sure symbols are unicque across editor session
// use symbol to control [MenuItem] to modify the editor tool bar 
public class CustomPreference : MonoBehaviour
{
    private class PreferenceInfo
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
        public bool Default;
        private PreferenceInfo SuperiorPreference;
        public PreferenceInfoList SubordinatePreferences;

        public PreferenceInfo(string name, string symbolSegment, string description = null, PreferenceInfoList subs = null)
        {
            Name = name;
            SymbolSegment = symbolSegment;
            Description = description;
            SubordinatePreferences = subs;
            if (SubordinatePreferences != null)
            {
                foreach (PreferenceInfo sub in SubordinatePreferences)
                {
                    sub.SuperiorPreference = this;
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
            Add(name, symbolSegment, string.Empty, null);
        }

        public void Add(string name, string symbolSegment, string description)
        {
            Add(name, symbolSegment, description, null);
        }

        public void Add(string name, string symbolSegment, PreferenceInfoList subs)
        {
            Add(name, symbolSegment, string.Empty, subs);
        }

        public void Add(string name, string symbolSegment, string description, PreferenceInfoList subs)
        {
            if (dict.ContainsKey(symbolSegment))
            {
                return;
            }
            list.Add(new PreferenceInfo(name, symbolSegment, description, subs));
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

    private static readonly PreferenceInfo Roles = new PreferenceInfo("Roles", "ROLE", string.Empty, new PreferenceInfoList() {
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
    private static Dictionary<string, bool> preferences = new Dictionary<string, bool>();

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

        bool value = EditorGUILayout.ToggleLeft(text, preferences[info.Symbol]);
        if (value)
        {
            PreferenceInfoListGUI(info.SubordinatePreferences);
        }

        if (value != preferences[info.Symbol])
        {
            preferences[info.Symbol] = value;
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
        preferences[info.Symbol] = EditorPrefs.GetBool(info.Symbol, info.Default);
        LoadPreferenceList(info.SubordinatePreferences);
    }

    private static void LoadPreferences()
    {
        LoadPreference(Roles);

        loaded = true;
    }

    private static void SavePreferences()
    {
        foreach (KeyValuePair<string, bool> preference in preferences)
        {
            EditorPrefs.SetBool(preference.Key, preference.Value);
        }
        saved = true;

        string symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> symbols = new List<string>(symbolString.Split(';'));
        foreach (KeyValuePair<string, bool> preference in preferences)
        {
            if (preference.Value)
            {
                if (!symbols.Contains(preference.Key))
                {
                    symbols.Add(preference.Key);
                }
            }
            else
            {
                symbols.RemoveAll((string symbol) => { return symbol.Equals(preference.Key); });
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
