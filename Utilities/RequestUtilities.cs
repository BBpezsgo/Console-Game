using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame;

public static class RequestKinds
{
    public const int OBJ_DETAILS_REQUEST = 1;
    public const int CLIENT_LIST = 1;
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Request : IEquatable<Request?>
{
    public readonly double SentTime;
    public readonly int Kind;
    public readonly int Guid;

    public Request(int kind, int guid)
    {
        SentTime = DateTime.UtcNow.TimeOfDay.TotalSeconds;
        Kind = kind;
        Guid = guid;
    }

    public override bool Equals(object? obj) => Equals(obj as Request);
    public bool Equals(Request? other) =>
        other is not null &&
        Kind == other.Kind &&
        Guid == other.Guid;
    public override int GetHashCode() => HashCode.Combine(Kind, Guid);

    public static bool operator ==(Request? left, Request? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(Request? left, Request? right) => !(left == right);

    public override string ToString()
    {
        int hour = (int)(SentTime / 3600f);
        int minute = (int)(SentTime / 60f);
        int second = (int)SentTime;

        return $"{{ Kind: {Kind} Guid: {Guid} }} at {hour}:{minute:00}:{second:00}";
    }
    string GetDebuggerDisplay() => ToString();
}

public class RequestManager
{
    readonly List<Request> requests = new();

    public bool IsRequested(Request request, [NotNullWhen(true)] out Request? sentRequest)
    {
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            if (requests[i].Equals(request))
            {
                sentRequest = requests[i];
                return true;
            }
        }
        sentRequest = null;
        return false;
    }

    public bool Request(Request request)
    {
        if (IsRequested(request, out Request? sentRequest))
        {
            double timeDifference = request.SentTime - sentRequest.SentTime;
            if (timeDifference < 2.0)
            {
                return false;
            }
            Finished(sentRequest);
        }

        requests.Add(request);
        return true;
    }

    public void Finished(Request request)
    {
        for (int i = requests.Count - 1; i >= 0; i--)
        {
            if (requests[i].Equals(request))
            { requests.RemoveAt(i); }
        }
    }
}
