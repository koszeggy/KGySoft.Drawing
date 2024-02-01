#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: StockIcon.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Represents the Windows stock icons can be retrieved by the <see cref="Icons.GetStockIcon">Icons.GetStockIcon</see> method on Windows Vista and above.
    /// <br/>See also the <a href="https://docs.microsoft.com/en-us/windows/desktop/api/shellapi/ne-shellapi-shstockiconid" target="_blank">SHSTOCKICONID Enumeration</a> at the Microsoft Docs site.
    /// </summary>
    public enum StockIcon
    {
        /// <summary>
        ///  Document of a type with no associated application.
        /// </summary>
        DocNoAssoc = 0,

        /// <summary>
        ///  Document of a type with an associated application.
        /// </summary>
        DocAssoc = 1,

        /// <summary>
        /// Generic application with no custom icon.
        /// </summary>
        Application = 2,

        /// <summary>
        /// Folder (generic, unspecified state).
        /// </summary>
        Folder = 3,

        /// <summary>
        ///  Folder (open).
        /// </summary>
        FolderOpen = 4,

        /// <summary>
        /// 5.25-inch disk drive.
        /// </summary>
        Drive525 = 5,

        /// <summary>
        ///  3.5-inch disk drive.
        /// </summary>
        Drive35 = 6,

        /// <summary>
        /// Removable drive.
        /// </summary>
        DriveRemove = 7,

        /// <summary>
        ///  Fixed drive (hard disk).
        /// </summary>
        DriveFixed = 8,

        /// <summary>
        /// Network drive (connected).
        /// </summary>
        DriveNet = 9,

        /// <summary>
        /// Network drive (disconnected).
        /// </summary>
        DriveNetDisabled = 10,

        /// <summary>
        /// CD drive.
        /// </summary>
        DriveCD = 11,

        /// <summary>
        /// RAM disk drive.
        /// </summary>
        DriveRam = 12,

        /// <summary>
        /// The entire network.
        /// </summary>
        World = 13,

        /// <summary>
        /// A computer on the network.
        /// </summary>
        Server = 15,

        /// <summary>
        ///  A local printer or print destination.
        /// </summary>
        Printer = 16,

        /// <summary>
        /// The Network virtual folder
        /// </summary>
        MyNetwork = 17,

        /// <summary>
        /// The Search feature.
        /// </summary>
        Find = 22,

        /// <summary>
        /// The Help and Support feature.
        /// </summary>
        Help = 23,

        /// <summary>
        /// Overlay for a shared item.
        /// </summary>
        Share = 28,

        /// <summary>
        /// Overlay for a shortcut.
        /// </summary>
        Link = 29,

        /// <summary>
        /// Overlay for items that are expected to be slow to access.
        /// </summary>
        SlowFile = 30,

        /// <summary>
        /// The Recycle Bin (empty).
        /// </summary>
        Recycler = 31,

        /// <summary>
        /// The Recycle Bin (not empty).
        /// </summary>
        RecyclerFull = 32,

        /// <summary>
        /// Audio CD media.
        /// </summary>
        MediaCDAudio = 40,

        /// <summary>
        /// Security lock.
        /// </summary>
        Lock = 47,

        /// <summary>
        /// A virtual folder that contains the results of a search.
        /// </summary>
        AutoList = 49,

        /// <summary>
        /// A network printer.
        /// </summary>
        PrinterNet = 50,

        /// <summary>
        /// A server shared on a network.
        /// </summary>
        ServerShare = 51,

        /// <summary>
        /// A local fax printer.
        /// </summary>
        PrinterFax = 52,

        /// <summary>
        /// A network fax printer.
        /// </summary>
        PrinterFaxNet = 53,

        /// <summary>
        /// A file that receives the output of a Print to file operation.
        /// </summary>
        PrinterFile = 54,

        /// <summary>
        /// A category that results from a Stack by command to organize the contents of a folder.
        /// </summary>
        Stack = 55,

        /// <summary>
        /// Super Video CD (SVCD) media.
        /// </summary>
        MediaSvcd = 56,

        /// <summary>
        /// A folder that contains only subfolders as child items.
        /// </summary>
        StuffedFolder = 57,

        /// <summary>
        /// Unknown drive type.
        /// </summary>
        DriveUnknown = 58,

        /// <summary>
        /// DVD drive.
        /// </summary>
        DriveDvd = 59,

        /// <summary>
        /// DVD media.
        /// </summary>
        MediaDvd = 60,

        /// <summary>
        /// DVD-RAM media.
        /// </summary>
        MediaDvdRam = 61,

        /// <summary>
        /// DVD-RW media.
        /// </summary>
        MediaDvdRW = 62,

        /// <summary>
        /// DVD-R media.
        /// </summary>
        MediaDvdR = 63,

        /// <summary>
        /// DVD-ROM media.
        /// </summary>
        MediaDvdRom = 64,

        /// <summary>
        /// CD+ (enhanced audio CD) media.
        /// </summary>
        MediaCDAudioPlus = 65,

        /// <summary>
        /// CD-RW media.
        /// </summary>
        MediaCDRW = 66,

        /// <summary>
        /// CD-R media.
        /// </summary>
        MediaCDR = 67,

        /// <summary>
        /// A writeable CD in the process of being burned.
        /// </summary>
        MediaCDBurn = 68,

        /// <summary>
        /// Blank writable CD media.
        /// </summary>
        MediaBlankCD = 69,

        /// <summary>
        /// CD-ROM media.
        /// </summary>
        MediaCDRom = 70,

        /// <summary>
        /// An audio file.
        /// </summary>
        AudioFiles = 71,

        /// <summary>
        /// An image file.
        /// </summary>
        ImageFiles = 72,

        /// <summary>
        /// A video file.
        /// </summary>
        VideoFiles = 73,

        /// <summary>
        /// A mixed file.
        /// </summary>
        MixedFiles = 74,

        /// <summary>
        /// Folder back.
        /// </summary>
        FolderBack = 75,

        /// <summary>
        /// Folder front.
        /// </summary>
        FolderFront = 76,

        /// <summary>
        /// Security shield. Use for UAC prompts only.
        /// </summary>
        Shield = 77,

        /// <summary>
        /// The warning icon.
        /// </summary>
        Warning = 78,

        /// <summary>
        /// The information icon.
        /// </summary>
        Information = 79,

        /// <summary>
        /// The error icon.
        /// </summary>
        Error = 80,

        /// <summary>
        /// Key.
        /// </summary>
        Key = 81,

        /// <summary>
        /// Software.
        /// </summary>
        Software = 82,

        /// <summary>
        /// A UI item, such as a button, that issues a rename command.
        /// </summary>
        Rename = 83,

        /// <summary>
        ///  A UI item, such as a button, that issues a delete command.
        /// </summary>
        Delete = 84,

        /// <summary>
        /// Audio DVD media.
        /// </summary>
        MediaAudioDvd = 85,

        /// <summary>
        /// Movie DVD media.
        /// </summary>
        MediamMoieDvd = 86,

        /// <summary>
        /// Enhanced CD media.
        /// </summary>
        MediaEnhancedCD = 87,

        /// <summary>
        /// Enhanced DVD media.
        /// </summary>
        MediaEnhancedDvd = 88,

        /// <summary>
        /// High definition DVD media in the HD DVD format.
        /// </summary>
        MediaHDDvd = 89,

        /// <summary>
        /// High definition DVD media in the Blu-ray Disc™ format.
        /// </summary>
        MediaBluray = 90,

        /// <summary>
        /// Video CD (VCD) media.
        /// </summary>
        MediaVcd = 91,

        /// <summary>
        /// DVD+R media.
        /// </summary>
        MediaDvdPlusR = 92,

        /// <summary>
        /// DVD+RW media.
        /// </summary>
        MediaDvdPlusRW = 93,

        /// <summary>
        /// A desktop computer.
        /// </summary>
        DesktopPC = 94,

        /// <summary>
        /// A mobile computer (laptop).
        /// </summary>
        MobilePC = 95,

        /// <summary>
        /// The User Accounts Control Panel item.
        /// </summary>
        Users = 96,

        /// <summary>
        ///  Smart media.
        /// </summary>
        MediaSmartMedia = 97,

        /// <summary>
        /// CompactFlash media.
        /// </summary>
        MediaCompactFlash = 98,

        /// <summary>
        /// A cell phone.
        /// </summary>
        DeviceCellphone = 99,

        /// <summary>
        /// A digital camera.
        /// </summary>
        DeviceCamera = 100,

        /// <summary>
        ///  A digital video camera.
        /// </summary>
        DeviceVideoCamera = 101,

        /// <summary>
        /// An audio player.
        /// </summary>
        DeviceAudioPlayer = 102,

        /// <summary>
        /// Connect to network.
        /// </summary>
        NetworkConnect = 103,

        /// <summary>
        /// The Network and Internet Control Panel item.
        /// </summary>
        Internet = 104,

        /// <summary>
        /// A compressed file with a .zip file name extension.
        /// </summary>
        ZipFile = 105,

        /// <summary>
        /// The Additional Options Control Panel item.
        /// </summary>
        Settings = 106,

        /// <summary>
        /// Windows Vista with Service Pack 1 (SP1) and later. High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.
        /// </summary>
        DriveHDDvd = 132,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.
        /// </summary>
        DriveBD = 133,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition DVD-ROM media in the HD DVD-ROM format.
        /// </summary>
        MediaHDDvdRom = 134,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition DVD-R media in the HD DVD-R format.
        /// </summary>
        MediaHDDvdR = 135,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition DVD-RAM media in the HD DVD-RAM format.
        /// </summary>
        MediaHDDvdRam = 136,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.
        /// </summary>
        MediaBDRom = 137,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition write-once media in the Blu-ray Disc BD-R format.
        /// </summary>
        MediaBDR = 138,

        /// <summary>
        /// Windows Vista with SP1 and later. High definition read/write media in the Blu-ray Disc BD-RE format.
        /// </summary>
        MediaBDRE = 139,

        /// <summary>
        /// Windows Vista with SP1 and later. A cluster disk array.
        /// </summary>
        ClusteredDrive = 140,
    }
}
