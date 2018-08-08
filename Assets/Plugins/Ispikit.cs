using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Timers;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class Ispikit : MonoBehaviour
{
    public delegate void initCallbackDelegate(int n);
    public delegate void resultCallbackDelegate(int score, int speed, string words);
    public delegate void completionCallbackDelegate(int completion);
    public delegate void newWordsCallbackDelegate(string words);
    public delegate void newAudioCallbackDelegate(int volume, string pitch, string waveform);
    public delegate void playbackDoneCallbackDelegate();

    [DllImport("upal")]
    private static extern int startInitialization(initCallbackDelegate icb, string path);
    [DllImport("upal")]
    private static extern int setPlaybackDoneCallback(playbackDoneCallbackDelegate pcb);
    [DllImport("upal")]
    private static extern int setResultCallback(resultCallbackDelegate rcb);
    [DllImport("upal")]
    private static extern int setCompletionCallback(completionCallbackDelegate ccb);
    [DllImport("upal")]
    private static extern int setNewWordsCallback(newWordsCallbackDelegate nwcb);
    [DllImport("upal")]
    private static extern int setNewAudioCallback(newAudioCallbackDelegate nacb);
    [DllImport("upal")]
    private static extern int startRecording(string sentences);
    [DllImport("upal")]
    private static extern int stopRecording();
    [DllImport("upal")]
    private static extern int setStrictness(int strictness);
    [DllImport("upal")]
    private static extern int startPlayback();
    [DllImport("upal")]
    private static extern int stopPlayback();
    [DllImport("upal")]
    private static extern int addWord(string word, string pronunciation);

    private static System.Timers.Timer timer;
    private static bool stopped;

    void Awake()
    {
        Debug.Log("About to initialize plugin");
        stopped = false;
        Debug.Log(startInitialization(new initCallbackDelegate(this.initCallback), Application.persistentDataPath));

    }

    void StopAll()
    {
        Debug.Log("Stopping all");
        stopped = true;
        stopRecording();
        stopPlayback();
    }

    void OnApplicationQuit()
    {
        StopAll();
    }

    public static string text123;
    public static string[] text_sourse = File.ReadAllLines("/mnt/sdcard/text.txt");
    public static string text_result = "";
    public static string text_record = "";
    public static bool start = false;

    GUIStyle largeFont;

    void Start()
    {
        largeFont = new GUIStyle();
        largeFont.fontSize = 72;

        for (int i = 0; i < text_sourse.Length; i++)
        {
            text_record += text_sourse[i] + ",";
        }

    }

    static void mythread1()
    {
        ftp ftpClient = new ftp(@"ftp://185.91.177.214/ftp/", "server", "12345678");
        ftpClient.upload("myfile.wav", @"/mnt/sdcard/myfile.wav");
    }

    void Update()
    {
        string[] StringsArray = text123.Split('-', ' ');
        int[] IntArray = StringsArray.Select(x => int.Parse(x)).ToArray();
        text_result = "";
        for (int j = 0; j < text_sourse.Length; j++)
        {
            for (int i = 0; i < IntArray.Length; i = i + 4)
            {
                if (IntArray[i] == j)
                {
                    string[] StringsSourse = text_sourse[j].Split(' ');
                    text_result += " " + StringsSourse[IntArray[i + 1]];
                }
            }
        }

    }

    void OnGUI()
    {
        if (text123 != "")
        {
            GUI.Label(new Rect(50, 50, 200, 100), text_result, largeFont);
            GUI.Label(new Rect(50, 200, 200, 100), text123, largeFont);
        }
        else
            GUI.Label(new Rect(50, 50, 200, 100), "lol", largeFont);

        if (start == true)
            GUI.Label(new Rect(50, 400, 200, 100), "говорите", largeFont);
        else
            GUI.Label(new Rect(50, 400, 200, 100), "молчите", largeFont);

        if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 50, 200, 100), "Запись"))
        {
            startrecord();
            StartCoroutine("record2");
        }
    }

    public void initCallback(int status)
    {
        Debug.Log("Plugin initialization done");
        Debug.Log(status);
        Debug.Log("About to wire callbacks");
        setResultCallback(new resultCallbackDelegate(resultCallback));
        setCompletionCallback(new completionCallbackDelegate(completionCallback));
        setNewWordsCallback(new newWordsCallbackDelegate(newWordsCallback));
        setNewAudioCallback(new newAudioCallbackDelegate(newAudioCallback));
    }

    private void onRecordingDone(object source, ElapsedEventArgs e)
    {
        Debug.Log("Stopping recording");
        stopRecording();
        start = false;
    }

    public static void resultCallback(int score, int speed, string words)
    {
        Debug.Log("Result");
        Debug.Log(score);
        Debug.Log(speed);
        Debug.Log(words);
    }

    private void startrecord()
    {
        Debug.Log("Starting record");
        start = true;
        if (stopped)
            return;
        startRecording(text_record);
        timer = new System.Timers.Timer(3000);
        timer.Elapsed += onRecordingDone;
        timer.AutoReset = false;
        timer.Enabled = true;
    }

    static AudioClip myAudioClip;

    IEnumerator record2()
    {
        myAudioClip = Microphone.Start(null, false, 3, 44100);
        yield return new WaitForSeconds(3);
        Savewaw.Save("/mnt/sdcard/myfile", myAudioClip);
        Thread thread1 = new Thread(mythread1);
        thread1.Start();
    }

    public static void completionCallback(int completion)
    {
        Debug.Log("Completion");
        Debug.Log(completion);
    }

    public static void newWordsCallback(string words)
    {
        Debug.Log("New words");
        Debug.Log(words);
        text123 = words;
    }

    public static void newAudioCallback(int volume, string pitch, string waveform)
    {
        Debug.Log("New audio data");

        Debug.Log(volume);
        Debug.Log(pitch);
        Debug.Log(waveform);
    }
}