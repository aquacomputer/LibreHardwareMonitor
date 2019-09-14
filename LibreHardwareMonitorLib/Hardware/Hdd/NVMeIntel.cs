﻿// Mozilla Public License 2.0
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors
// All Rights Reserved

using System;
using System.Runtime.InteropServices;
using System.Text;
using LibreHardwareMonitor.Interop;

namespace LibreHardwareMonitor.Hardware.Hdd
{
    internal class NVMeIntel : INVMeDrive
    {
        //intel nvme access

        public SafeHandle Identify(StorageInfo storageInfo)
        {
            return NVMeWindows.IdentifyDevice(storageInfo);
        }

        public bool IdentifyController(SafeHandle hDevice, out Kernel32.NVME_IDENTIFY_CONTROLLER_DATA data)
        {
            data = Kernel32.CreateStruct<Kernel32.NVME_IDENTIFY_CONTROLLER_DATA>();
            if (hDevice == null || hDevice.IsInvalid)
                return false;

            bool result = false;
            IntPtr buffer;

            Kernel32.NVME_PASS_THROUGH_IOCTL passThrough = Kernel32.CreateStruct<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            passThrough.srb.HeaderLenght = (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.srb.Signature = Encoding.ASCII.GetBytes(Kernel32.IntelNVMeMiniPortSignature1);
            passThrough.srb.Timeout = 10;
            passThrough.srb.ControlCode = Kernel32.NVMePassThroughSrbIoCode;
            passThrough.srb.ReturnCode = 0;
            passThrough.srb.Length = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>() - (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.NVMeCmd = new uint[16];
            passThrough.NVMeCmd[0] = 6; //identify
            passThrough.NVMeCmd[10] = 1; //return to host
            passThrough.Direction = Kernel32.NVME_DIRECTION.NVME_FROM_DEV_TO_HOST;
            passThrough.QueueId = 0;
            passThrough.DataBufferLen = (uint)passThrough.DataBuffer.Length;
            passThrough.MetaDataLen = 0;
            passThrough.ReturnBufferLen = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();

            var length = Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            buffer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(passThrough, buffer, false);

            var validTransfer = Kernel32.DeviceIoControl(hDevice, Kernel32.IOCTL.IOCTL_SCSI_MINIPORT, buffer, length, buffer, length, out _, IntPtr.Zero);
            if (validTransfer)
            {
                var offset = Marshal.OffsetOf<Kernel32.NVME_PASS_THROUGH_IOCTL>(nameof(Kernel32.NVME_PASS_THROUGH_IOCTL.DataBuffer));
                var newPtr = IntPtr.Add(buffer, offset.ToInt32());
                int finalSize = Marshal.SizeOf<Kernel32.NVME_IDENTIFY_CONTROLLER_DATA>();
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<Kernel32.NVME_IDENTIFY_CONTROLLER_DATA>());
                Kernel32.RtlZeroMemory(ptr, finalSize);
                int len = Math.Min(finalSize, passThrough.DataBuffer.Length);
                Kernel32.CopyMemory(ptr, newPtr, (uint)len);
                Marshal.FreeHGlobal(buffer);

                var item = Marshal.PtrToStructure<Kernel32.NVME_IDENTIFY_CONTROLLER_DATA>(ptr);
                data = item;
                Marshal.FreeHGlobal(ptr);
                result = true;
            }
            else
            {
                Marshal.FreeHGlobal(buffer);
            }

            return result;
        }

        public bool HealthInfoLog(SafeHandle hDevice, out Kernel32.NVME_HEALTH_INFO_LOG data)
        {
            data = Kernel32.CreateStruct<Kernel32.NVME_HEALTH_INFO_LOG>();
            if (hDevice == null || hDevice.IsInvalid)
                return false;

            bool result = false;

            Kernel32.NVME_PASS_THROUGH_IOCTL passThrough = Kernel32.CreateStruct<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            passThrough.srb.HeaderLenght = (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.srb.Signature = Encoding.ASCII.GetBytes(Kernel32.IntelNVMeMiniPortSignature1);
            passThrough.srb.Timeout = 10;
            passThrough.srb.ControlCode = Kernel32.NVMePassThroughSrbIoCode;
            passThrough.srb.ReturnCode = 0;
            passThrough.srb.Length = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>() - (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.NVMeCmd[0] = (uint)Kernel32.STORAGE_PROTOCOL_NVME_DATA_TYPE.NVMeDataTypeLogPage; // GetLogPage
            passThrough.NVMeCmd[1] = 0xFFFFFFFF; // address
            passThrough.NVMeCmd[10] = 0x007f0002; // uint cdw10 = 0x000000002 | (((size / 4) - 1) << 16);
            passThrough.Direction = Kernel32.NVME_DIRECTION.NVME_FROM_DEV_TO_HOST;
            passThrough.QueueId = 0;
            passThrough.DataBufferLen = (uint)passThrough.DataBuffer.Length;
            passThrough.MetaDataLen = 0;
            passThrough.ReturnBufferLen = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();

            var length = Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            var buffer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(passThrough, buffer, false);

            var validTransfer = Kernel32.DeviceIoControl(hDevice, Kernel32.IOCTL.IOCTL_SCSI_MINIPORT, buffer, length, buffer, length, out _, IntPtr.Zero);
            if (validTransfer)
            {
                var offset = Marshal.OffsetOf<Kernel32.NVME_PASS_THROUGH_IOCTL>(nameof(Kernel32.NVME_PASS_THROUGH_IOCTL.DataBuffer));
                var newPtr = IntPtr.Add(buffer, offset.ToInt32());
                var item = Marshal.PtrToStructure<Kernel32.NVME_HEALTH_INFO_LOG>(newPtr);
                data = item;
                Marshal.FreeHGlobal(buffer);
                result = true;
            }
            else
            {
                Marshal.FreeHGlobal(buffer);
            }
            return result;
        }

        public static SafeHandle IdentifyDevice(StorageInfo storageInfo)
        {
            var handle = Kernel32.OpenDevice(storageInfo.Scsi);
            if (handle == null || handle.IsInvalid)
                return null;

            bool validTransfer;
            IntPtr buffer;

            Kernel32.NVME_PASS_THROUGH_IOCTL passThrough = Kernel32.CreateStruct<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            passThrough.srb.HeaderLenght = (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.srb.Signature = Encoding.ASCII.GetBytes(Kernel32.IntelNVMeMiniPortSignature1);
            passThrough.srb.Timeout = 10;
            passThrough.srb.ControlCode = Kernel32.NVMePassThroughSrbIoCode;
            passThrough.srb.ReturnCode = 0;
            passThrough.srb.Length = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>() - (uint)Marshal.SizeOf<Kernel32.SRB_IO_CONTROL>();
            passThrough.NVMeCmd = new uint[16];
            passThrough.NVMeCmd[0] = 6; //identify
            passThrough.NVMeCmd[10] = 1; //return to host
            passThrough.Direction = Kernel32.NVME_DIRECTION.NVME_FROM_DEV_TO_HOST;
            passThrough.QueueId = 0;
            passThrough.DataBufferLen = (uint)passThrough.DataBuffer.Length;
            passThrough.MetaDataLen = 0;
            passThrough.ReturnBufferLen = (uint)Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();

            var length = Marshal.SizeOf<Kernel32.NVME_PASS_THROUGH_IOCTL>();
            buffer = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(passThrough, buffer, false);

            validTransfer = Kernel32.DeviceIoControl(handle, Kernel32.IOCTL.IOCTL_SCSI_MINIPORT, buffer, length, buffer, length, out _, IntPtr.Zero);
            Marshal.FreeHGlobal(buffer);

            if (validTransfer) { }
            else
            {
                handle.Close();
                handle = null;
            }
            return handle;
        }
    }
}
