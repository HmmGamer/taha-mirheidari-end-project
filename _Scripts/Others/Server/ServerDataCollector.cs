using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ServerDataCollector : Singleton_Abs<ServerDataCollector>
{
    private string _webhookUrl = "https://eo4i3anav2di8mj.m.pipedream.net";
    private float _startTime;
    private float _pausedTime;
    private float _pauseStartTime;
    private bool _isPaused;

    private void Start()
    {
        _startTime = Time.time;
        _pausedTime = 0f;
        _isPaused = false;
    }
    public void _PauseTime()
    {
        if (_isPaused)
            return;

        _pauseStartTime = Time.time;
        _isPaused = true;
    }
    public void _UnPauseTime()
    {
        if (!_isPaused)
            return;

        _pausedTime += Time.time - _pauseStartTime;
        _isPaused = false;
    }
    public void _SendDataToServer()
    {
        float totalElapsedTime = Time.time - _startTime;
        float totalActiveTime = totalElapsedTime - _pausedTime;
        int totalTime = Mathf.Max(0, Mathf.RoundToInt(totalActiveTime));

        bool isWin = WinAndLoseManager._instance._isWin;
        int score = ScoreManager._instance._currentScore;
        int level = LevelManager._instance._currentLevelIndex;
        int totalChallenges = ChallengeManager._instance._GetChallengeCount();
        int totalCoins = CoinManager._instance._completeCalculatedCoins;
        string description = "Nothing For Now";

        var payload = new LevelResultPayload()
        {
            level = level,
            score = score,
            activeChallenges = totalChallenges,
            totalEarnedCoins = totalCoins,
            completionTime = totalTime,
            description = description,
            isWin = isWin,
            timestampUtc = System.DateTime.UtcNow.ToString("o")
        };

        string json = JsonUtility.ToJson(payload);
        StartCoroutine(_PostJsonCoroutine(_webhookUrl, json));
    }
    private IEnumerator _PostJsonCoroutine(string iUrl, string iJson)
    {
        using (UnityWebRequest req = new UnityWebRequest(iUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(iJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"[ServerDataCollector] POST failed ({req.responseCode}): {req.error}");
            }
            else
            {
                //Debug.Log($"[ServerDataCollector] Data sent successfully! Response: {req.downloadHandler.text}");
            }
        }
    }

    [System.Serializable]
    private class LevelResultPayload
    {
        public int level;
        public int score;
        public int activeChallenges;
        public int totalEarnedCoins;
        public int completionTime;
        public string description;
        public bool isWin;
        public string timestampUtc;
    }
}