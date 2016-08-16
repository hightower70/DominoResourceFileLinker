///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2005-2016 Laszlo Arvai. All rights reserved.
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
// Binary data resource object
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

/// <summary>
/// Binary data resource chunk
/// </summary>
public class drfBinary: drfFileResource
{
  #region · Constants ·
  public const UInt32 ClassId = 0x414E4942;
  #endregion

  #region · Data Members ·
  private byte[] m_buffer;
  #endregion

  #region · Constructor&Destructor ·
  /// <summary>
  /// Constructor
  /// </summary>
  public drfBinary()
    : base(ClassId, "bin")
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

    // store length
    if( m_buffer.Length > Int16.MaxValue )
    {
      pos = m_binary_buffer.Modify((UInt16)((m_buffer.Length % Int16.MaxValue) | 0x8000), pos);
      pos = m_binary_buffer.Modify((UInt16)(m_buffer.Length / Int16.MaxValue), pos);
    }
    else
    {
      pos = m_binary_buffer.Modify((UInt16)m_buffer.Length, pos);
    }

    // store data
    pos = m_binary_buffer.Modify(m_buffer, pos);

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

    m_binary_buffer.Length = sizeof(UInt16) + // Length
                             ((m_buffer.Length > Int16.MaxValue) ? sizeof(UInt16) : 0) + // additional length if length > 32767
                             m_buffer.Length; // data


		in_message.AddLength(m_binary_buffer.Length);

    return 0;
  }

  /// <summary>
  /// Processes command line switches
  /// </summary>
  /// <param name="in_message"></param>
  /// <returns></returns>
  private int msgProcessCommandLine(drfmsgProcessCommandLine in_message)
  {
    if (in_message.Command == "binary")
    {
      if (!IsExists(in_message.Parameter))
      {
        // check identifier
        if (SetErrorIfResourceIdExists(in_message.Identifier))
        {
          // add wave file
          drfBinary binary_file = new drfBinary();
          m_parent_class.AddClass(binary_file);

          if (binary_file.Load(in_message.Parameter))
          {
            // set resource information
            binary_file.m_file_name = in_message.Parameter;
            binary_file.m_resource_id = in_message.Identifier;

            // register wave class in the header
            drfmsgRegisterFileHeader register_message = new drfmsgRegisterFileHeader();

            register_message.Entry = binary_file;
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
    Console.WriteLine(resString.UsageBinary);

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

    // read binary file
    long length = fs.Length;

    m_buffer = new byte[length];

    r.Read(m_buffer, 0, m_buffer.Length);

    // close file
    r.Close();
    fs.Close();

    // return 
    if (success)
      m_file_name = in_name;

    if (success && m_parent_class.VerboseMessages)
    {
      Console.WriteLine(resString.VerboseBinary, in_name, m_buffer.Length);
    }

    return success;
  }

  /// <summary>
  /// Returns file position priority index.
  /// </summary>
  /// <returns></returns>
  public override int GetFilePositionPriority()
  {
    return 50;
  }

  #endregion
}
