using System;
using System.Collections.Generic;
using TextQuestReader.Cinematic;
using TMPro;
using UnityEngine;
using QuestFormula;

public class ParameterService
{
    private readonly GamePanel gamePanel;
    private readonly TextParser textParser;
    private readonly RectTransform paramsContent;
    private readonly GameObject parameterTextPref;
    private readonly QuestionCell victoryCell;
    private readonly QuestionCell defeatCell;
    private readonly QuestionCell nextCell;
    private readonly Transform questionsContent;

    private readonly Dictionary<int, int> lastValueByIndex = new Dictionary<int, int>();

    public ParameterService(GamePanel gamePanel, TextParser textParser, RectTransform paramsContent, GameObject parameterTextPref,
                            QuestionCell victoryCell, QuestionCell defeatCell, QuestionCell nextCell, Transform questionsContent)
    {
        this.gamePanel = gamePanel;
        this.textParser = textParser;
        this.paramsContent = paramsContent;
        this.parameterTextPref = parameterTextPref;
        this.victoryCell = victoryCell;
        this.defeatCell = defeatCell;
        this.nextCell = nextCell;
        this.questionsContent = questionsContent;
    }

    public void ApplyInfluences(Unit unit, Action<string> showMainText)
    {
        for (int j = 0; j < unit.influences.Count; j++)
        {
            Parameter parameter = gamePanel.Player.quest.parameters[j];

            if (!parameter.isActive)
                continue;

            Influence influence = unit.influences[j];

            switch (influence.influenceType)
            {
                case InfluenceType.Units:
                    parameter.value = Clamp(parameter.value + influence.value, parameter);
                    break;

                case InfluenceType.Percent:
                    parameter.value = Clamp(
                        (int)(parameter.value * (influence.value / 100f + 1f)),
                        parameter);
                    break;

                case InfluenceType.Value:
                    parameter.value = Clamp(influence.value, parameter);
                    break;

                case InfluenceType.Formula:
                    ApplyFormulaInfluence(influence.formula, parameter);
                    break;
            }

            if (parameter.paramType != ParamType.Usual &&
                ((parameter.isCriticMax && parameter.value >= parameter.maxValue) ||
                 (!parameter.isCriticMax && parameter.value <= parameter.minValue)))
            {
                gamePanel.Player.gameOver = true;

                showMainText(textParser.Parse(parameter.criticText));
                ClearQuestions();

                switch (parameter.paramType)
                {
                    case ParamType.Successful:
                        victoryCell.gameObject.SetActive(true);
                        break;

                    case ParamType.Failed:
                        defeatCell.gameObject.SetActive(true);
                        break;
                }

                nextCell.gameObject.SetActive(false);

                if (!string.IsNullOrEmpty(parameter.critResources))
                {
                    string imageName = textParser.ExtractLastTagValue(ref parameter.critResources, "im");
                    string soundName = textParser.ExtractLastTagValue(ref parameter.critResources, "so");

                    if (!string.IsNullOrEmpty(imageName))
                        gamePanel.PictureNode.SetNewPicture(imageName, gamePanel.Player.quest.questName, mayBeSame: false);                  

                    AudioManager.Instance.PlaySfx(soundName, gamePanel.Player.quest.questName);
                }
            }
        }
    }

    public void Demonstrate(Unit unit)
    {
        ApplyVisibilityActions(unit);
        ClearParameters();

        List<Parameter> visibleParameters = GetVisibleParameters();

        int index = 0;

        foreach (Parameter parameter in visibleParameters)
        {
            ParamsRange range = parameter.FindCorrectRange();
            if (range == null)
            {
                index++;
                continue;
            }

            string output = range.output;
            output = output.Replace("<>", parameter.value.ToString());
            output = textParser.Parse(output, ignoreColor: true);

            for (int i = 0; i < gamePanel.Player.quest.parameters.Count; i++)
            {
                Parameter p = gamePanel.Player.quest.parameters[i];
                string key = $"p{i + 1}";
                output = output.Replace(key, p.value.ToString());
            }
       
            GameObject cell = UnityEngine.Object.Instantiate(parameterTextPref, paramsContent);
            cell.GetComponent<TMP_Text>().text = output;

            ParameterAnimator animator = cell.GetComponent<ParameterAnimator>();
            if (animator == null) animator = cell.AddComponent<ParameterAnimator>();

            bool wasKnown = lastValueByIndex.TryGetValue(parameter.index, out int prevValue);
            int delta = wasKnown ? parameter.value - prevValue : 0;
            bool nearCritical = IsNearCritical(parameter);

            animator.SetCriticalState(nearCritical);

            if (!wasKnown)
                animator.PlayAppear();
            else if (delta != 0 || nearCritical)
                animator.PlayChange(delta, nearCritical);

            lastValueByIndex[parameter.index] = parameter.value;

            index++;
        }
    }

    public void ClearParams()
    {
        foreach (Transform tr in paramsContent)
            GameObject.Destroy(tr.gameObject);

        lastValueByIndex.Clear();
    }

    private static bool IsNearCritical(Parameter parameter)
    {
        if (parameter.paramType == ParamType.Usual) return false;

        int range = Mathf.Max(1, parameter.maxValue - parameter.minValue);
        int threshold = Mathf.Max(1, range / 5);

        if (parameter.isCriticMax)
            return parameter.value >= parameter.maxValue - threshold;
        return parameter.value <= parameter.minValue + threshold;
    }

    private void ApplyFormulaInfluence(string formula, Parameter parameter)
    {
        if (string.IsNullOrEmpty(formula))
        {
            Debug.LogWarning($"Empty influence formula! Parameter: p{parameter.index}");
            return;
        }

        try
        {
            int value = FormulaEvaluator.Evaluate(formula, textParser.FillFormulaDict());
            parameter.value = Clamp(value, parameter);
        }
        catch
        {
            Debug.LogWarning($"Invalid influence formula! Parameter: p{parameter.index}");
        }
    }

    private void ApplyVisibilityActions(Unit unit)
    {
        for (int j = 0; j < gamePanel.Player.quest.parameters.Count; j++)
        {
            Parameter parameter = gamePanel.Player.quest.parameters[j];
            ParamsAction action = unit.paramsActions[j];

            switch (action)
            {
                case ParamsAction.Hide:
                    parameter.isHidden = true;
                    break;

                case ParamsAction.Show:
                    parameter.isHidden = false;
                    break;
            }
        }
    }

    private List<Parameter> GetVisibleParameters()
    {
        List<Parameter> visibleParameters = new List<Parameter>();

        foreach (Parameter parameter in gamePanel.Player.quest.parameters)
        {
            if (!parameter.isActive || parameter.isHidden)
                continue;

            ParamsRange range = parameter.FindCorrectRange();
            if (range == null)
                continue;

            if (!string.IsNullOrEmpty(range.output))
                visibleParameters.Add(parameter);
        }

        return visibleParameters;
    }

    private void ClearParameters()
    {
        foreach (Transform tr in paramsContent)
            UnityEngine.Object.Destroy(tr.gameObject);
    }

    private void ClearQuestions()
    {
        foreach (Transform tr in questionsContent)
            UnityEngine.Object.Destroy(tr.gameObject);
    }

    private static int Clamp(int value, Parameter parameter)
    {
        return Mathf.Clamp(value, parameter.minValue, parameter.maxValue);
    }
}