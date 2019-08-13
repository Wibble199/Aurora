using Microsoft.VisualBasic.Devices;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Aurora.Profiles {
    /// <summary>
    /// Class representing local computer information
    /// </summary>
    public class LocalPCInformation : Node<LocalPCInformation> {
        /// <summary>
        /// The current hour
        /// </summary>
        public int CurrentHour => Utils.Time.GetHours();

        /// <summary>
        /// The current minute
        /// </summary>
        public int CurrentMinute => Utils.Time.GetMinutes();

        /// <summary>
        /// The current second
        /// </summary>
        public int CurrentSecond => Utils.Time.GetSeconds();

        /// <summary>
        /// The current millisecond
        /// </summary>
        public int CurrentMillisecond => Utils.Time.GetMilliSeconds();

        /// <summary>
        /// The total number of milliseconds since the epoch
        /// </summary>
        public long MillisecondsSinceEpoch => Utils.Time.GetMillisecondsSinceEpoch();

        /// <summary>
        /// Used RAM
        /// </summary>
        public long MemoryUsed => PerformanceInfo.GetTotalMemoryInMiB() - PerformanceInfo.GetPhysicalAvailableMemoryInMiB();

        /// <summary>
        /// Available RAM
        /// </summary>
        public long MemoryFree => PerformanceInfo.GetPhysicalAvailableMemoryInMiB();

        /// <summary>
        /// Total RAM
        /// </summary>
        public long MemoryTotal => PerformanceInfo.GetTotalMemoryInMiB();

        /// <summary>
        /// Returns whether or not the device dession is in a locked state.
        /// </summary>
        public bool IsDesktopLocked => Utils.DesktopUtils.IsDesktopLocked;

        /// <summary>
        /// Gets the default endpoint for output (playback) devices e.g. speakers, headphones, etc.
        /// This will return null if there are no playback devices available.
        /// </summary>
        private MMDevice DefaultAudioOutDevice {
            get {
                try { return mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console); }
                catch { return null; }
            }
        }

        /// <summary>
        /// Gets the default endpoint for input (recording) devices e.g. microphones.
        /// This will return null if there are no recording devices available.
        /// </summary>
        private MMDevice DefaultAudioInDevice {
            get {
                try { return mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console); }
                catch { return null; }
            }
        }

        /// <summary>
        /// Current system volume (as set from the speaker icon)
        /// </summary>
        // Note: Manually checks if muted to return 0 since this is not taken into account with the MasterVolumeLevelScalar.
        public float SystemVolume => SystemVolumeIsMuted ? 0 : DefaultAudioOutDevice?.AudioEndpointVolume.MasterVolumeLevelScalar * 100 ?? 0;

        /// <summary>
        /// Gets whether the system volume is muted.
        /// </summary>
        public bool SystemVolumeIsMuted => DefaultAudioOutDevice?.AudioEndpointVolume.Mute ?? true;

        /// <summary>
        /// The volume level that is being recorded by the default microphone even when muted.
        /// </summary>
        public float MicrophoneLevel => DefaultAudioInDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being emitted by the default speaker even when muted.
        /// </summary>
        public float SpeakerLevel => DefaultAudioOutDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// The volume level that is being recorded by the default microphone if not muted.
        /// </summary>
        public float MicLevelIfNotMuted => MicrophoneIsMuted ? 0 : DefaultAudioInDevice?.AudioMeterInformation.MasterPeakValue * 100 ?? 0;

        /// <summary>
        /// Gets whether the default microphone is muted.
        /// </summary>
        public bool MicrophoneIsMuted => DefaultAudioInDevice?.AudioEndpointVolume.Mute ?? true;

        private static PerformanceCounter _CPUCounter;

        private static float _CPUUsage = 0.0f;
        private static float _SmoothCPUUsage = 0.0f;

        private static System.Timers.Timer cpuCounterTimer;

        private static MMDeviceEnumerator mmDeviceEnumerator = new MMDeviceEnumerator();
        private static NAudio.Wave.WaveInEvent waveInEvent = new NAudio.Wave.WaveInEvent();

        /// <summary>
        /// Current CPU Usage
        /// </summary>
        public float CPUUsage
        {
            get
            {
                //Global.logger.LogLine($"_CPUUsage = {_CPUUsage}\t\t_SmoothCPUUsage = {_SmoothCPUUsage}");

                if (_SmoothCPUUsage < _CPUUsage)
                    _SmoothCPUUsage += (_CPUUsage - _SmoothCPUUsage) / 10.0f;
                else if (_SmoothCPUUsage > _CPUUsage)
                    _SmoothCPUUsage -= (_SmoothCPUUsage - _CPUUsage) / 10.0f;

                return _SmoothCPUUsage;
            }
        }

        static LocalPCInformation() {
            try
            {
                _CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            catch(Exception exc)
            {
                Global.logger.LogLine("Failed to create PerformanceCounter. Try: https://stackoverflow.com/a/34615451 Exception: " + exc);
            }


            void StartStopRecording() {
                // We must start recording to be able to capture audio in, but only do this if the user has the option set. Allowing them
                // to turn it off will give them piece of mind we're not spying on them and will stop the Windows 10 mic icon appearing.
                try {
                    if (Global.Configuration.EnableAudioCapture)
                        waveInEvent.StartRecording();
                    else
                        waveInEvent.StopRecording();
                } catch { }
            }

            StartStopRecording();
            Global.Configuration.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "EnableAudioCapture")
                    StartStopRecording();
            };
        }

        internal LocalPCInformation() : base()
        {
            if (cpuCounterTimer == null)
            {
                cpuCounterTimer = new System.Timers.Timer(1000);
                cpuCounterTimer.Elapsed += CpuCounterTimer_Elapsed;
                cpuCounterTimer.Start();
            }
        }

        private void CpuCounterTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _CPUUsage = (_CPUUsage + _CPUCounter.NextValue()) / 2.0f;
            }
            catch (Exception exc)
            {
                Global.logger.Error("PerformanceCounter exception: " + exc);
            }
        }
    }

    static class PerformanceInfo
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        public struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        public static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().AvailablePhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);
        }

        public static Int64 GetTotalMemoryInMiB()
        {
            ulong availableMemory = new ComputerInfo().TotalPhysicalMemory;
            return Convert.ToInt64(availableMemory / 1048576);

        }
    }
}
