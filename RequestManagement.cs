using System.Runtime.InteropServices.JavaScript;

namespace cloudinteractive.documentcloud
{
    public static class RequestManagement
    {
        private static ILogger? logger;
        public static void Init()
        {
            logger = ApplicationLogging.CreateLogger("RequestManagement");
            logger.LogInformation("RequestManagement init..");
        }

        public enum RequestStatus {Queued, Exporting, Processing, Complete, Error}

        public static void ChangeRequestStatus(string requestId, RequestStatus status)
        {

        }

        public static void SetRequestResult(string requestId, string result)
        {

        }
    }
}
