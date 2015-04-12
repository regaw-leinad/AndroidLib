/*
 * AndroidController.cs - Handles communication between computer and Android devices
 * Developed by Dan Wager for AndroidLib.dll - 04/12/12
 */

using System;

namespace RegawMOD.Android {

    using System.Collections.Generic;
    using System.IO;
    using System.Management;
    using System.Threading;

    /// <summary>
    /// Controls communication to and from connected Android devices.  Use only one instance for the entire project.
    /// </summary>
    /// <remarks>
    /// <para><see cref="AndroidController"/> is the core class in AndroidLib. You must always call the <c>Dispose()</c> method when finished before program exits.</para>
    /// <para><see cref="AndroidController"/> specifically controls the Android Debug Bridge Server, and a developer should NEVER try to start/kill the server using an <see cref="AdbCommand"/></para>
    /// </remarks>
    /// <example>The following example shows how you can use the <c>AndroidController</c> class
    /// <code>
    /// // This example demonstrates using AndroidController, and writing the first connected Android device's serial number to the console
    /// using System;
    /// using RegawMOD.Android;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         AndroidController android = AndroidController.Instance;
    ///         Device device;
    ///         string serialNumber;
    ///         
    ///         Console.WriteLine("Waiting For Device...");
    ///
    ///         // This will wait until a device is connected to the computer
    ///         // Should ONLY be used in Console applications though, as it freezes WinForm apps
    ///         android.WaitForDevice();
    ///
    ///         // Gets first serial number of Device in collection
    ///         serialNumber = android.ConnectedDevices[0];
    ///
    ///         // New way to set 'device' to the first Device in the collection
    ///         device = android.GetConnectedDevice(serialNumber);
    ///
    ///         Console.WriteLine("Connected Device - {0}", device.SerialNumber);
    ///         
    ///         android.Dispose();
    ///     }
    /// }
    /// 
    ///	// The example displays the following output:
    ///	//		Waiting For Device...
    ///	//		Connected Device - {serial # here}
    /// </code>
    /// </example>
    public sealed class AndroidController {

        /// <summary>
        /// Occurs when a new Android device is added to the local machine.
        /// </summary>
        /// <example>
        /// Example use of this event:
        /// Please note that these example are not recommended for WPF.
        /// For WPF, please consider the MVVM pattern.
        /// 
        /// <code language="C#" title="C# Example (Lambda)">
        /// public class FooBar {
        ///         
        ///     private readonly AndroidController m_adbController;
        ///     
        ///     public FooBar() {
        ///         m_adbController = AndroidController.Instance;
        ///         m_adbController.MonitorUSB = true; // If you set this to <c>false</c>, USB ports will not be monitored, thus, events will not be fired.
        ///         m_adbController.OnDeviceAdded += (s, evt) => {
        ///             MessageBox.Show(string.Format("A new device was added!\nSerial: {0}", evt.Device.Serial));
        ///             // Do more
        ///         };
        ///     }
        /// }
        /// </code>
        /// 
        /// <code language="C#" title="C# Example (Conventional)">
        /// public class FooBar {
        ///     
        ///     private readonly AndroidController m_adbController;
        ///     
        ///     public FooBar() {
        ///         m_adbController = AndroidController.Instance;
        ///         m_adbController.MonitorUSB = true; // If you set this to <c>false</c>, USB ports will not be monitored, thus, events will not be fired.
        ///         m_adbController.OnDeviceAdded += adbController_OnDeviceAdded;
        ///     }
        ///     
        ///     private void adbController_OnDeviceAdded(object m_sender, OnDeviceAddedEventArgs m_eventArgs) {
        ///         MessageBox.Show(string.Format("A new device was added!\nSerial: {0}", evt.Device.Serial));
        ///         // Do more
        ///     }
        /// }
        /// </code>
        /// 
        /// <code language="Visual Basic" title="Visual Basic Example (Lambda)">
        /// Public Class FooBar
        /// 
        ///     Private ReadOnly m_adbController As AndroidController
        ///     
        ///     Public Sub New()
        ///         m_adbController = AndroidController.Instance
        ///         m_adbController.MonitorUSB = True ' If you set this to <c>False</c>, USB ports will not be monitored, thus, events will not be fired.
        ///         m_adbController.OnDeviceAdded += Function(s, evt)
        ///                                             MessageBox.Show(String.Format("A new device was added!{0}Serial: {1}", vbNewLine, evt.Device.Serial))
        ///                                             ' Do more
        ///                                          End Function
        ///     End Sub
        /// 
        /// End Class
        /// </code>
        /// 
        /// <code language="Visual Basic" title="Visual Basic Example (Conventional)">
        /// Public Class FooBar
        /// 
        ///     Private ReadOnly m_adbController As AndroidController
        ///     
        ///     Public Sub New()
        ///         m_adbController = AndroidController.Instance
        ///         m_adbController.MonitorUSB = True ' If you set this to <c>False</c>, USB ports will not be monitored, thus, events will not be fired.
        ///     End Sub
        ///     
        ///     Private Sub m_adbController_OnDeviceAdded(Object m_sender, OnDeviceAddedEventArgs m_eventArgs) Handles m_adbController.OnDeviceAdded
        ///         MessageBox.Show(String.Format("A new device was added!{0}Serial: {1}", vbNewLine, evt.Device.Serial))
        ///         ' Do more
        ///     End Sub
        /// 
        /// End Class
        /// </code>
        /// </example>
        public event DeviceAddedEventHandler OnDeviceAdded;
        public event DeviceRemovedEventHandler OnDeviceRemoved;

#region Static variables
        private const string ANDROID_CONTROLLER_TMP_FOLDER = "AndroidLib\\";
        private static readonly Dictionary<string, string> RESOURCES = new Dictionary<string, string>
        {
            {"adb.exe","862c2b75b223e3e8aafeb20fe882a602"},
            {"AdbWinApi.dll", "47a6ee3f186b2c2f5057028906bac0c6"},
            {"AdbWinUsbApi.dll", "5f23f2f936bdfac90bb0a4970ad365cf"},
            {"fastboot.exe", "35792abb2cafdf2e6844b61e993056e2"},
        };

        private static AndroidController instance;
#endregion

#region Instance variables
        private string resourceDirectory;
        private List<string> connectedDevices;
        private bool Extract_Resources = false;
        private ManagementEventWatcher m_eventWatcher;
        private bool m_monitorUSB = true;
#endregion

        /// <summary>
        /// Gets the current AndroidController Instance.
        /// </summary>
        public static AndroidController Instance {
            get {
                if (instance == null) {
                    instance = new AndroidController();
                    instance.CreateResourceDirectories();
                    instance.ExtractResources();
                    Adb.StartServer();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets a <c>List&lt;string&gt;</c> object containing the serial numbers of all connected Android devices
        /// </summary>
        public List<string> ConnectedDevices {
            get {
                this.UpdateDeviceList();
                return this.connectedDevices;
            }
        }

        internal string ResourceDirectory {
            get { return this.resourceDirectory; }
        }

        private AndroidController() {
            this.connectedDevices = new List<string>();
            ResourceFolderManager.Register(ANDROID_CONTROLLER_TMP_FOLDER);
            this.resourceDirectory = ResourceFolderManager.GetRegisteredFolderPath(ANDROID_CONTROLLER_TMP_FOLDER);
            this.m_eventWatcher = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2")
            );
            this.m_eventWatcher.EventArrived += (s, evt) => {
                var m_oldConnectedDevices = connectedDevices;
                var m_newConnectedDevices = ConnectedDevices;

                if (m_oldConnectedDevices.Count != m_newConnectedDevices.Count) {
                    // List sizes do not match.
                    // Depending on whether the new list is longer or short, fire the corresponding event.
                    if (m_newConnectedDevices.Count > m_oldConnectedDevices.Count) {
                        // A new device was added. 
                        // Find out which device it was.
                        foreach (string m_device in m_newConnectedDevices)
                            if (!m_oldConnectedDevices.Contains(m_device))
                                // We've found the device that was added.
                                // We know this because the old list doesn't contain this device's serial.
                                // Now fire OnDeviceAdded event(s).
                                foreach (DeviceAddedEventHandler m_handler in OnDeviceAdded.GetInvocationList())
                                    m_handler(
                                        this,
                                        new OnDeviceAddedEventArgs(
                                            "A new device was added to the local machine.",
                                            GetConnectedDevice(m_device)
                                        )
                                    );
                    } else {
                        // A device was removed.
                        // Find out which one it was.
                        foreach (string m_device in m_oldConnectedDevices)
                            if (!m_newConnectedDevices.Contains(m_device))
                                // We've found the device that was removed.
                                // We know this because the new list doesn't contain the device's serial.
                                // Now fire OnDeviceRemoved event(s).
                                foreach (DeviceRemovedEventHandler m_handler in OnDeviceRemoved.GetInvocationList())
                                    m_handler(
                                        this,
                                        new OnDeviceRemovedEventArgs(
                                            "A device was removed from the local machine.",
                                            m_device
                                        )
                                    );
                    }
                } else return; // Nothing to do here, so don't bother.

            };
            if (m_monitorUSB) m_eventWatcher.Start();
        }

        private void CreateResourceDirectories() {
            try {
                if (!Adb.ExecuteAdbCommand(new AdbCommand("version")).Contains(Adb.ADB_VERSION)) {
                    Adb.KillServer();
                    Thread.Sleep(1000);
                    ResourceFolderManager.Unregister(ANDROID_CONTROLLER_TMP_FOLDER);
                    Extract_Resources = true;
                }
            } catch (Exception) {
                Extract_Resources = true;
            }
            ResourceFolderManager.Register(ANDROID_CONTROLLER_TMP_FOLDER);
        }

        private void ExtractResources() {
            if (this.Extract_Resources) {
                string[] res = new string[RESOURCES.Count];
                RESOURCES.Keys.CopyTo(res, 0);
                Extract.Resources(this, this.resourceDirectory, "Resources.AndroidController", res);
            }
        }

        /// <summary>
        /// Releases all resources used by <see cref="AndroidController"/>        
        /// </summary>
        /// <remarks>Needs to be called when application has finished using <see cref="AndroidController"/></remarks>
        public void Dispose() {
            if (Adb.ServerRunning) {
                Adb.KillServer();
                Thread.Sleep(1000);
            }
            AndroidController.instance = null;
        }

        /// <summary>
        /// Gets the first <see cref="Device"/> in the internal collection of devices controlled by <see cref="AndroidController"/>
        /// </summary>
        /// <returns><see cref="Device"/> containing info about the device with the first serial number in the internal collection</returns>
        public Device GetConnectedDevice() {
            if (this.HasConnectedDevices)
                return new Device(this.connectedDevices[0]);

            return null;
        }

        /// <summary>
        /// Gets a <see cref="Device"/> containing data about a specified Android device.
        /// </summary>
        /// <remarks><paramref name="deviceSerial"/> must be a serial number of a connected device, or the method returns null</remarks>
        /// <param name="deviceSerial">Serial number of connected device</param>
        /// <returns><see cref="Device"/> containing info about the device with the serial number <paramref name="deviceSerial"/></returns>
        public Device GetConnectedDevice(string deviceSerial) {
            this.UpdateDeviceList();

            if (this.connectedDevices.Contains(deviceSerial))
                return new Device(deviceSerial);

            return null;
        }

        /// <summary>
        /// Gets a value indicating if there are any Android devices currently connected
        /// </summary>
        public bool HasConnectedDevices {
            get { this.UpdateDeviceList(); return (this.connectedDevices.Count > 0) ? true : false; }
        }

        /// <summary>
        /// Determines if the Android device with the serial number provided is currently connected
        /// </summary>
        /// <example>The following example shows how to use <c>IsDeviceConnected(string deviceSerial)</c> in one of your programs
        /// <code>
        /// //This example demonstrates how to use IsDeviceConnected(string deviceSerial) in your project
        /// //This example assumes there is an instance of AndroidController running named android.
        /// 
        /// string serialNumber = "HTC123456789";
        /// 
        /// bool currentlyConnected = android.IsDeviceConnected(serialNumber);
        /// </code>
        /// </example>
        /// <param name="deviceSerial">Serial number of Android device</param>
        /// <returns>A value indicating if the Android device with the serial number <paramref name="deviceSerial"/> is connected</returns>
        public bool IsDeviceConnected(string deviceSerial) {
            this.UpdateDeviceList();

            foreach (string s in this.connectedDevices)
                if (s.ToLower() == deviceSerial.ToLower())
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if the Android device tied to <paramref name="device"/> is currently connected
        /// </summary>
        /// <param name="device">Instance of <see cref="Device"/></param>
        /// <returns>A value indicating if the Android device indicated in <paramref name="device"/> is connected</returns>
        public bool IsDeviceConnected(Device device) {
            this.UpdateDeviceList();

            foreach (string d in this.connectedDevices)
                if (d == device.SerialNumber)
                    return true;

            return false;
        }

        /// <summary>
        /// Updates Internal Device List
        /// </summary>
        /// <remarks>Call this before checking for Devices, or setting a new Device, for most updated results</remarks>
        public void UpdateDeviceList() {
            string deviceList = "";

            this.connectedDevices.Clear();

            deviceList = Adb.Devices();
            if (deviceList.Length > 29) {
                using (StringReader s = new StringReader(deviceList)) {
                    string line;

                    while (s.Peek() != -1) {
                        line = s.ReadLine();

                        if (line.StartsWith("List") || line.StartsWith("\r\n") || line.Trim() == "")
                            continue;

                        if (line.IndexOf('\t') != -1) {
                            line = line.Substring(0, line.IndexOf('\t'));
                            this.connectedDevices.Add(line);
                        }
                    }
                }
            }

            deviceList = Fastboot.Devices();
            if (deviceList.Length > 0) {
                using (StringReader s = new StringReader(deviceList)) {
                    string line;

                    while (s.Peek() != -1) {
                        line = s.ReadLine();

                        if (line.StartsWith("List") || line.StartsWith("\r\n") || line.Trim() == "")
                            continue;

                        if (line.IndexOf('\t') != -1) {
                            line = line.Substring(0, line.IndexOf('\t'));
                            this.connectedDevices.Add(line);
                        }
                    }
                }
            }
        }

        private bool _CancelRequest;
        /// <summary>
        /// Set to true to cancel a WaitForDevice() method call
        /// </summary>
        public bool CancelWait {
            get { return _CancelRequest; }
            set { _CancelRequest = value; }
        }

        /// <summary>
        /// Pauses thread until 1 or more Android devices are connected
        /// </summary>
        /// <remarks>Do Not Use in Windows Forms applications, as this method pauses the current thread.  Works fine in Console Applications</remarks>
        public void WaitForDevice() {
            /* Entering an endless loop will exhaust CPU. 
             * Since this method must be called in a child thread in Windows Presentation Foundation (WPF) or Windows Form Apps,
             * sleeping thread for 250 miliSecond (1/4 of a second)
             * will be more friendly to the CPU. Nonetheless checking 4 times for a connected device in each second is more than enough,
             * and will not result in late response from the app if a device gets connected. 
             */
            while (!this.HasConnectedDevices && !this.CancelWait) {
                Thread.Sleep(250);
            }

            this.CancelWait = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this class shall monitor the USB (bus) and automatically check for new devices.
        /// </summary>
        /// <value>
        ///   If <c>true</c> then the object will monitor the USB (bus) and will fire events accordingly, when a device is added or removed.
        ///   If <c>false</c> then the object will cease monitoring the USB (bus) and will not fire events when a device is added or removed.
        /// </value>
        public bool MonitorUSB {
            get { return m_monitorUSB; }
            set {
                m_monitorUSB = value;
                if (value)
                    m_eventWatcher.Start();
                else m_eventWatcher.Stop();
            }
        }


    }
}
