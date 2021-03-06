﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

namespace LibreHardwareMonitor.Hardware
{
    public delegate void HardwareEventHandler(IHardware hardware);

    public interface IComputer : IElement
    {
        bool IsCpuEnabled { get; }

        bool IsControllerEnabled { get; }

        bool IsGpuEnabled { get; }

        IHardware[] Hardware { get; }

        bool IsStorageEnabled { get; }

        bool IsMotherboardEnabled { get; }

        bool IsNetworkEnabled { get; }

        bool IsMemoryEnabled { get; }

        string GetReport();

        event HardwareEventHandler HardwareAdded;

        event HardwareEventHandler HardwareRemoved;
    }
}
