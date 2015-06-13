/*
 * Battery.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RegawMOD.Android {
    /// <summary>
    /// Contains information about connected Android device's battery
    /// </summary>
    public class BatteryInfo {

        #region Variables
        /// <summary>
        /// The device associated with this class.
        /// </summary>
        private Device device;

        /// <summary>
        /// The dumped string from ADB.
        /// </summary>
        private string dump;

        /// <summary>
        /// Contains a value determining whether the device is connected to AC power or not.
        /// </summary>
        private bool acPower;

        /// <summary>
        /// Contains a value determining whether the device is connected to USB power or not.
        /// </summary>
        private bool usbPower;

        /// <summary>
        /// Contains a value determining wether the device is connected to a wireless powersource or not.
        /// </summary>
        private bool wirelessPower;

        /// <summary>
        /// Contains a value determining the battery's status.
        /// </summary>
        private int status;

        /// <summary>
        /// Contains a value determining the battery's health.
        /// </summary>
        private int health;

        /// <summary>
        /// Contains a value determining whether the battery is present in the device or not.
        /// </summary>
        private bool present;

        /// <summary>
        /// Contains a value determining the battery's level of charge.
        /// </summary>
        private int level;

        /// <summary>
        /// Contains a value determining the battery's scale. (How far the battery charges in %)
        /// </summary>
        private int scale;

        /// <summary>
        /// Contains a value determinining the battery's current voltage output.
        /// </summary>
        private int voltage;

        /// <summary>
        /// Contains a value determining the battery's current temperature.
        /// </summary>
        private double temperature;

        /// <summary>
        /// Contains a value determining the technology used in the battery.
        /// </summary>
        private string technology;

        /// <summary>
        /// Contains the string that is returned by <see cref="ToString()"/>
        /// </summary>
        private string outString;
        #endregion
        #region Properties
        /// <summary>
        /// Gets a value indicating if the connected Android device is on AC Power
        /// </summary>
        public bool ACPower {
            get { Update(); return this.acPower; }
            private set { this.acPower = value; OnInfoChanged("ACPower", string.Format("Device {0} charging via AC", value ? "is" : "is not")); }
        }

        /// <summary>
        /// Gets a value indicating if the connected Android device is on USB Power
        /// </summary>
        public bool USBPower {
            get { Update(); return usbPower; }
            private set { this.usbPower = value; OnInfoChanged("USBPower", string.Format("Device {0} charging via USB", value ? "is" : "is not")); }
        }

        /// <summary>
        /// Gets a value indicating if the connected Android device is on Wireless Power
        /// </summary>
        public bool WirelessPower {
            get { Update(); return wirelessPower; }
            private set { this.usbPower = value; OnInfoChanged("WirelessPower", string.Format("Device {0} charging via wireless powersource", value ? "is" : "is not")); }
        }

        /// <summary>
        /// Gets a value indicating the status of the battery
        /// </summary>
        public string Status {
            /* As defined in: http://developer.android.com/reference/android/os/BatteryManager.html
             * Property "Status" is changed from type "int" to type "string" to give a string representation
             * of the value obtained from dumpsys regarding battery status.
             * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
             */
            get {
                Update();
                switch (status) {
                    case 1:
                        return "Unknown Battery Status: " + status;
                    case 2:
                        return "Charging";
                    case 3:
                        return "Discharging";
                    case 4:
                        return "Not charging";
                    case 5:
                        return "Full";
                    default:
                        return "Unknown Value: " + status;
                }
            }
            private set {
                status = int.Parse(value);
                OnInfoChanged("Status", "The battery status has changed!");
            }
        }

        /// <summary>
        /// Gets a value indicating the health of the battery
        /// </summary>
        public string Health {

            /* As defined in: http://developer.android.com/reference/android/os/BatteryManager.html
             * Property "Health" is changed from type "int" to type "string" to give a string representation
             * of the value obtained from dumpsys regarding battery health.
             * Submitted By: Omar Bizreh [DeepUnknown from Xda-Developers.com]
             */
            get {
                Update();
                switch (health) {
                    case 1:
                        return "Unknown Health State: " + health;
                    case 2:
                        return "Good";
                    case 3:
                        return "Over Heat";
                    case 4:
                        return "Dead";
                    case 5:
                        return "Over Voltage";
                    case 6:
                        return "Unknown Failure";
                    case 7:
                        return "Cold Battery";
                    default:
                        return "Unknown Value: " + health;
                }

            }
            private set {
                health = int.Parse(value);
                OnInfoChanged("Health", "The battery's health has changed!");
            }
        }

        /// <summary>
        /// Gets a value indicating if there is a battery present
        /// </summary>
        public bool Present {
            get { Update(); return present; }
            private set { present = value; OnInfoChanged("Present", string.Format("Battery {0} present.", value ? "is" : "is no longer")); }
        }

        /// <summary>
        /// Gets a value indicating the current charge level of the battery
        /// </summary>
        public int Level {
            get { Update(); return level; }
            private set { level = value; OnInfoChanged("Level", "The battery level has changed!"); }
        }

        /// <summary>
        /// Gets a value indicating the scale of the battery
        /// </summary>
        public int Scale {
            get { Update(); return scale; }
            private set { scale = value; OnInfoChanged("Scale", "The battery's scale has changed!"); }
        }

        /// <summary>
        /// Gets a value indicating the current voltage of the battery
        /// </summary>
        public int Voltage {
            get { Update(); return voltage; }
            private set { voltage = value; OnInfoChanged("Voltage", "Battery's voltage has changed!"); }
        }

        /// <summary>
        /// Gets a value indicating the current temperature of the battery
        /// </summary>
        public double Temperature {
            get { Update(); return temperature; }
            private set { temperature = value; OnInfoChanged("Temperature", "The battery's temperature has changed!"); }
        }

        /// <summary>
        /// Gets a value indicating the battery's technology
        /// </summary>
        public string Technology {
            get { Update(); return technology; }
            // Even though this is very unlikely to happen, 
            // I like to keep things consistent.
            private set { technology = value; OnInfoChanged("Technology"); }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the BatteryInfo class
        /// </summary>
        /// <param name="device">Serial number of Android device</param>
        internal BatteryInfo(Device device, bool monitorBattery = true) {
            this.device = device;
            this.MonitorBattery = monitorBattery;
            Update();
        }

        /// <summary>
        /// Gets raw data about the device's battery and parses said data to then update all the data in this instance.
        /// </summary>
        private void Update() {
            if (this.device.State != DeviceState.ONLINE) {
                this.ACPower = false;
                this.dump = null;
                this.Health = "-1";
                this.Level = -1;
                this.Present = false;
                this.Scale = -1;
                this.Status = "-1";
                this.Technology = null;
                this.Temperature = -1;
                this.USBPower = false;
                this.Voltage = -1;
                this.WirelessPower = false;
                this.outString = "Device Not Online";
                return;
            }

            AdbCommand adbCmd = Adb.FormAdbShellCommand(this.device, false, "dumpsys", "battery");
            this.dump = Adb.ExecuteAdbCommand(adbCmd);

            using (StringReader r = new StringReader(this.dump)) {
                string line;

                while (true) {
                    line = r.ReadLine();

                    if (!line.Contains("Current Battery Service state")) {
                        continue;
                    } else {
                        this.dump = line + r.ReadToEnd();
                        break;
                    }
                }
            }

            using (StringReader r = new StringReader(this.dump)) {
                string line = "";

                while (r.Peek() != -1) {
                    line = r.ReadLine();

                    if (line == "")
                        continue;
                    else if (line.Contains("AC "))
                        if (bool.TryParse(line.Substring(14), out this.acPower))
                            ACPower = acPower;
                    else if (line.Contains("USB"))
                        if (bool.TryParse(line.Substring(15), out this.usbPower))
                            USBPower = usbPower;
                    else if (line.Contains("Wireless"))
                        if (bool.TryParse(line.Substring(20), out this.wirelessPower))
                            WirelessPower = wirelessPower;
                    else if (line.Contains("status"))
                        if (int.TryParse(line.Substring(10), out this.status))
                            Status = status.ToString();
                    else if (line.Contains("health"))
                        if (int.TryParse(line.Substring(10), out this.health))
                            Health = health.ToString();
                    else if (line.Contains("present"))
                        if (bool.TryParse(line.Substring(11), out this.present))
                            Present = present;
                    else if (line.Contains("level"))
                        if (int.TryParse(line.Substring(9), out this.level))
                            Level = level;
                    else if (line.Contains("scale"))
                        if (int.TryParse(line.Substring(9), out this.scale))
                            Scale = scale;
                    else if (line.Contains("voltage"))
                        if (int.TryParse(line.Substring(10), out this.voltage))
                            Voltage = voltage;
                    else if (line.Contains("temp")) {
                        var substring = line.Substring(15);
                        var lastChar = line[line.Length - 1];
                        var trimmedString = line.Remove(line.Length - 1);
                        var newString =
                            string.Concat(trimmedString, ".", lastChar).ToLower().Contains("temperature") ?
                                Regex.Split(string.Concat(trimmedString, ".", lastChar), ":\\s")[1] : string.Concat(trimmedString, ".", lastChar);
                        if (double.TryParse(newString, out this.temperature))
                            Temperature = temperature;
                    } else if (line.Contains("tech"))
                        this.Technology = line.Substring(14);
                }
            }

            this.outString = this.dump.Replace("Service state", "State For Device " + this.device.SerialNumber);
        }

        /// <summary>
        /// Returns a formatted string object containing all battery stats
        /// </summary>
        /// <returns>A formatted string containing all battery stats</returns>
        public override string ToString() {
            Update();
            return this.outString;
        }

        /// *********************************************
        /// Event handling added by Beatsleigher        *
        /// *********************************************
        #region Event Handling
        private bool m_monitorBattery = true;
        public bool MonitorBattery {
            get { return m_monitorBattery; }
            set { 
                m_monitorBattery = value;
                if (value) StartMonitorBattery();
            }
        }

        /// <summary>
        /// Gets or sets the update interval in milliseconds (x1000 = 1 second).
        /// Created by: Beatsleigher
        /// At:               08.06.2015, 13:07
        /// On:              BEATSLEIGHER-PC
        /// </summary>
        /// <value>
        /// The update interval.
        /// </value>
        public int UpdateInterval {
            get; set;
        }

        /// <summary>
        /// Monitors the battery.
        /// </summary>
        private async void StartMonitorBattery() {
            await Task.Run(new Action(() => {
                while (m_monitorBattery) {
                    Update();
                    Thread.Sleep(UpdateInterval);
                }
            }));
        }

        /// <summary>
        /// Occurs when a property in this instance has changed its value.
        /// Created by: Beatsleigher
        /// At:               08.06.2015, 13:14
        /// On:              BEATSLEIGHER-PC
        /// </summary>
        public event BatteryInfoChangedEventHandler InfoChanged;

        /// <summary>
        /// Called when a property in this class has changed its value.
        /// </summary>
        /// <param name="m_propertyName">Name of the m_property.</param>
        /// <param name="m_message">The m_message.</param>
        private void OnInfoChanged(string m_propertyName, string m_message = "A property has changed.") {
            if (InfoChanged != null && !(InfoChanged.GetInvocationList().Length > 0))
                foreach (BatteryInfoChangedEventHandler m_handler in InfoChanged.GetInvocationList())
                    m_handler(this, new OnBatteryInfoChangedEventArgs() { Device = this.device, Message = m_message, PropertyName = m_propertyName });
        }
        #endregion
    }
}
