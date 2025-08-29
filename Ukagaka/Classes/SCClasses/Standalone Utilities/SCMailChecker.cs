using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public enum MailCheckStatus
    {
        Success = 0,
        CannotConnectToServer = -1,
        ServerReturnedError = -2,
        CommunicationError = -3,
        UsernameWasInvalid = -4,
        PasswordWasInvalid = -5,
        CommandStatError = -6,
        CommunicationTimedOut = -7,
        UnexpectedError = -100
    }
    public class MailCheckResult
    {
    
        public MailCheckStatus Status { get; set; }
        public string MessageCount { get; set; }
        public string TotalSize { get; set; }
    }



    public static class SCMailChecker
    {
        

        public static async Task<MailCheckResult>
            Pop3CheckAsync(string server, string username, string password, CancellationToken cancellationToken = default)
        {
            MailCheckResult result = new MailCheckResult();
            try
            {
           
                using var client = new TcpClient();

                // Connect with timeout
                var connectTask = client.ConnectAsync(server, 110);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    result.Status = MailCheckStatus.CannotConnectToServer;

                    return result;
                }

                await connectTask; // Rethrow any connection errors

                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.ASCII);
                using var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };

                // Read initial response
                var response = await ReadLineAsync(reader, cancellationToken);
                if (response == null || !response.StartsWith("+OK"))
                {
                    result.Status = MailCheckStatus.ServerReturnedError;

                    return result;
                }

                // Authenticate
                await writer.WriteLineAsync($"USER {username}");
                response = await ReadLineAsync(reader, cancellationToken);
                if (response == null || !response.StartsWith("+OK"))
                {
                    result.Status = MailCheckStatus.UsernameWasInvalid;
                    return result;
                }

                await writer.WriteLineAsync($"PASS {password}");
                response = await ReadLineAsync(reader, cancellationToken);
                if (response == null || !response.StartsWith("+OK"))
                {
                    result.Status = MailCheckStatus.PasswordWasInvalid;
                    return result;
                }

                // Get mail stats
                await writer.WriteLineAsync("STAT");
                response = await ReadLineAsync(reader, cancellationToken);
                if (response == null || !response.StartsWith("+OK"))
                {
                    result.Status = MailCheckStatus.CommandStatError;

                    return result;
                    //return (MailCheckResult.CommandStatError, null, null);
                }

                var statParts = response.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (statParts.Length < 3)
                {
                    result.Status = MailCheckStatus.CommandStatError;

                    return result;


                    //return (MailCheckResult.CommandStatError, null, null);
                }

                var messageCount = statParts[1];
                var totalSize = statParts[2];

                // Quit
                await writer.WriteLineAsync("QUIT");

                result.Status = MailCheckStatus.Success;
                result.MessageCount = messageCount;
                result.TotalSize = totalSize;
                return result;



                //    return (MailCheckResult.Ok, messageCount, totalSize);
            }
            catch (OperationCanceledException)
            {

                result.Status = MailCheckStatus.CommunicationTimedOut;
             
                return result;



                // return (MailCheckResult.CommunicationTimedOut, null, null);
            }
            catch (SocketException)
            {
                result.Status = MailCheckStatus.CannotConnectToServer;
                return result;

                //return (MailCheckResult.CannotConnectToServer, null, null);
            }
            catch (IOException)
            {
                result.Status = MailCheckStatus.CommunicationError;
                return result;



                //return (MailCheckResult.CommunicationError, null, null);
            }
            catch
            {
                result.Status = MailCheckStatus.UnexpectedError;
                return result;


                //return (MailCheckResult.UnexpectedError, null, null);
            }
        }

        private static async Task<string> ReadLineAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            try
            {
                var readTask = reader.ReadLineAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                var completedTask = await Task.WhenAny(readTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException();
                }

                return await readTask;
            }
            catch
            {
                return null;
            }
        }
    }
}
