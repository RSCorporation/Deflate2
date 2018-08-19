using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace FactorioPrinter
{
    static class Deflate2
    {
        public static Dictionary<string, int> CountNGramms(string instring)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            Dictionary<string, List<int>> positions = new Dictionary<string, List<int>>();
            List<string>[] unusedgramms = new List<string>[instring.Length + 1];
            int maxgramm = instring.Length;
            positions.Add("", new List<int>());
            for(int i = 0; i <= instring.Length; i++)
            {
                unusedgramms[i] = new List<string>();
                positions[""].Add(i);
            }
            unusedgramms[instring.Length].Add("");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while(sw.ElapsedMilliseconds < 10000)
            {
                while(maxgramm > 1 && unusedgramms[maxgramm].Count == 0) maxgramm--;
                if(maxgramm == 1) break;

                string currgramm = unusedgramms[maxgramm].First();
                int newgrammln = currgramm.Length + 1;
                unusedgramms[maxgramm].Remove(currgramm);

                List<string> allnewgramms = new List<string>();
                foreach(int pos in positions[currgramm])
                {
                    try
                    {
                        string newgramm = instring.Substring(pos, newgrammln);
                        if(!positions.ContainsKey(newgramm))
                        {
                            positions.Add(newgramm, new List<int>());
                            allnewgramms.Add(newgramm);
                        }
                        positions[newgramm].Add(pos);
                    }
                    catch
                    {

                    }
                }
                if(currgramm.Length > 2)
                {
                    foreach(string __gramm in allnewgramms)
                    {
                        if(positions[__gramm].Count == positions[currgramm].Count)
                        {
                            unusedgramms[positions[__gramm].Count].Add(__gramm);
                            goto loopend;
                        }
                        for(int i = 1; i < __gramm.Length; i++)
                        {
                            string subgramm = __gramm.Substring(i);
                            if(positions.ContainsKey(subgramm) && positions[subgramm].Count == positions[__gramm].Count)
                            {
                                unusedgramms[positions[subgramm].Count].Remove(subgramm);
                                positions.Remove(subgramm);
                                unusedgramms[positions[__gramm].Count].Add(__gramm);
                                goto loopend;
                            }
                            if(result.ContainsKey(subgramm) && result[subgramm] == positions[__gramm].Count)
                            {

                            }
                        }
                    }
                }
                result.Add(currgramm, positions[currgramm].Count);
                foreach(string __gramm in allnewgramms)
                {
                   unusedgramms[positions[__gramm].Count].Add(__gramm);
                }

                loopend:
                positions.Remove(currgramm);
            }
            foreach(var i in unusedgramms[1])
            {
                if(i.Length == 1) result.Add(i, 1);
            }
            result.Remove("");
            sw.Stop();
            return result;
        }
        public static Tuple<Dictionary<string, int>, Dictionary<string, int>> DivideNGramms(IEnumerable<KeyValuePair<string, int>> NGramms)
        {
            List<Tuple<string, int>> mappeddictionary = new List<Tuple<string, int>>();
            
            long sum = 0;
            foreach(var pair in NGramms)
            {
                int val = pair.Value * pair.Key.Length;
                mappeddictionary.Add(Tuple.Create(pair.Key, val));
                sum += val;
            }

            int divcoef = (int)(sum / 10000);
            if(divcoef == 0) divcoef = 1;
            sum = 0;
            for(int i = 0; i < mappeddictionary.Count; i++)
            {
                mappeddictionary[i] = Tuple.Create(mappeddictionary[i].Item1, mappeddictionary[i].Item2 / divcoef);
                sum += mappeddictionary[i].Item2;
            }
            int avmaxsum = ((int)sum + 1) / 2;
            Tuple<int, string>[] oldraw;
            Tuple<int, string>[] newraw = new Tuple<int, string>[avmaxsum + 1];
            for(int ijk = 0; ijk <= avmaxsum; ijk++)
            {
                newraw[ijk] = Tuple.Create(0, "");
            }
            for(int i = 0; i < mappeddictionary.Count; i++)
            {
                oldraw = newraw;
                newraw = new Tuple<int, string>[avmaxsum + 1];
                for(int j = 0; j <= avmaxsum; j++)
                {
                    int notakedweight = oldraw[j].Item1;
                    if(j < mappeddictionary[i].Item2)
                    {
                        newraw[j] = Tuple.Create(notakedweight, oldraw[j].Item2 + "0");
                        continue;
                    }
                    int takedweight = oldraw[j - mappeddictionary[i].Item2].Item1 + mappeddictionary[i].Item2;
                    if(takedweight > j || notakedweight > takedweight)
                    {
                        newraw[j] = Tuple.Create(notakedweight, oldraw[j].Item2 + "0");
                    }
                    else
                    {
                        newraw[j] = Tuple.Create(takedweight, oldraw[j - mappeddictionary[i].Item2].Item2 + "1");
                    }
                }
            }
            string vector = newraw[avmaxsum].Item2;
            Dictionary<string, int> subset0 = new Dictionary<string, int>(), subset1 = new Dictionary<string, int>();
            Dictionary<string, int> realngramms = NGramms.ToDictionary(x => x.Key, x => x.Value);;
            for(int i = 0; i < mappeddictionary.Count; i++)
            {
                Dictionary<string, int> csubdict;
                if(vector[i] == '0')
                {
                    csubdict = subset0;
                }
                else
                {
                    csubdict = subset1;
                }
                csubdict.Add(mappeddictionary[i].Item1, realngramms[mappeddictionary[i].Item1]);
            }
            return Tuple.Create(subset0, subset1);
        }
        public static string BuildTree(IEnumerable<KeyValuePair<string, int>> NGramms)
        {
            if(NGramms.Count() == 1)
            {
                return "[" + (char) NGramms.First().Key.Length + NGramms.First().Key;
            }
            else
            {
                var subsets = DivideNGramms(NGramms);
                return "(" + BuildTree(subsets.Item1) + ")(" + BuildTree(subsets.Item2) + ")";
            }
        }
        public static byte[] CompressAPI(string tocompress, Deflate2Tree tree)
        {
            short[] idxs = new short[tocompress.Length];
            BitArray used = new BitArray(tocompress.Length);
            var srt = (from i in tree.GetReplacements() orderby i.Key.Length descending select i).ToArray();
            for(int ijk = 0; ijk < srt.Length; ijk++)
            {
                var i = srt[ijk];
                Console.WriteLine(i.Key);
                int pos = -1;
                do
                {
                    pos = tocompress.IndexOf(i.Key, pos + 1);
                    bool ok = true;
                    for(int j = pos; j < pos + i.Key.Length; j++)
                    {
                        if(used.Get(j))
                        {
                            ok = false;
                            break;
                        }
                    }
                    if(!ok) continue;
                    for(int j = pos; j < pos + i.Key.Length; j++)
                    {
                        used.Set(j, true);
                    }
                    idxs[pos] = (short)(ijk + 1);
                } while(pos >= 0);
            }
            StreamWriter sw = new StreamWriter("cerr.txt");
            foreach(var i in idxs)
            {
                sw.WriteLine(i);
            }
            sw.Close();
            return null;
        }
    }
    class Deflate2Tree
    {
        Deflate2TreeElement root;
        public Deflate2Tree(string newic)
        {
            Deflate2TreeElement current = new Deflate2TreeElement();
            int strptr = 0;
            int depth = 0;
            while(strptr < newic.Length)
            {
                switch(newic[strptr])
                {
                    case '[':
                        byte ln = (byte)newic[strptr + 1];
                        current.IsLeaf = true;
                        current.Value = newic.Substring(strptr + 2, ln);
                        strptr += 2 + ln;
                        break;
                    case '(':
                        if(current.SubTree0 == null)
                        {
                            Deflate2TreeElement st0 = new Deflate2TreeElement();
                            st0.Parent = current;
                            current.SubTree0 = st0;
                            current = st0;
                        }
                        else
                        {
                            Deflate2TreeElement st1 = new Deflate2TreeElement();
                            st1.Parent = current;
                            current.SubTree1 = st1;
                            current = st1;
                        }
                        strptr++;
                        depth++;
                        break;
                    case ')':
                        current = current.Parent;
                        strptr++;
                        depth--;
                        break;
                    default:
                        strptr++;
                        break;
                }
            }
            root = current;
        }
        public Dictionary<string, string> GetReplacements()
        {
            return root.GetReplacements();
        }
    }
    class Deflate2TreeElement
    {
        public bool IsLeaf { get; set; }
        public string Value { get; set; }
        public Deflate2TreeElement SubTree0 { get; set; }
        public Deflate2TreeElement SubTree1 { get; set; }
        public Deflate2TreeElement Parent { get; set; }
        public Dictionary<string, string> GetReplacements(string path = "")
        {
            if(IsLeaf)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add(Value, path);
                return dict;
            }
            return SubTree0.GetReplacements(path + '0').Concat(SubTree1.GetReplacements(path + '1')).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}