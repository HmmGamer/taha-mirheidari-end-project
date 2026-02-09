using System.Collections;
using System.Collections.Generic;

public static class FontTools
{
    private struct Glyph
    {
        public char Iso, Fin, Ini, Med;
        public bool JoinsAfter;
        public bool JoinsBefore;
    }

    private static readonly Dictionary<char, Glyph> _glyphs = new Dictionary<char, Glyph>()
    {
        ['ا'] = new Glyph { Iso = 'ﺍ', Fin = 'ﺎ', Ini = 'ﺍ', Med = 'ﺎ', JoinsAfter = false, JoinsBefore = true },
        ['آ'] = new Glyph { Iso = 'ﺁ', Fin = 'ﺂ', Ini = 'ﺁ', Med = 'ﺂ', JoinsAfter = false, JoinsBefore = true },
        ['ب'] = new Glyph { Iso = 'ﺏ', Fin = 'ﺐ', Ini = 'ﺑ', Med = 'ﺒ', JoinsAfter = true, JoinsBefore = true },
        ['پ'] = new Glyph { Iso = 'ﭖ', Fin = 'ﭗ', Ini = 'ﭘ', Med = 'ﭙ', JoinsAfter = true, JoinsBefore = true },
        ['ت'] = new Glyph { Iso = 'ﺕ', Fin = 'ﺖ', Ini = 'ﺗ', Med = 'ﺘ', JoinsAfter = true, JoinsBefore = true },
        ['ث'] = new Glyph { Iso = 'ﺙ', Fin = 'ﺚ', Ini = 'ﺛ', Med = 'ﺜ', JoinsAfter = true, JoinsBefore = true },
        ['ج'] = new Glyph { Iso = 'ﺝ', Fin = 'ﺞ', Ini = 'ﺟ', Med = 'ﺠ', JoinsAfter = true, JoinsBefore = true },
        ['چ'] = new Glyph { Iso = 'ﭺ', Fin = 'ﭻ', Ini = 'ﭼ', Med = 'ﭽ', JoinsAfter = true, JoinsBefore = true },
        ['ح'] = new Glyph { Iso = 'ﺡ', Fin = 'ﺢ', Ini = 'ﺣ', Med = 'ﺤ', JoinsAfter = true, JoinsBefore = true },
        ['خ'] = new Glyph { Iso = 'ﺥ', Fin = 'ﺦ', Ini = 'ﺧ', Med = 'ﺨ', JoinsAfter = true, JoinsBefore = true },
        ['د'] = new Glyph { Iso = 'ﺩ', Fin = 'ﺪ', Ini = 'ﺩ', Med = 'ﺪ', JoinsAfter = false, JoinsBefore = true },
        ['ذ'] = new Glyph { Iso = 'ﺫ', Fin = 'ﺬ', Ini = 'ﺫ', Med = 'ﺬ', JoinsAfter = false, JoinsBefore = true },
        ['ر'] = new Glyph { Iso = 'ﺭ', Fin = 'ﺮ', Ini = 'ﺭ', Med = 'ﺮ', JoinsAfter = false, JoinsBefore = true },
        ['ز'] = new Glyph { Iso = 'ﺯ', Fin = 'ﺰ', Ini = 'ﺯ', Med = 'ﺰ', JoinsAfter = false, JoinsBefore = true },
        ['ژ'] = new Glyph { Iso = 'ﮊ', Fin = 'ﮋ', Ini = 'ﮊ', Med = 'ﮋ', JoinsAfter = false, JoinsBefore = true },
        ['س'] = new Glyph { Iso = 'ﺱ', Fin = 'ﺲ', Ini = 'ﺳ', Med = 'ﺴ', JoinsAfter = true, JoinsBefore = true },
        ['ش'] = new Glyph { Iso = 'ﺵ', Fin = 'ﺶ', Ini = 'ﺷ', Med = 'ﺸ', JoinsAfter = true, JoinsBefore = true },
        ['ص'] = new Glyph { Iso = 'ﺹ', Fin = 'ﺺ', Ini = 'ﺻ', Med = 'ﺼ', JoinsAfter = true, JoinsBefore = true },
        ['ض'] = new Glyph { Iso = 'ﺽ', Fin = 'ﺾ', Ini = 'ﺿ', Med = 'ﻀ', JoinsAfter = true, JoinsBefore = true },
        ['ط'] = new Glyph { Iso = 'ﻁ', Fin = 'ﻂ', Ini = 'ﻃ', Med = 'ﻄ', JoinsAfter = true, JoinsBefore = true },
        ['ظ'] = new Glyph { Iso = 'ﻅ', Fin = 'ﻆ', Ini = 'ﻇ', Med = 'ﻈ', JoinsAfter = true, JoinsBefore = true },
        ['ع'] = new Glyph { Iso = 'ﻉ', Fin = 'ﻊ', Ini = 'ﻋ', Med = 'ﻌ', JoinsAfter = true, JoinsBefore = true },
        ['غ'] = new Glyph { Iso = 'ﻍ', Fin = 'ﻎ', Ini = 'ﻏ', Med = 'ﻐ', JoinsAfter = true, JoinsBefore = true },
        ['ف'] = new Glyph { Iso = 'ﻑ', Fin = 'ﻒ', Ini = 'ﻓ', Med = 'ﻔ', JoinsAfter = true, JoinsBefore = true },
        ['ق'] = new Glyph { Iso = 'ﻕ', Fin = 'ﻖ', Ini = 'ﻗ', Med = 'ﻘ', JoinsAfter = true, JoinsBefore = true },
        ['ک'] = new Glyph { Iso = 'ﮎ', Fin = 'ﮏ', Ini = 'ﮐ', Med = 'ﮑ', JoinsAfter = true, JoinsBefore = true },
        ['گ'] = new Glyph { Iso = 'ﮒ', Fin = 'ﮓ', Ini = 'ﮔ', Med = 'ﮕ', JoinsAfter = true, JoinsBefore = true },
        ['ل'] = new Glyph { Iso = 'ﻝ', Fin = 'ﻞ', Ini = 'ﻟ', Med = 'ﻠ', JoinsAfter = true, JoinsBefore = true },
        ['م'] = new Glyph { Iso = 'ﻡ', Fin = 'ﻢ', Ini = 'ﻣ', Med = 'ﻤ', JoinsAfter = true, JoinsBefore = true },
        ['ن'] = new Glyph { Iso = 'ﻥ', Fin = 'ﻦ', Ini = 'ﻧ', Med = 'ﻨ', JoinsAfter = true, JoinsBefore = true },
        ['و'] = new Glyph { Iso = 'ﻭ', Fin = 'ﻮ', Ini = 'ﻭ', Med = 'ﻮ', JoinsAfter = false, JoinsBefore = true },
        ['ه'] = new Glyph { Iso = 'ﻩ', Fin = 'ﻪ', Ini = 'ﻫ', Med = 'ﻬ', JoinsAfter = true, JoinsBefore = true },
        ['ی'] = new Glyph { Iso = 'ﯼ', Fin = 'ﯽ', Ini = 'ﯾ', Med = 'ﯿ', JoinsAfter = true, JoinsBefore = true },
        [' '] = new Glyph { Iso = ' ', Fin = ' ', Ini = ' ', Med = ' ', JoinsAfter = false, JoinsBefore = false },
        ['أ'] = new Glyph { Iso = 'ﺃ', Fin = 'ﺄ', Ini = 'ﺃ', Med = 'ﺄ', JoinsAfter = false, JoinsBefore = true },
        ['ئ'] = new Glyph { Iso = 'ﺋ', Fin = 'ﺌ', Ini = 'ﺋ', Med = 'ﺌ', JoinsAfter = true, JoinsBefore = true },
        ['ؤ'] = new Glyph { Iso = 'ﺅ', Fin = 'ﺆ', Ini = 'ﺅ', Med = 'ﺆ', JoinsAfter = false, JoinsBefore = true },
        ['ي'] = new Glyph { Iso = 'ﻱ', Fin = 'ﻲ', Ini = 'ﻳ', Med = 'ﻴ', JoinsAfter = true, JoinsBefore = true },
        ['ك'] = new Glyph { Iso = 'ﻙ', Fin = 'ﻚ', Ini = 'ﻛ', Med = 'ﻜ', JoinsAfter = true, JoinsBefore = true },
    };

    public static string _ConvertToPersian(string iText)
    {
        if (string.IsNullOrEmpty(iText))
            return string.Empty;

        var output = new List<char>(iText.Length);

        for (int i = 0; i < iText.Length; i++)
        {
            char c = iText[i];

            if (!_glyphs.TryGetValue(c, out var g))
            {
                output.Add(c);
                continue;
            }

            bool joinPrev =
                i > 0 &&
                _glyphs.TryGetValue(iText[i - 1], out var p) &&
                p.JoinsAfter &&
                g.JoinsBefore;

            bool joinNext =
                i < iText.Length - 1 &&
                _glyphs.TryGetValue(iText[i + 1], out var n) &&
                g.JoinsAfter &&
                n.JoinsBefore;

            if (joinPrev && joinNext) output.Add(g.Med);
            else if (joinPrev) output.Add(g.Fin);
            else if (joinNext) output.Add(g.Ini);
            else output.Add(g.Iso);
        }

        output.Reverse();
        return new string(output.ToArray());
    }
}