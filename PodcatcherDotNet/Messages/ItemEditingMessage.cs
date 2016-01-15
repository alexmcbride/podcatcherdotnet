using PodcatcherDotNet.ViewModels;

namespace PodcatcherDotNet.Messages {
    public class ItemEditingMessage {
        public ItemViewModel Item { get; private set; }

        public ItemEditingMessage(ItemViewModel item) {
            Item = item;
        }
    }
}
