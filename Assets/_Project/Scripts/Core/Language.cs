// Stream A. The game's language: Italian ("it", the default) or English ("en"). DataLoader reads
// Resources/Data/<lang>/<file>.json when it exists (made by Tools/localize/make_locale.py) and the English
// Resources/Data/<file>.json otherwise. The choice is remembered in PlayerPrefs (a setting, not a save game).
using System;
using UnityEngine;

namespace ByteWorld
{
    public static class Language
    {
        public const string Italian = "it";
        public const string English = "en";
        const string PrefKey = "byteworld.lang";

        /// <summary>Raised after the language changed and the data was reloaded.</summary>
        public static event Action Changed;

        public static string Current
        {
            get
            {
                try
                {
                    var saved = PlayerPrefs.GetString(PrefKey, Italian);
                    return saved == English ? English : Italian;
                }
                catch
                {
                    return Italian;
                }
            }
        }

        public static void Set(string language)
        {
            if (language == Current) return;
            try
            {
                PlayerPrefs.SetString(PrefKey, language == English ? English : Italian);
                PlayerPrefs.Save();
            }
            catch { /* storage blocked: the switch still works until the page reloads */ }
            DataLoader.Instance?.Reload();
            Changed?.Invoke();
        }

        public static void Toggle() => Set(Current == Italian ? English : Italian);
    }
}
