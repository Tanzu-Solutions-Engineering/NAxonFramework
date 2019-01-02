using System;

namespace NAxonFramework.Common.io
{
    public static class IOUtils
    {
        public static void CloseQuietly(IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                // ignore
            }

        }
    }
}