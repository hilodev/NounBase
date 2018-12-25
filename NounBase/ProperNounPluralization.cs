using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NounBase
{
    public static class TokenServiceExtensions
    {
        public static bool IsSingular(this string noun)
        {
            return noun != Instance.ToPlural(noun);
        }
        public static string ToPluralNoun(this string noun)
        {
            return Instance.ToPlural(noun);
        }
        public static string ToSingularNoun(this string noun)
        {
            return Instance.ToSingular(noun);
        }
        private static Lazy<ProperNounPluralization> _instance = new Lazy<ProperNounPluralization>(() => new ProperNounPluralization());
        public static ProperNounPluralization Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
    public class ProperNounPluralization
    {
        private readonly Dictionary<Regex, string> _pluralRules = PluralRules.GetRules();
        private readonly Dictionary<Regex, string> _singularRules = SingularRules.GetRules();
        private readonly List<string> _uncountables = Uncountables.GetUncountables();
        private readonly Dictionary<string, string> _irregularPlurals = IrregularRules.GetIrregularPlurals();
        private readonly Dictionary<string, string> _irregularSingles = IrregularRules.GetIrregularSingulars();
        private readonly Regex replacementRegex = new Regex("\\$(\\d{1,2})");
        public string ToPlural(string token)
        {
            return Transform(token, _irregularSingles, _irregularPlurals, _pluralRules);
        }
        public string ToSingular(string token)
        {
            return Transform(token, _irregularPlurals, _irregularSingles, _singularRules);
        }
        internal string UnCase(string token, string tokenTransformed)
        {
            if (token == tokenTransformed) return tokenTransformed;
            if (token == token.ToUpper()) return tokenTransformed.ToUpper();
            if (token[0] == char.ToUpper(token[0]))
                return char.ToUpper(tokenTransformed[0]) + tokenTransformed.Substring(1);
            return tokenTransformed.ToLower();
        }
        internal string ApplyRules(string token, string originalToken, Dictionary<Regex, string> rules)
        {
            if (string.IsNullOrEmpty(token) || _uncountables.Contains(token))
                return (UnCase(originalToken, token) + " group").Trim();
            var length = rules.Count;
            while (length-- > 0)
            {
                var rule = rules.ElementAt(length);
                if (rule.Key.IsMatch(originalToken))
                {
                    var match = rule.Key.Match(originalToken);
                    var matchString = match.Groups[0].Value;
                    if (string.IsNullOrWhiteSpace(matchString))
                        return rule.Key.Replace(originalToken, GetReplaceMethod(originalToken[match.Index - 1].ToString(), rule.Value), 1);
                    return rule.Key.Replace(originalToken, GetReplaceMethod(matchString, rule.Value), 1);
                }
            }
            return originalToken;
        }
        private MatchEvaluator GetReplaceMethod(string originalWord, string replacement)
        {
            return match =>
            {
                return UnCase(originalWord, replacementRegex.Replace(replacement, m => match.Groups[Convert.ToInt32(m.Groups[1].Value)].Value));
            };
        }
        internal string Transform(string word, Dictionary<string, string> replacables, Dictionary<string, string> keepables, Dictionary<Regex, string> rules)
        {
            var token = word.ToLower();
            if (keepables.ContainsKey(token)) return UnCase(word, token);
            if (replacables.ContainsKey(token)) return UnCase(word, replacables[token]);
            return ApplyRules(token, word, rules);
        }
    }
    internal static class IrregularRules
    {
        private static Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                // Pronouns.
                {"I", "we"},
                {"me", "us"},
                {"he", "they"},
                {"she", "they"},
                {"them", "them"},
                {"myself", "ourselves"},
                {"yourself", "yourselves"},
                {"itself", "themselves"},
                {"herself", "themselves"},
                {"himself", "themselves"},
                {"themself", "themselves"},
                {"is", "are"},
                {"was", "were"},
                {"has", "have"},
                {"this", "these"},
                {"that", "those"},
                // Words ending in with a consonant and `o`.
                {"echo", "echoes"},
                {"dingo", "dingoes"},
                {"volcano", "volcanoes"},
                {"tornado", "tornadoes"},
                {"torpedo", "torpedoes"},
                // Ends with `us`.
                {"genus", "genera"},
                {"viscus", "viscera"},
                // Ends with `ma`.
                {"stigma", "stigmata"},
                {"stoma", "stomata"},
                {"dogma", "dogmata"},
                {"lemma", "lemmata"},
                {"schema", "schemata"},
                {"anathema", "anathemata"},
                // Other irregular rules.
                {"ox", "oxen"},
                {"axe", "axes"},
                {"die", "dice"},
                {"yes", "yeses"},
                {"foot", "feet"},
                {"eave", "eaves"},
                {"goose", "geese"},
                {"tooth", "teeth"},
                {"quiz", "quizzes"},
                {"human", "humans"},
                {"proof", "proofs"},
                {"carve", "carves"},
                {"valve", "valves"},
                {"looey", "looies"},
                {"thief", "thieves"},
                {"groove", "grooves"},
                {"pickaxe", "pickaxes"},
                {"whiskey", "whiskies"}
            };
        public static Dictionary<string, string> GetIrregularPlurals()
        {
            var result = new Dictionary<string, string>();
            foreach (var item in dictionary.Reverse())
            {
                if (!result.ContainsKey(item.Value)) result.Add(item.Value, item.Key);
            }
            return result;
        }
        public static Dictionary<string, string> GetIrregularSingulars()
        {
            return dictionary;
        }
    }
    internal static class PluralRules
    {
        public static Dictionary<Regex, string> GetRules()
        {
            return new Dictionary<Regex, string>
            {
                {new Regex("s?$",RegexOptions.IgnoreCase), "s"},
                {new Regex("[^\u0000-\u007F]$",RegexOptions.IgnoreCase), "$0"},
                {new Regex("([^aeiou]ese)$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(ax|test)is$",RegexOptions.IgnoreCase), "$1es"},
                {new Regex("(alias|[^aou]us|tlas|gas|ris)$",RegexOptions.IgnoreCase), "$1es"},
                {new Regex("(e[mn]u)s?$",RegexOptions.IgnoreCase), "$1s"},
                {new Regex("([^l]ias|[aeiou]las|[emjzr]as|[iu]am)$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(alumn|syllab|octop|vir|radi|nucle|fung|cact|stimul|termin|bacill|foc|uter|loc|strat)(?:us|i)$",RegexOptions.IgnoreCase), "$1i"},
                {new Regex("(alumn|alg|vertebr)(?:a|ae)$",RegexOptions.IgnoreCase), "$1ae"},
                {new Regex("(seraph|cherub)(?:im)?$",RegexOptions.IgnoreCase), "$1im"},
                {new Regex("(her|at|gr)o$",RegexOptions.IgnoreCase), "$1oes"},
                {new Regex("(agend|addend|millenni|dat|extrem|bacteri|desiderat|strat|candelabr|errat|ov|symposi|curricul|automat|quor)(?:a|um)$",RegexOptions.IgnoreCase), "$1a"},
                {new Regex("(apheli|hyperbat|periheli|asyndet|noumen|phenomen|criteri|organ|prolegomen|hedr|automat)(?:a|on)$",RegexOptions.IgnoreCase), "$1a"},
                {new Regex("sis$",RegexOptions.IgnoreCase), "ses"},
                {new Regex("(?:(kni|wi|li)fe|(ar|l|ea|eo|oa|hoo)f)$",RegexOptions.IgnoreCase), "$1$2ves"},
                {new Regex("([^aeiouy]|qu)y$",RegexOptions.IgnoreCase), "$1ies"},
                {new Regex("([^ch][ieo][ln])ey$",RegexOptions.IgnoreCase), "$1ies"},
                {new Regex("(x|ch|ss|sh|zz)$",RegexOptions.IgnoreCase), "$1es"},
                {new Regex("(matr|cod|mur|sil|vert|ind|append)(?:ix|ex)$",RegexOptions.IgnoreCase), "$1ices"},
                {new Regex("(m|l)(?:ice|ouse)$",RegexOptions.IgnoreCase), "$1ice"},
                {new Regex("(pe)(?:rson|ople)$",RegexOptions.IgnoreCase), "$1ople"},
                {new Regex("(child)(?:ren)?$",RegexOptions.IgnoreCase), "$1ren"},
                {new Regex("eaux$",RegexOptions.IgnoreCase), "$0"},
                {new Regex("m[ae]n$",RegexOptions.IgnoreCase), "men"},
                {new Regex("^thou$",RegexOptions.IgnoreCase), "you" },
                {new Regex("pox$",RegexOptions.IgnoreCase),        "$0"},
                {new Regex("ois$",RegexOptions.IgnoreCase),        "$0"},
                {new Regex("deer$",RegexOptions.IgnoreCase),       "$0"},
                {new Regex("fish$",RegexOptions.IgnoreCase),       "$0"},
                {new Regex("sheep$",RegexOptions.IgnoreCase),      "$0"},
                {new Regex("measles$/",RegexOptions.IgnoreCase),   "$0"},
                {new Regex("[^aeiou]ese$",RegexOptions.IgnoreCase),"$0"}
            };
        }
    }
    internal static class SingularRules
    {
        public static Dictionary<Regex, string> GetRules()
        {
            return new Dictionary<Regex, string>
            {
                {new Regex("s$",RegexOptions.IgnoreCase), ""},
                {new Regex("(ss)$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)(?:sis|ses)$",RegexOptions.IgnoreCase), "$1sis"},
                {new Regex("(^analy)(?:sis|ses)$",RegexOptions.IgnoreCase), "$1sis"},
                {new Regex("(wi|kni|(?:after|half|high|low|mid|non|night|[^\\w]|^)li)ves$",RegexOptions.IgnoreCase), "$1fe"},
                {new Regex("(ar|(?:wo|[ae])l|[eo][ao])ves$",RegexOptions.IgnoreCase), "$1f"},
                {new Regex("ies$",RegexOptions.IgnoreCase), "y"},
                {new Regex("\\b([pl]|zomb|(?:neck|cross)?t|coll|faer|food|gen|goon|group|lass|talk|goal|cut)ies$",RegexOptions.IgnoreCase), "$1ie"},
                {new Regex("\\b(mon|smil)ies$",RegexOptions.IgnoreCase), "$1ey"},
                {new Regex("(m|l)ice$",RegexOptions.IgnoreCase), "$1ouse"},
                {new Regex("(seraph|cherub)im$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(x|ch|ss|sh|zz|tto|go|cho|alias|[^aou]us|tlas|gas|(?:her|at|gr)o|ris)(?:es)?$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(e[mn]u)s?$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(movie|twelve)s$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(cris|test|diagnos)(?:is|es)$",RegexOptions.IgnoreCase), "$1is"},
                {new Regex("(alumn|syllab|octop|vir|radi|nucle|fung|cact|stimul|termin|bacill|foc|uter|loc|strat)(?:us|i)$",RegexOptions.IgnoreCase), "$1us"},
                {new Regex("(agend|addend|millenni|dat|extrem|bacteri|desiderat|strat|candelabr|errat|ov|symposi|curricul|quor)a$",RegexOptions.IgnoreCase), "$1um"},
                {new Regex("(apheli|hyperbat|periheli|asyndet|noumen|phenomen|criteri|organ|prolegomen|hedr|automat)a$",RegexOptions.IgnoreCase), "$1on"},
                {new Regex("(alumn|alg|vertebr)ae$",RegexOptions.IgnoreCase), "$1a"},
                {new Regex("(cod|mur|sil|vert|ind)ices$",RegexOptions.IgnoreCase), "$1ex"},
                {new Regex("(matr|append)ices$",RegexOptions.IgnoreCase), "$1ix"},
                {new Regex("(pe)(rson|ople)$",RegexOptions.IgnoreCase), "$1rson"},
                {new Regex("(child)ren$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("(eau)x?$",RegexOptions.IgnoreCase), "$1"},
                {new Regex("men$",RegexOptions.IgnoreCase), "man" },
                {new Regex("pox$",RegexOptions.IgnoreCase),        "$0"},
                {new Regex("ois$",RegexOptions.IgnoreCase),        "$0"},
                {new Regex("deer$",RegexOptions.IgnoreCase),       "$0"},
                {new Regex("fish$",RegexOptions.IgnoreCase),       "$0"},
                {new Regex("sheep$",RegexOptions.IgnoreCase),      "$0"},
                {new Regex("measles$/",RegexOptions.IgnoreCase),   "$0"},
                {new Regex("[^aeiou]ese$",RegexOptions.IgnoreCase),"$0"}
            };
        }
    }
    internal static class Uncountables
    {
        public static List<string> GetUncountables()
        {
            return new List<string> { 
                    // Singular words with no plurals.
                    "advice",
                    "adulthood",
                    "agenda",
                    "aid",
                    "alcohol",
                    "ammo",
                    "athletics",
                    "bison",
                    "blood",
                    "bream",
                    "buffalo",
                    "butter",
                    "carp",
                    "cash",
                    "chassis",
                    "chess",
                    "clothing",
                    "commerce",
                    "cod",
                    "cooperation",
                    "corps",
                    "digestion",
                    "debris",
                    "diabetes",
                    "energy",
                    "equipment",
                    "elk",
                    "excretion",
                    "expertise",
                    "flounder",
                    "fun",
                    "gallows",
                    "garbage",
                    "graffiti",
                    "headquarters",
                    "health",
                    "herpes",
                    "highjinks",
                    "homework",
                    "housework",
                    "information",
                    "jeans",
                    "justice",
                    "kudos",
                    "labour",
                    "literature",
                    "machinery",
                    "mackerel",
                    "mail",
                    "media",
                    "mews",
                    "moose",
                    "music",
                    "news",
                    "pike",
                    "plankton",
                    "pliers",
                    "pollution",
                    "premises",
                    "rain",
                    "research",
                    "rice",
                    "salmon",
                    "scissors",
                    "series",
                    "sewage",
                    "shambles",
                    "shrimp",
                    "species",
                    "staff",
                    "swine",
                    "trout",
                    "traffic",
                    "transporation",
                    "tuna",
                    "wealth",
                    "welfare",
                    "whiting",
                    "wildebeest",
                    "wildlife",
                    "you"
            };
        }
    }
}