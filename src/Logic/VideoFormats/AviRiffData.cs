using System;
using System.Runtime.InteropServices;
using System.Text;

/// <copyright>
/// Giora Tamir (giora@gtamir.com), 2005
/// </copyright>

namespace Nikse.SubtitleEdit.Logic
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct AVIMAINHEADER
    {    // 'avih'
        public int dwMicroSecPerFrame;
        public int dwMaxBytesPerSec;
        public int dwPaddingGranularity;
        public int dwFlags;
        public int dwTotalFrames;
        public int dwInitialFrames;
        public int dwStreams;
        public int dwSuggestedBufferSize;
        public int dwWidth;
        public int dwHeight;
        public int dwReserved0;
        public int dwReserved1;
        public int dwReserved2;
        public int dwReserved3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AVIEXTHEADER
    {          // 'dmlh'
        public int dwGrandFrames;          // total number of frames in the file
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 244)]
        public int[] dwFuture;             // to be defined later
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RECT
    {
        public short left;
        public short top;
        public short right;
        public short bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AVISTREAMHEADER
    { // 'strh'
        public int fccType;      // stream type codes
        public int fccHandler;
        public int dwFlags;
        public short wPriority;
        public short wLanguage;
        public int dwInitialFrames;
        public int dwScale;
        public int dwRate;       // dwRate/dwScale is stream tick rate in ticks/sec
        public int dwStart;
        public int dwLength;
        public int dwSuggestedBufferSize;
        public int dwQuality;
        public int dwSampleSize;
        public RECT rcFrame;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AVIOLDINDEXENTRY
    {
        public int dwChunkId;
        public int dwFlags;
        public int dwOffset;    // offset of riff chunk header for the data
        public int dwSize;      // size of the data (excluding riff header size)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TIMECODE
    {
        public short wFrameRate;
        public short wFrameFract;
        public int cFrames;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TIMECODEDATA
    {
        TIMECODE time;
        public int dwSMPTEflags;
        public int dwUser;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WAVEFORMATEX
    {
        public short  wFormatTag;
        public short  nChannels;
        public int    nSamplesPerSec;
        public int    nAvgBytesPerSec;
        public short  nBlockAlign;
        public short  wBitsPerSample;
        public short  cbSize;
    }


    static class AviRiffData
    {
        #region AVI constants

        // AVIMAINHEADER flags
        public const int AVIF_HASINDEX = 0x00000010; // Index at end of file?
        public const int AVIF_MUSTUSEINDEX = 0x00000020;
        public const int AVIF_ISINTERLEAVED = 0x00000100;
        public const int AVIF_TRUSTCKTYPE = 0x00000800; // Use CKType to find key frames
        public const int AVIF_WASCAPTUREFILE = 0x00010000;
        public const int AVIF_COPYRIGHTED = 0x00020000;

        // AVISTREAMINFO flags
        public const int AVISF_DISABLED = 0x00000001;
        public const int AVISF_VIDEO_PALCHANGES = 0x00010000;

        // AVIOLDINDEXENTRY flags
        public const int AVIIF_LIST = 0x00000001;
        public const int AVIIF_KEYFRAME = 0x00000010;
        public const int AVIIF_NO_TIME = 0x00000100;
        public const int AVIIF_COMPRESSOR = 0x0FFF0000;  // unused?

        // TIMECODEDATA flags
        public const int TIMECODE_SMPTE_BINARY_GROUP = 0x07;
        public const int TIMECODE_SMPTE_COLOR_FRAME = 0x08;


        // AVI stream FourCC codes
        public static readonly int streamtypeVIDEO = RiffParser.ToFourCC("vids");
        public static readonly int streamtypeAUDIO = RiffParser.ToFourCC("auds");
        public static readonly int streamtypeMIDI = RiffParser.ToFourCC("mids");
        public static readonly int streamtypeTEXT = RiffParser.ToFourCC("txts");

        // AVI section FourCC codes
        public static readonly int ckidAVIHeaderList = RiffParser.ToFourCC("hdrl");
        public static readonly int ckidMainAVIHeader = RiffParser.ToFourCC("avih");
        public static readonly int ckidODML = RiffParser.ToFourCC("odml");
        public static readonly int ckidAVIExtHeader = RiffParser.ToFourCC("dmlh");
        public static readonly int ckidAVIStreamList = RiffParser.ToFourCC("strl");
        public static readonly int ckidAVIStreamHeader = RiffParser.ToFourCC("strh");
        public static readonly int ckidStreamFormat = RiffParser.ToFourCC("strf");
        public static readonly int ckidAVIOldIndex = RiffParser.ToFourCC("idx1");
        public static readonly int ckidINFOList = RiffParser.ToFourCC("INFO");
        public static readonly int ckidAVIISFT = RiffParser.ToFourCC("ISFT");
        public static readonly int ckidMP3 = 0x0055;
        public static readonly int ckidWaveFMT = RiffParser.ToFourCC("fmt ");

        #endregion

    }
}
