using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Windows.Speech;
using UnityEngine.UI;

public class SayShellsScript : MonoBehaviour {

    public KMAudio audio;
    public KMSelectable[] buttons;
    public MeshRenderer[] btnHighlights;
    public Text display;
    public MeshRenderer[] ledOn;
    public MeshRenderer[] ledOff;
    public Color[] textColors;

    string[] starters = { "she sells", "she shells", "sea shells", "sea sells" };
    string[] finishers = { "sea shore", "she sore", "she sure", "seesaw", "seizure", "shell sea", "steep store", "sheer sort", "seed spore", "sieve horn", "steel sword" };
    string[,] seaShellsTable = { { "DAABDAB", "ACEEAC", "BDABDAB", "EACEACE" }, { "BEEDA", "CDCCDB", "BEEBBE", "EAEAEA" }, { "ABDBAA", "EAAEEA", "ABABA", "DBEAC" }, { "CECEC", "DBAEC", "ACACEAC", "EBDADAB" } };
    string[,] seaShellsTable2 = { { "shoe", "shih tzu", "she", "sit", "sushi" }, { "can", "toucan", "tutu", "2", "cancan" }, { "witch", "switch", "itch", "twitch", "stitch" }, { "burglar alarm", "bulgaria", "armour", "burger", "llama" } };
    string[,] moneyGameTable = { { "1523351", "145322", "1542413", "5315524" }, { "43233", "322431", "544343", "545225" }, { "123531", "453151", "35335", "31415" }, { "51142", "42135", "3435545", "3515413" } };
    List<List<string>> moneyGameLyrics = new List<List<string>>() {
        new List<string> { "she", "sells", "sea", "shells", "on", "the", "shore", "but", "value", "of", "these", "will", "fall", "due", "to", "laws", "supply", "and", "demand", "no", "one", "wants", "buy", "cus", "there's", "loads", "sand" },
        new List<string> { "you", "must", "create", "a", "sense", "of", "scarcity", "shells", "will", "sell", "much", "better", "if", "the", "people", "think", "they're", "rare", "see", "bare", "with", "me", "and", "take", "as", "many", "can", "find", "hide", "'em", "on", "an", "island", "stockpile", "high", "until", "rarer", "than", "diamond" },
        new List<string> { "you", "gotta", "make", "the", "people", "think", "that", "they", "want", "'em", "really", "fuckin", "hit", "like", "bronson", "influencers", "product", "placement", "featured", "prime", "time", "entertainment", "if", "haven't", "got", "a", "shell", "then", "you're", "just", "fucking", "waste", "man" },
        new List<string> { "it's", "monopoly", "invest", "inside", "some", "property", "start", "a", "corporation", "make", "logo", "do", "it", "properly", "these", "shells", "must", "sell", "that", "will", "be", "your", "new", "philosophy", "swallow", "all", "morals", "they're", "poor", "man's", "quality" },
        new List<string> { "expand", "clear", "forest", "make", "land", "fresh", "blood", "on", "hands" },
        new List<string> { "why", "just", "shells", "limit", "yourself", "she", "sells", "sea", "sell", "oil", "as", "well" },
        new List<string> { "guns", "sell", "stocks", "diamonds", "rocks", "water", "to", "a", "fish", "the", "time", "clock" },
        new List<string> { "press", "on", "the", "gas", "take", "your", "foot", "off", "brakes", "then", "run", "to", "be", "president", "of", "united", "states" },
        new List<string> { "big", "smile", "mate", "wave", "that's", "great", "now", "the", "truth", "is", "overrated", "tell", "lies", "out", "gate" },
        new List<string> { "polarize", "the", "people", "controversy", "is", "game", "it", "don't", "matter", "if", "they", "hate", "you", "all", "say", "your", "name" },
        new List<string> { "the", "world", "is", "yours", "step", "out", "on", "a", "stage", "to", "round", "of", "applause", "you're", "liar", "cheat", "devil", "whore", "and", "you", "sell", "sea", "shells", "shore" }
    };
    bool[] stagesCompleted = new bool[3];
    bool errorMode;
    string stage1Phrase = "";
    string stage2Phrase = "";
    string stage3Phrase = "";
    string stage1Sequence = "";
    string stage2Sequence = "";
    string[] stage2Words = new string[5];
    int curState;

    static DictationRecognizer dictationRecognizer;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable button = obj;
            button.OnInteract += delegate () { PressButton(button); return false; };
            button.OnHighlight += delegate () { HighlightButton(button); };
            button.OnHighlightEnded += delegate () { if (curState == 2) display.text = stage2Phrase; };
        }
    }

    void Start()
    {
        StartDictationEngine();
        int choice1 = UnityEngine.Random.Range(0, 4);
        int choice2 = UnityEngine.Random.Range(0, 4);
        int choice3 = UnityEngine.Random.Range(0, 4);
        stage1Phrase += starters[choice1] + " ";
        stage1Phrase += starters[choice2] + " on the ";
        stage1Phrase += finishers[choice3];
        Debug.LogFormat("[Say Shells #{0}] Stage 1 phrase: \"{1}\"", moduleId, stage1Phrase);
        string letterSeq = seaShellsTable[choice1, choice2];
        for (int i = 0; i < letterSeq.Length; i++)
        {
            stage1Sequence += seaShellsTable2[choice3, letterSeq[i] - 'A'];
            if (i != letterSeq.Length - 1)
                stage1Sequence += " ";
        }
        Debug.LogFormat("[Say Shells #{0}] Expected sequence of words for stage 1: \"{1}\"", moduleId, stage1Sequence);
        choice1 = UnityEngine.Random.Range(0, 4);
        choice2 = UnityEngine.Random.Range(0, 4);
        choice3 = UnityEngine.Random.Range(0, 11);
        stage2Phrase += starters[choice1] + " ";
        stage2Phrase += starters[choice2] + " on the ";
        stage2Phrase += finishers[choice3];
        Debug.LogFormat("[Say Shells #{0}] Stage 2 phrase: \"{1}\"", moduleId, stage2Phrase);
        for (int i = 0; i < stage2Words.Length; i++)
        {
            string word = moneyGameLyrics[choice3].PickRandom();
            while (stage2Words.Contains(word))
                word = moneyGameLyrics[choice3].PickRandom();
            stage2Words[i] = word;
        }
        Debug.LogFormat("[Say Shells #{0}] Stage 2 words: \"{1}\"", moduleId, stage2Words.Join("\", \""));
        string numberSeq = moneyGameTable[choice1, choice2];
        for (int i = 0; i < numberSeq.Length; i++)
        {
            int ct = 0;
            for (int j = 0; j < moneyGameLyrics[choice3].Count; j++)
            {
                if (stage2Words.Contains(moneyGameLyrics[choice3][j]))
                    ct++;
                if (ct == int.Parse(numberSeq[i].ToString()))
                {
                    stage2Sequence += moneyGameLyrics[choice3][j];
                    break;
                }
            }
            if (i != numberSeq.Length - 1)
                stage2Sequence += " ";
        }
        Debug.LogFormat("[Say Shells #{0}] Expected sequence of words for stage 2: \"{1}\"", moduleId, stage2Sequence);
        stage3Phrase += starters.PickRandom() + " ";
        stage3Phrase += starters.PickRandom() + " on the ";
        stage3Phrase += finishers.PickRandom();
        Debug.LogFormat("[Say Shells #{0}] Stage 3 phrase: \"{1}\"", moduleId, stage3Phrase);
    }

    void OnDestroy()
    {
        CloseDictationEngine();
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            pressed.AddInteractionPunch();
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            if (errorMode)
            {
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                Debug.LogFormat("[Say Shells #{0}] Module solved", moduleId);
                return;
            }
            int index = Array.IndexOf(buttons, pressed);
            if (index == 4)
            {
                curState = index;
                if (stage3Phrase == "")
                {
                    stage3Phrase += starters.PickRandom() + " ";
                    stage3Phrase += starters.PickRandom() + " on the ";
                    stage3Phrase += finishers.PickRandom();
                    Debug.LogFormat("[Say Shells #{0}] New stage 3 phrase: \"{1}\"", moduleId, stage3Phrase);
                }
                if (stagesCompleted[2])
                    display.color = textColors[1];
                else
                    display.color = textColors[0];
                display.text = stage3Phrase;
            }
            else if (index == 0)
            {
                curState = index;
                display.color = textColors[0];
                display.text = stage1Phrase;
            }
            else if (index == 1)
            {
                if (curState != 1)
                {
                    curState = index;
                    if (stagesCompleted[0])
                    {
                        display.color = textColors[1];
                        display.text = stage1Sequence;
                    }
                    else
                    {
                        display.color = textColors[0];
                        display.text = "";
                    }
                }
                else if (!stagesCompleted[0])
                {
                    Debug.LogFormat("[Say Shells #{0}] Submitted \"{1}\" for stage 1", moduleId, display.text);
                    string[] saidSeq = display.text.Split(' ');
                    string[] expSeq = stage1Sequence.Split(' ');
                    if (saidSeq.Length != expSeq.Length)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Say Shells #{0}] Not good enough, try again", moduleId);
                    }
                    else
                    {
                        for (int i = 0; i < saidSeq.Length; i++)
                        {
                            if (!saidSeq[i].Equals(expSeq[i]))
                            {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Say Shells #{0}] Not good enough, try again", moduleId);
                                return;
                            }
                        }
                        ledOff[0].enabled = false;
                        ledOn[0].enabled = true;
                        stagesCompleted[0] = true;
                        display.color = textColors[1];
                        Debug.LogFormat("[Say Shells #{0}] Stage 1 complete", moduleId);
                        if (stagesCompleted.All(x => x))
                        {
                            moduleSolved = true;
                            GetComponent<KMBombModule>().HandlePass();
                            Debug.LogFormat("[Say Shells #{0}] Module solved", moduleId);
                        }
                    }
                }
            }
            else if (index == 2)
            {
                curState = index;
                display.color = textColors[0];
                display.text = stage2Phrase;
            }
            else if (index == 3)
            {
                if (curState != 3)
                {
                    curState = index;
                    if (stagesCompleted[1])
                    {
                        display.color = textColors[1];
                        display.text = stage2Sequence;
                    }
                    else
                    {
                        display.color = textColors[0];
                        display.text = "";
                    }
                }
                else if (!stagesCompleted[1])
                {
                    Debug.LogFormat("[Say Shells #{0}] Submitted \"{1}\" for stage 2", moduleId, display.text);
                    string[] saidSeq = display.text.Split(' ');
                    string[] expSeq = stage2Sequence.Split(' ');
                    if (saidSeq.Length != expSeq.Length)
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Say Shells #{0}] Not good enough, try again", moduleId);
                    }
                    else
                    {
                        for (int i = 0; i < saidSeq.Length; i++)
                        {
                            if (!saidSeq[i].Equals(expSeq[i]))
                            {
                                GetComponent<KMBombModule>().HandleStrike();
                                Debug.LogFormat("[Say Shells #{0}] Not good enough, try again", moduleId);
                                return;
                            }
                        }
                        ledOff[1].enabled = false;
                        ledOn[1].enabled = true;
                        stagesCompleted[1] = true;
                        display.color = textColors[1];
                        Debug.LogFormat("[Say Shells #{0}] Stage 2 complete", moduleId);
                        if (stagesCompleted.All(x => x))
                        {
                            moduleSolved = true;
                            GetComponent<KMBombModule>().HandlePass();
                            Debug.LogFormat("[Say Shells #{0}] Module solved", moduleId);
                        }
                    }
                }
            }
        }
    }

    void HighlightButton(KMSelectable highlighted)
    {
        if (curState == 2)
        {
            int index = Array.IndexOf(buttons, highlighted);
            display.text = stage2Words[index];
        }
    }

    void StartDictationEngine()
    {
        try
        {
            if (dictationRecognizer == null)
                dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult += DictationRecognizer_OnDictationResult;
            dictationRecognizer.DictationComplete += DictationRecognizer_OnDictationComplete;
            dictationRecognizer.DictationError += DictationRecognizer_OnDictationError;
            dictationRecognizer.Start();
        } catch
        {
            errorMode = true;
            Debug.LogFormat("[Say Shells #{0}] Failed to get the defuser's microphone, press any button to solve", moduleId);
        }
    }

    void CloseDictationEngine()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.DictationComplete -= DictationRecognizer_OnDictationComplete;
            dictationRecognizer.DictationResult -= DictationRecognizer_OnDictationResult;
            dictationRecognizer.DictationError -= DictationRecognizer_OnDictationError;
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
                dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
            dictationRecognizer = null;
        }
    }

    void DictationRecognizer_OnDictationComplete(DictationCompletionCause completionCause)
    {
        switch (completionCause)
        {
            case DictationCompletionCause.TimeoutExceeded:
            case DictationCompletionCause.PauseLimitExceeded:
            case DictationCompletionCause.Canceled:
            case DictationCompletionCause.Complete:
                // Restart required
                CloseDictationEngine();
                StartDictationEngine();
                break;
            case DictationCompletionCause.UnknownError:
            case DictationCompletionCause.AudioQualityFailure:
            case DictationCompletionCause.MicrophoneUnavailable:
            case DictationCompletionCause.NetworkFailure:
                // Error
                CloseDictationEngine();
                break;
        }
    }

    void DictationRecognizer_OnDictationResult(string text, ConfidenceLevel confidence)
    {
        text = text.ToLower();
        if (curState == 4)
        {
            if (stage3Phrase == "" || stagesCompleted[2])
                return;
            string[] saidPhrase = text.Replace("seashells", "sea shells").Replace("seashore", "sea shore").Split(' ');
            string[] displayedPhrase = stage3Phrase.Split(' ');
            display.text = "";
            bool fail = false;
            for (int i = 0; i < displayedPhrase.Length; i++)
            {
                if (i >= saidPhrase.Length)
                {
                    display.text += displayedPhrase[i];
                    fail = true;
                }
                else if (displayedPhrase[i].Equals(saidPhrase[i]))
                    display.text += "<color=lime>" + displayedPhrase[i] + "</color>";
                else if (!displayedPhrase[i].Equals(saidPhrase[i]))
                {
                    display.text += "<color=red>" + displayedPhrase[i] + "</color>";
                    fail = true;
                }
                if (i != displayedPhrase.Length - 1)
                    display.text += " ";
            }
            Debug.LogFormat("[Say Shells #{0}] I heard you say \"{1}\" for stage 3", moduleId, saidPhrase.Join());
            if (fail)
            {
                stage3Phrase = "";
                Debug.LogFormat("[Say Shells #{0}] Not good enough, try again", moduleId);
            }
            else
            {
                ledOff[2].enabled = false;
                ledOn[2].enabled = true;
                stagesCompleted[2] = true;
                Debug.LogFormat("[Say Shells #{0}] Stage 3 complete", moduleId);
                if (stagesCompleted.All(x => x))
                {
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                    Debug.LogFormat("[Say Shells #{0}] Module solved", moduleId);
                }
            }
        }
        else if (curState == 1)
        {
            if (stagesCompleted[0])
                return;
            string[] saidPhrase = text.Split(' ');
            for (int i = 0; i < saidPhrase.Length; i++)
            {
                if (saidPhrase[i] == "armor")
                    saidPhrase[i] = "armour";
            }
            display.text = saidPhrase.Join();
        }
        else if (curState == 3)
        {
            if (stagesCompleted[1])
                return;
            string[] saidPhrase = text.Split(' ');
            for (int i = 0; i < saidPhrase.Length; i++)
            {
                if (saidPhrase[i] == "*******")
                    saidPhrase[i] = "fucking";
                else if (saidPhrase[i] == "******")
                    saidPhrase[i] = "fuckin";
                else if (saidPhrase[i] == "*****")
                    saidPhrase[i] = "whore";
                else if (saidPhrase[i] == "em")
                    saidPhrase[i] = "'em";
            }
            display.text = saidPhrase.Join();
        }
    }

    void DictationRecognizer_OnDictationError(string error, int hresult)
    {
        errorMode = true;
        Debug.LogFormat("<Say Shells #{0}> Error: {1}", moduleId, error);
        Debug.LogFormat("[Say Shells #{0}] An error occurred, press any button to solve", moduleId);
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press TL/TR/ML/MR/B [Presses the button in the specified position] | !{0} cycle [Highlights each button briefly] | !{0} say JUST OIL JUST JUST OIL [Say something]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.EqualsIgnoreCase("cycle"))
        {
            yield return null;
            for (int i = 0; i < 5; i++)
            {
                btnHighlights[i].enabled = true;
                buttons[i].OnHighlight();
                yield return new WaitForSeconds(1);
                btnHighlights[i].enabled = false;
                buttons[i].OnHighlightEnded();
            }
        }
        else if (command.EqualsIgnoreCase("press tl"))
        {
            yield return null;
            buttons[0].OnInteract();
        }
        else if (command.EqualsIgnoreCase("press tr"))
        {
            yield return null;
            buttons[1].OnInteract();
        }
        else if (command.EqualsIgnoreCase("press ml"))
        {
            yield return null;
            buttons[2].OnInteract();
        }
        else if (command.EqualsIgnoreCase("press mr"))
        {
            yield return null;
            buttons[3].OnInteract();
        }
        else if (command.EqualsIgnoreCase("press b"))
        {
            yield return null;
            buttons[4].OnInteract();
        }
        else if (command.StartsWith("say "))
        {
            yield return null;
            DictationRecognizer_OnDictationResult(command.Substring(4), ConfidenceLevel.High);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (errorMode)
        {
            buttons[0].OnInteract();
            yield break;
        }
        if (!stagesCompleted[0])
        {
            buttons[1].OnInteract();
            yield return new WaitForSeconds(.1f);
            DictationRecognizer_OnDictationResult(stage1Sequence, ConfidenceLevel.High);
            yield return new WaitForSeconds(.1f);
            buttons[1].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
        if (!stagesCompleted[1])
        {
            buttons[3].OnInteract();
            yield return new WaitForSeconds(.1f);
            DictationRecognizer_OnDictationResult(stage2Sequence, ConfidenceLevel.High);
            yield return new WaitForSeconds(.1f);
            buttons[3].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
        if (!stagesCompleted[2])
        {
            buttons[4].OnInteract();
            yield return new WaitForSeconds(.1f);
            DictationRecognizer_OnDictationResult(stage3Phrase, ConfidenceLevel.High);
            yield return new WaitForSeconds(.1f);
        }
    }
}