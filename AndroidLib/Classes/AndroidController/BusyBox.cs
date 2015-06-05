/*
 * BusyBox.cs - Developed by Dan Wager for AndroidLib.dll
 */


namespace RegawMOD.Android {

    using System.Collections.Generic;
    using System.IO;
    
    /// <summary>
    /// Contains information about device's busybox.
    /// </summary>
    public class BusyBox {

        /// <summary>
        /// The executable
        /// </summary>
        internal const string EXECUTABLE = "busybox";

        /// <summary>
        /// The device associated with this class.
        /// </summary>
        private Device device;

        /// <summary>
        /// Contains a value determining whether Busybox is installed on the device.
        /// </summary>
        private bool isInstalled;

        /// <summary>
        /// Contains a value determining the version of the Busybox installation on the device.
        /// </summary>
        private string version;

        /// <summary>
        /// A list of commands for the device's Busybox installation.
        /// </summary>
        private List<string> commands;

        /// <summary>
        /// Gets a value indicating if busybox is installed on the current device
        /// </summary>
        public bool IsInstalled { get { return this.isInstalled; } }

        /// <summary>
        /// Gets a value indicating the version of busybox installed
        /// </summary>
        public string Version { get { return this.version; } }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> containing busybox's commands
        /// </summary>
        public List<string> Commands { get { return this.commands; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusyBox"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        internal BusyBox(Device device) {
            this.device = device;

            this.commands = new List<string>();

            Update();
        }

        /// <summary>
        /// Updates the instance of busybox
        /// </summary>
        /// <remarks>Generally called only if busybox may have changed on the device</remarks>
        public void Update() {
            this.commands.Clear();

            if (!this.device.HasRoot || this.device.State != DeviceState.ONLINE) {
                SetNoBusybox();
                return;
            }

            AdbCommand adbCmd = Adb.FormAdbShellCommand(this.device, false, EXECUTABLE);
            using (StringReader s = new StringReader(Adb.ExecuteAdbCommand(adbCmd))) {
                string check = s.ReadLine();

                if (check.Contains(string.Format("{0}: not found", EXECUTABLE))) {
                    SetNoBusybox();
                    return;
                }

                this.isInstalled = true;

                this.version = check.Split(' ')[1].Substring(1);

                while (s.Peek() != -1 && s.ReadLine() != "Currently defined functions:") { }

                string[] cmds = s.ReadToEnd().Replace(" ", "").Replace("\r\r\n\t", "").Trim('\t', '\r', '\n').Split(',');

                if (cmds.Length.Equals(0)) {
                    SetNoBusybox();
                } else {
                    foreach (string cmd in cmds)
                        this.commands.Add(cmd);
                }
            }
        }

        /// <summary>
        /// If no installation of Busybox could be found on the device, all variables suggesting otherwise will be set to false.
        /// </summary>
        private void SetNoBusybox() {
            this.isInstalled = false;
            this.version = null;
        }
    }
}