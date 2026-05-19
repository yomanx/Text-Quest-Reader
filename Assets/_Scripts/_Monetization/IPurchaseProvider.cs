using System;

namespace TextQuestReader.Monetization
{
    public interface IPurchaseProvider
    {
        bool IsAvailable { get; }
        string ProviderId { get; }

        void Initialize(MonetizationConfig config, Action<bool> onReady);
        void Purchase(ProductDefinition product, Action<PurchaseResult> callback);
        void RestorePurchases(Action<bool> callback);
    }
}
