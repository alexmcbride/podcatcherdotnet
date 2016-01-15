using System;

namespace PodcatcherDotNet.Messages {
    public class RemoveOldItemsMessage {
        public DateTime Date { get; private set; }
        public int MinItems { get; private set; }

        public RemoveOldItemsMessage(DateTime date, int minItems) {
            Date = date;
            MinItems = minItems;
        }
    }
}
