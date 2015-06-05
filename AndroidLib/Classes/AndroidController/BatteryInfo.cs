/*
 * Battery.cs - Developed by Dan Wager for AndroidLib.dll
 */

using System.IO;
using System.Text.RegularExpressions;

namespace RegawMOD.Android {
    /// <summary>
    /// Contains information about connected Android device's battery
    /// </summary>
    public class BatteryInfo {

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

        /// <summary>
        /// Gets a value indicating if the connected Android device is on AC Power
        /// </summary>
        public bool ACPower {
            get { Update(); return this.acPower; }
        }

        /// <summary>
        /// Gets a value indicating if the connected Android device is on USB Power
        /// </summary>
        public bool USBPower {
            get { Update(); return usbPower; }
        }

        /// <summary>
        /// Gets a value indicating if the connected Android device is on Wireless Power
        /// </summary>
        public bool WirelessPower {
            get { Update(); return wirelessPower; }
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
        }

        /// <summary>
        /// Gets a value indicating if there is a battery present
        /// </summary>
        public bool Present {
            get { Update(); return present; }
        }

        /// <summary>
        /// Gets a value indicating the current charge level of the battery
        /// </summary>
        public int Level {
            get { Update(); return level; }
        }

        /// <summary>
        /// Gets a value indicating the scale of the battery
        /// </summary>
        public int Scale {
            get { Update(); return scale; }
        }

        /// <summary>
        /// Gets a value indicating the current voltage of the battery
        /// </summary>
        public int Voltage {
            get { Update(); return voltage; }
        }

        /// <summary>
        /// Gets a value indicating the current temperature of the battery
        /// </summary>
        public double Temperature {
            get { Update(); return temperature; }
        }

        /// <summary>
        /// Gets a value indicating the battery's technology
        /// </summary>
        public string Technology {
            get { Update(); return technology; }
        }

        /// <summary>
        /// Initializes a new instance of the BatteryInfo class
        /// </summary>
        /// <param name="device">Serial number of Android device</param>
        internal BatteryInfo(Device device) {
            this.device = device;
            Update();
        }

        /// <summary>
        /// Gets raw data about the device's battery and parses said data to then update all the data in this instance.
        /// </summary>
        private void Update() {
            if (this.device.State != DeviceState.ONLINE) {
                this.acPower = false;
                this.dump = null;
                this.health = -1;
                this.level = -1;
                this.present = false;
                this.scale = -1;
                this.status = -1;
                this.technology = null;
                this.temperature = -1;
                this.usbPower = false;
                this.voltage = -1;
                this.wirelessPower = false;
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
                        bool.TryParse(line.Substring(14), out this.acPower);
                    else if (line.Contains("USB"))
                        bool.TryParse(line.Substring(15), out this.usbPower);
                    else if (line.Contains("Wireless"))
                        bool.TryParse(line.Substring(20), out this.wirelessPower);
                    else if (line.Contains("status"))
                        int.TryParse(line.Substring(10), out this.status);
                    else if (line.Contains("health"))
                        int.TryParse(line.Substring(10), out this.health);
                    else if (line.Contains("present"))
                        bool.TryParse(line.Substring(11), out this.present);
                    else if (line.Contains("level"))
                        int.TryParse(line.Substring(9), out this.level);
                    else if (line.Contains("scale"))
                        int.TryParse(line.Substring(9), out this.scale);
                    else if (line.Contains("voltage"))
                        int.TryParse(line.Substring(10), out this.voltage);
                    else if (line.Contains("temp")) {
                        var substring = line.Substring(15);
                        var lastChar = line[line.Length - 1];
                        var trimmedString = line.Remove(line.Length - 1);
                        var newString =
                            string.Concat(trimmedString, ".", lastChar).ToLower().Contains("temperature") ?
                                Regex.Split(string.Concat(trimmedString, ".", lastChar), ":\\s")[1] : string.Concat(trimmedString, ".", lastChar);
                        double.TryParse(newString, out this.temperature);
                    } else if (line.Contains("tech"))
                        this.technology = line.Substring(14);
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
    }
}
