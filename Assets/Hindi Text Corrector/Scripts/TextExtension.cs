using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;


public static class TextExtension // created extension for Text and TextMeshPro
{
    public static void SetHindiText(this Text text, string value , bool useColorTag = false)
    {
        Font krutiDev = Resources.Load("Fonts/rwizenHindi") as Font;
        text.font = krutiDev;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(value);
        text.text = HindiCorrector.GetCorrectedHindiText(sb,useColorTag).ToString();
     
    }

    public static void SetHindiTMPro(this TextMeshProUGUI text, string value ,bool useColorTag = false)
    {
        Regex reg = new Regex("<font=\"(.*?)\">(.*?)</font>", RegexOptions.Singleline);
        List<string> exceptionals = new List<string>();
        int frequency = -1;
        Match match = reg.Match(value);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(value);


        sb.Replace("द्र", "Dra");
        sb.Replace("क्षि", "Shi");
        sb.Replace("द्व", "Dva") ;
        sb.Replace("द्", "Dva");
        sb.Replace("क्ष्", "Sha");
        sb.Replace("क्ष", "Sha");
        



        while (match.ToString() != "")
        {
            frequency++;
            exceptionals.Add(match.ToString());
            value = HindiCorrector.ReplaceFirstOccurrence(sb, match.ToString(), "क्ष्" + frequency + "द्व").ToString();
            match = reg.Match(value);
        }
 

        TMP_FontAsset krutiDev = Resources.Load("Fonts/rwizenHindiSDF") as TMP_FontAsset;
        text.font = krutiDev;
        value = HindiCorrector.GetCorrectedHindiText(sb).ToString();

        //  Debug.Log(value);

        value = value.Replace("Shi", "f{{");
        value = value.Replace("Dra", "æ");
        value = value.Replace("Dva", "}}");
        value = value.Replace("Sha", "{{");
       

        //  Debug.Log(value);

        value = string.Format(value, exceptionals.ToArray());
    
        text.text = value;

       

    }
}
