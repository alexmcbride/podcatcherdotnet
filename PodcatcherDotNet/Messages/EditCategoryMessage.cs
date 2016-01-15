
namespace PodcatcherDotNet.Messages {
    public class EditCategoryMessage {
        public string OldName { get; private set; }
        public string NewName { get; private set; }

        public EditCategoryMessage(string oldName, string newName) {
            OldName = oldName;
            NewName = newName;
        }
    }
}
