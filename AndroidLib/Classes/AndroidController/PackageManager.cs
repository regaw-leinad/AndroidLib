/*
 * PackageManager.cs        Developed by Simon C. for AndroidLib.
 * © Simon Cahill 2015
 * Date created: 24.03.2015 01:31
 * Date draft finished: 30.03.2015 00:15
 */

using System;

namespace RegawMOD.Android {

    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <c>PackageManager</c> class is a wrapper class for Android's pm executable.
    /// This class provides methods and functions for interacting with the pm executable.
    /// This class is inheritable.
    /// </summary>
    /// <remarks>
    /// Add remarks here. Use following format:
    /// {Date}, {Name}: {Changes/Remarks}
    /// </remarks>
    /// <example>
    /// Example use of this class:
    /// C#:
    /// <code>
    /// public class MyClass {
    /// private readonly AndroidController m_adbController;
    /// private PackageManager m_pkgManager;
    /// public MyClass() { this.m_adbController = AndroidController.Instance; Init(); }
    /// private void Init() {
    /// var m_device = m_adbController.GetConnectedDevice(m_adbController.ConnectedDevices[number]);
    /// m_pgkManager = m_device.PackageManager;
    /// }
    /// // Do something with the <c>PackageManager</c> class.
    /// }
    /// </code>
    /// Visual Basic:
    /// <code>
    /// Public Class MyClass
    /// Private ReadOnly m_adbController As AndroidController
    /// Private ReadOnly m_pkgManager As PackageManager
    /// Public Sub New()
    /// Me.m_adbController = AndroidController.Instance
    /// Init()
    /// End Sub
    /// Private Sub Init()
    /// Dim m_device As Device = m_adbController.GetConnectedDevice(m_adbController.ConnectedDevices(number))
    /// Me.m_pkgManager = m_device.PackageManager
    /// End Sub
    /// ' Do something with the <c>PackageManager</c> class.
    /// End Class
    /// </code></example>
    public class PackageManager {

        #region Class variables
        private readonly Device m_device;
        #endregion

        #region Enumerations
        /// <summary>
        /// An enum contaning options for the <see cref="RegawMOD.Android.PackageManager.ListPackages"/> method.
        /// </summary>
        public enum ListPackagesOption { 
            /// <summary>
            /// Includes the associated file of the package.
            /// E.g.: /data/app/myapp/com.myapp.apk
            /// </summary>
            AssociatedFiles, // -f
            /// <summary>
            /// Includes the uninstalled packages to the list.
            /// </summary>
            IncludeUninstalled, // -u
            /// <summary>
            /// Includes the installer of the package.
            /// The installer is the app -- or user -- who installed the package.
            /// E.g.: com.android.vending
            /// </summary>
            Installers,  // -i
            /// <summary>
            /// No options are specified, only package names are listed.
            /// </summary>
            NoOption
        }

        /// <summary>
        /// An enum containing different filters for the <see cref="RegawMOD.Android.PackageManager.ListPackages"/> method.
        /// </summary>
        public enum ListPackagesFilter {
            /// <summary>
            /// List only enabled packages.
            /// </summary>
            EnabledPackages,    // -e
            /// <summary>
            /// List only disabled packages.
            /// </summary>
            DisabledPackages,   // -d
            /// <summary>
            /// Do not apply a filter.
            /// </summary>
            NoFilter,
            /// <summary>
            /// List only system packages.
            /// </summary>
            SystemPackages,     // -s
            /// <summary>
            /// List only third party packages.
            /// </summary>
            ThirdPartyPackages  // -3
        }

        /// <summary>
        /// An enum containing options for listing permissions using the <see cref="RegawMOD.Android.PackageManager.ListPermissions"/> method.
        /// </summary>
        public enum ListPermissionOption {
            /// <summary>
            /// Gets all the information available.
            /// </summary>
            AllInfo, // -f
            /// <summary>
            /// Organizes the permissions by the groups they're in.
            /// </summary>
            ByGroup, // -g
            /// <summary>
            /// Lists only dangerous permissions.
            /// </summary>
            DangerousOnly, // -d
            /// <summary>
            /// No options or filters are applied. Will print out only permissions.
            /// (Will also nullify ze group if applicable).
            /// </summary>
            NoOptions,
            /// <summary>
            /// Lists only permissions that are visible to the users.
            /// </summary>
            UserVisibleOnly // -u
        }

        /// <summary>
        /// This enum contains most of the options for the package manager's install command.
        /// </summary>
        public enum InstallOption {
            /// <summary>
            /// Allows version code downgrade.
            /// (Older apps can replace newer ones.)
            /// </summary>
            AllowDowngrade, // -d
            /// <summary>
            /// Allows text APKs to be installed.
            /// </summary>
            AllowTestApks, // -t
            /// <summary>
            /// Installs the application with a forward lock.
            /// </summary>
            ForwardLock, // -l
            /// <summary>
            /// Installs the application to the system's internal storage.
            /// </summary>
            InternalStorage, // -f
            /// <summary>
            /// Reinstalls the app, keeping its data.
            /// </summary>
            ReinstallKeepData, // -r
            /// <summary>
            /// Installs the application to the system's shared mass storage device (such as an SD card).
            /// </summary>
            SharedMassStorage, // -s
        }
        #endregion

        /// <summary>
        /// Creates a new instance of this class.
        /// Is only visible within the RegawMOD.Android namespace of the library.
        /// </summary>
        /// <param name="m_device">The device associated with this class.</param>
        internal PackageManager(Device m_device) { this.m_device = m_device; }

        /// <summary>
        ///     Queries for and lists all packages on the associated device (<see cref="RegawMOD.Android.PackageManager.m_device"/>).
        /// </summary>
        /// <param name="m_listOps">
        ///     One or more listing options. <seealso cref="RegawMOD.Android.PackageManager.ListPackagesOption"/>
        /// </param>
        /// <param name="m_filter">
        ///     The filter for the list. <seealso cref="RegawMOD.Android.PackageManager.ListPackagesFilter"/>
        /// </param>
        /// <returns>
        ///     Returns a List containing <see cref="RegawMod.Android.Package"/> objects representing the packages on the device.
        /// </returns>
        public List<Package> ListPackages(ListPackagesFilter m_filter = ListPackagesFilter.NoFilter, params ListPackagesOption[] m_listOps) {
            var pkgList = new List<Package>();
            var options = "";
            var filter = " ";
            var noOptionsRegEx = new Regex(@"(package:)([a-z]{1,}\.?){1,}", RegexOptions.IgnoreCase);
            var associatedFilesRegEx = new Regex(@"(package:)([^=]){1,}=(([a-z]{1,}\.?){1,})", RegexOptions.IgnoreCase);
            var installersRegEx = new Regex(@"(package:)([a-z]{1,}\.?){1,}(\s){1,}(installer=)([a-z]{1,}\.?){1,}", RegexOptions.IgnoreCase);
            var filesAndInstallersRegEx = new Regex(@"(package:)([^=]){1,}=(([a-z]{1,}\.?){1,})(\s){1,}(installer=)([a-z]{1,}\.?){1,}", RegexOptions.IgnoreCase);

#region Loops
            foreach (ListPackagesOption lstOpt in m_listOps) {
                switch (lstOpt) {
                    case ListPackagesOption.AssociatedFiles:
                        options = string.Format(options + " {0}", "-f");
                        break;
                    case ListPackagesOption.IncludeUninstalled:
                        options = string.Format(options + " {0}", "-u");
                        break;
                    case ListPackagesOption.Installers:
                        options = string.Format(options + " {0}", "-i");
                        break;
                    case ListPackagesOption.NoOption:
                        options = "";
                        break;
                }

            }

            switch (m_filter) {
                case ListPackagesFilter.DisabledPackages:
                    filter = "-e";
                    break;
                case ListPackagesFilter.EnabledPackages:
                    filter = "-d";
                    break;
                case ListPackagesFilter.NoFilter: break;
                case ListPackagesFilter.SystemPackages:
                    filter = "-s";
                    break;
                case ListPackagesFilter.ThirdPartyPackages:
                    filter = "-3";
                    break;
            }
#endregion

            using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", Regex.Split(string.Concat(options, filter), @"\s"))))) {
                var line = "";
                while (m_reader.Peek() != -1) {
                    line = m_reader.ReadLine();

                    if (noOptionsRegEx.IsMatch(line)) {
#region
                        // package:org.telegram.messenger
                        pkgList.Add(
                            new Package(Regex.Split(line, ":")[1])
                        );
#endregion
                    } else if (associatedFilesRegEx.IsMatch(line)) {
#region
                        // package:/data/app/org.telegram.messenger-2/base.apk=org.telegram.messenger
                        pkgList.Add(
                            new Package(
                                Regex.Split(line, "=")[1],
                                Regex.Split(
                                    Regex.Split(line, ":")[1], 
                                    "="
                                )[0]
                            )
                        );
#endregion
                    } else if (installersRegEx.IsMatch(line)) {
#region
                        // package:org.telegram.messenger  installer=com.android.vending
                        pkgList.Add(
                            new Package(
                                Regex.Split(
                                    Regex.Split(line, @"\s{1,}")[0], 
                                    @":"
                                )[1],
                                "", // No file path was specified, as this lists only package and installer.
                                Regex.Split(line, "installer=")[1]
                            )
                        );
#endregion
                    } else if (filesAndInstallersRegEx.IsMatch(line)) {
#region
                        // package:/data/app/org.telegram.messenger-2/base.apk=org.telegram.messenger  installer=com.android.vending
                        pkgList.Add(
                            new Package(
                                Regex.Split(
                                    Regex.Split(line, @"\s{1,}")[0],
                                    "="
                                )[1],
                                Regex.Split(
                                    Regex.Split(line, "=")[0],
                                    ":"
                                )[1],
                                Regex.Split(line, "installer=")[1]
                            )
                        );
#endregion
                    } else continue;
                }
            }

            return pkgList;
        }

        /// <summary>
        /// Gets a <see cref="System.Collections.Generic.List<PermissionGroup>"/> contaning all the permission groups on the associated Android device.
        /// </summary>
        /// <returns></returns>
        public List<PermissionGroup> ListPermissionGroups() {
            var m_permGroupList = new List<PermissionGroup>();
            var m_permGroupRegex = new Regex(@"(permission)\s{1,}?(group:)([\s\S]{1,})?", RegexOptions.IgnoreCase);

            using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permission-groups")))) {
                var m_line = "";
                while (m_reader.Peek() != -1) {
                    m_line = m_reader.ReadLine();
                    if (m_permGroupRegex.IsMatch(m_line))
                        m_permGroupList.Add(new PermissionGroup(Regex.Split(m_line, ":")[1]));
                }
                m_reader.Close();
            }

            return m_permGroupList;
        }

        /// <summary>
        /// Gets a <see cref="System.Collections.Generic.List"/> containing all the permissions on the associated Android device.
        /// </summary>
        /// <param name="m_listOptions">
        /// The option(s) to use when listing permissions.
        /// Default: NoOptions.
        /// If this option is set to NoOptions, <paramref name="m_group"/> is set to null!
        /// </param>
        /// <param name="m_group">
        /// 
        /// </param>
        /// <returns></returns>
        public List<Permission> ListPermissions(ListPermissionOption m_listOptions = ListPermissionOption.NoOptions, PermissionGroup m_group = null) {
            var m_permList = new List<Permission>();
            var m_permissionRegex = new Regex(@"(\+\s{1,})?(permission:)([a-z]{2,}.){2,}([A-Z_]{1,})"); // Matches permissions
            var m_groupRegex = new Regex(@"(group:)([a-z-]{2,}\.){2,}([A-Z_]{1,})"); // Matches permission groups
            var m_packageRegex = new Regex(@"(package:)([a-z_-]{2,}\.?){2,}", RegexOptions.IgnoreCase); // Matches packages.
            var m_labelRegex = new Regex(@"(label:)[^\r\n]{1,}", RegexOptions.IgnoreCase); // Matches labels
            var m_descriptionRegex = new Regex(@"(description:)[^\r\n]{1,}", RegexOptions.IgnoreCase); // Matches descriptions
            var m_protectionLevelRegex = new Regex(@"(protectionlevel:)(dangerous|normal|signature|signatureOrSystem)", RegexOptions.IgnoreCase);

            // Nullify m_group if no options (otherwise it would cause an error)
            if (m_listOptions == ListPermissionOption.NoOptions)
                m_group = null;

            switch (m_listOptions) {
                case ListPermissionOption.AllInfo:
#region
                    using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permissions", "-f")))) {
                        var m_line = "";
                        while (m_reader.Peek() != -1) {
                            m_line = m_reader.ReadLine();

                        Match:
                            if (m_permissionRegex.IsMatch(m_line)) {
                                var permission = new Permission(Regex.Split(m_line, ":")[1]);
                                while (!m_permissionRegex.IsMatch(m_line = m_reader.ReadLine())) {
                                    if (m_packageRegex.IsMatch(m_line)) {
                                        permission.PermissionPackage = Regex.Split(m_line, ":")[1];
                                        continue;
                                    } else if (m_labelRegex.IsMatch(m_line)) {
                                        permission.PermissionLabel = Regex.Split(m_line, ":")[1];
                                        continue;
                                    } else if (m_descriptionRegex.IsMatch(m_line)) {
                                        permission.PermissionDescription = Regex.Split(m_line, ":")[1];
                                        continue;
                                    } else if (m_protectionLevelRegex.IsMatch(m_line)) {
                                        switch (Regex.Split(m_line, ":")[1].ToLower()) {
                                            case "dangerous": permission.PermissionProtectionLevel = Permission.ProtectionLevel.Dangerous; break;
                                            case "normal": permission.PermissionProtectionLevel = Permission.ProtectionLevel.Normal; break;
                                            case "signature": permission.PermissionProtectionLevel = Permission.ProtectionLevel.Signature; break;
                                            case "signatureorsystem": permission.PermissionProtectionLevel = Permission.ProtectionLevel.SignatureOrSystem; break;
                                            default: permission.PermissionProtectionLevel = Permission.ProtectionLevel.Normal; break;
                                        }
                                    } else continue;
                                }

                                // Loop has ended. Add Permission object to list.
                                m_permList.Add(permission);
                                goto Match;
                            } 
                        }
                        m_reader.Close();
                    }
#endregion
                    return m_permList;
                case ListPermissionOption.ByGroup:
#region
                    using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permissions", "-g")))) {
                        var m_line = "";
                        while (m_reader.Peek() != -1) {
                            m_line = m_reader.ReadLine();
                        Match:
                            if (m_groupRegex.IsMatch(m_line)) {
                                var m_group1 = Regex.Split(m_line, ":")[1];
                                while (!m_groupRegex.IsMatch(m_line = m_reader.ReadLine())) {
                                    if (string.IsNullOrEmpty(m_line) || string.IsNullOrWhiteSpace(m_line))
                                        continue;
                                    if (m_permissionRegex.IsMatch(m_line)) {
                                        m_permList.Add(new Permission(Regex.Split(m_line, ":")[1], m_group1));
                                    }
                                }
                                goto Match;
                            }
                        }
                        m_reader.Close();
                    }
#endregion
                    return m_permList;
                case ListPermissionOption.DangerousOnly:
#region
                    using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permissions", "-d")))) {
                        var m_line = "";
                        while (m_reader.Peek() != -1) {
                            m_line = m_reader.ReadLine();
                            if (m_permissionRegex.IsMatch(m_line))
                                m_permList.Add(new Permission(Regex.Split(m_line, ":")[1]));
                        }
                        m_reader.Close();
                    }
#endregion
                    return m_permList;
                case ListPermissionOption.NoOptions:
#region
                    using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permissions")))) {
                        var m_line = "";
                        while (m_reader.Peek() != -1) {
                            m_line = m_reader.ReadLine();
                            if (m_permissionRegex.IsMatch(m_line))
                                m_permList.Add(new Permission(Regex.Split(m_line, ":")[1]));
                        }
                        m_reader.Close();
                    }
#endregion
                    return m_permList;
                case ListPermissionOption.UserVisibleOnly:
#region
                    using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "permissions", "-d")))) {
                        var m_line = "";
                        while (m_reader.Peek() != -1) {
                            m_line = m_reader.ReadLine();
                            if (m_permissionRegex.IsMatch(m_line))
                                m_permList.Add(new Permission(Regex.Split(m_line, ":")[1]));
                        }
                        m_reader.Close();
                    }
#endregion
                    return m_permList;
                default: throw new Exception();
            }
        }

        /// <summary>
        /// Gets and returns a list of features on the associated <see cref="RegawMOD.Android.Device"/>
        /// </summary>
        /// <returns></returns>
        public List<Feature> ListFeatures() {
            var m_featureList = new List<Feature>();

            using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "features")))) {
                var m_line = "";
                while (m_reader.Peek() != -1) {
                    m_line = m_reader.ReadLine();

                    if (Regex.IsMatch(m_line, @"feature:[^\r\n]", RegexOptions.IgnoreCase))
                        m_featureList.Add(new Feature(Regex.Split(m_line, ":")[1]));
                }
            }

            return m_featureList;
        }

        /// <summary>
        /// Gets and returns a list of all the libraries the associated device supports.
        /// </summary>
        /// <returns></returns>
        public List<string> ListLibraries() {
            var m_list = new List<string>();

            using (StringReader m_reader = new StringReader(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "list", "libraries")))) {
                var m_line = "";

                while (m_reader.Peek() != -1) {
                    m_line = m_reader.ReadLine();
                    m_list.Add(Regex.Split(m_line, ":")[1]);
                }
                m_reader.Close();
            }

            return m_list;
        }

        /*
         * To-Do: Add user listing.
         * Find out more about user listing syntax first.
         */

        /// <summary>
        /// Gets and returns the path of a given package.
        /// E.g:
        /// Package: org.telegram.messenger
        /// Path: /data/app/org.telegram.messenger-2/base.apk
        /// </summary>
        /// <param name="m_package"></param>
        /// <returns></returns>
        public string GetPackagePath(string m_package) {
            return Regex.Split(Adb.ExecuteAdbCommand(Adb.FormAdbShellCommand(m_device, false, "pm", "path", m_package)), ":")[1];
        }

        /// <summary>
        /// Installs an application file (*.apk) that is on the device (internal memory or SD card).
        /// The application can be installed using multiple parameters provided by this method.
        /// It is recommended you query for the file on the device before calling this method.
        /// </summary>
        /// <param name="m_apkPath">
        /// The path to the (*.apk) on the device's shared mass storage. (SD/eMMc)
        /// If this parameter is set to null (Visual Basic: Nothing) then this method will throw an ArgumentNullException.
        /// If this parameter is does not match a package pattern (e.g.: com.myapp.application) then this method will throw an ArgumentException.
        /// Be sure to use try-catch blocks for this method, as this method expects to be terminated after an exception occurs.
        /// </param>
        /// <param name="m_installerPackage">
        /// Optional: Specify the application's installer package (e.g.: com.android.vending)
        /// This parameter is set to null be default.
        /// </param>
        /// <param name="m_installOptions">
        /// Optional: The options and parameters used to install the application.
        /// These parameters are optional and failure to provide a sufficient amount of cash to this variable will
        /// result in the immediate destruction of your planet, you puny ape!
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// The method will throw this exception if the <paramref name="m_apkPath"/> 
        /// parameter is set to null (Visual Basic: Nothing), is empty or plain whitespace.
        /// In those cases, the exception is thrown. It is recommended that you use a try-catch block for this method, 
        /// as it expects to be terminated after an exception has occurred.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The method will throw this exception if the <paramref name="m_apkPath"/> parameter does not match
        /// the pattern for application packages. (e.g.: com.myapp.application).
        /// The case does not matter, this method uses case-insensitive regular expressions.
        /// It is recommended you use a try-catch block for this method, as it expects to be terminated after an exception has occurred.
        /// </exception>
        public void InstallPackage(string m_apkPath, string m_installerPackage = null, params InstallOption[] m_installOptions) {
            // Check if (*.APK) is empty or not. If it is null or empty, throw exception and terminate method.
            if (string.IsNullOrEmpty(m_apkPath) || string.IsNullOrWhiteSpace(m_apkPath)) 
                throw new ArgumentNullException("The APK path cannot be null!");

            //////////////////////////////////////////////////////////////////////////////
            // Check if m_apkPath matches a Linux directory.                            //
            // Regex for checking Linux filepaths:                                      //
            // \/(([^\r\n]{1,}\/){1,})?([A-z0-9_-]{1,}\.apk)                            //
            // If so, continue with method, else throw exception and terminate method.  //
            // If m_apkPath is a go, check if file exists on device memory.             //
            // If so, continue with install, else throw exception and terminate method. //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (!Regex.IsMatch(m_apkPath, @"\/(([^\r\n]{1,}\/){1,})?([A-z0-9_-]{1,}\.apk)"))                                                //
                throw new ArgumentException(                                                                                                //
                    string.Format(                                                                                                          //
                          "The argument {0} does not match a Linux directory structure!\n"                                                  //
                        + "Please make sure that you have provided a file path from the device. (Serial: {1}, Model: {2})\n"                //
                        + "Provided argument: {3}\n"                                                                                        //
                        + "Please make sure the path matches this regular expression: {4}",                                                 //
                        "m_apkPath", m_device.SerialNumber,                                                                                 //
                        Regex.Split(                                                                                                        //
                            Adb.ExecuteAdbCommand(                                                                                          //
                                Adb.FormAdbShellCommand(m_device, false, "cat", "/system/build.prop", "|", "grep", "\"ro.product.device\"") //
                            ), "=", RegexOptions.IgnoreCase                                                                                 //
                        )[1],                                                                                                               //
                        m_apkPath,                                                                                                          //
                        @"\/(([^\r\n]{1,}\/){1,})?([A-z0-9_-]{1,}\.apk)"                                                                    //
                    )                                                                                                                       //
                );                                                                                                                          //
            if (m_device.FileSystem.FileOrDirectory(m_apkPath) != ListingType.FILE)                                                         //
                throw new ArgumentException(                                                                                                //
                    string.Format(                                                                                                          //
                          "The argument m_apkPath contains a file path that does not exist!\n"                                              //
                        + "Please make sure that only valid files are provided! (Serial: {0}, Model: {1})\n"                                //
                        + "Provided argument: {2}\n"                                                                                        //
                        + "File type: {3}",                                                                                                 //
                        m_device.SerialNumber,                                                                                              //
                        Regex.Split(                                                                                                        //
                            Adb.ExecuteAdbCommand(                                                                                          //
                                Adb.FormAdbShellCommand(m_device, false, "cat", "/system/build.prop", "|", "grep", "\"ro.product.device\"") //
                            ), "=", RegexOptions.IgnoreCase                                                                                 //
                        )[1], m_apkPath, m_device.FileSystem.FileOrDirectory(m_apkPath).ToString()                                          //
                    )                                                                                                                       //
                );                                                                                                                          //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Vars
            var m_optionList = new List<string>();
            m_optionList.Add("pm");
            m_optionList.Add("install");

            if (m_installOptions != null || m_installOptions.Length > 0) {
                foreach (InstallOption m_option in m_installOptions)
                    switch (m_option) {
                        case InstallOption.AllowDowngrade: m_optionList.Add("-d"); break;
                        case InstallOption.AllowTestApks: m_optionList.Add("-t"); break;
                        case InstallOption.ForwardLock: m_optionList.Add("-l"); break;
                        case InstallOption.InternalStorage: m_optionList.Add("-f"); break;
                        case InstallOption.ReinstallKeepData: m_optionList.Add("-r"); break;
                        case InstallOption.SharedMassStorage: m_optionList.Add("-s"); break;
                    }
            }

            if (!string.IsNullOrEmpty(m_installerPackage) || !string.IsNullOrWhiteSpace(m_installerPackage)) {
                m_optionList.Add("-i");
                m_optionList.Add(m_installerPackage);
            }
            m_optionList.Add(m_apkPath);
        }

        /// <summary>
        /// Uninstalls an application from a given device.
        /// This method takes a package (e.g.: com.myapp.application) as the argument to determine the to-uninstall app.
        /// </summary>
        /// <param name="m_package">
        /// The package to remove from the device. 
        /// If this parameter is set to null (Visual Basic: Nothing) then this method will throw an ArgumentNullException.
        /// If this parameter is does not match a package pattern (e.g.: com.myapp.application) then this method will throw an ArgumentException.
        /// Be sure to use try-catch blocks for this method, as this method expects to be terminated after an exception occurs.
        /// </param>
        /// <param name="m_keepData">
        /// If set to true, the application's data will not be touched. Instead it will be kept on the device.
        /// This parameter is set to false by default.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// The method will throw this exception if the <paramref name="m_package"/> 
        /// parameter is set to null (Visual Basic: Nothing), is empty or plain whitespace.
        /// In those cases, the exception is thrown. It is recommended that you use a try-catch block for this method, 
        /// as it expects to be terminated after an exception has occurred.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The method will throw this exception if the <paramref name="m_package"/> parameter does not match
        /// the pattern for application packages. (e.g.: com.myapp.application).
        /// The case does not matter, this method uses case-insensitive regular expressions.
        /// It is recommended you use a try-catch block for this method, as it expects to be terminated after an exception has occurred.
        /// </exception>
        public void UninstallPackage(string m_package, bool m_keepData = false) {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // Check if m_package is null or empty, if so throw an exception and terminate the method.        //
            // Else continue execution and                                                                    //
            // check if m_package matches package syntax, if not throw an error and terminate the method,     //
            // otherwise continue with method execution; check other params and uninstall application.        //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (string.IsNullOrEmpty(m_package) || string.IsNullOrWhiteSpace(m_package))                                                    //
                throw new ArgumentNullException("Package cannot be null or empty!");                                                        //
                                                                                                                                            //
            if (!Regex.IsMatch(m_package, @"([a-z]{1,}\.?){1,}", RegexOptions.IgnoreCase))                                                  //
                throw new ArgumentException(                                                                                                //
                    string.Format(                                                                                                          //
                          "The argument m_package does not match an Android package!\n"                                                     //
                        + "Please make sure you pass only packages from the device. (Serial: {0}, Model: {1})"                              //
                        + "Provided argument: {1}\n"                                                                                        //
                        + "Make sure the packages you pass have the following syntax:\n"                                                    //
                        + "E.g.: com.myapp.application2015\n",                                                                              //
                        m_device.SerialNumber,                                                                                              //
                        Regex.Split(                                                                                                        //
                            Adb.ExecuteAdbCommand(                                                                                          //
                                Adb.FormAdbShellCommand(m_device, false, "cat", "/system/build.prop", "|", "grep", "\"ro.product.device\"") //
                            ), "=", RegexOptions.IgnoreCase                                                                                 //
                        )[1],                                                                                                               //
                        m_package                                                                                                           //
                    )                                                                                                                       //
                );                                                                                                                          //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Adb.ExecuteAdbCommandNoReturn(
                Adb.FormAdbShellCommand(
                    m_device, false, "pm", "uninstall", m_package, (m_keepData ? "-k" : null)
                )
            );
        }

        /// <summary>
        /// Clears data associated with a package <paramref name="m_package"/>.
        /// This method takes a package (e.g.: com.myapp.application) as an argument.
        /// </summary>
        /// <param name="m_package">
        /// The package to clear the data from.
        /// This parameter may not be null or empty.
        /// If this parameter is null or empty, the method will throw an exception.
        /// Make sure you enter a valid package name, otherwise the method will also throw an exception.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// The method will throw this exception if the <paramref name="m_package"/> 
        /// parameter is set to null (Visual Basic: Nothing), is empty or plain whitespace.
        /// In those cases, the exception is thrown. It is recommended that you use a try-catch block for this method, 
        /// as it expects to be terminated after an exception has occurred.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// The method will throw this exception if the <paramref name="m_package"/> parameter does not match
        /// the pattern for application packages. (e.g.: com.myapp.application).
        /// The case does not matter, this method uses case-insensitive regular expressions.
        /// It is recommended you use a try-catch block for this method, as it expects to be terminated after an exception has occurred.
        /// </exception>
        public void Clear(string m_package) {
            //////////////////////////////////////////////////////////////////////////////////////////
            // Check if m_package is null or empty.                                                 //
            // If so, throw an exception and terminate the method.                                  //
            // Otherwise continue with execution and check if m_package represents a package        //
            // If so, continue with execution, else throw an exception and terminate the method.    //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (string.IsNullOrEmpty(m_package) || string.IsNullOrWhiteSpace(m_package))                                                    //
                throw new ArgumentNullException("m_package cannot be null or empty!");                                                      //
            if (!Regex.IsMatch(m_package, @"([a-z]{1,}\.?){1,}", RegexOptions.IgnoreCase))                                                  //
                throw new ArgumentException(                                                                                                //
                    string.Format(                                                                                                          //
                          "The argument m_package does not match an Android package!\n"                                                     //
                        + "Please make sure you pass only packages from the device. (Serial: {0}, Model: {1})"                              //
                        + "Provided argument: {1}\n"                                                                                        //
                        + "Make sure the packages you pass have the following syntax:\n"                                                    //
                        + "E.g.: com.myapp.application2015\n",                                                                              //
                        m_device.SerialNumber,                                                                                              //
                        Regex.Split(                                                                                                        //
                            Adb.ExecuteAdbCommand(                                                                                          //
                                Adb.FormAdbShellCommand(m_device, false, "cat", "/system/build.prop", "|", "grep", "\"ro.product.device\"") //
                            ), "=", RegexOptions.IgnoreCase                                                                                 //
                        )[1],                                                                                                               //
                        m_package                                                                                                           //
                    )                                                                                                                       //
                );                                                                                                                          //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Adb.ExecuteAdbCommandNoReturn(
                Adb.FormAdbShellCommand(m_device, false, "pm", "clear", m_package)
            );

        }

        /// <summary>
        /// Disables a user on the device.
        /// </summary>
        /// <param name="m_packageOrComponent"></param>
        /// <param name="m_user"></param>
        public void DisableUser(string m_packageOrComponent, string m_user = null) {
            //////////////////////////////////////////////////////////////////
            // Check if m_packageOrComponent is null or empty.              //
            // If so, throw an exception and terminate the method.          //
            // Else continue execution and check if m_packageOrComponent    //
            // matches permission/package RegEx                             //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (string.IsNullOrEmpty(m_packageOrComponent) || string.IsNullOrWhiteSpace(m_packageOrComponent))                              //
                throw new ArgumentNullException("m_packageOrComponent must not be null or empty!");                                         //
            if (!Regex.IsMatch(m_packageOrComponent, @"([a-z]{1,}\.?){1,}|([a-z]{2,}.){2,}([A-Z_]{1,})", RegexOptions.IgnoreCase))          //
                throw new ArgumentException(                                                                                                //
                    string.Format(                                                                                                          //
                          "The argument m_packageOrComponent does not match an Android package!\n"                                          //
                        + "Please make sure you pass only packages from the device. (Serial: {0}, Model: {1})"                              //
                        + "Provided argument: {1}\n"                                                                                        //
                        + "Make sure the packages you pass have the following syntax:\n"                                                    //
                        + "E.g.: com.myapp.application2015\n"                                                                               //
                        + "Or:\n"                                                                                                           //
                        + "E.g.: com.myapp.application2015.PERMISSION_WRITE_DATABASE",                                                      //
                        m_device.SerialNumber,                                                                                              //
                        Regex.Split(                                                                                                        //
                            Adb.ExecuteAdbCommand(                                                                                          //
                                Adb.FormAdbShellCommand(m_device, false, "cat", "/system/build.prop", "|", "grep", "\"ro.product.device\"") //
                            ), "=", RegexOptions.IgnoreCase                                                                                 //
                        )[1],                                                                                                               //
                        m_packageOrComponent                                                                                                //
                    )                                                                                                                       //
                );                                                                                                                          //
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Adb.ExecuteAdbCommandNoReturn(
                Adb.FormAdbShellCommand(
                    m_device, false, "pm", "disable-user", 
                    string.IsNullOrEmpty(m_user) || string.IsNullOrWhiteSpace(m_user) ? 
                        new object[] { "--user", m_user, m_packageOrComponent } : (object)m_packageOrComponent
                )
            );

        }

        /// <summary>
        /// Creates a new user profile on the device.
        /// </summary>
        /// <param name="m_username">
        /// The desired name for the user.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// The method will throw this exception if the <paramref name="m_username"/> 
        /// parameter is set to null (Visual Basic: Nothing), is empty or plain whitespace.
        /// In those cases, the exception is thrown. It is recommended that you use a try-catch block for this method, 
        /// as it expects to be terminated after an exception has occurred.
        /// </exception>
        public void CreateUser(string m_username) {
            //////////////////////////////////////////////////////////
            // Check if m_username is null, empty or whitespace.    //
            // If so, throw exception and terminate method.         //
            // Otherwise continue with execution and create user.   //
            //////////////////////////////////////////////////////////////////////////////////////
            if (string.IsNullOrEmpty(m_username) || string.IsNullOrWhiteSpace(m_username))      //
                throw new ArgumentNullException("m_username must not be null or empty!");       //
            //////////////////////////////////////////////////////////////////////////////////////

            Adb.ExecuteAdbCommandNoReturn(
                Adb.FormAdbShellCommand(m_device, false, "pm", m_username)
            );

        }

        /// <summary>
        /// Removes (deletes) a user from the device.
        /// </summary>
        /// <param name="m_userID">The user to remove. (Input the user ID)</param>
        /// <exception cref="System.ArgumentNullException">
        /// The method will throw this exception if <paramref name="m_userID"/> is null.
        /// </exception>
        public void RemoveUser(int m_userID) {
            //////////////////////////////////////////////////////
            // Check if m_userID is null.                       //
            // If so, throw exception and terminate method.     //
            // Otherwise continue execution and remove user.    //
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if (m_userID == null)                                                                               //
                throw new ArgumentNullException("m_userID must not be null!\nPlease enter a valid user ID!");   //
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            Adb.ExecuteAdbCommandNoReturn(
                Adb.FormAdbShellCommand(m_device, false, "pm", "remove-user", m_userID)
            );
        }

        // To-Do: Add other PackageManager methods!
    }

    /// <summary>
    /// A class representing an application package on a given device.
    /// This class provides properties for easy access to information on packages.
    /// 
    /// See also: <seealso cref="RegawMOD.Android.PackageManager"/>
    /// </summary>
    public class Package {

        /// <summary>
        /// Property containing the package's name.
        /// </summary>
        public string PackageName { get; internal set; }

        /// <summary>
        /// Gets the package's associated file.
        /// </summary>
        public string AssociatedFile { get; internal set; }

        /// <summary>
        /// Gets the package's installer.
        /// </summary>
        public string PackageInstaller { get; internal set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="name">
        /// The name of the package.
        /// E.g.: org.myapp.myapp
        /// </param>
        /// <param name="fileName">
        /// The filename and path of the package.
        /// E.g.: /data/app/myapp/com.myapp.myapp.apk
        /// </param>
        /// <param name="installer">
        /// The user or application that installed the package.
        /// E.g.: com.android.vending
        /// </param>
        public Package(string name = "", string fileName = "", string installer = "") { 
            PackageName = name;
            AssociatedFile = fileName;
            PackageInstaller = installer;
        }

    }

    /// <summary>
    /// This class represents an Android permission group.
    /// </summary>
    public class PermissionGroup {

        /// <summary>
        /// Gets the name of the permission group.
        /// </summary>                                                                                                                                                                                                        vv
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// Throws an <see cref="System.ArgumentException"/> if <paramref name="m_name"/> is null or empty.
        /// </summary>
        /// <param name="m_name">
        /// The name of the permission group.
        /// </param>
        public PermissionGroup(string m_name) {
            if (string.IsNullOrEmpty(m_name))
                throw new ArgumentException("Permission group name cannot be null or empty!");
            else Name = m_name;
        }

    }

    /// <summary>
    /// Represents a permission along with all possible attributes on a given Android device.
    /// </summary>
    public class Permission {

        /// <summary>
        /// An enumeration representing the protection level of a given permission.
        /// </summary>
        public enum ProtectionLevel {
            /// <summary>
            /// A higher-risk permission that would give a requesting application access to 
            /// private user data or control over the device that can negatively impact the user. 
            /// Because this type of permission introduces potential risk, 
            /// the system may not automatically grant it to the requesting application. 
            /// For example, any dangerous permissions requested by an application may be displayed 
            /// to the user and require confirmation before proceeding, 
            /// or some other approach may be taken to avoid the user automatically allowing the use of such facilities.
            /// -- From the Android Documentation --
            /// </summary>
            Dangerous,
            /// <summary>
            /// The default value. 
            /// A lower-risk permission that gives requesting applications access to 
            /// isolated application-level features, with minimal risk to other applications, 
            /// the system, or the user. The system automatically grants this type of permission 
            /// to a requesting application at installation, without asking for the user's explicit 
            /// approval (though the user always has the option to review these permissions before installing).
            /// -- From the Android Documentation --
            /// </summary>
            Normal,
            /// <summary>
            /// A permission that the system grants only if the requesting application 
            /// is signed with the same certificate as the application that declared 
            /// the permission. If the certificates match, the system automatically 
            /// grants the permission without notifying the user or asking for the user's explicit approval.
            /// -- From the Android Documentation --
            /// </summary>
            Signature,
            /// <summary>
            /// A permission that the system grants only to applications 
            /// that are in the Android system image or that are signed 
            /// with the same certificate as the application that declared the permission. 
            /// Please avoid using this option, as the signature protection level should 
            /// be sufficient for most needs and works regardless of exactly where applications 
            /// are installed. 
            /// The "signatureOrSystem" permission is used for certain special situations where 
            /// multiple vendors have applications built into a system image and need to share specific 
            /// features explicitly because they are being built together.
            /// -- From the Android Documentation --
            /// </summary>
            SignatureOrSystem
        }

        /// <summary>
        /// Gets and returns the permission's description.
        /// </summary>
        public string PermissionDescription { get; internal set; }

        /// <summary>
        /// Gets and returns the permission's label
        /// </summary>
        public string PermissionLabel { get; internal set; }

        /// <summary>
        /// Gets and returns the permission's group.
        /// </summary>
        public string PermissionGroup { get; internal set; }

        /// <summary>
        /// Gets and returns the Permissions name.
        /// </summary>
        public string PermissionName { get; internal set; }

        /// <summary>
        /// Gets and returns the permission's package.
        /// </summary>
        public string PermissionPackage { get; internal set; }

        /// <summary>
        /// Gets and returns the permission's protection level.
        /// </summary>
        public ProtectionLevel PermissionProtectionLevel { get; internal set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="m_permissionName">
        /// The name of the permission.
        /// </param>
        public Permission(string m_permissionName) {
            PermissionName = m_permissionName;
            PermissionGroup = "null";
            PermissionLabel = "null";
            PermissionDescription = "null";
            PermissionProtectionLevel = ProtectionLevel.Normal;
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="m_permissionName">The name of the permission.</param>
        /// <param name="m_permissionGroup">The group the permission is in.</param>
        public Permission(string m_permissionName, string m_permissionGroup) {
            PermissionName = m_permissionName;
            PermissionGroup = m_permissionGroup;
            PermissionLabel = "null";
            PermissionDescription = "null";
            PermissionProtectionLevel = ProtectionLevel.Normal;
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="m_permissionName">The name of the permission.</param>
        /// <param name="m_permissionGroup">The group the permission is in.</param>
        /// <param name="m_permissionLabel">The label of the permission.</param>
        /// <param name="m_permissionDescription">The permission's description.</param>
        /// <param name="m_protectionLevel">The permission's protection level. Default: Normal</param>
        public Permission(string m_permissionName, string m_permissionPackage, string m_permissionLabel, string m_permissionDescription, string m_permissionGroup = "null", 
            ProtectionLevel m_protectionLevel = ProtectionLevel.Normal) {

            PermissionName = m_permissionName;
            PermissionPackage = m_permissionPackage;
            PermissionGroup = m_permissionGroup;
            PermissionLabel = m_permissionLabel;
            PermissionDescription = m_permissionDescription;
            PermissionProtectionLevel = m_protectionLevel;
        }

    }

    /// <summary>
    /// This class represents a feature present on a given Android device.
    /// </summary>
    public class Feature {

        /// <summary>
        /// Gets and returns the feature's name.
        /// </summary>
        public string FeatureName { get; internal set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="m_name"></param>
        public Feature(string m_name) {
            FeatureName = m_name;
        }

    }

}
