# UnityProfileIntervalSave
Save Profiler's log for Unity 5.6 or newer

Read this in other languages: English, [日本語](README.ja.md)<br />

## IntervalSave
This tool is available from Unity 5.6.<br />
From Unity 5.6 , we can save the result of Unity ProfilerWindow!<br />
Also there is API "UnityEditorInternal.ProfilerDriver.SaveProfile".<br />
<br />
It is possible to save the result of Unity profiler by watching the status of Unity Profiler and by calling "UnityEditorInternal.ProfilerDriver.SaveProfile" before over history (300 Frame)<br />

## How to record
Call "Tools->IntervalProfilerSave", then window will appear.<br/>
※This tool requires ProfilerWindow.

![Alt text](/Documentation~/img/IntervalRecordMode.png)

1).Directory that log files will be saved<br />
2).Mode change tab."Recorder"<br />
   "Recorder" is for saving the result. If it is "Recorder" , this tool watch the status of Unity Profiler and save the result.<br />
   "Viewer" is for checking the result.<br />
3).Clear Unity Profiler<br />
4).Current target of Unity Profieler<br />
5).This shows the file which was written last.<br />

## How to watch log

![Alt text](/Documentation~/img/IntervalViewMode.png)

1).Refresh the list of files.<br />
2).Profiler will read the file.<br />


