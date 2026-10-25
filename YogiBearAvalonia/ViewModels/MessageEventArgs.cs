using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvalonia.ViewModel;

public class MessageEventArgs : EventArgs
{
    public string Message { get; }
    public string Title { get; }
    public MessageType Type { get; }
    public bool ShouldResumeGameAfter { get; }
    public bool ShouldShowRetry { get; }

    public MessageEventArgs(string message, string title, MessageType type, bool shouldResumeGameAfter = false, bool shouldShowRetry = false)
    {
        Message = message;
        Title = title;
        Type = type;
        ShouldResumeGameAfter = shouldResumeGameAfter;
        ShouldShowRetry = shouldShowRetry;
    }
}

public enum MessageType
{
    Information,
    Warning,
    Error,
    Question
}
