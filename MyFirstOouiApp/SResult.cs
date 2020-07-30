using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFirstOouiApp
{
    public class SResult
    {
        public int surah, ayah; public string word;
        public SResult(int s, int v, string w)
        {
            surah = s;
            ayah = v;
            word = w;
        }
        public override string ToString()
        {
            return surah.ToString() + ":" + ayah.ToString();
        }
    }
    public class Stat
    {
        public StatType type;
        public string word;
        public int verses = 0;
        public int words = 0;
        public int appears = 0;
        public Stat(string w, StatType t)
        {
            word = w;
            type = t;
        }
        public override string ToString()
        {
            switch (type)
            {
                case (StatType.OnlyVerse):
                {
                    return "The phrases/words " + word + " appeared in " + verses + " verses.";
                };
                case (StatType.CountAndVerse):
                {
                    //if (word.Trim().Split(' ').Length == 1)
                        return "The root " + word + " appeared in " + words + " words in " + verses + " verses.";
                    //else return "The phrase " + word + " appeared " + words + " times in " + verses + " verses.";
                };
                case (StatType.SubWord):
                {
                    return "The word/letter(s) " + word + " appeared " + appears + " times in " + words + " words in " + verses + " verses.";
                };
                case (StatType.Root):
                {
                    return "The root " + word + " appeared in " + words + " words in " + verses + " verses.";
                };
                case (StatType.RootAndPhrase):
                {
                        return "The phrase " + word + " appeared " + words + " times in " + verses + " verses.";
                };
            }
            return "Error in Code: Wrong Statistics Type.";
        }
    }
    public enum StatType
    {
        SubWord,
        CountAndVerse,
        OnlyVerse,
        Root,
        RootAndPhrase
    }
}
