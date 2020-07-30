using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFirstOouiApp
{
    public class Search
    {
        Term[] terms;
        public Search (string search)
        {
            search = search.Trim();
            string[] termsplit = search.Split(";");
            List<Term> termList = new List<Term>();
            for (int i = 0; i < termsplit.Length; i++)
            {
                termList.Add(new Term(termsplit[i]));
            }
            for (int i = termList.Count - 1; i >= 0; i--)
            {
                if (termList[i].isEmpty())
                    termList.RemoveAt(i);
            }
            terms = termList.ToArray();
        }
        public (bool isMatch, string actual) Match (string verse, string roots, string tran)
        {
            bool match = false;
            string matching = "";

            foreach (Term term in terms)
            {
                var result = term.Match(verse, roots, tran);
                if (result.isMatch)
                {
                    match = true;
                    matching += " ; " + result.actual;
                }
            }

            if (match)
            {
                return (true, matching.Substring(3));
            }
            else
            {
                return (false, "");
            }
        }
        public bool isEmpty ()
        {
            return terms.Length == 0;
        }
        public override string ToString()
        {
            string joined = "";
            string LTRMark = "\u200E";
            if (isEmpty()) return "";
            foreach (Term t in terms)
            {
                joined += " " + LTRMark + "; " + t.ToString();
            }
            return joined.Substring(3);
        }
    }
    class Term
    {
        bool include;
        Phrase[] phrases;
        public Term (string term)
        {
            if (term.Length >= 3 && term.Substring(0, 3) == "-_ ")
            {
                include = false;
                term = term.Substring(3).Trim();
            }
            else include = true;
            string[] termsplit = term.Split("&");
            List<Phrase> phraseList = new List<Phrase>();
            for (int i = 0; i < termsplit.Length; i++)
            {
                if (termsplit[i].Trim() != "")
                {
                    phraseList.Add(new Phrase(termsplit[i].Trim()));
                }
            }
            for (int i = phraseList.Count - 1; i >= 0; i--)
            {
                if (phraseList[i].isEmpty()) 
                    phraseList.RemoveAt(i) ;
            }
            phrases = phraseList.ToArray();
        }
        public (bool isMatch, string actual) Match(string verse, string roots, string tran)
        {
            bool match = true;
            string matching = "";

            foreach (Phrase phrase in phrases)
            {
                var result = phrase.Match(verse, roots, tran);
                if (result.isMatch)
                {
                    matching += " & " + result.actual;
                }
                else match = false;
            }

            if (!match) matching = "";
            else if (!include) matching = "";
            if (matching != "") matching = matching.Substring(3);

            //              Match   No Match
            //Include       True    False
            //No Include    False   True

            return (match == include, matching);
        }
        public bool isEmpty ()
        {
            return phrases.Length == 0;
        }
        public override string ToString()
        {
            string RTLMark = "\u200F";
            string LTRMark = "\u200E";
            string joined = "";
            if (isEmpty()) return "";
            foreach (Phrase p in phrases)
            {
                joined += " " + LTRMark + "& " + p.ToString();
            }
            return RTLMark + joined.Substring(3);
        }
    }
    enum PhraseType { Arabic, English}
    class Phrase
    {
        PhraseType type;
        bool include;
        Word[] words;
        public Phrase (string phrase)
        {
            if (phrase.Length >= 2 && phrase.Substring(0, 2) == "- ")
            {
                include = false;
                phrase = phrase.Substring(2).Trim();
            }
            else include = true;
            if (phrase.Length >= 1 && phrase.Substring(0, 1) == ">")
            {
                type = PhraseType.English;
                phrase = phrase.Substring(1).Trim();
            }
            else type = PhraseType.Arabic;

            phrase = spaceRemover(phrase);
            string[] phrasesplit = phrase.Split(" ");
            List<Word> wordList = new List<Word>();

            for (int i = 0; i < phrasesplit.Length; i++)
            {
                Word newword = new Word(phrasesplit[i], type == PhraseType.English);
                if (!newword.isEmpty()) wordList.Add (newword);
            }
            words = wordList.ToArray();
        }
        string spaceRemover (string phrase)
        {
            phrase = phrase.Trim();
            while (phrase.Contains("  "))
            {
                phrase = phrase.Replace("  ", " ");
            }
            return phrase;
        }

        public (bool isMatch, string actual) Match(string verse, string roots, string tran)
        {
            if (type == PhraseType.Arabic)
                return Match(verse, roots);
            else if (type == PhraseType.English)
                return Match(tran);
            else return (false, "ERROR");
        }

        //Assumes type is Arabic
        (bool isMatch, string actual) Match(string verse, string roots)
        {
            string[] vwords = verse.Split(" ");
            string[] rwords = roots.Split(" ");

            List<int> starters = new List<int>();
            for (int i = 0; i < vwords.Length; i++)
            {
                if (words[0].Match(vwords[i], rwords[i]))
                    starters.Add(i);
            }

            bool match = false;
            string matching = "";
            foreach (int s in starters)
            {
                for (int i = s; i < vwords.Length; i++)
                {
                    if (words[i - s].Match(vwords[i], rwords[i]))
                    {
                        matching += vwords[i] + " ";
                        if (i - s == words.Length - 1)
                        {
                            match = true;
                            break;
                        }
                    }
                    else
                    {
                        matching = "";
                        break;
                    }
                }
                if (match) break;
            }

            if (!match) matching = "";
            else if (!include) matching = "";
            if (matching != "") matching = matching.TrimEnd();

            //              Match   No Match
            //Include       True    False
            //No Include    False   True

            return (match == include, matching);
        }
        //Assumes type is English
        (bool isMatch, string actual) Match (string tran)
        {
            string[] twords = tran.Split(" ");

            List<int> starters = new List<int>();
            for (int i = 0; i < twords.Length; i++)
            {
                if (words[0].Match(twords[i], ""))
                    starters.Add(i);
            }

            bool match = false;
            string matching = "";
            foreach (int s in starters)
            {
                for (int i = s; i < twords.Length; i++)
                {
                    if (words[i - s].Match(twords[i], ""))
                    {
                        matching += twords[i] + " ";
                        if (i - s == words.Length - 1)
                        {
                            match = true;
                            break;
                        }
                    }
                    else
                    {
                        matching = "";
                        break;
                    }
                }
                if (match) break;
            }

            if (!match) matching = "";
            else if (!include) matching = "";

            //              Match   No Match
            //Include       True    False
            //No Include    False   True

            return (match == include, matching);
        }
        public (int count, CountType countType) Count (string verse, string roots, string tran)
        {
            return (1, CountType.Phrase);
        }
        public bool isEmpty()
        {
            return words.Length == 0;
        }
        public override string ToString()
        {
            string LTRMark = "\u200E";
            string RTLMark = "\u200F";
            string joined = "";
            foreach (Word w in words)
            {
                joined += " " + w.ToString();
            }
            if (type == PhraseType.English)
            {
                 return LTRMark + joined.Substring(1);
            }
            else
            {
                return joined.Substring(1);
            }
        }
    }
    enum WordType { Arabic, English}
    class Word
    {
        WordType type;
        bool include;
        WordReq[] reqs;
        public Word (string rs, bool english)
        {
            if (rs.Length >= 2 && rs.Substring(0, 2) == "-_")
            {
                include = false;
                rs = rs.Substring(2);
            }
            else include = true;

            string[] wordsplit = rs.Split("/");
            List<WordReq> reqList = new List<WordReq>();
            for (int i = 0; i < wordsplit.Length; i++)
            {
                WordReq newwr = new WordReq(wordsplit[i], english);
                if (!newwr.isEmpty()) reqList.Add(newwr);
            }
            reqs = reqList.ToArray();
        }
        public bool Match (string word, string root)
        {
            foreach (WordReq wr in reqs)
            {
                if (type == WordType.Arabic)
                    if (wr.Match(word, root) == false) return !include;
                else if (type == WordType.English)
                    if (wr.Match(word) == false) return !include;
            }
            return include;
        }
        public (int count, CountType countType) Count (string word, string root)
        {
            if (reqs.Length == 1 && reqs[0].getType() != WordReqType.Root && include)
            {
                return (reqs[0].Count(word), CountType.SubWord);
            }
            else 
            {
                if (Match(word, root))
                {
                    return (1, CountType.Word);
                }
                else
                {
                    return (0, CountType.Word);
                }
            }
        }
        public bool isEmpty ()
        {
            return reqs.Length == 0;
        }
        public override string ToString()
        {
            string joined = "";
            foreach (WordReq wr in reqs)
            {
                joined += "/" + wr.ToString();
            }
            return joined.Substring(1);
        }
    }
    enum WordReqType { Arabic, English, Root }
    enum CountType { SubWord, Word, Phrase, Verse}
    class WordReq
    {
        WordReqType type;
        bool include;
        string req;
        public WordReq(string r, bool english)
        {
            if (r.Length >= 1 && r.ToCharArray()[0] == '-')
            {
                include = false;
                r = r.Substring(1);
            }
            else include = true;
            req = r;
            if (english) type = WordReqType.English;
            else
            {
                req = dealSpecialLetters(req);
                req = arabify(req);
                req = dealSpecialLetters(req);
                if (req.Contains(","))
                {
                    type = WordReqType.Root;
                    req.Replace(",", "");
                }
                else type = WordReqType.Arabic;
            }
        }
        public bool Match (string word, string root)
        {
            word = dealSpecialLetters(word);
            root = dealSpecialLetters(root);
            word = "~" + word + "~";
            if (type == WordReqType.Arabic)
            {
                //int cnt = (word.Length - word.Replace(req, "").Count()) / req.Length;
                if (word.Contains(req))
                    return (include);
                else return (!include);
            }
            else if (type == WordReqType.English)
            {
                //int cnt = (word.Length - word.Replace(req, "").Count()) / req.Length;
                if (word.Contains(req))
                    return include;
                else return !include;
            }
            else if (type == WordReqType.Root)
            {
                if (root == req.Replace(",", ""))
                    return include;
                else return !include;
            }
            else return false;
        }
        public bool Match(string word)
        {
            word = word = "~" + word + "~";
            if (type == WordReqType.English)
            {
                if (word.Contains(req) || word == "*")
                    return include;
                else return !include;
            }
            else return false;
        }
        public int Count (string word)
        {
            return (word.Length - word.Replace(req, "").Count()) / req.Length;
        }
        public bool isEmpty ()
        {
            return req.Trim() == "";
        }
        string arabify(string term)
        {
            string word = term;
            word = " " + word + " ";
            word = word.Replace("Allah", "!للَّه");
            word = word.Replace("lillah", "لِلَّه");
            word = word.Replace("Aa", "@aa");
            word = word.Replace("Ee", "@ee");
            word = word.Replace("Oo", "@oo");
            word = word.Replace("al-", "$");

            word = word.Replace(" a", " !");
            word = word.Replace(",a", ",!");
            word = word.Replace("&a", "&!");
            word = word.Replace(";a", ";!");

            word = word.Replace("Ai", "@ai");
            word = word.Replace("Ao", "@ao");
            word = word.Replace("An", "@an");
            word = word.Replace("In", "@in");
            word = word.Replace("On", "@on");

            word = word.Replace("ai", "َيْ");
            // word = word.Replace("aa", "َا");
            word = word.Replace("aa", "َ!");
            word = word.Replace("ee", "ِي");
            word = word.Replace("oo", "ُو");
            word = word.Replace("ao", "َوْ");
            word = word.Replace("in", "ٍ");
            word = word.Replace("an", "ً");
            word = word.Replace("on", "ٌ");
            word = word.Replace("a.", "aأْ");
            word = word.Replace("o.", "oؤْ");
            word = word.Replace("i.", "iئ.");
            //word = word.Replace("A", "[أَ,ءَ,ؤَ,ئَ]");
            word = word.Replace("A", "@a");
            word = word.Replace("I", "@i");
            word = word.Replace("O", "@o");
            word = word.Replace("a", "َ");
            word = word.Replace("i", "ِ");
            word = word.Replace("o", "ُ");
            word = word.Replace("u", "ُ");
            word = word.Replace(".", "ْ");
            word = word.Replace("-", "ْ");
            // "ابتثجحخدذرزسشصضطظعغفقكلمنهوي"
            word = word.Replace("#", "ّ");
            word = word.Replace("thth", "ثّ");
            word = word.Replace("HH", "حّ");
            word = word.Replace("khkh", "خّ");
            word = word.Replace("dhdh", "ضّ");
            word = word.Replace("zhzh", "ظّ");
            word = word.Replace("''", "عّ");
            word = word.Replace("ghgh", "غّ");
            word = word.Replace("zz", "ذّ");
            word = word.Replace("ZZ", "زّ");
            word = word.Replace("shsh", "شّ");
            word = word.Replace("TT", "طّ");
            word = word.Replace("bb", "بّ");
            word = word.Replace("tt", "تّ");
            word = word.Replace("SS", "صّ");
            word = word.Replace("ss", "سّ");
            word = word.Replace("jj", "جّ");
            word = word.Replace("dd", "دّ");
            word = word.Replace("rr", "رّ");
            word = word.Replace("ff", "فّ");
            word = word.Replace("qq", "قّ");
            word = word.Replace("kk", "كّ");
            word = word.Replace("ll", "لّ");
            word = word.Replace("mm", "مّ");
            word = word.Replace("nn", "نّ");
            word = word.Replace("NN", "نّ");
            word = word.Replace("hh", "هّ");
            word = word.Replace("vv", "وّ");
            word = word.Replace("ww", "وّ");
            word = word.Replace("yy", "يّ");
            while (word.Contains("$"))
            {
                try
                {
                    if (word[word.IndexOf("$") + 2] == 'ّ')
                    {
                        word = ReplaceFirst(word, "$", "!ل");
                    }
                    else
                    {
                        word = ReplaceFirst(word, "$", "!لْ");
                    }
                }
                catch
                {
                    if (word.IndexOf("$") == word.Length - 1)
                    {
                        word = ReplaceFirst(word, "$", "!ل");
                    }
                    else word = ReplaceFirst(word, "$", "!لْ");
                }

            }

            word = word.Replace("th", "ث");
            word = word.Replace("ht", "ة");
            word = word.Replace("H", "ح");
            word = word.Replace("kh", "خ");
            word = word.Replace("dh", "ض");
            word = word.Replace("zh", "ظ");
            word = word.Replace("'", "ع");
            word = word.Replace("gh", "غ");
            word = word.Replace("z", "ذ");
            word = word.Replace("Z", "ز");
            word = word.Replace("sh", "ش");
            word = word.Replace("T", "ط");
            word = word.Replace("b", "ب");
            /*word = word.Replace("@", "[أ,ء,ؤ,ئ]");
            word = word.Replace("A", "[أَ,ءَ,ؤَ,ئَ]");
            word = word.Replace("I", "[إِ,ءِ,ئِ]");
            word = word.Replace("O", "[أُ,ءُ,ؤُ,ئُ]");
            word = word.Replace("E", "ا");*/
            word = word.Replace("t", "ت");
            word = word.Replace("S", "ص");
            word = word.Replace("s", "س");
            word = word.Replace("j", "ج");
            word = word.Replace("d", "د");
            word = word.Replace("r", "ر");
            word = word.Replace("f", "ف");
            word = word.Replace("q", "ق");
            word = word.Replace("k", "ك");
            word = word.Replace("l", "ل");
            word = word.Replace("m", "م");
            word = word.Replace("n", "ن");
            word = word.Replace("N", "ن");
            word = word.Replace("h", "ه");
            word = word.Replace("v", "و");
            word = word.Replace("w", "و");
            word = word.Replace("y", "ي");
            word = word.Replace("Y", "ى");
            word = word.Replace("e", "@");
            return word.Trim();
        }
        string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        string displayArabic(string term)
        {
            string word = term;
            //"[أَ,ءَ,ؤَ,ئَ]"
            word = word.Replace(",", "");
            word = word.Replace(")", "﴿");
            word = word.Replace("(", "﴾");
            word = word.Replace("!@", "اء");
            word = word.Replace("ي@", "يء");
            word = word.Replace("و@", "وء");
            word = word.Replace("يْ@", "يْء");
            word = word.Replace("وْ@", "وْء");
            word = word.Replace("@َ!", "آ");
            word = word.Replace("@" + "ِ", "إِ");
            word = word.Replace("@" + "َ", "أَ");
            word = word.Replace("@" + "ُ", "أُ");
            word = word.Replace("@" + "ِي", "إِي");

            word = word.Replace("َ" + "@ْ", "َ" + "أْ");
            word = word.Replace("ُ" + "@ْ", "ُ" + "ؤْ");
            word = word.Replace("ِ" + "@ْ", "ِ" + "ئْ");

            word = word.Replace("@", "ء");


            word = word.Replace("!", "ا");

            return word;
        }
        string dealSpecialLetters(string verse)
        {
            verse = verse.Replace("ئ", "@");
            verse = verse.Replace("ؤ", "@");
            verse = verse.Replace("ء", "@");
            verse = verse.Replace("أ", "@");
            verse = verse.Replace("إ", "@");
            verse = verse.Replace("آ", "@" + "َ" + "!");

            verse = verse.Replace("ا", "!");
            verse = verse.Replace("ٰ", "!");
            verse = verse.Replace("ى", "!");

            return verse;
        }
        public WordReqType getType()
        {
            return type;
        }
        public override string ToString()
        {
            string RTLMark = "\u200F";
            if (type == WordReqType.English)
            {
                return req;
            }
            else
            {
                return displayArabic(req) + RTLMark;
            }
        }
    }

    class VerseStat
    {
        public int surahs, verses;
    }
    class PhraseWordStat : VerseStat
    {
        public int phrases;
    }
    class SubWordStat : PhraseWordStat
    {
        public int subwords;
    }
    class Test
    {
        void test1()
        {
            VerseStat vstat = new SubWordStat();
            ///test2(new SubWordStat());
            
        }
        void test2 ()
        {

        }
    }
}