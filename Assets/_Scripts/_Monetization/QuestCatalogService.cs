using System.Collections.Generic;

namespace TextQuestReader.Monetization
{
    public enum QuestSourceKind
    {
        Local,
        Remote,
        PremiumLocked
    }

    public class CatalogEntry
    {
        public QuestShort QuestShort;
        public QuestSourceKind Source;
        public QuestAccessState AccessState;
        public bool IsFeatured;
        public ProductDefinition Product;

        public bool IsLocked => AccessState == QuestAccessState.LockedPremium;
        public bool IsPremium => AccessState != QuestAccessState.FreeOpen;
    }

    public class QuestCatalogService
    {
        public List<CatalogEntry> BuildEntries(List<QuestShort> quests, QuestSourceKind source)
        {
            List<CatalogEntry> entries = new List<CatalogEntry>();
            if (quests == null) return entries;

            MonetizationService monetization = MonetizationService.Instance;

            foreach (QuestShort quest in quests)
            {
                if (quest == null) continue;

                QuestAccessState state = monetization != null
                    ? monetization.GetAccessState(quest.QuestName)
                    : QuestAccessState.FreeOpen;

                bool featured = monetization != null && monetization.IsFeaturedQuest(quest.QuestName);
                ProductDefinition product = monetization?.FindProductForQuest(quest.QuestName);

                QuestSourceKind effectiveSource = state == QuestAccessState.LockedPremium ? QuestSourceKind.PremiumLocked : source;

                entries.Add(new CatalogEntry
                {
                    QuestShort = quest,
                    Source = effectiveSource,
                    AccessState = state,
                    IsFeatured = featured,
                    Product = product
                });
            }

            entries.Sort((a, b) =>
            {
                if (a.IsFeatured != b.IsFeatured) return a.IsFeatured ? -1 : 1;
                int orderCmp = a.QuestShort.Order.CompareTo(b.QuestShort.Order);
                if (orderCmp != 0) return orderCmp;
                return string.Compare(a.QuestShort.QuestName, b.QuestShort.QuestName, System.StringComparison.OrdinalIgnoreCase);
            });

            return entries;
        }
    }
}
