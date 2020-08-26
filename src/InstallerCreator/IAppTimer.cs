using System.Diagnostics;

namespace InstallerCreator
{
    public interface IAppTimer
    {
        void Start();
        string GetCurrent();
        void Pause();
    }
    public class AppTimer : IAppTimer {
        private readonly Stopwatch _timer;

        public AppTimer()
        {
            _timer = new Stopwatch();
        }
        public string GetCurrent() => _timer.Elapsed.ToString();

        public void Pause() {
            _timer.Stop();
        }

        public void Start() {
            _timer.Start();
        }
    }
}