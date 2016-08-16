///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2005-2014 Laszlo Arvai. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Wave resource object handler
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

/// <summary>
/// Wave type resource chunk
/// </summary>
public class drfWave : drfFileResource
{
  #region · Constants ·
  public const UInt32 ClassId = 0x45564157;

  // Wave format constants
  public const byte WFFormatLengthMask = 0x03;
  public const byte WFMono = 0;
  public const byte WFStereo = (1 << 2);
  public const byte WFMonoStereoMask = (1 << 2);
  public const byte WF8bit = 0;
  public const byte WF16bit = (1 << 3);
  public const byte WF8bit16bitMask = (1 << 3);
  public const byte WF8000Hz = 0x00;
  public const byte WF11025Hz = 0x10;
  public const byte WF22050Hz = 0x20;
  public const byte WF44100Hz = 0x30;
  public const byte WFCustomSampleRate = 0xf0;
  public const byte WFSampleRateMask = 0xf0;

  #endregion

  #region · Data Members ·
  private byte[] m_wav_buffer;
  private int m_sample_rate;
  private byte m_channel_number;
  private byte m_sample_resolution;
  private int m_sample_count;
  #endregion

  #region · Constructor&Destructor ·
  /// <summary>
  /// Constructor
  /// </summary>
  public drfWave() : base(ClassId,"wave")
  {
    m_file_name = "";
  }

  #endregion

  #region · Message handlers ·
  /// <summary>
  /// Message handler
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  protected override int ProcessMessage(drfmsgMessageBase in_message)
  {
		int retval;

		retval = base.ProcessMessage(in_message);

    switch (in_message.MessageType)
    {
      case drfmsgMessageBase.drfmsgProcessCommandLine:
        return msgProcessCommandLine((drfmsgProcessCommandLine)in_message);

      case drfmsgMessageBase.drfmsgPrepareBinaryData:
        return msgPrepareBinaryData((drfmsgPrepareBinaryData)in_message);

      case drfmsgMessageBase.drfmsgUpdateBinaryData:
        return msgUpdateBinaryData((drfmsgUpdateBinaryData)in_message);

      case drfmsgMessageBase.drfmsgDisplayHelpMessage:
        return msgDisplayHelpMessage((drfmsgDisplayHelpMessage)in_message);
    }

    return retval;
  }

  /// <summary>
  /// Updates binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgUpdateBinaryData(drfmsgUpdateBinaryData in_message)
  {
    // update binary data
    int pos = 0;
    byte[] format = FormatToBinary();

    pos = m_binary_buffer.Modify(format, pos);
    pos = m_binary_buffer.Modify((UInt32)m_wav_buffer.Length, pos);
    pos = m_binary_buffer.Modify(m_wav_buffer, pos);

    return 0;
  }

    /// <summary>
  /// Prepare binary data
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgPrepareBinaryData(drfmsgPrepareBinaryData in_message)
  {
    // prepare binary data
    m_binary_buffer = new clsBinaryBuffer();

    byte[] format = FormatToBinary();

    m_binary_buffer.Length = format.Length + // format
                              sizeof(UInt32) + // size
                              m_wav_buffer.Length; // data
 
    return 0;
  }

  /// <summary>
  /// Processes command line switches
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "wave")
    {
      if (!IsExists(in_message.Parameter))
      {
        // check identifier
        if (SetErrorIfResourceIdExists(in_message.Identifier))
        {
          // add wave file
          drfWave wave_file = new drfWave();
          m_parent_class.AddClass(wave_file);

          if (wave_file.Load(in_message.Parameter))
          {
            // set resource information
            wave_file.m_file_name = in_message.Parameter;
            wave_file.m_resource_id = in_message.Identifier;

            // register wave class in the header
            drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

            register_message.Entry = wave_file;
            register_message.Id = ClassId;

            m_parent_class.BroadcastMessage(register_message);
          }
        }

        in_message.Used = true;
      }
    }

    return 0;
  }

  /// <summary>
  /// Displays help message
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgDisplayHelpMessage(drfmsgDisplayHelpMessage in_message)
  {
    Console.WriteLine(resString.UsageWave);

    return 0;
  }

  #endregion

  #region · Member functions ·

  /// <summary>
  /// Load wave file into the internal buffer
  /// </summary>
  /// <param name="in_name"></param>
  /// <returns></returns>
  private bool Load(string in_name)
  {
  	bool success = true;

    FileStream fs = new FileStream(in_name, FileMode.Open, FileAccess.Read);
    BinaryReader r = new BinaryReader(fs);
    UInt32 chunk_id;
    UInt32 chunk_length;
    UInt32 format;
    long pos;

    // load first chunk ID and length and format
    chunk_id = r.ReadUInt32();
    chunk_length = r.ReadUInt32();
    format = r.ReadUInt32();

    if (chunk_id != 0x46464952 || format != 0x45564157)
      success = false;

    // go through all entries of wave chunk
    while( success )
    {
      // load chunk id and length
      try
      {
        chunk_id = r.ReadUInt32();
        chunk_length = r.ReadUInt32();
      }
      catch
      {
        break;
      }

      // process chunk
      if (success)
      {
        pos = fs.Position;

        switch (chunk_id)
        {
          // format chunk
          case 0x20746d66:
            {
              UInt16 AudioFormat = r.ReadUInt16();
              UInt16 NumChannels = r.ReadUInt16();
              UInt32 SampleRate = r.ReadUInt32();
              UInt32 ByteRate = r.ReadUInt32();
              UInt16 BlockAlign = r.ReadUInt16();
              UInt16 BitsPerSample = r.ReadUInt16();

              if (AudioFormat != 1)
              {
                success = false;
                Console.WriteLine(resString.ErrorInvalidWaveFileParameter);
              }

              m_sample_rate = (int)SampleRate;
              m_sample_resolution = (byte)BitsPerSample;
              m_channel_number = (byte)NumChannels;
            }
            break;

          // data chunk
          case 0x61746164:
            {
              m_wav_buffer = new byte[chunk_length];

              r.Read(m_wav_buffer, 0, (int)chunk_length);

              m_sample_count = (int)(chunk_length / (m_sample_resolution / 8) / m_channel_number);
            }
            break;
        }

        fs.Seek(pos + chunk_length, SeekOrigin.Begin);
      }
    }

    // close file
    r.Close();
    fs.Close();

    // return 
    if (success)
      m_file_name = in_name;

    if (success && m_parent_class.VerboseMessages)
    {
      Console.WriteLine(resString.VerboseWave, in_name, m_sample_rate, m_channel_number, m_sample_resolution, m_sample_count);
    }

    return success;
  }

  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 20;
  }

  private byte[] FormatToBinary()
  {
    int format_buffer_size = 1;
    byte format_byte = (byte)format_buffer_size;
    byte[] buffer = new byte[1];

    switch(m_sample_rate)
    {
      case 8000:
        format_byte |= WF8000Hz;
        break;

      case 11025:
        format_byte |= WF11025Hz;
        break;

      case 22050:
        format_byte |= WF22050Hz;
        break;

      case 44100:
        format_byte |= WF44100Hz;
        break;

      default:
        format_byte = WFCustomSampleRate | 3;
        buffer = new byte[3];
        buffer[1] = (byte)(m_sample_rate % 256);
        buffer[2] = (byte)(m_sample_rate / 256);
        break;
    }

    // generate format byte
    if( m_channel_number == 2 )
    {
      format_byte |= WFStereo ;
    }

    if( m_sample_resolution == 16 )
    {
      format_byte |= WF16bit ;
    }

    buffer[0] = format_byte;

    return buffer;
  }

  #endregion
}
