using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace ApplicationLib.Utilities
{
    public static class Text
    {

        /// <summary>
        /// http://stackoverflow.com/questions/14906232/regular-expressions-with-the-cyrillic-alphabet
        /// </summary>
        public static String GenerateSlug(String Val, Boolean ToLower, Boolean RemoveSpaces)
        {
            String str = RemoveAccent(Val);
            str.Replace(".", ""); // Remove .
            if (ToLower) str = str.ToLower();
            string pattern = "[^a-zA-ZА-Яа-я0-9{0}]";
            string replacement = " ";
            Regex regEx = new Regex(pattern);
            str = Regex.Replace(regEx.Replace(str, replacement), @"\s+", " ");                         
            if (RemoveSpaces)
                str = str.Replace(" ", ""); // remove spaces
            else
                str = Regex.Replace(str, @"\s", "-"); // replace spaces by hyphens
            return str;
        }

        public static String RemoveSymbolsAndSpaces(String Val, Boolean ToLower)
        {
            String str = RemoveAccent(Val);
            str.Replace(".", ""); // Remove .
            if (ToLower) str = str.ToLower();
            string pattern = "[\\~#%&*{}+/:<>@.?|\"-]";
            string replacement = "";
            Regex regEx = new Regex(pattern);
            str = Regex.Replace(regEx.Replace(str, replacement), @"\s+", "");
            str = str.Replace(" ", ""); // remove spaces
            str = Regex.Replace(str, @"\s", ""); // replace spaces by hyphens
            return str;
        }

        public static String RemoveAccent(String text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static String RemoveHtml(String htmlString)
        {
            const string pattern = @"<(.|\n)*?>";
            string sOut = Regex.Replace(htmlString, pattern, String.Empty);
            sOut = sOut.Replace("&nbsp;", String.Empty);
            sOut = sOut.Replace("&amp;", "&");
            sOut = sOut.Replace("&gt;", ">");
            sOut = sOut.Replace("&lt;", "<");
            return sOut;
        }

        public static String CleanString(String s)
        {
            if (String.IsNullOrEmpty(s)) return s;
            String[] chars = { "\n", "\r", "\t" };
            foreach (String c in chars)
            {
                s = s.Replace(c, "");
            }
            return s;
        }

        public static String SubWords(String Body, Int32 Length)
        {
            return SubWords(Body, Length, true);
        }

        public static String SubWords(String Body, Int32 Length, Boolean AddDots)
        {
            Int32 newLastIndex;
            Body = Utilities.Text.RemoveHtml(Body);
            if (Body.Length > Length)
            {
                Body = Body.Substring(0, Length);
                newLastIndex = Body.LastIndexOf(' ');
                Body = Body.Substring(0, newLastIndex > 0 ? newLastIndex : Length);
                if (AddDots)
                    Body += " (...)";
            }
            return Body;
        }

        public static String GetFormatedDate(DateTime dt, String ToAppend, Boolean Extended)
        {
            if (dt == null)
                return null;

            String Date = (Extended) ? dt.Day + " de " + GetMonth(dt.Month) + " de " + dt.Year : dt.ToShortDateString();
            return (String.IsNullOrEmpty(ToAppend)) ? Date : ToAppend + ", " + Date;
        }

        public static String GetFormatedPayPalPrice(Object value)
        {
            return (String.IsNullOrEmpty(value.ToString())) ? "0.00" : String.Format("{0:f}", value).Replace(",", ".");
        }

        public static String GetFormatedAmount(Double Amount)
        {
            return String.Format(new CultureInfo("pt"), "{0:N}", Amount);
        }

        public static String GetFormatedAmount(Decimal Amount)
        {
            return String.Format(new CultureInfo("pt"), "{0:N}", Amount);
        }

        public static String GetAmount(Double Amount)
        {
            return GetAmount(Amount,4);
        }

        public static String GetAmount(Decimal Amount)
        {
            return GetAmount(Amount, 4);
        }

        public static String GetAmount(Double Amount, int decimals)
        {
            var dec = "";
            for (int i = 0; i < decimals; i++)
                dec += "0";
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0." + dec + "}", Amount);
        }

        public static String GetAmount(Decimal Amount, int decimals)
        {
            var dec = "";
            for (int i = 0; i < decimals; i++)
                dec += "0";
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0." + dec + "}", Amount);
        }

        public static List<Int32> GetIntegerListFromRequest(string data)
        {
            var result = new List<Int32>();
            if (string.IsNullOrEmpty(data))
                return result;
            int num = 0;
            foreach (var number in data.Split(','))
            {
                if (Int32.TryParse(number,out num))
                    result.Add(num);
            }
            return result;
        }

        public static String GetExternalUrl(String Link)
        {
            if (String.IsNullOrEmpty(Link))
                return "#";
            var copy = Link.ToLower();
            return copy.StartsWith("http://") || copy.StartsWith("https://") ? Link : "http://" + Link;
        }

        public static String GetMonth(int Month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
        }

        public static String Singularize(String IdLanguage, String input)
        {
            if (IdLanguage == "pt")
            {
                //http://www.flip.pt/FLiPOnline/Gram%C3%A1tica/MorfologiaPartesdodiscurso/N%C3%BAmero/Pluraldosadjectivosesubstantivos/tabid/639/Default.aspx
                Int32 width;
                var words = input.Split(' ');
                var output = new StringBuilder();
                foreach (var word in words)
                {
                    if (output.Length > 0)
                        output.Append(" ");

                    width = word.Length;

                    //Os substantivos e adjectivos cuja terminação no singular é a letra consoante formam o plural com o acrescentamento de –es: 
                    //Ex.: Ex.: mar - mares; particular -  particulares; professor - professores; merecedor - merecedores; talher -  talheres;
                    //Os substantivos cuja terminação no singular seja an, en e on formam o plural com acrescentamento de –es: 
                    //Ex.: íman - ímanes; espécimen -  especímenes; hífen - hífenes; líquen  - líquenes; abdómen - abdómenes; 
                    if (width > 5 && word[width - 2] == 'e' && word[width - 1] == 's')
                    {
                        output.Append(word.Substring(0, width - 2));
                    }
                    else
                        //Alguns adjectivos e substantivos, que no singular terminam em ão, formam o plural com o acrescentamento de um s:
                        //Ex.: alão - alãos; grão -  grãos; sótão - sótãos; órfão -  órfãos; irmão - irmãos;
                        if (width > 5 && word[width - 3] == 'a' && word[width - 2] == 'o' && word[width - 1] == 's')
                        {
                            output.Append(word.Substring(0, width - 2));
                        }
                        else
                            //Os substantivos e adjectivos que no singular terminam em a, e, i, o ou u formam o plural com o acrescentamento de um s: 
                            //Ex.: casa - casas; caiada -  caiadas; pá - pás; ameixa sã -  ameixas sãs; elefante grande - elefantes grandes;
                            if (width > 5 && word[width - 1] == 's' && (word[width - 2] == 'a' || word[width - 2] == 'e' || word[width - 2] == 'i' || word[width - 2] == 'o' || word[width - 2] == 'u'))
                            {
                                output.Append(word.Substring(0, width - 1));
                            }
                            else
                                //Os substantivos e adjectivos cuja terminação no singular seja em, im, om, um formam o plural com o acrescentamento de um s, mas na escrita o m muda-se em n: 
                                //Ex.: homem - homens; armazém -  armazéns; origem - origens; botequim  - botequins; jardim - jardins; ruim - ruins;
                                if (width > 5 && word[width - 2] == 'n' && word[width - 1] == 's')
                                {
                                    output.Append(word.Substring(0, width - 2));
                                }
                                else
                                    if (width > 1) output.Append(word);
                }
                return output.ToString();
            }
            return input;
        }

        public static String RemoveNumbers(String input)
        {
            //var output = Regex.Replace(input, @"[\d-]", string.Empty);
            var output = new String(input.Where(c => c != '-' && (c < '0' || c > '9')).ToArray()); //performance test shows that this is about five times faster than using a regular expression
            return output;
        }
    }
}