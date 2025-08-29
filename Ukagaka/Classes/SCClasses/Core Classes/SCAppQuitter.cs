using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
namespace Ukagaka
{

    public class SCAppQuitter
    {
        private Task _quitTask;
        private CancellationTokenSource _cts;

        public bool IsAlive()
        {
           return _quitTask != null && !_quitTask.IsCompleted;
        }
        public async Task QuitAsync()
        {
            // Create cancellation token source
            _cts = new CancellationTokenSource();

            // Close all sessions asynchronously
            List<SCSession> sessions = SCFoundation.SharedFoundation().GetSessionsList();
            List<Task> closeTasks = new List<Task>();

            lock (sessions)
            {
                foreach (SCSession session in sessions)
                {
                    closeTasks.Add(Task.Run(() =>
                    {
                        // Check for cancellation before each close operation
                        _cts.Token.ThrowIfCancellationRequested();
                        session.PerformClose();
                    }, _cts.Token));
                }
            }

            try
            {
                // Wait for all sessions to close
                await Task.WhenAll(closeTasks);

                // Wait until all sessions are actually closed
                while (SCFoundation.SharedFoundation().NumOfSessions() > 0)
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    await Task.Delay(200, _cts.Token); // 200ms delay
                }

                // Shutdown the application on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (OperationCanceledException)
            {
                // Quit operation was cancelled
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        public void Start()
        {
            if (IsAlive()) return;

            _quitTask = QuitAsync();

            // Handle any exceptions that might occur during quitting
            _quitTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Log the error
                   // Debug.print("Error during application quit", t.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }

}