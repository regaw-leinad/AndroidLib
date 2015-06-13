/*
 * Delegates.cs     Developed by Simon C. for AndroidLib
 */

namespace RegawMOD.Android {

    using System;

    /// <summary>
    /// The event handler that is executed whenever a device is connected to the local machine.
    /// </summary>
    /// <param name="m_sender">The m_sender.</param>
    /// <param name="m_eventArgs">The <see cref="OnDeviceAddedEventArgs"/> instance containing the event data.</param>
    public delegate void DeviceAddedEventHandler(object m_sender, OnDeviceAddedEventArgs m_eventArgs);

    /// <summary>
    /// The event handler that is executed whenever a device is removed from the local machine.
    /// </summary>
    /// <param name="m_sender">The m_sender.</param>
    /// <param name="m_eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
    public delegate void DeviceRemovedEventHandler(object m_sender, OnDeviceRemovedEventArgs m_eventArgs);

    /// <summary>
    /// The event handler that is executed whenever the battery level of a connected device changes.
    /// 
    /// RegawMOD.Android.BatteryLevelChangedEventHandler
    /// Created by: Beatsleigher
    /// At:               08.06.2015, 11:07
    /// On:              BEATSLEIGHER-PC
    /// 
    /// </summary>
    /// <param name="m_sender">The m_sender.</param>
    /// <param name="m_eventArgs">The <see cref="OnBatteryInfoChangedEventArgs"/> instance containing the event data.</param>
    public delegate void BatteryInfoChangedEventHandler(object m_sender, OnBatteryInfoChangedEventArgs m_eventArgs);

}