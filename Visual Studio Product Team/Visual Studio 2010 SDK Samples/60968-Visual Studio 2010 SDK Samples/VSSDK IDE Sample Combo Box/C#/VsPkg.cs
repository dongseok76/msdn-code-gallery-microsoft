﻿/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;
using Microsoft.VisualStudio;
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Samples.VisualStudio.ComboBox")]
namespace Microsoft.Samples.VisualStudio.ComboBox
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental instance. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#100", "#102", "1.0", IconResourceID = 400)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource(1000, 1)]
    // This attribute registers a tool window exposed by this package.
    [Guid(GuidList.guidComboBoxPkgString)]
    public sealed class ComboBoxPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ComboBoxPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // DropDownCombo
                //	 a DROPDOWNCOMBO does not let the user type into the combo box; they can only pick from the list.
                //   The string value of the element selected is returned.
                //	 For example, this type of combo could be used for the "Solution Configurations" on the "Standard" toolbar.
                //
                //   A DropDownCombo box requires two commands:
                //     One command (cmdidMyCombo) is used to ask for the current value of the combo box and to 
                //     set the new value when the user makes a choice in the combo box.
                //
                //     The second command (cmdidMyComboGetList) is used to retrieve this list of choices for the combo box.
                // 
                // Normally IOleCommandTarget::QueryStatus is used to determine the state of a command, e.g.
                // enable vs. disable, shown vs. hidden, etc. The QueryStatus method does not have any way to 
                // control the statue of a combo box, e.g. what list of items should be shown and what is the 
                // current value. In order to communicate this information actually IOleCommandTarget::Exec
                // is used with a non-NULL varOut parameter. You can think of these Exec calls as extended 
                // QueryStatus calls. There are two pieces of information needed for a combo, thus it takes
                // two commands to retrieve this information. The main command id for the command is used to 
                // retrieve the current value and the second command is used to retrieve the full list of 
                // choices to be displayed as an array of strings.
                CommandID menuMyDropDownComboCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyDropDownCombo);
                OleMenuCommand menuMyDropDownComboCommand = new OleMenuCommand(new EventHandler(OnMenuMyDropDownCombo), menuMyDropDownComboCommandID);
                menuMyDropDownComboCommand.ParametersDescription = "$"; // accept any argument string
                mcs.AddCommand(menuMyDropDownComboCommand);

                CommandID menuMyDropDownComboGetListCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyDropDownComboGetList);
                MenuCommand menuMyDropDownComboGetListCommand = new OleMenuCommand(new EventHandler(OnMenuMyDropDownComboGetList), menuMyDropDownComboGetListCommandID);
                mcs.AddCommand(menuMyDropDownComboGetListCommand);

                // IndexCombo
                //	 An INDEXCOMBO is the same as a DROPDOWNCOMBO in that it is a "pick from list" only combo.
                //	 The difference is an INDEXCOMBO returns the selected value as an index into the list (0 based).
                //	 For example, this type of combo could be used for the "Solution Configurations" on the "Standard" toolbar.
                //
                //   An IndexCombo box requires two commands:
                //     One command is used to ask for the current value of the combo box and to set the new value when the user
                //     makes a choice in the combo box.
                //
                //     The second command is used to retrieve this list of choices for the combo box.
                CommandID menuMyIndexComboCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyIndexCombo);
                OleMenuCommand menuMyIndexComboCommand = new OleMenuCommand(new EventHandler(OnMenuMyIndexCombo), menuMyIndexComboCommandID);
                menuMyIndexComboCommand.ParametersDescription = "$"; // accept any argument string
                mcs.AddCommand(menuMyIndexComboCommand);

                CommandID menuMyIndexComboGetListCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyIndexComboGetList);
                MenuCommand menuMyIndexComboGetListCommand = new OleMenuCommand(new EventHandler(OnMenuMyIndexComboGetList), menuMyIndexComboGetListCommandID);
                mcs.AddCommand(menuMyIndexComboGetListCommand);

                // MRUCombo
                //   An MRUCOMBO allows the user to type into the edit box. The history of strings entered
                //   is automatically persisted by the IDE on a per-user/per-machine basis. 
                //	 For example, this type of combo is used for the "Find" combo on the "Standard" toolbar.
                //
                //   An MRU Combo box requires only one command:
                //     One command is used to ask for the current value of the combo box and to set the new value when the user
                //     makes a choice in the combo box.
                //
                //     The list of choices entered is automatically remembered by the IDE on a per-user/per-machine basis.
                CommandID menuMyMRUComboCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyMRUCombo);
                OleMenuCommand menuMyMRUComboCommand = new OleMenuCommand(new EventHandler(OnMenuMyMRUCombo), menuMyMRUComboCommandID);
                menuMyMRUComboCommand.ParametersDescription = "$"; // accept any argument string
                mcs.AddCommand(menuMyMRUComboCommand);

                // DynamicCombo
                //   A DYNAMICCOMBO allows the user to type into the edit box or pick from the list. The 
                //	 list of choices is usually fixed and is managed by the command handler for the command.
                //	 For example, this type of combo is used for the "Zoom" combo on the "Class Designer" toolbar.
                //
                //   A Combo box requires two commands:
                //     One command is used to ask for the current value of the combo box and to set the new value when the user
                //     makes a choice in the combo box.
                //
                //     The second command is used to retrieve this list of choices for the combo box.
                CommandID menuMyDynamicComboCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyDynamicCombo);
                OleMenuCommand menuMyDynamicComboCommand = new OleMenuCommand(new EventHandler(OnMenuMyDynamicCombo), menuMyDynamicComboCommandID);
                menuMyDynamicComboCommand.ParametersDescription = "$"; // accept any argument string
                mcs.AddCommand(menuMyDynamicComboCommand);

                CommandID menuMyDynamicComboGetListCommandID = new CommandID(GuidList.guidComboBoxCmdSet, (int)PkgCmdIDList.cmdidMyDynamicComboGetList);
                MenuCommand menuMyDynamicComboGetListCommand = new OleMenuCommand(new EventHandler(OnMenuMyDynamicComboGetList), menuMyDynamicComboGetListCommandID);
                mcs.AddCommand(menuMyDynamicComboGetListCommand);
            }
        }

        #endregion

        #region Combo Box Commands

        private string[] dropDownComboChoices = { Resources.Apples, Resources.Oranges, Resources.Pears, Resources.Bananas };
        private string currentDropDownComboChoice = Resources.Apples;

        // DropDownCombo
        //	 a DROPDOWNCOMBO does not let the user type into the combo box; they can only pick from the list.
        //   The string value of the element selected is returned.
        //	 For example, this type of combo could be used for the "Solution Configurations" on the "Standard" toolbar.
        //
        //   A DropDownCombo box requires two commands:
        //     One command (cmdidMyCombo) is used to ask for the current value of the combo box and to 
        //     set the new value when the user makes a choice in the combo box.
        //
        //     The second command (cmdidMyComboGetList) is used to retrieve this list of choices for the combo box.
        private void OnMenuMyDropDownCombo(object sender, EventArgs e)
        {
            if (e == EventArgs.Empty)
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                string newChoice = eventArgs.InValue as string;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero && newChoice != null)
                {
                    throw (new ArgumentException(Resources.BothInOutParamsIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    Marshal.GetNativeVariantForObject(this.currentDropDownComboChoice, vOut);
                }

                else if (newChoice != null)
                {
                    // new value was selected or typed in
                    // see if it is one of our items
                    bool validInput = false;
                    int indexInput = -1;
                    for (indexInput = 0; indexInput < dropDownComboChoices.Length; indexInput++)
                    {
                        if (String.Compare(dropDownComboChoices[indexInput], newChoice, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            validInput = true;
                            break;
                        }
                    }

                    if (validInput)
                    {
                        this.currentDropDownComboChoice = dropDownComboChoices[indexInput];
                        ShowMessage(Resources.MyDropDownCombo, this.currentDropDownComboChoice);
                    }
                    else
                    {
                        throw (new ArgumentException(Resources.ParamNotValidStringInList)); // force an exception to be thrown
                    }
                }
                else
                {
                    // We should never get here
                    throw (new ArgumentException(Resources.InOutParamCantBeNULL)); // force an exception to be thrown
                }
            }
            else
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }
        }

        // A DropDownCombo box requires two commands:
        //    This command is used to retrieve this list of choices for the combo box.
        // 
        // Normally IOleCommandTarget::QueryStatus is used to determine the state of a command, e.g.
        // enable vs. disable, shown vs. hidden, etc. The QueryStatus method does not have any way to 
        // control the statue of a combo box, e.g. what list of items should be shown and what is the 
        // current value. In order to communicate this information actually IOleCommandTarget::Exec
        // is used with a non-NULL varOut parameter. You can think of these Exec calls as extended 
        // QueryStatus calls. There are two pieces of information needed for a combo, thus it takes
        // two commands to retrieve this information. The main command id for the command is used to 
        // retrieve the current value and the second command is used to retrieve the full list of 
        // choices to be displayed as an array of strings.
        private void OnMenuMyDropDownComboGetList(object sender, EventArgs e)
        {
            if ((null == e) || (e == EventArgs.Empty))
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentNullException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (inParam != null)
                {
                    throw (new ArgumentException(Resources.InParamIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    Marshal.GetNativeVariantForObject(this.dropDownComboChoices, vOut);
                }
                else
                {
                    throw (new ArgumentException(Resources.OutParamRequired)); // force an exception to be thrown
                }
            }

        }

        private string[] indexComboChoices = { Resources.Lions, Resources.Tigers, Resources.Bears};
        private int currentIndexComboChoice = 0;

        // IndexCombo
        //	 An INDEXCOMBO is the same as a DROPDOWNCOMBO in that it is a "pick from list" only combo.
        //	 The difference is an INDEXCOMBO returns the selected value as an index into the list (0 based).
        //	 For example, this type of combo could be used for the "Solution Configurations" on the "Standard" toolbar.
        //
        //   An IndexCombo box requires two commands:
        //     One command is used to ask for the current value of the combo box and to set the new value when the user
        //     makes a choice in the combo box.
        //
        //     The second command is used to retrieve this list of choices for the combo box.
        private void OnMenuMyIndexCombo(object sender, EventArgs e)
        {
            if ((null == e) || (e == EventArgs.Empty))
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object input = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero && input != null)
                {
                    throw (new ArgumentException(Resources.BothInOutParamsIllegal)); // force an exception to be thrown
                }
                if (vOut != IntPtr.Zero)
                {
                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    Marshal.GetNativeVariantForObject(this.indexComboChoices[this.currentIndexComboChoice], vOut);
                }

                else if (input != null)
                {
                    int newChoice = -1;
                    try
                    {
                        // user typed a string argument in command window.
                        int index = int.Parse(input.ToString(), CultureInfo.CurrentCulture);
                        if (index >= 0 && index < indexComboChoices.Length)
                        {
                            newChoice = index;
                        }
                        else
                        {
                            string errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.InvalidIndex, indexComboChoices.Length);
                            throw (new ArgumentOutOfRangeException(errorMessage));
                        }
                    }
                    catch (FormatException)
                    {
                        // user typed in a non-numeric value, see if it is one of our items
                        for (int i = 0; i < indexComboChoices.Length; i++)
                        {
                            if (String.Compare(indexComboChoices[i], input.ToString(), StringComparison.CurrentCultureIgnoreCase) == 0)
                            {
                                newChoice = i;
                                break;
                            }
                        }
                    }
                    catch (OverflowException)
                    {
                        // user typed in too large of a number, ignore it
                    }

                    // new value was selected or typed in
                    if (newChoice != -1)
                    {
                        this.currentIndexComboChoice = newChoice;
                        ShowMessage(Resources.MyIndexCombo, this.currentIndexComboChoice.ToString(CultureInfo.CurrentCulture));
                    }
                    else
                    {
                        throw (new ArgumentException(Resources.ParamMustBeValidIndexOrStringInList)); // force an exception to be thrown
                    }
                }
                else
                {
                    // We should never get here; EventArgs are required.
                    throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
                }
            }
            else
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }
        }

        // An IndexCombo box requires two commands:
        //    This command is used to retrieve this list of choices for the combo box.
        // 
        // Normally IOleCommandTarget::QueryStatus is used to determine the state of a command, e.g.
        // enable vs. disable, shown vs. hidden, etc. The QueryStatus method does not have any way to 
        // control the statue of a combo box, e.g. what list of items should be shown and what is the 
        // current value. In order to communicate this information actually IOleCommandTarget::Exec
        // is used with a non-NULL varOut parameter. You can think of these Exec calls as extended 
        // QueryStatus calls. There are two pieces of information needed for a combo, thus it takes
        // two commands to retrieve this information. The main command id for the command is used to 
        // retrieve the current value and the second command is used to retrieve the full list of 
        // choices to be displayed as an array of strings.
        private void OnMenuMyIndexComboGetList(object sender, EventArgs e)
        {
            if (e == EventArgs.Empty)
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (inParam != null)
                {
                    throw (new ArgumentException(Resources.InParamIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    Marshal.GetNativeVariantForObject(this.indexComboChoices, vOut);
                }
                else
                {
                    throw (new ArgumentException(Resources.OutParamRequired)); // force an exception to be thrown
                }
            }
        }

        private string currentMRUComboChoice = null;

        // MRUCombo
        //   An MRUCOMBO allows the user to type into the edit box. The history of strings entered
        //   is automatically persisted by the IDE on a per-user/per-machine basis. 
        //	 For example, this type of combo is used for the "Find" combo on the "Standard" toolbar.
        //
        //   An MRU Combo box requires only one command:
        //     One command is used to ask for the current value of the combo box and to set the new value when the user
        //     makes a choice in the combo box.
        //
        //     The list of choices entered is automatically remembered by the IDE on a per-user/per-machine basis.
        private void OnMenuMyMRUCombo(object sender, EventArgs e)
        {
            if (e == EventArgs.Empty)
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object input = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero && input != null)
                {
                    throw (new ArgumentException(Resources.BothInOutParamsIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    Marshal.GetNativeVariantForObject(this.currentMRUComboChoice, vOut);
                }

                else if (input != null)
                {
                    string newChoice = input.ToString();

                    // new value was selected or typed in
                    if (!string.IsNullOrEmpty(newChoice))
                    {
                        this.currentMRUComboChoice = newChoice;
                        ShowMessage(Resources.MyMRUCombo, this.currentMRUComboChoice);
                    }
                    else
                    {
                        // We should never get here
                        throw (new ArgumentException(Resources.EmptyStringIllegal)); // force an exception to be thrown
                    }
                }
                else
                {
                    throw (new ArgumentException(Resources.BothInOutParamsIllegal)); // force an exception to be thrown
                }
            }
            else
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }
        }

        private double[] numericZoomLevels = { 4.0, 3.0, 2.0, 1.5, 1.25, 1.0, .75, .66, .50, .33, .25, .10 };
        private string zoomToFit = Resources.ZoomToFit;
        private string zoom_to_Fit = Resources.Zoom_to_Fit;
        private string[] zoomLevels = null;
        private NumberFormatInfo numberFormatInfo;
        private double currentZoomFactor = 1.0;

        // DynamicCombo
        //   A DYNAMICCOMBO allows the user to type into the edit box or pick from the list. The 
        //	 list of choices is usually fixed and is managed by the command handler for the command.
        //	 For example, this type of combo is used for the "Zoom" combo on the "Class Designer" toolbar.
        //
        //   A Combo box requires two commands:
        //     One command is used to ask for the current value of the combo box and to set the new value when the user
        //     makes a choice in the combo box.
        //
        //     The second command is used to retrieve this list of choices for the combo box.
        private void OnMenuMyDynamicCombo(object sender, EventArgs e)
        {
            if ((null == e) || (e == EventArgs.Empty))
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object input = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (vOut != IntPtr.Zero && input != null)
                {
                    throw (new ArgumentException(Resources.BothInOutParamsIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    // when vOut is non-NULL, the IDE is requesting the current value for the combo
                    if (this.currentZoomFactor == 0)
                    {
                        Marshal.GetNativeVariantForObject(this.zoom_to_Fit, vOut);
                    }
                    else
                    {
                        string factorString = currentZoomFactor.ToString("P0", this.numberFormatInfo);
                        Marshal.GetNativeVariantForObject(factorString, vOut);
                    }

                }
                else if (input != null)
                {
                    // new zoom value was selected or typed in
                    string inputString = input.ToString();

                    if (inputString.Equals(this.zoomToFit) || inputString.Equals(this.zoom_to_Fit))
                    {
                        currentZoomFactor = 0;
                        ShowMessage(Resources.MyDynamicCombo, this.zoom_to_Fit);
                    }
                    else
                    {
                        // There doesn't appear to be any percent-parsing routines in the framework (even though you can create
                        // a localized percentage in a string!).  So, we need to remove any occurence of the localized Percent 
                        // symbol, then parse the value that's left
                        try
                        {
                            float newZoom = Single.Parse(inputString.Replace(NumberFormatInfo.InvariantInfo.PercentSymbol, ""), CultureInfo.CurrentCulture);

                            newZoom = (float)Math.Round(newZoom);
                            if (newZoom < 0)
                            {
                                throw (new ArgumentException(Resources.ZoomMustBeGTZero)); // force an exception to be thrown
                            }

                            currentZoomFactor = newZoom / (float)100.0;

                            ShowMessage(Resources.MyDynamicCombo, newZoom.ToString(CultureInfo.CurrentCulture));
                        }
                        catch (FormatException)
                        {
                            // user typed in a non-numeric value, ignore it
                        }
                        catch (OverflowException)
                        {
                            // user typed in too large of a number, ignore it
                        }
                    }
                }
                else
                {
                    // We should never get here
                    throw (new ArgumentException(Resources.InOutParamCantBeNULL)); // force an exception to be thrown
                }
            }
            else
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentException(Resources.EventArgsRequired)); // force an exception to be thrown
            }
        }

        // A Combo box requires two commands:
        //    This command is used to retrieve this list of choices for the combo box.
        // 
        // Normally IOleCommandTarget::QueryStatus is used to determine the state of a command, e.g.
        // enable vs. disable, shown vs. hidden, etc. The QueryStatus method does not have any way to 
        // control the statue of a combo box, e.g. what list of items should be shown and what is the 
        // current value. In order to communicate this information actually IOleCommandTarget::Exec
        // is used with a non-NULL varOut parameter. You can think of these Exec calls as extended 
        // QueryStatus calls. There are two pieces of information needed for a combo, thus it takes
        // two commands to retrieve this information. The main command id for the command is used to 
        // retrieve the current value and the second command is used to retrieve the full list of 
        // choices to be displayed as an array of strings.
        private void OnMenuMyDynamicComboGetList(object sender, EventArgs e)
        {
            if ((null == e) || (e == EventArgs.Empty))
            {
                // We should never get here; EventArgs are required.
                throw (new ArgumentNullException(Resources.EventArgsRequired)); // force an exception to be thrown
            }

            OleMenuCmdEventArgs eventArgs = e as OleMenuCmdEventArgs;

            if (eventArgs != null)
            {
                object inParam = eventArgs.InValue;
                IntPtr vOut = eventArgs.OutValue;

                if (inParam != null)
                {
                    throw (new ArgumentException(Resources.InParamIllegal)); // force an exception to be thrown
                }
                else if (vOut != IntPtr.Zero)
                {
                    // initialize the zoom value array if needed
                    if (zoomLevels == null)
                    {
                        this.numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentUICulture.NumberFormat.Clone();
                        if (this.numberFormatInfo.PercentPositivePattern == 0)
                            this.numberFormatInfo.PercentPositivePattern = 1;
                        if (this.numberFormatInfo.PercentNegativePattern == 0)
                            this.numberFormatInfo.PercentNegativePattern = 1;

                        zoomLevels = new String[numericZoomLevels.Length + 1];
                        for (int i = 0; i < numericZoomLevels.Length; i++)
                        {
                            zoomLevels[i] = numericZoomLevels[i].ToString("P0", this.numberFormatInfo);
                        }

                        zoomLevels[zoomLevels.Length - 1] = zoom_to_Fit;
                    }

                    Marshal.GetNativeVariantForObject(zoomLevels, vOut);
                }
                else
                {
                    throw (new ArgumentException(Resources.OutParamRequired)); // force an exception to be thrown
                }
            }
        }
        #endregion

        // Helper method to show a message box using the SVsUiShell/IVsUiShell service
        public void ShowMessage(string title, string message)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result = VSConstants.S_OK;
            int hr = uiShell.ShowMessageBox(0,
                                ref clsid,
                                title,
                                message,
                                null,
                                0,
                                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                OLEMSGICON.OLEMSGICON_INFO,
                                0,        // false = application modal; true would make it system modal
                                out result);
            ErrorHandler.ThrowOnFailure(hr);
        }
    }
}