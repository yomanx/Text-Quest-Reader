using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace TextQuestReader.Monetization
{
    /// <summary>
    /// Top-level façade. UI calls this for "is X locked?", "buy Y", "what
    /// products exist?". Holds the active IPurchaseProvider (defaults to mock)
    /// and the EntitlementService. Configuration loaded from StreamingAssets/monetization.json
    /// — if absent, the catalog is empty and every quest is treated as free.
    /// </summary>
    public class MonetizationService : MonoBehaviour
    {
        public static MonetizationService Instance { get; private set; }

        public event Action ProductsChanged;
        public event Action<PurchaseResult> PurchaseCompleted;

        private MonetizationConfig config;
        private EntitlementService entitlement;
        private IPurchaseProvider purchaseProvider;

        public MonetizationConfig Config => config;
        public EntitlementService Entitlement => entitlement;
        public IPurchaseProvider Provider => purchaseProvider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            entitlement = new EntitlementService();
            purchaseProvider = new MockPurchaseProvider();

            LoadConfig();

            entitlement.Initialize(config);
            purchaseProvider.Initialize(config, _ => { });
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetPurchaseProvider(IPurchaseProvider provider)
        {
            purchaseProvider = provider ?? new MockPurchaseProvider();
            purchaseProvider.Initialize(config, _ => { });
        }

        private void LoadConfig()
        {
            config = new MonetizationConfig();

            string path = Path.Combine(Application.streamingAssetsPath, "monetization.json");
            if (!File.Exists(path))
            {
                Debug.Log("[MonetizationService] No monetization.json found — defaulting to empty catalog (all quests free).");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                MonetizationConfig loaded = JsonConvert.DeserializeObject<MonetizationConfig>(json, SaveLoadManager.JsonSettings);
                if (loaded != null)
                    config = loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MonetizationService] Failed to load monetization.json: {ex.Message}");
            }
        }

        public ProductDefinition FindProductForQuest(string questName)
        {
            if (config == null || string.IsNullOrEmpty(questName)) return null;

            foreach (ProductDefinition product in config.products)
            {
                if (product.grantedQuestNames == null) continue;

                foreach (string grant in product.grantedQuestNames)
                {
                    if (string.Equals(grant, questName, StringComparison.OrdinalIgnoreCase))
                        return product;
                }
            }

            return null;
        }

        public ProductDefinition FindProduct(string productId)
        {
            if (config == null || string.IsNullOrEmpty(productId)) return null;

            foreach (ProductDefinition product in config.products)
                if (product.id == productId) return product;

            return null;
        }

        public bool IsQuestUnlocked(string questName)
        {
            if (entitlement == null) return true;
            QuestAccessState state = entitlement.GetAccessState(questName);
            return state == QuestAccessState.FreeOpen || state == QuestAccessState.OwnedPremium;
        }

        public QuestAccessState GetAccessState(string questName)
        {
            return entitlement != null ? entitlement.GetAccessState(questName) : QuestAccessState.FreeOpen;
        }

        public bool IsFeaturedQuest(string questName)
        {
            return entitlement != null && entitlement.IsFeaturedQuest(questName);
        }

        public void BeginPurchase(ProductDefinition product, Action<PurchaseResult> callback)
        {
            if (product == null)
            {
                callback?.Invoke(PurchaseResult.Fail("?", "Product is null"));
                return;
            }

            if (entitlement.OwnsProduct(product.id))
            {
                PurchaseResult already = PurchaseResult.Already(product.id);
                callback?.Invoke(already);
                PurchaseCompleted?.Invoke(already);
                return;
            }

            purchaseProvider.Purchase(product, result =>
            {
                if (result.Status == PurchaseStatus.Success)
                    entitlement.GrantProduct(product.id);

                callback?.Invoke(result);
                PurchaseCompleted?.Invoke(result);

                if (result.Status == PurchaseStatus.Success)
                    ProductsChanged?.Invoke();
            });
        }

        public void RestorePurchases(Action<bool> callback)
        {
            purchaseProvider.RestorePurchases(success =>
            {
                callback?.Invoke(success);
                if (success) ProductsChanged?.Invoke();
            });
        }

        public List<ProductDefinition> GetVisibleProducts()
        {
            List<ProductDefinition> list = new List<ProductDefinition>();
            if (config?.products == null) return list;
            list.AddRange(config.products);
            return list;
        }

        public void DebugUnlockAll()
        {
            if (config?.products == null) return;
            foreach (ProductDefinition product in config.products)
                entitlement.GrantProduct(product.id);
            ProductsChanged?.Invoke();
        }

        public void DebugLockAll()
        {
            entitlement?.ResetAllForDebug();
            ProductsChanged?.Invoke();
        }
    }
}
