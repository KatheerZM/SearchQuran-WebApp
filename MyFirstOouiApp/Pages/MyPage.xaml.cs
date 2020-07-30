using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms;

namespace MyFirstOouiApp.Pages
{
    public partial class MyPage : ContentPage
    {
        string[][] surahs;
        string[][] rsurahs;
        string[][] tsurahs;
        Dictionary<string, string> qroots = new Dictionary<string, string>();
        List<string[]> allTerms = new List<string[]>();
        XmlDocument tran;
        XmlDocument meta;
        int sstart = 0;
        int send = 113;
        string occurText = "";
        string statsText = "";
        int tab = 0;
        Search searchTerm = new Search("");

        public MyPage()
        {
            InitializeComponent();

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MyPage)).Assembly;
            List<string[]> surahstemp = new List<string[]>();

            for (int i = 1; i <= 114; i++)
            {
                Stream stream = assembly.GetManifestResourceStream("MyFirstOouiApp.surahs.s" + i.ToString() + ".txt");
                String text;
                using (var reader = new System.IO.StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                    if (i != 1 && i != 9) text = text.Substring("بِسْمِ اللَّهِ الرَّحْمَنِ الرَّحِيمِ  ".Length);
                }

                surahstemp.Add(text.Split('\n'));

                int last = surahstemp.Count - 1;
                for (int j = 0; j < surahstemp[last].Length; j++)
                {
                    string verse = surahstemp[last][j];
                    verse = " " + verse + " ";
                    verse = verse.Replace(" يَا ", " يَا");
                    verse = verse.Replace(" وَيَا ", " وَيَا");
                    verse = verse.Replace(" هَا ", " هَا");
                    surahstemp[last][j] = verse.Trim();
                }

                stream.Close();
            }
            surahs = surahstemp.ToArray();

            List<string[]> rsurahstemp = new List<string[]>();
            for (int i = 1; i <= 114; i++)
            {
                Stream stream = assembly.GetManifestResourceStream("MyFirstOouiApp.roots.s" + i.ToString() + ".txt");
                String text;
                using (var reader = new System.IO.StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                rsurahstemp.Add(text.Split('\n'));
                stream.Close();
            }
            rsurahs = rsurahstemp.ToArray();

            Stream s = assembly.GetManifestResourceStream("MyFirstOouiApp.en.sahih.xml");
            tran = new XmlDocument();
            tran.Load(s);
            s.Close();

            List<string[]> tsurahstemp = new List<string[]>();
            for (int sn = 0; sn < 114; sn++)
            {
                XmlNode xsurah = tran.LastChild.ChildNodes[sn];
                string[] verses = new string[surahs[sn].Length - 1];
                for (int v = 0; v < surahs[sn].Length - 1; v++)
                {
                    XmlNode xverse = xsurah.ChildNodes[v];
                    string translation = xverse.Attributes[1].Value;
                    verses[v] = translation;
                }
                tsurahstemp.Add(verses);
            }
            tsurahs = tsurahstemp.ToArray();
            

            s = assembly.GetManifestResourceStream("MyFirstOouiApp.quranmeta.xml");
            meta = new XmlDocument();
            meta.Load(s);
            s.Close();

            //Roots
            s = assembly.GetManifestResourceStream("MyFirstOouiApp.oneroot.txt");
            String[] roots;
            using (var reader = new System.IO.StreamReader(s))
            {
                roots = reader.ReadToEnd().Split('\n');
            }
            foreach (string line in roots)
            {
                qroots.Add(line.Split(' ')[0], line.Split(' ')[1]);
            }


            occurtab.Pressed += (sender, args) =>
            {
                stattab.BackgroundColor = Color.LightGray;
                occurtab.BackgroundColor = Color.LightGreen;
                tab = 0;
                updateView(results);
            };
            stattab.Pressed += (sender, args) =>
            {
                occurtab.BackgroundColor = Color.LightGray;
                stattab.BackgroundColor = Color.LightGreen;
                tab = 1;
                updateView(results);
            };

            find.Clicked += (sender, args) =>
            {
                search(find, searchBar, results);
            };
            searchBar.TextChanged += (sender, args) =>
            {
                string stext = searchBar.Text;
                if (searchBar.Text.Length > 3 && searchBar.Text.Trim().Substring(0, 3) == "cp:")
                {
                    stext = stext.Substring(3);
                }
                try
                {
                    searchTerm = new Search(debrack(stext));

                    if (searchTerm.isEmpty()) find.IsEnabled = false;
                    else find.IsEnabled = true;

                    searchingFor.Text = searchTerm.ToString();
                    searchBar.TextColor = Color.Black;
                }
                catch
                {
                    searchBar.TextColor = Color.Red;
                    find.IsEnabled = false;
                }
            };
            searchBar.Completed += (sender, args) =>
            {
                search(find, searchBar, results);
            };
            startSurah.TextChanged += (sender, args) =>
            {
                try
                {
                    int num = Convert.ToInt32(startSurah.Text);
                    if (num > 114 || num < 1)
                    {
                        throw new Exception();
                    }
                    startSurah.BackgroundColor = Color.White;
                    sstart = num - 1;
                }
                catch
                {
                    startSurah.TextColor = Color.Red;
                }
            };
            endSurah.TextChanged += (sender, args) =>
            {
                try
                {
                    int num = Convert.ToInt32(endSurah.Text);
                    if (num > 114 || num < 1)
                    {
                        throw new Exception();
                    }
                    endSurah.BackgroundColor = Color.White;
                    send = num - 1;
                }
                catch
                {
                    endSurah.TextColor = Color.Red;
                }
            };
            searchLabel.FontSize = 2 * Device.GetNamedSize(NamedSize.Small, typeof(Label));
            results.FontSize = 1.2 * Device.GetNamedSize(NamedSize.Small, typeof(Label));

            string instructs;
            s = assembly.GetManifestResourceStream("MyFirstOouiApp.Instruction.txt");
            using (var reader = new System.IO.StreamReader(s))
            {
                instructs = reader.ReadToEnd();
            }
            instructs = instructs.Replace("\n", "&#10");
            //instructions.Text = instructs;
        }
        void search(Button find, Entry searchBar, Editor results)
        {
            find.Text = "Searching...";
            find.IsEnabled = false;
            searchBar.IsEnabled = false;
            Find(searchBar.Text);
            updateView(results);
            searchBar.IsEnabled = true;
            find.IsEnabled = true;
            find.Text = "Find!";
        }
        void updateView (Editor viewer)
        {
            if (tab == 0)
            {
                viewer.Text = occurText;
            }
            else if (tab == 1)
            {
                viewer.Text = statsText;
            }
        } 
        string[] debrack(string[] terms)
        {
            List<string> result = terms.ToList();
            bool isBrackets = false;
            foreach (string t in result)
            {
                if (t.Contains("[") || t.Contains("]"))
                {
                    isBrackets = true;
                    break;
                }
            }
            if (!isBrackets) return terms;

            for (int i = 0; i < result.Count; i++)
            {
                string t = result[i].Trim();
                if (!bracketsCorrect(t)) return null;
                if (t.Contains("["))
                {
                    string first = t.Split('[')[0];
                    string second = t.Substring(first.Length + 1);
                    string inside = "";
                    int b = 0;
                    for (int c = 0; c < second.Length; c++)
                    {
                        if (second[c] == '[') b++;
                        else if (second[c] == ']') b--;
                        if (b == -1)
                        {
                            inside = second.Substring(0, c);
                            break;
                        }
                    }
                    second = String.Join("]", second.Split("]").ToList().GetRange(1, second.Split(']').Length - 1));
                    string[] ops = inside.Split(',');
                    result.RemoveAt(i);
                    for (int j = ops.Length - 1; j >= 0; j--)
                    {
                        result.Insert(i, first + ops[j] + second);
                    }
                    break;
                }
            }
            return debrack(result.ToArray());
        }
        string removeFirst(string words, string replaced)
        {
            return String.Join(replaced, words.Split(replaced).ToList().GetRange(1, words.Split(replaced).Length - 1));
        }
        string debrack(string terms)
        {
            return String.Join(" ; ", debrack(terms.Split(";")));
        }
        void arabify(string[] terms)
        {
            for (int i = 0; i < terms.Length; i++)
            {
                terms[i] = arabify(terms[i]);
            }
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
            word = word.Replace("@", "ء");


            word = word.Replace("!", "ا");

            return word;
        }
        string arabifyIgnoringEng(string term)
        {
            string result = "";
            for (int i = 0; i <  term.Split(';').Length; i++)
            {
                string s = term.Split(';')[i];
                if (i != 0) result += " ; ";
                for (int j = 0; j < s.Trim().Split('&').Length; j++)
                {
                    string a = s.Trim().Split('&')[j];
                    if (j != 0) result += " & ";
                    if (a.Length != 0 && a.Trim().ToCharArray()[0] != '>') result += arabify(a);
                    else result += a;
                }
            }
            return result;
        }
        string arabify(string term)
        {
            string word = term;
            word = " " + word + " ";
            word = word.Replace("Allah", "اللَّه");
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
            word = word.Replace("e", "ء");
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
        bool bracketsCorrect(string t)
        {
            int b = 0;
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == '[') b++;
                else if (t[i] == ']') b--;
                if (b < 0 || b > 1) return false;
            }
            if (b == 0) return true;
            else return false;
        }
        Stat[] statslist = new Stat[0];
        SResult[] lookup(string[] terms)
        {
            terms = Ligase(terms);
            int min, max;
            if (sstart <= send)
            {
                min = sstart;
                max = send;
            }
            else
            {
                min = send;
                max = sstart;
            }
            List<SResult> results = new List<SResult>();
            List<Stat> statresults = new List<Stat>();
            for (int t = 0; t < terms.Length; t++)
            {
                string term = displayArabic(terms[t].Trim());
                if (term.Contains('&'))
                {
                    statresults.Add(new Stat(term, StatType.OnlyVerse));
                }
                else if (term.Trim().Contains(' '))
                {
                    statresults.Add(new Stat(term, StatType.CountAndVerse));
                }
                else if (terms[t].Contains(','))
                {
                    statresults.Add(new Stat(term, StatType.CountAndVerse));
                }
                else
                {
                    statresults.Add(new Stat(term, StatType.SubWord));
                }
            }
            for (int s = min; s < max + 1; s++)
            {
                for (int v = 0; v < surahs[s].Length; v++)
                {
                    string verse = surahs[s][v];
                    verse = dealSpecialLetters(verse);
                    for (int t = 0; t < terms.Length; t++)
                    {
                        string term = dealSpecialLetters(terms[t].Trim());
                        if (shouldCheck (term, verse, s, v)) //verse.Contains(term.Split('&')[0].Trim().Split(' ')[0]) )
                        {
                            string res = phrasesInVerse(term, verse, s, v);
                            if (statresults[t].type == StatType.CountAndVerse)
                            {
                                statresults[t].words += countPhraseInVerse(term, verse, s, v);
                            }
                            else if (statresults[t].type == StatType.SubWord)
                            {
                                statresults[t].appears += countLettersInVerse(term, verse);
                                statresults[t].words += countPhraseInVerse(term, verse, s, v);
                            }
                            if (res != null)
                            {
                                results.Add(new SResult(s, v, res));
                                statresults[t].verses++;
                            }
                            if (results.Count > 500000) return results.ToArray();
                        }
                        
                    }
                    Console.WriteLine(s.ToString() + ":" + v.ToString());
                }
            }
            statslist = statresults.ToArray();
            return results.ToArray();
        }
        bool shouldCheck (string term, string verse, int s, int v)
        {
            string word = term.Trim().Split('&')[0].Trim().Split(' ')[0];
            if (!word.Contains(","))
            {
                if (!verse.Contains(word))
                {
                    return false;
                }
                else return true;
            }
            else
            {
                if (!dealSpecialLetters(rsurahs[s][v]).Contains(word.Replace(",", ""))) return false;
                else return true;
            }
        }
        string phrasesInVerse(string phrases, string verse, int s, int v)
        {
            string[] phrasesList = phrases.Split('&');
            string keys = "";
            for (int i = 0; i < phrasesList.Length; i++)
            {
                string key;
                if (phrasesList[i].Contains(','))
                {
                    key = phraseInVerseRoot(phrasesList[i].Trim(), verse, s, v);
                }
                else
                {
                    key = phraseInVerse(phrasesList[i].Trim(), verse, s, v);
                }
                if (key == null) return null;
                keys += " & " + key;
            }

            return keys.Substring(3);
        }
        string phraseInVerse(string phrase, string verse, int s, int v)
        {
            int pno = 0;
            string[] words = phrase.Split(' ');
            string found = "";

            for (int i = 0; i < verse.Split(' ').Length; i++)
            {
                string lafz = verse.Split(' ')[i];

                if (lafz.Contains(words[pno]))
                { pno++; found += surahs[s][v].Split(' ')[i] + " "; }
                else { pno = 0; found = ""; }
                if (pno == words.Length) return found.Trim();
            }
            return null;
        }
        string phraseInVerseRoot(string phrase, string verse, int s, int v)
        {
            int pno = 0;
            string[] words = phrase.Split(' ');
            string found = "";

            string[] rverse = dealSpecialLetters(rsurahs[s][v]).Split(' ');
            if (s == 1 && v == 9)
            {

            }
            for (int i = 0; i < verse.Split(' ').Length; i++)
            {
                
                string lafz = verse.Split(' ')[i];
                string rlafz = rverse[i];

                if ((!words[pno].Contains(',') && lafz.Contains(words[pno]))
                    || (words[pno].Contains(',') && rlafz == words[pno].Replace(",", "")))
                { pno++; found += surahs[s][v].Split(' ')[i] + " "; }
                else { pno = 0; found = ""; }
                if (pno == words.Length) return found.Trim();
            }
            return null;
        }
        bool containOrRoot(string lafz, string word,int s, int v, int w)
        {
            if (word.Contains(","))
            {
                word = word.Replace(",", "");
                if (qroots[(s + 1).ToString() + ":" + (v + 1).ToString() + ":" + (w + 1).ToString()] == word)
                    return true;
            }
            else
            {
                if (lafz.Contains(word))
                    return true;
            }
            return false;
        }
        int countPhraseInVerse(string phrase, string vrse, int s, int v)
        {
            int count = 0;
            int pno = 0;
            string[] words = phrase.Split(' ');
            string found = "";
            string[] verse = vrse.Split(' ');
            string[] rverse = dealSpecialLetters(rsurahs[s][v]).Split(' ');

            for (int i = 0; i < verse.Length; i++)
            {
                string lafz = verse[i];
                string rlafz = rverse[i];
                if ((!words[pno].Contains (',') && lafz.Contains(words[pno]))
                    || (words[pno].Contains(',') && rlafz == words[pno].Replace(",","")))
                { pno++; found += surahs[s][v].Split(' ')[i] + " "; }
                else { pno = 0; found = ""; }
                if (pno == words.Length)
                {
                    count++;
                    pno = 0; 
                    found = "";
                }
            }
            return count;
        }
        int countLettersInVerse(string word, string verse)
        {
            /*verse = " " + verse.Trim() + " ";
            word = word.Replace("/", " ");
            */
            int full = verse.Length;
            int less = verse.Replace(word, "").Length;
            return (full - less) / word.Length;
        }
        string removeSC(string input)
        {
            return input.Replace("/", "");
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

        SResult[] Ligase(SResult[] res)
        {
            List<SResult> listres = res.ToList();
            for (int i = listres.Count - 1; i > 0; i--)
            {
                if (listres[i].surah == listres[i - 1].surah && listres[i].ayah == listres[i - 1].ayah)
                {
                    listres[i - 1].word += " ; " + listres[i].word;
                    listres.RemoveAt(i);
                }
            }
            return listres.ToArray();
        }
        string[] Ligase(string[] searches)
        {
            List<string> listsearch = searches.ToList();
            for (int i = listsearch.Count - 1; i >= 0; i--)
            {
                if (listsearch[i].Trim() == "")
                {
                    listsearch.RemoveAt(i);
                }
            }
            for (int i = 0; i < listsearch.Count; i++)
            {
                listsearch[i] = listsearch[i].Trim();
            }
            for (int i = 0; i < listsearch.Count; i++)
            {
                for (int j = i + 1; j < listsearch.Count; j++)
                {
                    if (listsearch[i] == listsearch[j]) listsearch.RemoveAt(j);
                }
            }
            return listsearch.ToArray();
        }
        SResult[] lookup2(Search search)
        {
            int min, max;
            if (sstart <= send)
            {
                min = sstart;
                max = send;
            }
            else
            {
                min = send;
                max = sstart;
            }
            List<SResult> results = new List<SResult>();
            for (int s = min; s < max + 1; s++)
            {
                for (int v = 0; v < surahs[s].Length - 1; v++)
                {
                    var result = search.Match(surahs[s][v], rsurahs[s][v], tsurahs[s][v]);
                    if (result.isMatch)
                    {
                        results.Add(new SResult(s, v, result.actual));
                    }
                }
            }
            return results.ToArray();
        }
        void Find(string search)
        {
            bool quick = false;
            if (search.Length > 3 && search.Trim().Substring(0,3) == "cp:")
            {
                quick = true;
            }

            //search = arabifyIgnoringEng(debrack(search));
            //SResult[] finds = lookup(search.Split(';'));
            SResult[] finds = lookup2(searchTerm);
            occurText = "";
            statsText = "";

            finds = Ligase(finds);

            for (int i = 0; i < statslist.Length; i++)
            {
                statsText += statslist[i].ToString() + "\n";
            }

            
            if (finds.Length > 500000)
            {
                occurText += "There are more than 500,000 entries! Subhanallah, what are you searching??\n";
                return;
            }
            else if (finds.Length > 1000)
            {
                occurText += "There are " + finds.Length + " entries! Too many to write.\n";
                return;
            }
            else if (quick) 
            {
                occurText += "For Copy-Paste\n";
                for (int i = 0; i < finds.Length; i++)
                {
                    int s = finds[i].surah;
                    int v = finds[i].ayah;
                    occurText += (s + 1) + ":" + (v + 1) + ", ";
                }
                occurText.Substring(0, occurText.Length - 3);
                return;
            }
            else  if (finds.Length > 200)
            {
                occurText += "There are " + finds.Length + " entries. Too many to print all their texts. Only references will be given.\n";
                int ps = -1;
                for (int i = 0; i < finds.Length; i++)
                {
                    int s = finds[i].surah;
                    int v = finds[i].ayah;
                    string surahname = meta.LastChild.ChildNodes[s].Attributes[4].Value;
                    if (ps != s)
                        occurText += (s + 1) + ":" + (v + 1) + " (" + surahname + ")" + ":- " + finds[i].word + "\n";
                    else
                        occurText += (s + 1) + ":" + (v + 1) + ":- " + finds[i].word + "\n";
                    ps = s;
                }
                return;
            }
            for (int i = 0; i < finds.Length; i++)
            {
                int s = finds[i].surah;
                int v = finds[i].ayah;
                string surahname = meta.LastChild.ChildNodes[s].Attributes[4].Value;
                string translation = tsurahs[s][v];
                TextCell entry = new TextCell
                {
                    Text = s + ":" + v + ":- " + finds[i].word,
                    Detail = surahs[s][v],
                };
                occurText += (s + 1) + ":" + (v + 1) + " (" + surahname + ")" + ":- " + finds[i].word + "\n";
                occurText += surahs[s][v] + "\n" + translation + "\n\n";
            }
            if (finds.Length == 0) occurText = "Nothing found.";
        }
    }
}
