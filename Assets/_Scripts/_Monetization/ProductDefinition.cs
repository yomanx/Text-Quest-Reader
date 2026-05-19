using System;
using System.Collections.Generic;

namespace TextQuestReader.Monetization
{
    public enum ProductKind
    {
        QuestPack,
        SingleQuest,
        CosmeticTheme,
        HintBundle,
        CheckpointToken,
        SupporterPass,
        SubscriptionMonth
    }

    [Serializable]
    public class ProductDefinition
    {
        public string id;
        public string displayName;
        public string description;
        public string price;
        public ProductKind kind;
        public List<string> grantedQuestNames;
        public List<string> grantedThemeIds;
        public bool isMockOnly;
        public string previewImage;

        public ProductDefinition()
        {
            grantedQuestNames = new List<string>();
            grantedThemeIds = new List<string>();
            price = "$0.99";
            kind = ProductKind.QuestPack;
        }
    }

    [Serializable]
    public class MonetizationConfig
    {
        public List<ProductDefinition> products = new List<ProductDefinition>();
        public List<string> premiumQuestNames = new List<string>();
        public List<string> featuredQuestNames = new List<string>();
    }
}
