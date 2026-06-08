using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AILanguageLearningApp.Models
{
    public class TextChunkReceivedMessage : ValueChangedMessage<string>
    {
        public TextChunkReceivedMessage(string value) : base(value) { }
    }
}
