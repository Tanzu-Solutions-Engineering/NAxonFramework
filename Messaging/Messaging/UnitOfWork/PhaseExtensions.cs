namespace NAxonFramework.Messaging.UnitOfWork
{
    public static class PhaseExtensions
    {
        public static bool IsStarted(this Phase phase)
        {
            
            switch (phase)
            {
                case Phase.NOT_STARTED:
                    return false;
                case Phase.STARTED:
                    return true;
                case Phase.PREPARE_COMMIT:
                    return true;
                case Phase.COMMIT:
                    return true;
                case Phase.ROLLBACK:
                    return true;
                case Phase.AFTER_COMMIT:
                    return true;
                case Phase.CLEANUP:
                    return false;
                case Phase.CLOSED:
                    return false;
                default:
                    return false;
            }
        }
        public static bool IsReverseCallbackOrder(this Phase phase)
        {
            switch (phase)
            {
                case Phase.NOT_STARTED:
                    return false;
                case Phase.STARTED:
                    return false;
                case Phase.PREPARE_COMMIT:
                    return false;
                case Phase.COMMIT:
                    return true;
                case Phase.ROLLBACK:
                    return true;
                case Phase.AFTER_COMMIT:
                    return true;
                case Phase.CLEANUP:
                    return true;
                case Phase.CLOSED:
                    return true;
                default:
                    return true;
            }
        }

        public static bool IsBefore(this Phase phase, Phase otherPhase) => phase < otherPhase;
        public static bool IsAfter(this Phase phase, Phase otherPhase) => phase > otherPhase;
    }
}