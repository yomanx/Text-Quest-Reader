namespace TextQuestReader.Monetization
{
    public enum PurchaseStatus
    {
        Success,
        Cancelled,
        AlreadyOwned,
        Failed
    }

    public class PurchaseResult
    {
        public PurchaseStatus Status;
        public string ProductId;
        public string Message;

        public static PurchaseResult Ok(string productId) => new PurchaseResult { Status = PurchaseStatus.Success, ProductId = productId };
        public static PurchaseResult Already(string productId) => new PurchaseResult { Status = PurchaseStatus.AlreadyOwned, ProductId = productId };
        public static PurchaseResult Cancel(string productId) => new PurchaseResult { Status = PurchaseStatus.Cancelled, ProductId = productId };
        public static PurchaseResult Fail(string productId, string message) => new PurchaseResult { Status = PurchaseStatus.Failed, ProductId = productId, Message = message };
    }

    public enum QuestAccessState
    {
        FreeOpen,
        OwnedPremium,
        LockedPremium,
        Unknown
    }
}
