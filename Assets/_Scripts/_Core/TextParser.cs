using System.Collections.Generic;
using QuestFormula;
using UnityEngine;

public class TextParser
{
    private readonly GamePanel gamePanel;
    private const string replacementColor = "<color=#D6A85E>";

    public TextParser(GamePanel gamePanel)
    {
        this.gamePanel = gamePanel;
    }

    public string Parse(string text, bool ignoreColor = false)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string result = text;

        List<string> expressions = GetBetween(result, "{", "}");

        foreach (string expression in expressions)
        {
            try
            {
                int value = FormulaEvaluator.Evaluate(expression.Replace("{", "").Replace("}", ""), FillFormulaDict());
                string replacement = ignoreColor ? value.ToString() : replacementColor + value + "</color>";

                result = result.Replace(expression, replacement);
            }
            catch
            {
                Debug.LogWarning("Invalid substitution formula!");
            }
        }

        return result;
    }

    public string CleanLeadingSpaces(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        int spaces = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
                spaces++;
            else
                break;
        }

        return spaces > 0 ? text.Remove(0, spaces) : text;
    }

    public List<string> GetBetween(string source, string start, string end)
    {
        List<string> result = new List<string>();

        while (source.Contains(start) && source.Contains(end))
        {
            int startIndex = source.IndexOf(start, System.StringComparison.Ordinal);
            int endIndex = source.IndexOf(end, startIndex, System.StringComparison.Ordinal) + end.Length;

            string content = source.Substring(startIndex, endIndex - startIndex);
            result.Add(content);

            source = source.Replace(content, "");
        }

        return result;
    }

    public Dictionary<string, int> FillFormulaDict()
    {
        Dictionary<string, int> paramsDict = new Dictionary<string, int>(gamePanel.Player.quest.parameters.Count);

        foreach (Parameter parameter in gamePanel.Player.quest.parameters)
            paramsDict["p" + parameter.index] = parameter.value;

        return paramsDict;
    }

    public string ExtractLastTagValue(ref string text, string tag)
    {
        List<string> tags = GetBetween(text, "<" + tag, tag + ">");
        string value = "";

        foreach (string fullTag in tags)
        {
            text = text.Replace(fullTag, "");
            value = fullTag.Replace("<" + tag, "").Replace(tag + ">", "").Replace(" ", "");
        }

        return value;
    }

    public List<string> ExtractAllTagValues(ref string text, string tag)
    {
        List<string> tags = GetBetween(text, "<" + tag, tag + ">");
        List<string> values = new List<string>();

        foreach (string fullTag in tags)
        {
            text = text.Replace(fullTag, "");
            string raw = fullTag.Replace("<" + tag, "").Replace(tag + ">", "").Trim();

            if (!string.IsNullOrEmpty(raw))
                values.Add(raw);
        }

        return values;
    }
}
