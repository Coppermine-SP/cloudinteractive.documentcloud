using System.Collections.Concurrent;
using cloudinteractive.document;
using cloudinteractive.document.Util;
using OpenAI_API.Models;

namespace cloudinteractive.documentcloud
{

    public class RequestObject
    {
        public string RequestId;
        public FileType Type;
        public string Prompt;
        public Stream File;
    }

    public enum FileType
    {
        PDF,
        Image
    };

    public static class WorkerPool
    {
        private static ILogger? _logger = ApplicationLogging.CreateLogger("RequestManagement");
        private static ConcurrentQueue<RequestObject> _requestQueue = new ConcurrentQueue<RequestObject>();
        private static List<Task> _taskList = new List<Task>();

        public static async Task<string> CreateNewRequest(string prompt, FileType type, Stream file)
        {
            string requestId = Guid.NewGuid().ToString();
            var stream = new MemoryStream();
            file.CopyTo(stream);
            _requestQueue.Enqueue(new RequestObject()
            {
                Prompt = prompt,
                File = stream,
                RequestId = requestId,
                Type = type
            });
            RequestManagement.ChangeRequestStatus(requestId, RequestManagement.RequestStatus.Queued);
            _logger.LogInformation($"#{requestId}: created.");


            return requestId;
        }

        public static void StartThread(int threads)
        {
            for (int i = 0; i < threads; i++)
            {
                var task = Task.Run(() => WorkerTask());
                _taskList.Add(task);
            }
        }

        public static async void WorkerTask()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            var logger = ApplicationLogging.CreateLogger($"WorkerThread #{threadId}");

            logger.LogInformation("Task Start.");
            while (true)
            {
                RequestObject request;
                if (_requestQueue.TryDequeue(out request))
                {
                    logger.LogInformation($"Request #{request.RequestId} - begin");
                    try
                    {
                        RequestManagement.ChangeRequestStatus(request.RequestId,
                            RequestManagement.RequestStatus.Exporting);
                        IExportableDocument document;

                        if (request.Type == FileType.PDF)
                        {
                            document = await PdfDocument.ImportFromStream(request.File);
                        }
                        else
                        {
                            MemoryStream stream = new MemoryStream();
                            request.File.Position = 0;
                            request.File.CopyTo(stream);
                            request.File.Close();

                            document = await ImageDocument.ImportFromStream(stream);
                            stream.Close();
                        }

                        logger.LogInformation($"Request #{request.RequestId} - Exporting text...");
                        var texts = await AzureComputerVision.ExportTextFromDocument(document);

                        logger.LogInformation($"Request #{request.RequestId} - Waiting for OpenAI API Response...");
                        RequestManagement.ChangeRequestStatus(request.RequestId, RequestManagement.RequestStatus.Processing);
                        var result = await OpenAI.GetChatCompletion(request.Prompt, texts, Model.GPT4_Turbo);

                        RequestManagement.SetRequestResult(request.RequestId, result);
                        RequestManagement.ChangeRequestStatus(request.RequestId, RequestManagement.RequestStatus.Complete);
                        logger.LogInformation($"Request #{request.RequestId} - end");
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Request #{request.RequestId} - Exception:\n" + e.ToString());
                        RequestManagement.ChangeRequestStatus(request.RequestId, RequestManagement.RequestStatus.Error);
                    }
                }
                await Task.Delay(200);
            }
        }
    }
}