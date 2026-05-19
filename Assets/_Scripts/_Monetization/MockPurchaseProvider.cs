using System;
using UnityEngine;

namespace TextQuestReader.Monetization
{
    /// <summary>
    /// In-memory purchase provider for editor/local builds. Always succeeds
    /// (after a short fake delay) so the unlock UI can be exercised without
    /// any external store integration. Replace this with UnityIAPPurchaseProvider
    /// or a Steamworks bridge in a real build.
    /// </summary>
    public class MockPurchaseProvider : IPurchaseProvider
    {
        public bool IsAvailable => true;
        public string ProviderId => "mock";

        public void Initialize(MonetizationConfig config, Action<bool> onReady)
        {
            onReady?.Invoke(true);
        }

        public void Purchase(ProductDefinition product, Action<PurchaseResult> callback)
        {
            if (product == null)
            {
                callback?.Invoke(PurchaseResult.Fail("?", "Product is null"));
                return;
            }

            Debug.Log($"[MockPurchaseProvider] Simulating purchase: {product.id}");

            if (MonetizationService.Instance != null)
            {
                MonetizationService.Instance.StartCoroutine(MockCompleteRoutine(product, callback));
            }
            else
            {
                callback?.Invoke(PurchaseResult.Ok(product.id));
            }
        }

        public void RestorePurchases(Action<bool> callback)
        {
            callback?.Invoke(true);
        }

        private System.Collections.IEnumerator MockCompleteRoutine(ProductDefinition product, Action<PurchaseResult> callback)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            callback?.Invoke(PurchaseResult.Ok(product.id));
        }
    }
}
