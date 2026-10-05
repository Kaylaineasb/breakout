using System;
using System.Collections.Generic;
using UnityEngine;

// Guarda as últimas partidas no PlayerPrefs (no WebGL fica no IndexedDB do navegador)
public static class ScoreHistory
{
    const string Key = "RecentScores";
    const int MaxEntries = 3;

    // Mais recente primeiro
    public static int[] Load()
    {
        string raw = PlayerPrefs.GetString(Key, string.Empty);
        if (string.IsNullOrEmpty(raw)) return Array.Empty<int>();

        var scores = new List<int>();
        foreach (string part in raw.Split(','))
            if (int.TryParse(part, out int value)) scores.Add(value);
        return scores.ToArray();
    }

    public static int[] Add(int score)
    {
        var scores = new List<int>(Load());
        scores.Insert(0, score);
        if (scores.Count > MaxEntries)
            scores.RemoveRange(MaxEntries, scores.Count - MaxEntries);

        PlayerPrefs.SetString(Key, string.Join(",", scores));
        PlayerPrefs.Save(); // necessário no WebGL para persistir na hora
        return scores.ToArray();
    }
}
