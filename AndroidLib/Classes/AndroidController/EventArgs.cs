/*
 * EventArgs.cs     Developed by Simon C. for AndroidLib.
 */

namespace RegawMOD.Android {

    using System;

    /// <summary>
    /// The event arguments provided when a new device is connected to the local machine.
    /// </summary>
    public class OnDeviceAddedEventArgs: EventArgs {

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; internal set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public Device Device { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnDeviceAddedEventArgs"/> class.
        /// </summary>
        /// <param name="m_message">The m_message.</param>
        /// <param name="m_deviceSerial">The m_device serial.</param>
        public OnDeviceAddedEventArgs(string m_message, string m_deviceSerial) {
            this.Message = m_message;
            this.Device = AndroidController.Instance.GetConnectedDevice(m_deviceSerial);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnDeviceAddedEventArgs"/> class.
        /// </summary>
        /// <param name="m_message">The m_message.</param>
        /// <param name="m_device">The m_device.</param>
        public OnDeviceAddedEventArgs(string m_message, Device m_device) {
            this.Message = m_message;
            this.Device = m_device;
        }

    }

    /// <summary>
    /// The event arguments provided when a device is removed from the local machine.
    /// </summary>
    public class OnDeviceRemovedEventArgs: EventArgs {

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; internal set; }

        /// <summary>
        /// Gets the device serial.
        /// </summary>
        /// <value>
        /// The device serial.
        /// </value>
        public string DeviceSerial { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnDeviceRemovedEventArgs"/> class.
        /// </summary>
        /// <param name="m_message">The m_message.</param>
        /// <param name="m_deviceSerial">The m_device serial.</param>
        public OnDeviceRemovedEventArgs(string m_message, string m_deviceSerial) {
            this.Message = m_message;
            this.DeviceSerial = m_deviceSerial;
        }

    }

}