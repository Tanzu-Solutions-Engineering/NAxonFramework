using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public class ReplayToken : ITrackingToken, IWrappedToken
    {
        public ITrackingToken TokenAtReset { get; }

        public ITrackingToken CurrentToken { get; }


        public static ITrackingToken CreateReplayToken(ITrackingToken tokenAtReset, ITrackingToken startPosition) 
        {
            if (tokenAtReset == null) 
            {
                return null;
            }
            if (tokenAtReset is ReplayToken) 
            {
                return CreateReplayToken(((ReplayToken) tokenAtReset).TokenAtReset, startPosition);
            }
            if (startPosition != null && startPosition.Covers(tokenAtReset)) 
            {
                return startPosition;
            }
            return new ReplayToken(tokenAtReset, startPosition);
        }

        public static ITrackingToken CreateReplayToken(ITrackingToken tokenAtReset) 
        {
            return CreateReplayToken(tokenAtReset, null);
        }

        public static bool IsReplay(IMessage message) 
        {
            return message is ITrackedEventMessage
                && IsReplay(((ITrackedEventMessage) message).TrackingToken);
        }

        public static bool IsReplay(ITrackingToken trackingToken) 
        {
            return trackingToken is ReplayToken
                && ((ReplayToken) trackingToken).IsReplay();

        }

        public ReplayToken(ITrackingToken tokenAtReset) : this(tokenAtReset, null)
        {
        }

        public ReplayToken(ITrackingToken tokenAtReset, ITrackingToken currentToken)
        {
            TokenAtReset = tokenAtReset;
            CurrentToken = currentToken;
        }

        public ITrackingToken AdvancedTo(ITrackingToken newToken) 
        {
            if (TokenAtReset == null
                || (newToken.Covers(this.TokenAtReset) && !TokenAtReset.Covers(newToken))) 
            {
                // we're done replaying
                return newToken;
            } 
            else if (TokenAtReset.Covers(newToken)) 
            {
                // we're still well behind
                return new ReplayToken(TokenAtReset, newToken);
            } 
            else 
            {
                // we're getting an event that we didn't have before, but we haven't finished replaying either
                return new ReplayToken(TokenAtReset.UpperBound(newToken), newToken);
            }
        }

        public ITrackingToken LowerBound(ITrackingToken other)
        {
            if (other is ReplayToken) {
                return new ReplayToken(this, ((ReplayToken) other).CurrentToken);
            }
            return new ReplayToken(this, other);
        }

        public ITrackingToken UpperBound(ITrackingToken other) => AdvancedTo(other);

        public bool Covers(ITrackingToken other)
        {
            if (other is ReplayToken) 
            {
                return CurrentToken != null && CurrentToken.Covers(((ReplayToken) other).CurrentToken);
            }
            return CurrentToken != null && CurrentToken.Covers(other);
        }

        private bool IsReplay() => CurrentToken == null || TokenAtReset.Covers(CurrentToken);

        public ITrackingToken Unwrap() => CurrentToken;

        protected bool Equals(ReplayToken other)
        {
            return Equals(TokenAtReset, other.TokenAtReset) && Equals(CurrentToken, other.CurrentToken);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReplayToken) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TokenAtReset != null ? TokenAtReset.GetHashCode() : 0) * 397) ^ (CurrentToken != null ? CurrentToken.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"ReplayToken{{currentToken={CurrentToken}, tokenAtReset={TokenAtReset}}}";
        }
    }
}