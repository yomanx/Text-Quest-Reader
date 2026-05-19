using System.Collections.Generic;
using TextQuestReader.Settings;
using UnityEngine;

namespace TextQuestReader.Monetization
{
    /// <summary>
    /// Stores per-user product ownership in PlayerPrefs. Designed as the single
    /// source of truth for "does the player own X right now". Cheap to query
    /// from UI; persists across sessions; safe to mock.
    /// </summary>
    public class EntitlementService
    {
        private readonly HashSet<string> ownedProducts = new HashSet<string>();
        private MonetizationConfig config;

        public void Initialize(MonetizationConfig config)
        {
            this.config = config;
            ownedProducts.Clear();

            if (config == null) return;

            foreach (ProductDefinition product in config.products)
            {
                string key = SettingsKeys.OwnedProductsPrefix + product.id;
                if (PlayerPrefs.GetInt(key, 0) == 1)
                    ownedProducts.Add(product.id);
            }
        }

        public bool OwnsProduct(string productId)
        {
            return !string.IsNullOrEmpty(productId) && ownedProducts.Contains(productId);
        }

        public bool OwnsQuest(string questName)
        {
            if (config == null || string.IsNullOrEmpty(questName)) return false;

            foreach (ProductDefinition product in config.products)
            {
                if (!ownedProducts.Contains(product.id)) continue;
                if (product.grantedQuestNames == null) continue;

                foreach (string grantedQuest in product.grantedQuestNames)
                {
                    if (string.Equals(grantedQuest, questName, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public bool IsPremiumQuest(string questName)
        {
            if (config == null || string.IsNullOrEmpty(questName)) return false;

            if (config.premiumQuestNames == null) return false;

            foreach (string premium in config.premiumQuestNames)
            {
                if (string.Equals(premium, questName, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public bool IsFeaturedQuest(string questName)
        {
            if (config == null || string.IsNullOrEmpty(questName)) return false;
            if (config.featuredQuestNames == null) return false;

            foreach (string featured in config.featuredQuestNames)
            {
                if (string.Equals(featured, questName, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public QuestAccessState GetAccessState(string questName)
        {
            if (!IsPremiumQuest(questName))
                return QuestAccessState.FreeOpen;
            if (OwnsQuest(questName))
                return QuestAccessState.OwnedPremium;
            return QuestAccessState.LockedPremium;
        }

        public void GrantProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return;
            ownedProducts.Add(productId);
            PlayerPrefs.SetInt(SettingsKeys.OwnedProductsPrefix + productId, 1);
            PlayerPrefs.Save();
        }

        public void RevokeProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return;
            ownedProducts.Remove(productId);
            PlayerPrefs.DeleteKey(SettingsKeys.OwnedProductsPrefix + productId);
            PlayerPrefs.Save();
        }

        public void ResetAllForDebug()
        {
            if (config == null) return;
            foreach (ProductDefinition product in config.products)
                RevokeProduct(product.id);
        }

        public IEnumerable<string> OwnedProductIds => ownedProducts;
    }
}
