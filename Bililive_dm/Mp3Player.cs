using System.Runtime.InteropServices;
using System.Text;

namespace Bililive_dm
{
    class Mp3Player
    {
        //To import the dll winmn.dll which allows to play mp3 files
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, int hwndCallback);

        public void Open(string file)
        {
            string command = "open \"" + file + "\" type MPEGVideo alias Music";
            mciSendString(command, null, 0, 0);
        }

        public void Play()
        {
            string command = "play Music";
            mciSendString(command, null, 0, 0);
        }

        public void Stop()
        {
            string command = "stop Music";
            mciSendString(command, null, 0, 0);

            command = "close Music";
            mciSendString(command, null, 0, 0);
        }

        public void AutoPlay(string file)
        {
            mciSendString("open \"" + file + "\" type MPEGVideo alias Music", null, 0, 0);
            mciSendString("play Music wait", null, 0, 0);
            mciSendString("stop Music", null, 0, 0);
            mciSendString("close Music", null, 0, 0);
        }
    }
}
