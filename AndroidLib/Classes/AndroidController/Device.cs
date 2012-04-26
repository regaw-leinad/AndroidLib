/*
 * Device.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.IO;
using System.Threading;

namespace RegawMOD.Android
{
    /// <summary>
    /// Manages connected Android device's info and commands
    /// </summary>
    public partial class Device
    {
        private BatteryInfo battery;
        private BuildProp buildProp;
        private BusyBox busyBox;
        private FileSystem fileSystem;
        //private PackageManager packageManager;
        private Phone phone;
        //private Processes processes;
        private Su su;
        private string serialNumber;
        private DeviceState state;

        /// <summary>
        /// Initializes a new instance of the Device class
        /// </summary>
        /// <param name="deviceSerial">Serial number of Android device</param>
        internal Device(string deviceSerial)
        {
            this.serialNumber = deviceSerial;
            Update();
        }

        private DeviceState SetState()
        {
            string state = null;

            using (StringReader r = new StringReader(Adb.Devices()))
            {
                string line;

                while (r.Peek() != -1)
                {
                    line = r.ReadLine();

                    if (line.Contains(this.serialNumber))
                        state = line.Substring(line.IndexOf('\t') + 1);
                }
            }

            if (state == null)
            {
                using (StringReader r = new StringReader(Fastboot.Devices()))
                {
                    string line;

                    while (r.Peek() != -1)
                    {
                        line = r.ReadLine();

                        if (line.Contains(this.serialNumber))
                            state = line.Substring(line.IndexOf('\t') + 1);
                    }
                }
            }

            switch (state)
            {
                case "device":
                    return DeviceState.ONLINE;
                case "recovery":
                    return DeviceState.RECOVERY;
                case "fastboot":
                    return DeviceState.FASTBOOT;
                default:
                    return DeviceState.UNKNOWN;
            }
        }

        /// <summary>
        /// Gets the device's <see cref="BatteryInfo"/> instance
        /// </summary>
        /// <remarks>See <see cref="BatteryInfo"/> for more details</remarks>
        public BatteryInfo Battery { get { return this.battery; } }

        /// <summary>
        /// Gets the device's <see cref="BuildProp"/> instance
        /// </summary>
        /// <remarks>See <see cref="BuildProp"/> for more details</remarks>
        public BuildProp BuildProp { get { return this.buildProp; } }

        /// <summary>
        /// Gets the device's <see cref="BusyBox"/> instance
        /// </summary>
        /// <remarks>See <see cref="BusyBox"/> for more details</remarks>
        public BusyBox BusyBox { get { return this.busyBox; } }

        /// <summary>
        /// Gets the device's <see cref="FileSystem"/> instance
        /// </summary>
        /// <remarks>See <see cref="FileSystem"/> for more details</remarks>
        public FileSystem FileSystem { get { return this.fileSystem; } }
        
        ///// <summary>
        ///// Gets the device's <see cref="PackageManager"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="PackageManager"/> for more details</remarks>
        //public PackageManager PackageManager { get { return this.packageManager; } }

        /// <summary>
        /// Gets the device's <see cref="Phone"/> instance
        /// </summary>
        /// <remarks>See <see cref="Phone"/> for more details</remarks>
        public Phone Phone { get { return this.phone; } }

        ///// <summary>
        ///// Gets the device's <see cref="Processes"/> instance
        ///// </summary>
        ///// <remarks>See <see cref="Processes"/> for more details</remarks>
        //public Processes Processes { get { return this.processes; } }

        /// <summary>
        /// Gets the device's <see cref="Su"/> instance
        /// </summary>
        /// <remarks>See <see cref="Su"/> for more details</remarks>
        public Su Su { get { return this.su; } }

        /// <summary>
        /// Gets the device's serial number
        /// </summary>
        public string SerialNumber { get { return this.serialNumber; } }

        /// <summary>
        /// Gets a value indicating the device's current state
        /// </summary>
        /// <remarks>See <see cref="DeviceState"/> for more details</remarks>
        public DeviceState State { get { return this.state; } internal set { this.state = value; } }

        /// <summary>
        /// Gets a value indicating if the device has root
        /// </summary>
        public bool HasRoot { get { return this.su.Exists; } }

        /// <summary>
        /// Reboots the device regularly
        /// </summary>
        public void Reboot()
        {
            Thread t = new Thread(new ThreadStart(RebootThread));
            t.Start();
        }

        private void RebootThread()
        {
            Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbShellCommand(this, false, "reboot"));
        }

        /// <summary>
        /// Reboots the device into recovery
        /// </summary>
        public void RebootRecovery()
        {
            Thread t = new Thread(new ThreadStart(RebootRecoveryThread));
            t.Start();
        }

        private void RebootRecoveryThread()
        {
            Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbShellCommand(this, false, "reboot", "recovery"));
        }

        /// <summary>
        /// Reboots the device into the bootloader
        /// </summary>
        public void RebootBootloader()
        {
            Thread t = new Thread(new ThreadStart(RebootBootloaderThread));
            t.Start();
        }

        private void RebootBootloaderThread()
        {
            Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbShellCommand(this, false, "reboot", "bootloader"));
        }

        /// <summary>
        /// Pulls a file from the device
        /// </summary>
        /// <param name="fileOnDevice">Path to file to pull from device</param>
        /// <param name="destinationDirectory">Directory on local computer to pull file to</param>
        /// <returns>True if file is pulled, false if file doesn't exist or pull failed</returns>
        public bool PullFile(string fileOnDevice, string destinationDirectory)
        {
            AdbCommand adbCmd = Adb.FormAdbCommand(this, "pull", fileOnDevice, "\"" + destinationDirectory + "\"");

            if (Adb.ExecuteAdbCommand(adbCmd).Contains(" does not exist"))
                return false;

            return true;
        }

        /// <summary>
        /// Pulls a full directory recursively from the device
        /// </summary>
        /// <param name="location">Path to folder to pull from device</param>
        /// <param name="destination">Directory on local computer to pull file to</param>
        /// <returns>True if directory is pulled, false if directory doesn't exist or pull failed</returns>
        public bool PullDirectory(string location, string destination)
        {
            AdbCommand adbCmd = Adb.FormAdbCommand(this, "pull", (location.EndsWith("/") ? location : location + "/"), "\"" + destination + "\"");
            return (Adb.ExecuteAdbCommandReturnExitCode(adbCmd) == 0);
        }

        /// <summary>
        /// Updates all values in current instance of <see cref="Device"/>
        /// </summary>
        public void Update()
        {
            this.state = SetState();

            this.su = new Su(this);
            this.battery = new BatteryInfo(this);
            this.buildProp = new BuildProp(this);
            this.busyBox = new BusyBox(this);
            this.phone = new Phone(this);
            this.fileSystem = new FileSystem(this);
        }
    }
}