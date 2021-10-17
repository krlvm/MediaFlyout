using System;

namespace MediaFlyout.AppInformation
{
    public enum SourceAppInfoDataType
    {
        FromProcessId,
        FromAppUserModelId
    }

    public class SourceAppInfoData
    {
        public string AppUserModelId { get; set; }

        public IntPtr MainWindowHandle { get; set; }

        public uint ProcessId { get; set; }

        public SourceAppInfoDataType DataType { get; set; }
    }
}