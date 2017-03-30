using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

#if UNITY_5_6_OR_NEWER

using UnityEngine.Profiling;

public class ProfilerIntervalSave : EditorWindow {
    private const string DefaultSaveDir = "ProfilerIntervalSave";
    private const string EditorIdentifier = "Editor";
    private static readonly string[] modeSelect = { "Recorder", "Viewer" };
    private const int MODE_RECORDER = 0;
    private const int MODE_VIEWER = 1;

    private struct SavedCondition{
        public int firstIndex { get; private set; }
        public int lastIndex { get; private set; }
        public string connectedTarget { get; private set; }

        public void SetData(string target , int first, int last)
        {
            this.connectedTarget = target;
            this.firstIndex = first;
            this.lastIndex = last;
        }

        public void Clear()
        {
            this.firstIndex = 0;
            this.lastIndex = 0;
            this.connectedTarget = EditorIdentifier;
        }
    }


    private string saveDir;
    private SavedCondition savedCondition;
    private string lastSaveFile;
    private string lastTarget;
    private string[] fileList;
    private Vector2 scrollPosition;


    private int mode;

    private int stopPlayingCount;


    [MenuItem("Tools/ProfilerIntervalSave")]
    public static void Create()
    {
        ProfilerIntervalSave.GetWindow<ProfilerIntervalSave>();
    }

    void Awake()
    {
        string defaultPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), DefaultSaveDir);
        if ( !System.IO.Directory.Exists(defaultPath))
        {
            System.IO.Directory.CreateDirectory(defaultPath);
        }
        this.saveDir = defaultPath;


        savedCondition.Clear();
        this.CallProfilerWindow();
        this.mode = 0;
        OnEnterRecordMode();
    }

    public void OnGUI()
    {
        OnGUISelectDir();
        int oldMode = mode;
        mode = GUILayout.Toolbar(mode, modeSelect);

        if (oldMode != mode)
        {
            switch (mode)
            {
                case MODE_RECORDER:
                    OnEnterRecordMode();
                    break;
                case MODE_VIEWER:
                    OnEnterViewerMode();
                    break;
            }
        }


        switch (mode)
        {
            case MODE_RECORDER:
                OnGUIRecordMode();
                break;
            case MODE_VIEWER:
                OnGUIViewer();
                break;
        }
    }
    public void Update()
    {
        if (mode == MODE_RECORDER)
        {
            this.UpdateInterlPolling();
        }
    }


    private void CallProfilerWindow()
    {
        Type type = null;
        foreach (System.Reflection.Assembly asm in
               System.AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType("UnityEditor.ProfilerWindow");
            if (type != null) { break; }
        }
        EditorWindow.GetWindow(type);
    }

    private void OnGUISelectDir()
    {
        EditorGUILayout.LabelField("Working Directory");
        EditorGUILayout.BeginHorizontal();
        saveDir = EditorGUILayout.TextField(saveDir);
        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            var select = EditorUtility.OpenFolderPanel("Select Working Directory", saveDir, "");
            if (!string.IsNullOrEmpty(select))
            {
                if (saveDir != select)
                {
                    ClearRecordData();
                    fileList = null;
                }
                saveDir = select;
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    #region RECORD_MODE_LOGIC

    private void OnEnterRecordMode()
    {
        ProfilerDriver.ClearAllFrames();
        ClearRecordData();
    }

    private void ClearRecordData()
    {

        savedCondition.Clear();
        lastSaveFile = null;
    }

    private void OnGUIRecordMode(){
        if (GUILayout.Button("Clear Profiler", GUILayout.Width(120)))
        {
            ProfilerDriver.ClearAllFrames();
            ClearRecordData();
            this.CallProfilerWindow();
        }

        EditorGUILayout.LabelField("Current Target:" + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));


        if( !string.IsNullOrEmpty(lastSaveFile) ){
            EditorGUILayout.LabelField("Last save:" + lastSaveFile );
        }
    }


    private void UpdateInterlPolling(){
        if (string.IsNullOrEmpty(saveDir))
        {
            return;
        }
        string target = ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler).Replace('.','_').Replace('@','_').Replace(':','_');
        int firstFrameIdx = ProfilerDriver.firstFrameIndex;
        int lastFrameIdx = ProfilerDriver.lastFrameIndex;
        // update
        if (target == EditorIdentifier)
        {
            if (IsEditorStopPlaying() )
            {
                ++ stopPlayingCount;
            }else{
                stopPlayingCount = 0;
            }
        }

        // cleared Profiler History
        if (lastFrameIdx < savedCondition.lastIndex)
        {
            savedCondition.Clear();
        }

        if (ShouldSaveResult(target, firstFrameIdx, lastFrameIdx))
        {
            SaveProfileResult(lastTarget, firstFrameIdx, lastFrameIdx);
            this.savedCondition.SetData(target, firstFrameIdx, lastFrameIdx);
            this.Repaint();
        }
        if (this.lastTarget != target)
        {
            this.Repaint();
        }
        this.lastTarget = target;
    }

    private bool ShouldSaveResult(string target,int firstFrameIdx, int lastFrameIdx)
    {
        int maxHistory = ProfilerDriver.maxHistoryLength;
        if (lastFrameIdx <= 0)
        {
            return false;
        }
        // disconnect from player
        if (target != savedCondition.connectedTarget && target == EditorIdentifier )
        {
            return true;
        }
        // stop in editor play
        if (target == EditorIdentifier && IsEditorStopPlaying() && this.stopPlayingCount > 4 &&
            !(savedCondition.firstIndex == firstFrameIdx && savedCondition.lastIndex == lastFrameIdx) )
        {
            return true;
        }
        if (lastFrameIdx - savedCondition.lastIndex >= maxHistory  )
        {
            return true;
        }
        return false;
    }

    private static bool IsEditorStopPlaying()
    {
        return (!EditorApplication.isPlaying && !EditorApplication.isPaused);
    }


    private void SaveProfileResult(string target,int firstFrameIdx , int lastFrameIdx)
    {
        StringBuilder sb = new StringBuilder();

        ProfilerDriver.GetAvailableProfilers();

        sb.Append(target).Append("_").Append(firstFrameIdx).Append("-").Append(lastFrameIdx).Append(".data");
        lastSaveFile = sb.ToString();

        string path = System.IO.Path.Combine(saveDir, lastSaveFile);
        ProfilerDriver.SaveProfile( path );
    }
    #endregion RECORD_MODE_LOGIC

    #region VIEWER_MODE_LOGC
    private void OnEnterViewerMode()
    {
        ProfilerDriver.ClearAllFrames();
        this.fileList = null;
    }

    private void OnGUIViewer()
    {
        if (GUILayout.Button("Reload",GUILayout.Width(120) ))
        {
            this.fileList = null;
        }
        if (fileList == null) { LoadProfiledList(); return; }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var file in fileList)
        {
            if (GUILayout.Button(file, GUILayout.Width(320)))
            {
                string path = System.IO.Path.Combine(saveDir,file.Substring(0,file.Length-5) );
                Profiler.AddFramesFromFile(path);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void LoadProfiledList()
    {
        if (string.IsNullOrEmpty(saveDir))
        {
            return;
        }
        fileList = System.IO.Directory.GetFiles(saveDir,"*.data");

        for (int i = 0; i < fileList.Length; ++i)
        {
            fileList[i] = fileList[i].Substring( fileList[i].LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1 );
        }
    }
    #endregion VIEWER_MODE_LOGC
}

#endif
